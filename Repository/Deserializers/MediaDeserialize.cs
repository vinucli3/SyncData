using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SyncData.Interface.Deserializers;
using SyncData.Model;
using System.Xml.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace SyncData.Repository.Deserializers
{
    public class MediaDeserialize : IMediaDeserialize
	{
		private readonly ILogger<MediaDeserialize> _logger;
		private IMediaService _mediaService;
		private readonly IWebHostEnvironment _webHostEnvironment;
		//private readonly MediaFileManager _mediaFileManager;
		//private readonly MediaUrlGeneratorCollection _mediaUrlGeneratorCollection;
		//private readonly IShortStringHelper _shortStringHelper;
		//private readonly IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider;
		//private IContentService _contentService;

		List<string> allFiles = new List<string>();

		public MediaDeserialize(
				ILogger<MediaDeserialize> logger,
				IMediaService mediaService,
				IWebHostEnvironment webHostEnvironment
				//MediaFileManager mediaFileManager,
				//MediaUrlGeneratorCollection mediaUrlGenerators,
				//IShortStringHelper shortStringHelper,
				//IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
				//IContentService contentService
			 )
		{
			_logger = logger;
			_mediaService = mediaService;
			_webHostEnvironment = webHostEnvironment;
			//_mediaFileManager = mediaFileManager;
			//_mediaUrlGeneratorCollection = mediaUrlGenerators;
			//_shortStringHelper = shortStringHelper;
			//_contentTypeBaseServiceProvider = contentTypeBaseServiceProvider;
			//_contentService = contentService;
		}

		public async Task<bool> HandlerAsync()
		{
			try
			{
				string folder = "cSync\\Media";
				if (!Directory.Exists(folder)) return false;
				string[] files = Directory.GetFiles(folder);
				allFiles = files.ToList();
				foreach (string file in files)
				{
					CreateMedia(file);
				}
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError("MediaDeserialize {ex}", ex);
				return false;
			}
		}


		void CreateMedia(string filePath)
		{
			XElement readFile = XElement.Load(filePath); 
			XElement? root = new XElement(readFile.Name, readFile.Attributes());

			string? keyVal = root.Attribute("Key").Value ?? "";
			string? aliasVal = root.Attribute("Alias").Value ?? "";
			string? levelVal = root.Attribute("Level").Value ?? "";
			if (_mediaService.GetById(new Guid(keyVal)) != null)
			{
				return;
			}

			string? parent = readFile.Element("Info").Element("Parent").Value ?? "";
			string? path = readFile.Element("Info").Element("Path").Value ?? "";
			string? trashed = readFile.Element("Info").Element("Trashed").Value ?? "";
			string? contentType = readFile.Element("Info").Element("ContentType").Value ?? "";

			string? nodeName = readFile.Element("Info").Element("NodeName").Attribute("Default").Value ?? "";
			string? sortOrder = readFile.Element("Info").Element("SortOrder").Value ?? "";
			MediaNameKey? variations = new MediaNameKey();
			int parentExist = Constants.System.Root;
			string[]? fileName = null;
			if (contentType != "Folder")
			{
				try
				{
					variations = JsonConvert.DeserializeObject<MediaNameKey>(readFile.Element("Properties").Element("umbracoFile").Value ?? "");
					if (variations.Src.IsNullOrWhiteSpace()) variations.Src = "";
				}
				catch
				{
					variations.Src = (string)readFile.Element("Properties").Element("umbracoFile").Value ?? "";
				}

				fileName = variations.Src.Split('/');
				if (!parent.IsNullOrWhiteSpace())
				{
					string? parentKey = readFile.Element("Info").Element("Parent").Attribute("Key").Value;
					IMedia? parentDetail = _mediaService.GetById(new Guid(parentKey));
					if (parentDetail is null)
					{
						IEnumerable<string>? parentFile = allFiles.Where(item => item.ToString().ToLower().Contains(parent.ToLower()));
						foreach (string pfile in parentFile)
						{
							XElement readPFile = XElement.Load(pfile);
							XElement? response = new XElement(readPFile.Name, readPFile.Attributes());
							string? keyValP = response.Attribute("Key").Value ?? "";

							if (keyValP == parentKey)
							{
								CreateMedia(pfile);
							}
						}
						parentDetail = _mediaService.GetById(new Guid(parentKey));
					}

					parentExist = parentDetail != null ? parentDetail.Id : -1;
				}
				IMedia? _media = _mediaService.GetRootMedia().Where(x => x.Name == fileName[fileName.Length - 1]).FirstOrDefault();
				try
				{
					//stream = System.IO.File.OpenRead(_webHostEnvironment.MapPathWebRoot(variations.src));
					using (Stream stream = System.IO.File.OpenRead(_webHostEnvironment.MapPathWebRoot(variations.Src)))
					{
						// Initialize a new image at the root of the media archive
						IMedia media = _mediaService.CreateMedia(nodeName, parentExist, contentType);
						media.SetValue(Constants.Conventions.Media.File, variations.Src);
						media.Key = new Guid(keyVal);
						media.Level = Convert.ToInt32(levelVal);
						media.SortOrder = Convert.ToInt32(sortOrder);
						var result = _mediaService.Save(media);
					}
				}
				catch (Exception ex)
				{
					_logger.LogError("Media open file from wwwroot issue {ex}", ex);
				}
			}
			// Initialize a new image at the root of the media archive
			else
			{
				IMedia media = _mediaService.CreateMedia(nodeName, parentExist, contentType);
				media.Key = new Guid(keyVal);
				media.Level = Convert.ToInt32(levelVal);
				media.SortOrder = Convert.ToInt32(sortOrder);
				// Save the media
				_mediaService.Save(media);
			}
		}
	}
}
