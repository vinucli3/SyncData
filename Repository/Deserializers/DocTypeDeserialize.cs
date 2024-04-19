using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SyncData.Interface.Deserializers;
using System.Xml.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace SyncData.Repository.Deserializers
{
	public class DocTypeDeserialize : IDocTypeDeserialize
	{
		private readonly ILogger<DocTypeDeserialize> _logger;

		private IContentTypeService _contentTypeService;
		private IDataTypeService _dataTypeService;
		private readonly IShortStringHelper _shortStringHelper;
		private IFileService _fileService;
		public DocTypeDeserialize(
			 ILogger<DocTypeDeserialize> logger,
			 IContentTypeService contentTypeService,
			 IDataTypeService dataTypeService,
			 IShortStringHelper shortStringHelper,
			 IFileService fileService
			 )
		{
			_logger = logger;
			_contentTypeService = contentTypeService;
			_dataTypeService = dataTypeService;
			_shortStringHelper = shortStringHelper;
			_fileService = fileService;
		}

		public async Task<bool> Handler()
		{
			try
			{
				string folder = "cSync\\ContentType";
				if (!Directory.Exists(folder)) return false;
				string[] files = Directory.GetFiles(folder);
				allfiles = files.ToList();
				_contentTypeService.GetAll().ToList().ForEach(x => _contentTypeService.Delete(x));
				var sd = _contentTypeService.GetAll();
				foreach (string file in files)
				{
					createContentType(file);
				}
				return true;
			}
			catch(Exception ex) {
				_logger.LogError("DictionaryDeserialize {ex}", ex);
				return false;
			}
		}
		List<string> allfiles = new List<string>();
		void createContentType(string file)
		{
			XElement readFile = XElement.Load(file);
			XElement? root = new XElement(readFile.Name, readFile.Attributes());

			string? keyVal = root?.Attribute("Key")?.Value ?? "";
			string? aliasVal = root?.Attribute("Alias")?.Value ?? "";
			string? levelVal = root?.Attribute("Level")?.Value ?? "";

			XElement? info = readFile.Element("Info");
			string? name = info?.Element("Name")?.Value ?? "";
			string? icon = info?.Element("Icon")?.Value ?? "";
			string? thumbnail = info?.Element("Thumbnail")?.Value ?? "";
			string? description = info?.Element("Description")?.Value ?? "";
			string? allowAtRoot = info?.Element("AllowAtRoot")?.Value ?? "";
			string? isListView = info?.Element("IsListView")?.Value ?? "";
			string? isElement = info?.Element("IsElement")?.Value ?? "";
			string? variations = info?.Element("Variations")?.Value ?? "";
			XElement? historyClnup = info?.Element("HistoryCleanup");
			string? preventCleanup = "false";
			string? keepAllVersionsNewerThanDays = "0";
			string? keepLatestVersionPerDayForDays = "0";
			if (historyClnup != null)
			{
				preventCleanup = historyClnup?.Element("PreventCleanup")?.Value ?? "false";
				keepAllVersionsNewerThanDays =
					historyClnup?.Element("KeepAllVersionsNewerThanDays")?.Value != "" ?
					historyClnup?.Element("KeepAllVersionsNewerThanDays")?.Value : "0";
				keepLatestVersionPerDayForDays =
					historyClnup?.Element("KeepLatestVersionPerDayForDays")?.Value != "" ?
					historyClnup?.Element("KeepLatestVersionPerDayForDays")?.Value : "0";
			}
			string? folderVal = info?.Element("Folder")?.Value ?? "";
			XElement? compositions = info?.Element("Compositions");

			XElement? composition = compositions?.Element("Composition");
			string? nameComposition = "";
			string? keyComposition = "";
			if (composition != null)
			{
				nameComposition = composition.Value ?? "";
				keyComposition = composition.Attribute("Key")?.Value ?? "";
			}
			string? defaultTemplate = info?.Element("DefaultTemplate")?.Value ?? "";

			IEnumerable<XElement>? allowedTemplate = info?.Element("AllowedTemplates").Elements();
			string? templName = ""; string? templKey = "";
			if (!allowedTemplate.IsNullOrEmpty())
			{
				foreach (XElement template in allowedTemplate)
				{
					templName = template.Value ?? "";
					templKey = template.Attribute("Key")?.Value ?? "";
				}
			}

			try
			{
				//Create Folder 
				EntityContainer? container = _contentTypeService.GetContainers(folderVal, 1)?.FirstOrDefault();
				if (container is null)
				{
					if (!folderVal.IsNullOrWhiteSpace())
					{
						var attempt =
						_contentTypeService.CreateContainer(-1, Guid.NewGuid(), folderVal);
						EntityContainer? result = attempt.Result?.Entity;
						_contentTypeService.SaveContainer(result);
						container = _contentTypeService.GetContainers(folderVal, 1)?.FirstOrDefault();
					}

				}
				IEnumerable<XElement>? tabDetail = readFile.Element("Tabs").Elements();
				PropertyGroupCollection? propColl = new PropertyGroupCollection();
				//Create Tab 
				foreach (XElement tab in tabDetail)
				{
					string? keyT = tab?.Element("Key")?.Value ?? "";
					string? caption = tab?.Element("Caption")?.Value ?? "";
					string? aliasT = tab?.Element("Alias")?.Value ?? "";
					string? type = tab?.Element("Type")?.Value ?? "";
					string? sortOrderT = tab?.Element("SortOrder")?.Value ?? "";

					PropertyGroup? tabSet = new PropertyGroup(
						new PropertyTypeCollection(true))
					{
						Key = new Guid(keyT),
						Alias = aliasT,
						Name = caption,
						Type = (PropertyGroupType)Enum.Parse(typeof(PropertyGroupType), type),
						SortOrder = Convert.ToInt16(sortOrderT)
					};

					IEnumerable<XElement>? genericProperties = readFile.Element("GenericProperties").Elements();

					foreach (XElement genericProperty in genericProperties)
					{
						string? key = genericProperty?.Element("Key")?.Value ?? "";
						string? nameGp = genericProperty?.Element("Name")?.Value ?? "";
						string? alias = genericProperty?.Element("Alias")?.Value ?? "";
						string? definition = genericProperty?.Element("Definition")?.Value ?? "";
						string? mandatory = genericProperty?.Element("Mandatory")?.Value ?? "";
						string? validation = genericProperty?.Element("Validation")?.Value ?? "";
						string? descriptionGp = genericProperty?.Element("Description")?.Value ?? "";
						string? sortOrder = genericProperty?.Element("SortOrder")?.Value ?? "";
						string? tabName = genericProperty?.Element("Tab")?.Value ?? "";
						string? variationsGp = genericProperty?.Element("Variations")?.Value ?? "";
						string? mandatoryMessage = genericProperty?.Element("MandatoryMessage")?.Value ?? "";
						string? validationRegExpMessage = genericProperty?.Element("ValidationRegExpMessage")?.Value ?? "";
						string? labelOnTop = genericProperty?.Element("LabelOnTop")?.Value ?? "";

						try
						{
							IDataType dt = _dataTypeService.GetDataType(new Guid(definition));
							tabSet?.PropertyTypes?.Add(new PropertyType(_shortStringHelper, dt)
							{
								Key = new Guid(key),
								Name = nameGp,
								Alias = alias,
								Mandatory = Convert.ToBoolean(mandatory),
								Description = descriptionGp,
								SortOrder = Convert.ToInt16(sortOrder),
								Variations = (ContentVariation)Enum.Parse(typeof(ContentVariation), variationsGp),
								MandatoryMessage = mandatoryMessage,
								ValidationRegExpMessage = validationRegExpMessage,
								LabelOnTop = Convert.ToBoolean(labelOnTop),
								ValidationRegExp = validation
							});
						}
						catch (Exception ex)
						{
							//_logger.LogError("Create datatype error {ex}", ex);
						}
					}
					if (tabSet is not null)
						propColl.Add(tabSet);
				}
				List<ContentTypeSort>? contentTypeSorts = new List<ContentTypeSort>();
				IEnumerable<XElement>? allowdContent = readFile.Element("Structure").Elements();
				foreach (XElement content in allowdContent)
				{
					string? key = content.Attribute("Key").Value ?? "";
					string? sortOrd = content.Attribute("SortOrder").Value ?? "";
					var contentType = _contentTypeService.Get(new Guid(key));
					if (contentType != null)
					{
						ContentTypeSort? conTypSort = new ContentTypeSort()
						{
							Alias = contentType.Alias,
							SortOrder = Convert.ToInt16(sortOrd),
							Id = new Lazy<int>(() => contentType.Id)
						};
						contentTypeSorts.Add(conTypSort);
					}
					else
					{
						var parentFile = allfiles.Where(item => item.ToString().ToLower().Contains(content.Value.ToLower()));
						foreach (var item in parentFile)
						{
							XElement prntFile = XElement.Load(item);
							if (key == prntFile.Attribute("Key").Value)
							{
								createContentType(item);
								contentType = _contentTypeService.Get(new Guid(key));
								ContentTypeSort? conTypSort = new ContentTypeSort()
								{
									Alias = contentType.Alias,
									SortOrder = Convert.ToInt16(sortOrd),
									Id = new Lazy<int>(() => contentType.Id)
								};
								contentTypeSorts.Add(conTypSort);
							}
						}

					}
				}
				// Initialize tab
				List<IContentTypeComposition> compositionsToAdd = new List<IContentTypeComposition>();
				IContentType? myComp = _contentTypeService.Get(nameComposition);
				if (myComp != null)
				{
					compositionsToAdd.Add(myComp);
				}
				ITemplate? masterDetail = null;
				if (templKey != "")
					masterDetail = _fileService.GetTemplate(new Guid(templKey));

				IContentType newCT = _contentTypeService.Get(name);
				if (newCT == null)
				{
					newCT = new ContentType(_shortStringHelper, container != null ? container.Id : -1)
					{
						Key = new Guid(keyVal),
						Name = name,
						Alias = aliasVal,
						Icon = icon,
						Thumbnail = thumbnail,
						Description = description,
						AllowedAsRoot = Convert.ToBoolean(allowAtRoot),
						IsContainer = Convert.ToBoolean(isListView),
						IsElement = Convert.ToBoolean(isElement),
						Variations = (ContentVariation)Enum.Parse(typeof(ContentVariation), variations),
						HistoryCleanup = new Umbraco.Cms.Core.Models.ContentEditing.HistoryCleanup()
						{
							KeepAllVersionsNewerThanDays = Convert.ToInt16(keepAllVersionsNewerThanDays),
							KeepLatestVersionPerDayForDays = Convert.ToInt16(keepLatestVersionPerDayForDays),
							PreventCleanup = Convert.ToBoolean(preventCleanup)
						},
						PropertyGroups = propColl,
						AllowedTemplates = _fileService.GetTemplates(masterDetail != null ? masterDetail.Alias : ""),
						ContentTypeComposition = compositionsToAdd,
						AllowedContentTypes = contentTypeSorts
					};

					_contentTypeService.Save(newCT);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}

	}
}
