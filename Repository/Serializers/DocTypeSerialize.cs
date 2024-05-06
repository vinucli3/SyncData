using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SyncData.Interface.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace SyncData.Repository.Serializers
{
    public class DocTypeSerialize : IDocTypeSerialize
	{
		private readonly ILogger<DocTypeSerialize> _logger;
		private IContentTypeService _contentTypeService;
		public DocTypeSerialize(
			 ILogger<DocTypeSerialize> logger,
			 IContentTypeService contentTypeService
			)
		{
			_logger = logger;
			_contentTypeService = contentTypeService;
		}
		public async Task<bool> Handler()
		{
			try
			{
				//contentType
				IEnumerable<IContentType> contentTypes = _contentTypeService.GetAll();
				foreach (IContentType contentType in contentTypes)
				{
					XElement contentDetail = new XElement("ContentType", new XAttribute("Key", contentType.Key), new XAttribute("Alias", contentType.Alias), new XAttribute("Level", contentType.Level));
					EntityContainer container = _contentTypeService.GetContainer(contentType.ParentId);

					IContentTypeComposition? compositionAliases = contentType.ContentTypeComposition.FirstOrDefault();

					XElement info =
							new XElement("Info",
							new XElement("Name", contentType.Name?.Trim(" ")),
							new XElement("Icon", contentType.Icon),
							new XElement("Thumbnail", contentType.Thumbnail),
							new XElement("Description", contentType.Description == null ? "" : contentType.Description),
							new XElement("AllowAtRoot", contentType.AllowedAsRoot.ToString().ToLower()),
							new XElement("IsListView", contentType.IsContainer.ToString().ToLower()),
							new XElement("Variations", contentType.Variations),
							new XElement("IsElement", contentType.IsElement.ToString().ToLower()),
							new XElement("HistoryCleanup", new XElement("PreventCleanup", contentType?.HistoryCleanup?.PreventCleanup.ToString().ToLower()),
							new XElement("KeepAllVersionsNewerThanDays", contentType?.HistoryCleanup?.KeepAllVersionsNewerThanDays == null ? "" : contentType.HistoryCleanup.KeepAllVersionsNewerThanDays),
							new XElement("KeepLatestVersionPerDayForDays", contentType?.HistoryCleanup?.KeepLatestVersionPerDayForDays == null ? "" : contentType.HistoryCleanup.KeepLatestVersionPerDayForDays)),
							new XElement("Folder", container?.Name),
							new XElement("Compositions", compositionAliases != null ? new XElement("Composition",
							new XAttribute("Key", compositionAliases?.Key == null ? "" : compositionAliases.Key), compositionAliases?.Alias) : compositionAliases),
							new XElement("DefaultTemplate", contentType?.DefaultTemplate == null ? "" : contentType?.DefaultTemplate.Alias),
							new XElement("AllowedTemplates", contentType.AllowedTemplates.Count() != 0 ? new XElement("Template",
							new XAttribute("Key", contentType.AllowedTemplates.Select(x => x.Key).FirstOrDefault()),
							contentType.AllowedTemplates.Select(x => x.Alias).FirstOrDefault()) : contentType.AllowedTemplates
							));

					contentDetail.Add(info);
					var tabs = contentType.PropertyGroups;
					XElement tabDetail = new XElement("Tabs", "");
					XElement genericProperties = new XElement("GenericProperties", "");

					foreach (var tab in tabs)
					{
						IEnumerable<IPropertyType>? getProperties = tab.PropertyTypes.OrderBy(x => x.Alias).ToList();
						foreach (IPropertyType item in getProperties)
						{
							if (compositionAliases != null) if (item.Key == compositionAliases.CompositionPropertyTypes.FirstOrDefault().Key) continue;
							XElement genericProperty = new XElement("GenericProperty",
								new XElement("Key", item.Key),
								new XElement("Name", item.Name),
								new XElement("Alias", item.Alias),
								new XElement("Definition", item.DataTypeKey),
								new XElement("Type", item.PropertyEditorAlias),
								new XElement("Mandatory", item.Mandatory != null ? item.Mandatory : ""),
								new XElement("Validation", item.ValidationRegExp != null ? item.ValidationRegExp : ""),
								new XElement("Description", new XCData(item.Description != null ? item.Description : "")),
								new XElement("SortOrder", item.SortOrder),
								new XElement("Tab", new XAttribute("Alias", tab.Alias), tab.Name),
								new XElement("Variations", item.Variations != null ? item.Variations : ""),
								new XElement("MandatoryMessage", item.MandatoryMessage != null ? item.MandatoryMessage : ""),
								new XElement("ValidationRegExpMessage", item.ValidationRegExpMessage != null ? item.ValidationRegExpMessage : ""),
								new XElement("LabelOnTop", item.LabelOnTop)
								);
							genericProperties.Add(genericProperty);
						}

						XElement tabContent =
								new XElement("Tab",
								new XElement("Key", tab.Key),
								new XElement("Caption", tab.Name),
								new XElement("Alias", tab.Alias),
								new XElement("Type", tab.Type),
								new XElement("SortOrder", tab.SortOrder));

						tabDetail.Add(tabContent);
					}

					XElement? structure = new XElement("Structure");
					foreach (ContentTypeSort item in contentType.AllowedContentTypes)
					{
						IContentType? medTy = _contentTypeService.GetAll().Where(x => x.Alias.ToLower() == item.Alias.ToLower()).FirstOrDefault();
						if (medTy != null)
						{
							XElement? contType = new XElement("ContentType", new XAttribute("Key", medTy.Key), new XAttribute("SortOrder", item.SortOrder), medTy.Alias);
							structure.Add(contType);
						}
					}
					contentDetail.Add(structure);
					contentDetail.Add(genericProperties);
					contentDetail.Add(tabDetail);
					string folder = "cSync\\ContentType";
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
							if (contentType.Key == new Guid(keyVal))
							{
								System.IO.File.Delete(file); break;
							}
						}
					}
					string path = "cSync\\ContentType\\" + contentType.Alias?.Replace(" ", " -").ToLower() + ".config";
					contentDetail.Save(path);
				}

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError("DocTypeSerialize Serialize error {ex}", ex);
				return false;
			}
		}
	}
}
