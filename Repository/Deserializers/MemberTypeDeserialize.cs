using Microsoft.Extensions.Logging;
using SyncData.Interface.Deserializers;
using System.Xml.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace SyncData.Repository.Deserializers
{
    public class MemberTypeDeserialize : IMemberTypeDeserialize
	{
		private readonly ILogger<MemberTypeDeserialize> _logger;

		private IMemberTypeService _memberTypeService;
		private IDataTypeService _dataTypeService;
		private IShortStringHelper _shortStringHelper;

		List<string> allMemberFiles = new List<string>();

		public MemberTypeDeserialize(
			 ILogger<MemberTypeDeserialize> logger,
			 IMemberTypeService memberTypeService,
			 IDataTypeService dataTypeService,
			 IShortStringHelper shortStringHelper
			 )
		{
			_logger = logger;
			_memberTypeService = memberTypeService;
			_dataTypeService = dataTypeService;
			_shortStringHelper = shortStringHelper;
		}

		public async Task<bool> Handler()
		{
			try
			{
				string folder = "cSync\\MemberTypes";
				if (!Directory.Exists(folder)) return false;
				string[] files = Directory.GetFiles(folder);
				allMemberFiles = files.ToList();
				foreach (IMemberType item in _memberTypeService.GetAll())
				{
					_memberTypeService.Delete(item);
				}

				foreach (string file in files)
				{
					CreateMemberType(file);
				}

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError("MemberTypeDeserialize {ex}", ex);
				return false;
			}
		}
		void CreateMemberType(string file)
		{
			XElement readFile = XElement.Load(file);
			XElement? root = new XElement(readFile.Name, readFile.Attributes());

			string? keyVal = root?.Attribute("Key")?.Value ?? "";
			string? aliasVal = root?.Attribute("Alias")?.Value ?? "";
			string? levelVal = root?.Attribute("Level")?.Value ?? "";
			IMemberType? exist = _memberTypeService.GetAll().Where(x => x.Key == (new Guid(keyVal))).FirstOrDefault();
			if (exist is not null)
			{
				return;
			}
			string? name = readFile.Element("Info").Element("Name")?.Value ?? "";
			string? icon = readFile.Element("Info").Element("Icon")?.Value ?? "";
			string? thumbnail = readFile.Element("Info").Element("Thumbnail")?.Value ?? "";
			string? description = readFile.Element("Info").Element("Description")?.Value ?? "";
			string? allowAtRoot = readFile.Element("Info").Element("AllowAtRoot")?.Value ?? "";
			string? isListView = readFile.Element("Info").Element("IsListView")?.Value ?? "";
			string? variations = readFile.Element("Info").Element("Variations")?.Value ?? "";
			string? isElement = readFile.Element("Info").Element("IsElement")?.Value ?? "";
			IEnumerable<XElement>? compositions = readFile.Element("Info").Element("Compositions").Elements();

			PropertyGroupCollection? propColl = new PropertyGroupCollection();
			List<IPropertyType> propTypes = new List<IPropertyType>();

			IEnumerable<XElement>? tabs = readFile.Element("Tabs").Elements();
			foreach (XElement tab in tabs)
			{
				string? tabKey = tab.Element("Key")?.Value ?? "";
				string? tabCaption = tab.Element("Caption")?.Value ?? "";
				string? tabAlias = tab.Element("Alias")?.Value ?? "";
				string? tabMandatory = tab.Element("Type")?.Value ?? "";
				string? tabSortOrder = tab.Element("SortOrder")?.Value ?? "";

				PropertyGroup? tabSet = new PropertyGroup(
						new PropertyTypeCollection(true))
				{
					Key = new Guid(tabKey),
					Alias = tabAlias,
					Name = tabCaption,
					Type = PropertyGroupType.Tab,
					SortOrder = Convert.ToInt16(tabSortOrder)
				};

				IEnumerable<XElement>? GenericProperties = readFile.Element("GenericProperties").Elements();

				foreach (XElement genericProperty in GenericProperties)
				{
					string? genKey = genericProperty.Element("Key")?.Value ?? "";
					string? genName = genericProperty.Element("Name")?.Value ?? "";
					string? genAlias = genericProperty.Element("Alias")?.Value ?? "";
					string? genDefinition = genericProperty.Element("Definition")?.Value ?? "";
					string? genType = genericProperty.Element("Type")?.Value ?? "";
					string? genMandatory = genericProperty.Element("Mandatory")?.Value ?? "";
					string? genValidation = genericProperty.Element("Validation")?.Value ?? "";
					string? genDescription = genericProperty.Element("Description")?.Value ?? "";
					string? genSortOrder = genericProperty.Element("SortOrder")?.Value ?? "";
					string? genTab = genericProperty.Element("Tab")?.Value ?? "";
					string? genTabAlias = genericProperty.Element("Tab")?.Attribute("Alias").Value ?? "";

					string? genCanEdit = genericProperty.Element("CanEdit")?.Value ?? "";
					string? genCanView = genericProperty.Element("CanView")?.Value ?? "";
					string? genIsSensitive = genericProperty.Element("IsSensitive")?.Value ?? "";
					string? genMandatoryMessage = genericProperty.Element("MandatoryMessage")?.Value ?? "";
					string? genValidationRegExpMessage = genericProperty.Element("ValidationRegExpMessage")?.Value ?? "";
					string? genLabelOnTop = genericProperty.Element("LabelOnTop")?.Value ?? "";
					try
					{
						IDataType dt = _dataTypeService.GetDataType(new Guid(genDefinition));
						if (genTabAlias == tabAlias)
						{
							tabSet?.PropertyTypes?.Add(new PropertyType(_shortStringHelper, dt)
							{
								Key = new Guid(genKey),
								Name = genName,
								Alias = genAlias,
								Mandatory = Convert.ToBoolean(genMandatory),
								Description = genDescription,
								SortOrder = Convert.ToInt16(genSortOrder),
								Variations = (ContentVariation)Enum.Parse(typeof(ContentVariation), variations),
								MandatoryMessage = genMandatoryMessage,
								ValidationRegExpMessage = genValidationRegExpMessage,
								LabelOnTop = Convert.ToBoolean(genLabelOnTop),
							});
							propColl.Add(tabSet);
						}
					}
					catch (Exception ex)
					{
						_logger.LogError("DataType add error {ex}", ex);
					}
				}
			}

			List<IContentTypeComposition> newCompositions = new List<IContentTypeComposition>();
			foreach (XElement composition in compositions)
			{
				string? compKey = composition.Attribute("Key").Value ?? "";
				string? compName = composition.Value ?? "";
				IMemberType? comp = _memberTypeService.GetAll().Where(x => x.Key == (new Guid(compKey))).FirstOrDefault(); ;
				if (comp == null)
				{
					foreach (string membFile in allMemberFiles)
					{
						string[]? splitFileName = membFile.Split("\\");
						if (splitFileName[splitFileName.Length - 1].ToLower().Contains(compName.ToLower()))
						{
							CreateMemberType(membFile);

							comp = _memberTypeService.GetAll().Where(x => x.Key == (new Guid(compKey))).FirstOrDefault();
						}
					}
				}
				newCompositions.Add(comp);
			}

			MemberType? newMemberType = new MemberType(_shortStringHelper, -1)
			{
				Icon = icon,
				Name = name,
				Key = new Guid(keyVal),
				Level = Convert.ToInt16(levelVal),
				AllowedAsRoot = Convert.ToBoolean(allowAtRoot),
				Alias = aliasVal,
				ContentTypeComposition = newCompositions,
				PropertyGroups = propColl
			};
			_memberTypeService.Save(newMemberType);
		}
		
	}
}
