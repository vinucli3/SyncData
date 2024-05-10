using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace SyncData.Repository
{
	public class UpdateSettings
	{
		private IContentTypeService _contentTypeService;
		private IDataTypeService _dataTypeService;
		private IShortStringHelper _shortStringHelper;
		public UpdateSettings(
			IContentTypeService contentTypeService, 
			IDataTypeService dataTypeService, 
			IShortStringHelper shortStringHelper)
        {
            _contentTypeService = contentTypeService;
			_dataTypeService = dataTypeService;
			_shortStringHelper = shortStringHelper;
        }
        public bool AddDatatype(XElement x)
		{
			XElement readFile = x;

			XElement? root = new XElement(readFile.Name, readFile.Attributes());

			string? keyVal = root?.Attribute("Key")?.Value ?? "";

			var contType = _contentTypeService.GetAll().Where(x => x.Key == new Guid(keyVal)).FirstOrDefault();
			if (contType == null) return false;

			var tabDetail = contType.PropertyGroups; ;
			PropertyGroupCollection? propColl = new PropertyGroupCollection();
			foreach (var tab in tabDetail)
			{

				IEnumerable<XElement>? genericProperties = readFile.Element("GenericProperties").Elements();
				
				foreach (XElement genericProperty in genericProperties)
				{
					string? key = genericProperty?.Element("Key")?.Value ?? "";
					string? tabName = genericProperty?.Element("Tab")?.Value ?? "";
					var getProperty = tab.PropertyTypes.Where(x => x.Key == new Guid(key)).FirstOrDefault();
					if (getProperty != null || tab.Name != tabName) continue;

					string? nameGp = genericProperty?.Element("Name")?.Value ?? "";
					string? alias = genericProperty?.Element("Alias")?.Value ?? "";
					string? definition = genericProperty?.Element("Definition")?.Value ?? "";
					string? mandatory = genericProperty?.Element("Mandatory")?.Value ?? "";
					string? validation = genericProperty?.Element("Validation")?.Value ?? "";
					string? descriptionGp = genericProperty?.Element("Description")?.Value ?? "";
					string? sortOrder = genericProperty?.Element("SortOrder")?.Value ?? "";

					string? variationsGp = genericProperty?.Element("Variations")?.Value ?? "";
					string? mandatoryMessage = genericProperty?.Element("MandatoryMessage")?.Value ?? "";
					string? validationRegExpMessage = genericProperty?.Element("ValidationRegExpMessage")?.Value ?? "";
					string? labelOnTop = genericProperty?.Element("LabelOnTop")?.Value ?? "";

					IDataType dt = _dataTypeService.GetDataType(new Guid(definition));
					PropertyType newPropType = new PropertyType(_shortStringHelper, dt)
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
						ValidationRegExp = validation,
						DataTypeKey = dt.Key
					};
					tab?.PropertyTypes?.Add(newPropType);
				}
			}
			_contentTypeService.Save(contType);
			return true;
		}
	}
}
