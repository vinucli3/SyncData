using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SyncData.Interface.Deserializers;
using SyncData.Model;
using System.Xml.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Scoping;

namespace SyncData.Repository.Deserializers
{
	public class ContentDeserialize : IContentDeserialize
	{
		private readonly ILogger<ContentDeserialize> _logger;

		private IContentTypeService _contentTypeService;
		private IContentService _contentService;
		private readonly IDataTypeService _dataTypeService;
		private readonly IMediaService _mediaService;
		private IFileService _fileService;
		private readonly IScopeProvider _scopeprovider;
		private IMemberGroupService _memberGroupService;
		public ContentDeserialize(
			 ILogger<ContentDeserialize> logger,
			 IContentTypeService contentTypeService,
			 IContentService contentService,
			 IDataTypeService dataTypeService,
			 IMediaService mediaService,
			  IScopeProvider scopeProvider,
			 IFileService fileService,
			 IMemberGroupService memberGroupService
			 )
		{
			_logger = logger;
			_contentTypeService = contentTypeService;
			_contentService = contentService;
			_dataTypeService = dataTypeService;
			_mediaService = mediaService;
			_fileService = fileService;
			_scopeprovider = scopeProvider;
			_memberGroupService = memberGroupService;
		}
		public async Task<bool> HandlerAsync()
		{
			try
			{
				////Collect all//////
				var allPubUnPubContent = new List<IContent>();
				var rootNodes = _contentService.GetRootContent();

				var recycledContent = _contentService.GetPagedContentInRecycleBin(0, 100, out long total).ToList();

				var query = new Query<IContent>(_scopeprovider.SqlContext).Where(x => x.Published && x.Trashed);

				foreach (var c in rootNodes)
				{
					allPubUnPubContent.Add(c);
					var descendants = _contentService.GetPagedDescendants(c.Id, 0, int.MaxValue, out long totalNodes, null);
					allPubUnPubContent.AddRange(descendants);
				}
				allPubUnPubContent.AddRange(recycledContent);
				////Delete all//////
				foreach (var item in allPubUnPubContent)
				{
					_contentService.Delete(item);
				}
				//////
				string folder = "cSync\\Content";
				if (!Directory.Exists(folder)) return false;
				string[] fyles = Directory.GetFiles(folder);
				fileList = fyles.ToList();
				foreach (string file in fyles)
				{
					await creatContentAsync(file);
				}
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError("BlueprintDeserialize {ex}", ex);
				return false;
			}
		}
		List<string> fileList = new List<string>();
		public async Task creatContentAsync(string file)
		{
			XElement response = XElement.Load(file);
			XElement? root = new XElement(response.Name, response.Attributes());
			string? keyVal = root.Attribute("Key").Value;
			string? aliasVal = root.Attribute("Alias").Value;
			string? levelVal = root.Attribute("Level").Value;
			if (_contentService.GetById(new Guid(keyVal)) != null)
			{
				return;
			}

			string? parentKeyVal = response.Element("Info").Element("Parent").Attribute("Key").Value;
			string? parentnameVal = response.Element("Info").Element("Parent").Value;
			string? path = response.Element("Info").Element("Path").Value;
			string? trashed = response.Element("Info").Element("Trashed").Value;
			string? contentType = response.Element("Info").Element("ContentType").Value;
			string? nodeName = response.Element("Info").Element("NodeName").Attribute("Default").Value;
			string? sortOrder = response.Element("Info").Element("SortOrder").Value;
			string? publishedNode = response.Element("Info")?.Element("Published").Attribute("Default").Value;

			
			if (new Guid(parentKeyVal) != Guid.Empty)
			{
				foreach (var item in fileList)
				{
					string[]? a = item.Split("\\");
					string? b = a[a.Length - 1];
					string? c = b.Replace("-", "").Replace(".config", "");
					string? d = parentnameVal.Replace(" ", "").ToLower();
					if (c == d)
					{
						await creatContentAsync(item);
					}
				}
			}

			IContent? parentNode = _contentService.GetById(new Guid(parentKeyVal));
			// Create a new child item of type 'Product'
			IContent? newContent = _contentService.Create(nodeName, parentNode != null ? parentNode.Id : -1, contentType);
			IEnumerable<XElement>? properties = response.Element("Properties").Elements();
			foreach (XElement property in properties)
			{
				string? prop = _contentTypeService?.Get(contentType)?
						.CompositionPropertyGroups
						.SelectMany(prop => prop.PropertyTypes
							.Where(x => x.Alias == property.Name.LocalName)
							.Select(x => x.PropertyEditorAlias))
						.FirstOrDefault();
				IEnumerable<IDataType>? allDataType = _dataTypeService.GetAll();
				IDataType? editor = allDataType.Where(x => x.EditorAlias == prop).FirstOrDefault();
				try
				{
					if (editor?.EditorAlias == "Umbraco.TrueFalse")
					{
						newContent.SetValue(property.Name.LocalName, property.Value == "1" ? true : false);
					}
					else if (editor?.EditorAlias == "Umbraco.MemberGroupPicker")
					{
						var memberGRp = _memberGroupService.GetByName(property.Value);

						if (memberGRp != null)
							newContent.SetValue(property.Name.LocalName, memberGRp.Id);

					}
					else
					{
						newContent.SetValue(property.Name.LocalName, property.Value);
					}
				}
				catch (Exception ex)
				{
					_logger.LogError("Editor value add error {ex}", ex);
				}
			}

			List<XElement>? schedule = response.Element("Info").Element("Schedule").Elements().ToList();
			if (schedule.Count != 0)
			{
				ContentScheduleCollection? contentScheduleCollection = new ContentScheduleCollection();
				foreach (XElement item in schedule)
				{
					string? culture = item.Element("Culture").Value;
					string? action = item.Element("Action").Value;
					string? date = item.Element("Date").Value;
					DateTime actualDate = DateTime.Parse(date).ToUniversalTime();
					ContentSchedule sched = new ContentSchedule(culture,
						actualDate,
						(ContentScheduleAction)Enum.Parse(typeof(ContentScheduleAction), action));
					
					contentScheduleCollection.Add(sched);
					_contentService.Save(newContent, contentSchedule: contentScheduleCollection);
				}
			}
			newContent.SortOrder = Convert.ToInt16(sortOrder);
			newContent.Key = new Guid(keyVal);
			var existTempl = response.Element("Info")?.Element("Template").Value;
			if (!existTempl.IsNullOrWhiteSpace())
			{
				XElement? templateNode = response.Element("Info")?.Element("Template");
				string? templateKey = templateNode.Attribute("Key").Value;
				string? templateValue = templateNode.Value;

				ITemplate? template = _fileService.GetTemplate(new Guid(templateKey));
				newContent.TemplateId = template?.Id;
			}
			
			
			if (Convert.ToBoolean(publishedNode))
			{
				try
				{
					_contentService.SaveAndPublish(newContent);
				}
				catch (Exception ex)
				{
					_logger.LogError("Publish error {ex}", ex);
				}
			}
			else
			{
				_contentService.Save(newContent);
			}

			if(Convert.ToBoolean(trashed))
			{
				_contentService.MoveToRecycleBin(newContent);
			}
		}
		public async Task<bool> SingleHandlerAsync(XElement source)
		{
			return true;
		}
	}
}
