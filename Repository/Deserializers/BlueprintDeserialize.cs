using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SyncData.Interface.Deserializers;
using SyncData.Model;
using System.Xml.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace SyncData.Repository.Deserializers
{
    public class BlueprintDeserialize : IBlueprintDeserialize
	{
		private readonly ILogger<BlueprintDeserialize> _logger;

		private IContentTypeService _contentTypeService;
		private IContentService _contentService;
		private readonly IDataTypeService _dataTypeService;
		private readonly IMediaService _mediaService;
		private IFileService _fileService;
		public BlueprintDeserialize(
			 ILogger<BlueprintDeserialize> logger,
			 IContentTypeService contentTypeService,
			 IContentService contentService,
			 IDataTypeService dataTypeService,
			 IMediaService mediaService,
			 IFileService fileService
			 )
		{
			_logger = logger;
			_contentTypeService = contentTypeService;
			_contentService = contentService;
			_dataTypeService = dataTypeService;
			_mediaService = mediaService;
			_fileService = fileService;
		}

		List<string> fileList = new List<string>();
		public async Task<bool> HandlerAsync()
		{
			try
			{
				string folder = "cSync\\Blueprints";
				if (!Directory.Exists(folder)) return false;
				string[] fyles = Directory.GetFiles(folder);
				fileList = fyles.ToList();
				foreach (string file in fyles)
				{
					creatContent(file);
				}
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError("BlueprintDeserialize {ex}", ex);
				return false;
			}
		}

		void creatContent(string file)
		{

			XElement response = XElement.Load(file);
			XElement? root = new XElement(response.Name, response.Attributes());

			string? keyVal = root.Attribute("Key").Value;
			string? aliasVal = root.Attribute("Alias").Value;
			string? levelVal = root.Attribute("Level").Value;
			if (_contentService.GetById(new Guid(keyVal)) is not null)
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


			XElement? templateNode = response.Element("Info")?.Element("Template");
			string? templateKey = templateNode.Attribute("Key").Value;
			string? templateValue = templateNode.Value;

			if (new Guid(parentKeyVal) != Guid.Empty)
			{
				foreach (string item in fileList)
				{
					string[]? a = item.Split("\\");
					string? b = a[a.Length - 1];
					string? c = b.Replace("-", "").Replace(".config", "");
					string? d = parentnameVal.Replace(" ", "").ToLower();
					if (c == d)
					{
						creatContent(item);
					}
				}
			}

			Guid parentId = Guid.Parse(parentKeyVal);
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
					if (editor?.EditorAlias == "Umbraco.MediaPicker3")
					{
						string? propString = property.Value.ToString().Trim('[', ']');
						ImageProp? imageProp = JsonConvert.DeserializeObject<ImageProp>(propString);
						if (imageProp != null)
						{
							
							IMedia? media = _mediaService.GetById(new Guid(imageProp.MediaKey.ToString()));

							if (media != null)
							{
								newContent.SetValue(property.Name.LocalName, media.GetUdi().ToString());
								continue;
							}
						}
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

			List<XElement>? schedule = response?.Element("Info")?.Element("Schedule")?.Elements().ToList();

			if (schedule?.Count != 0)
			{
				foreach (XElement item in schedule)
				{
					string? culture = item.Element("Culture").Value;
					string? action = item.Element("Action").Value;
					string? date = item.Element("Date").Value;
					DateTime actualDate = DateTime.Parse(date).ToUniversalTime();

					ContentSchedule sched = new ContentSchedule(culture,
						actualDate,
						(ContentScheduleAction)Enum.Parse(typeof(ContentScheduleAction), action));

					ContentScheduleCollection? contentScheduleCollection = new ContentScheduleCollection();
					contentScheduleCollection.Add(sched);

					_contentService.Save(newContent, contentSchedule: contentScheduleCollection);
				}
			}

			ITemplate? template = _fileService.GetTemplate(new Guid(templateKey));
			// Create content node from content template
			IContent? content = _contentService.CreateContentFromBlueprint(newContent, nodeName);
			content.TemplateId = template?.Id;
			content.SortOrder = Convert.ToInt32(sortOrder);
			content.Key = new Guid(keyVal);
			_contentService.SaveBlueprint(content);
		}
	}
}
