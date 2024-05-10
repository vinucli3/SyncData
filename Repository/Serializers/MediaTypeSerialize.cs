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

namespace SyncData.Repository.Serializers
{
	public class MediaTypeSerialize : IMediaTypeSerialize
	{
		private readonly ILogger<MediaTypeSerialize> _logger;
		private IMediaTypeService _mediaTypeService;
		public MediaTypeSerialize(
			 ILogger<MediaTypeSerialize> logger,
			 IMediaTypeService mediaTypeService
			)
		{
			_logger = logger;
			_mediaTypeService = mediaTypeService;
		}

		public async Task<bool> HandlerAsync()
		{
			try
			{
				IEnumerable<IMediaType>? mediaTypes = _mediaTypeService.GetAll();
				foreach (IMediaType mediaType in mediaTypes)
				{
					XElement mediaTypeDetail = new XElement("Content", new XAttribute("Key", mediaType.Key),
							new XAttribute("Alias", mediaType.Alias),
							new XAttribute("Level", mediaType.Level));

					XElement info =
							new XElement("Info",
							new XElement("Name", mediaType.Name),
							new XElement("Icon", mediaType.Icon),
							new XElement("Thumbnail", mediaType.Thumbnail),
							new XElement("Description", mediaType.Description != null ? mediaType.Description : ""),
							new XElement("AllowAtRoot", mediaType.AllowedAsRoot),
							new XElement("IsListView", mediaType.IsContainer),
							new XElement("Variations", mediaType.Variations),
							new XElement("IsElement", mediaType.IsElement),
							new XElement("Compositions", ""));

					mediaTypeDetail.Add(info);
					XElement genericProperties =
							new XElement("GenericProperties", "");

					foreach (IPropertyType item in mediaType.CompositionPropertyTypes)
					{
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
							new XElement("MandatoryMessage", item.MandatoryMessage != null ? item.MandatoryMessage : ""),
							new XElement("Tab", new XAttribute("Alias", mediaType.Name.ToLower()), mediaType.Name),
							new XElement("LabelOnTop", item.LabelOnTop));

						genericProperties.Add(genericProperty);
					}

					XElement? structure = new XElement("Structure");
					foreach (ContentTypeSort item in mediaType.AllowedContentTypes)
					{
						IMediaType? medTy = mediaTypes.Where(x => x.Alias.ToLower() == item.Alias.ToLower()).FirstOrDefault();
						if (medTy != null)
						{
							XElement? contentType = new XElement("MediaType", new XAttribute("Key", medTy.Key), medTy.Name);
							structure.Add(contentType);
						}
					}

					mediaTypeDetail.Add(genericProperties);
					mediaTypeDetail.Add(structure);
					try
					{
						PropertyGroup? tab = mediaType.PropertyGroups.First();
						XElement tabDetail =
							new XElement("Tabs", new XElement("Tab",
							new XElement("Key", tab.Key),
							new XElement("Caption", tab.Name),
							new XElement("Alias", tab.Alias),
							new XElement("Type", tab.Type),
							new XElement("SortOrder", tab.SortOrder)));
						mediaTypeDetail.Add(tabDetail);
					}
					catch (Exception ex)
					{
						_logger.LogError("mediaType property add error {ex}", ex);
					}

					string folder = "cSync\\MediaTypes";
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
							if (mediaType.Key == new Guid(keyVal))
							{
								System.IO.File.Delete(file); break;
							}
						}
					}
					string path = "cSync\\MediaTypes\\" + mediaType.Alias?.Replace(" ", " -").ToLower() + ".config";
					mediaTypeDetail.Save(path);
				}
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError("MediaTypeSerialize Serialize error {ex}", ex);
				return false;
			}
		}
	}
}
