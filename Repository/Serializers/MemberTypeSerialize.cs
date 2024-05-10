using Microsoft.Extensions.Logging;
using SyncData.Interface.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using static Umbraco.Cms.Core.Constants.Conventions;

namespace SyncData.Repository.Serializers
{
	public class MemberTypeSerialize : IMemberTypeSerialize
	{
		private ILoggerFactory _loggerFactory;
		private readonly ILogger<MemberTypeSerialize> _logger;
		private readonly IMemberTypeService _memberTypeServices;
		public MemberTypeSerialize(
			ILoggerFactory loggerFactory,
			 ILogger<MemberTypeSerialize> logger,
			 IMemberTypeService memberTypeService
			)
		{
			_logger = logger;
			_loggerFactory = loggerFactory;
			_memberTypeServices = memberTypeService;
		}

		public async Task<bool> HandlerAsync()
		{
			try
			{
				IEnumerable<IMemberType>? memberTypes = _memberTypeServices.GetAll();
				foreach (IMemberType memberType in memberTypes)
				{
					XElement memberDetail = new XElement("MemberType",
							new XAttribute("Key", memberType.Key),
							new XAttribute("Alias", memberType.Alias),
							new XAttribute("Level", memberType.Level));
					XElement info =
							new XElement("Info",
							new XElement("Name", memberType.Name?.Trim(" ")),
							new XElement("Icon", memberType.Icon),
							new XElement("Thumbnail", memberType.Thumbnail),
							new XElement("Description", memberType.Description == null ? "" : memberType.Description),
							new XElement("AllowAtRoot", memberType.AllowedAsRoot),
							new XElement("IsListView", memberType.IsContainer),
							new XElement("Variations", memberType.Variations),
							new XElement("IsElement", memberType.IsElement));

					XElement compxElement = new XElement("Compositions");
					IEnumerable<IContentTypeComposition>? compositions = memberType.ContentTypeComposition;
					foreach (IContentTypeComposition composition in compositions)
					{
						XElement? comp = new XElement("Composition",
							new XAttribute("Key", composition.Key),
							composition.Name);
						compxElement.Add(comp);
					}
					info.Add(compxElement);
					memberDetail.Add(info);
					XElement genericProperties =
							new XElement("GenericProperties", "");

					foreach (IPropertyType item in memberType.PropertyTypes)
					{
						PropertyGroup? tabGrp = memberType
							.PropertyGroups
							.Where(x => x.PropertyTypes
							.Select(x => x.Alias == item.Alias).FirstOrDefault()).FirstOrDefault();
						Func<string?, bool>? dsd = memberType.MemberCanEditProperty;
						XElement genericProperty =
							new XElement("GenericProperty",
							new XElement("Key", item.Key),
							new XElement("Name", item.Name),
							new XElement("Alias", item.Alias),
							new XElement("Definition", item.DataTypeKey),
							new XElement("Type", item.PropertyEditorAlias),
							new XElement("Mandatory", item.Mandatory),
							new XElement("Validation", item.Variations.ToString() != "Nothing" ? item.Variations : ""),
							new XElement("Description", new XCData(item.Description != null ? item.Description : "")),
							new XElement("SortOrder", item.SortOrder),
							new XElement("Tab", new XAttribute("Alias", tabGrp.Alias), tabGrp.Name),
							new XElement("CanEdit", "false"),
							new XElement("CanView", "false"),
							new XElement("IsSensitive", "false"),
							new XElement("MandatoryMessage", item.MandatoryMessage != null ? item.MandatoryMessage : ""),
							new XElement("ValidationRegExpMessage", item.ValidationRegExpMessage),
							new XElement("LabelOnTop", item.LabelOnTop));

						genericProperties.Add(genericProperty);
					}

					memberDetail.Add(genericProperties);

					XElement allTabs =
							new XElement("Tabs");
					List<PropertyGroup>? tabs = memberType.PropertyGroups.ToList();
					foreach (PropertyGroup tab in tabs)
					{
						XElement tabDetail = new XElement("Tab",
							new XElement("Key", tab.Key),
							new XElement("Caption", tab.Name),
							new XElement("Alias", tab.Alias),
							new XElement("Type", tab.Type),
							new XElement("SortOrder", tab.SortOrder));
						allTabs.Add(tabDetail);
					}
					memberDetail.Add(allTabs);

					string folder = "cSync\\MemberTypes";
					if (!Directory.Exists(folder))
					{
						Directory.CreateDirectory(folder);
					}
					else
					{
						string[] fyles = Directory.GetFiles(folder);

						foreach (string file in fyles)
						{
							XElement response = XElement.Load(file);
							XElement? root = new XElement(response.Name, response.Attributes());
							string? keyVal = root.Attribute("Key").Value;
							if (memberType.Key == new Guid(keyVal))
							{
								System.IO.File.Delete(file); break;
							}
						}
					}
					string path = "cSync\\MemberTypes\\" + memberType.Alias?.Replace(" ", " -").ToLower() + ".config";
					memberDetail.Save(path);
				}
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError("MemberTypeSerialize Serialize error {ex}", ex);
				return false;
			}
		}
		public async Task<bool> SingleHandlerAsync(IMemberType memberType)
		{
			try
			{
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError("MemberTypeSerialize Serialize error {ex}", ex);
				return false;
			}
		}
	}
}
