using Microsoft.Extensions.Logging;
using SyncData.Interface.Deserializers;
using System.Xml.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace SyncData.Repository.Deserializers
{
	public class MediaTypeDeserialize : IMediaTypeDeserialize
	{

		private readonly ILogger<MediaTypeDeserialize> _logger;

		private IDataTypeService _dataTypeService;
		private IShortStringHelper _shortStringHelper;
		private IMediaTypeService _mediaTypeService;
		public MediaTypeDeserialize(
			 ILogger<MediaTypeDeserialize> logger,
			 IMediaTypeService mediaTypeService,
			 IDataTypeService dataTypeService,
			 IShortStringHelper shortStringHelper
			 )
		{
			_logger = logger;
			_mediaTypeService = mediaTypeService;
			_dataTypeService = dataTypeService;
			_shortStringHelper = shortStringHelper;
		}

		public async Task<bool> HandlerAsync()
		{
			try
			{
				string folder = "cSync\\MediaTypes";
				if (!Directory.Exists(folder)) return false;
				string[] files = Directory.GetFiles(folder);

				foreach (string file in files)
				{
					XElement readFile = XElement.Load(file); // XElement.Parse(stringWithXmlGoesHere)
					XElement? root = new XElement(readFile.Name, readFile.Attributes());

					string? keyVal = root.Attribute("Key").Value ?? "";
					string? aliasVal = root.Attribute("Alias").Value ?? "";
					string? levelVal = root.Attribute("Level").Value ?? "";

					string? nameVal = readFile.Element("Info").Element("Name").Value ?? "";
					string? icon = readFile.Element("Info").Element("Icon").Value ?? "";
					string? thumbnail = readFile.Element("Info").Element("Thumbnail").Value ?? "";
					string? description = readFile.Element("Info").Element("Description").Value ?? "";

					string? allowAtRoot = readFile.Element("Info").Element("AllowAtRoot").Value ?? "";
					string? isListView = readFile.Element("Info").Element("IsListView").Value ?? "";
					string? variations = readFile.Element("Info").Element("Variations").Value ?? "";
					string? isElement = readFile.Element("Info").Element("IsElement").Value ?? "";
					string? compositions = readFile.Element("Info").Element("Compositions").Value ?? "";

					IMediaType? mediaType = _mediaTypeService.GetAll().Where(x => x.Name == nameVal).FirstOrDefault();

					IEnumerable<XElement>? allowdContent = readFile.Element("Structure").Elements();
					List<ContentTypeSort>? contentTypeSorts = new List<ContentTypeSort>();
					PropertyGroupCollection? propColl = new PropertyGroupCollection();
					var tabExist = (String)readFile.Element("Tabs");
					if (!tabExist.IsNullOrWhiteSpace())
					{

						var tabDetail = readFile.Element("Tabs").Elements();
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
								string? typeGp = genericProperty?.Element("Type")?.Value ?? "";
								string? mandatory = genericProperty?.Element("Mandatory")?.Value ?? "";
								string? validation = genericProperty?.Element("Validation")?.Value ?? "";
								string? descriptionGp = genericProperty?.Element("Description")?.Value ?? "";
								string? sortOrder = genericProperty?.Element("SortOrder")?.Value ?? "";
								string? mandatoryMessage = genericProperty?.Element("MandatoryMessage")?.Value ?? "";
								string? tabName = genericProperty?.Element("Tab")?.Value ?? "";
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
										//Variations = (ContentVariation)Enum.Parse(typeof(ContentVariation), variationsGp),
										MandatoryMessage = mandatoryMessage,
										//ValidationRegExpMessage = validationRegExpMessage,
										LabelOnTop = Convert.ToBoolean(labelOnTop),
										ValidationRegExp = validation
									});
								}
								catch (Exception ex)
								{
									_logger.LogError("Create datatype error {ex}", ex);
								}
							}
							if (tabSet is not null)
								propColl.Add(tabSet);
						}
					}

					foreach (XElement content in allowdContent)
					{
						string? aliasType = content.Value ?? "";
						IMediaType? type = _mediaTypeService.GetAll().Where(x => x.Name == aliasType).FirstOrDefault();
						if (type != null)
						{
							ContentTypeSort? conTypSort = new ContentTypeSort()
							{
								Alias = type.Alias,
								SortOrder = type.SortOrder,
								Id = new Lazy<int>(() => type.Id)
							};
							contentTypeSorts.Add(conTypSort);
						}
					}

					mediaType.PropertyGroups = propColl;
					mediaType.AllowedContentTypes = contentTypeSorts;
					mediaType.Key = new Guid(keyVal);
					mediaType.Alias = aliasVal;
					mediaType.Level = Convert.ToInt16(levelVal);
					mediaType.Name = nameVal;
					mediaType.Icon = icon;
					mediaType.Thumbnail = thumbnail;
					mediaType.Description = description;
					mediaType.AllowedAsRoot = Convert.ToBoolean(allowAtRoot);
					mediaType.IsContainer = Convert.ToBoolean(isListView);
					mediaType.Variations = (ContentVariation)Enum.Parse(typeof(ContentVariation), variations);
					mediaType.IsElement = Convert.ToBoolean(isElement);
					_mediaTypeService.Save(mediaType);
				}
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError("DictionaryDeserialize {ex}", ex);
				return false;
			}
		}
	}
}
