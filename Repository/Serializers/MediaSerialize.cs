using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SyncData.Interface.Serializers;
using SyncData.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace SyncData.Repository.Serializers
{
    public class MediaSerialize : IMediaSerialize
	{
		private readonly ILogger<MediaSerialize> _logger;
		private readonly IMediaService _mediaService;
		public MediaSerialize(
			 ILogger<MediaSerialize> logger,
			IMediaService mediaService
			)
		{
			_logger = logger;
			_mediaService = mediaService;
		}

		public async Task<bool> HandlerAsync()
		{
			try
			{
				DirectoryInfo? directory = new DirectoryInfo(@"cSync\\Media");
				if (directory.Exists)
					foreach (FileInfo file in directory.GetFiles()) file.Delete();

				List<IMedia>? medias = _mediaService.GetRootMedia().ToList();
				List<IMedia>? temp = new List<IMedia>();
				foreach (IMedia media in medias)
				{
					long count = 0;
					List<IMedia>? childs = _mediaService.GetPagedDescendants(media.Id, 0, 100, out count).ToList();
					if (childs.Count != 0)
					{
						temp.AddRange(childs);
					}
				}
				medias.AddRange(temp);
				foreach (IMedia _media in medias)
				{
					//var property = _media.Properties.FirstOrDefault();
					MediaNameKey? path = new MediaNameKey();
					if (_media.ContentType.Name == "Image")
						path = JsonConvert.DeserializeObject<MediaNameKey>(_media.Properties.FirstOrDefault().Values.FirstOrDefault().EditedValue.ToString());
					else if (_media.ContentType.Name != "Folder")
					{
						path.Src = _media.Properties.FirstOrDefault().Values.FirstOrDefault().EditedValue.ToString();
					}
					IMedia? parent = _mediaService.GetById(_media.ParentId);
					string? pathVal = parent != null ? "/" + parent.Name.Replace(" ", "") : "";
					XElement mediaDetail = new XElement("Media",
						new XAttribute("Key", _media.Key),
						new XAttribute("Alias", _media.Name),
						new XAttribute("Level", _media.Level));
					XElement info =
					new XElement("Info",
					new XElement("Parent",
						new XAttribute("Key", parent != null ? parent.Key : Guid.Empty), parent != null ? parent.Name : ""),
					new XElement("Path", pathVal + "/" + _media.Name.Replace(" ", "")),
					new XElement("Trashed", _media.Trashed),
					new XElement("ContentType", _media.ContentType.Alias),
					new XElement("CreateDate", _media.CreateDate),
					new XElement("NodeName", new XAttribute("Default", _media.Name)),
					new XElement("SortOrder", _media.SortOrder));
					mediaDetail.Add(info);

					XElement properties =
					new XElement("Properties",
					new XElement("umbracoFile", new XElement("Value", new XCData(JsonConvert.SerializeObject(path)))));
					mediaDetail.Add(properties);


					if (!directory.Exists)
					{
						directory.Create();
					}
					string filePath = "cSync\\Media\\" + _media.Name?.Replace(" ", "-").ToLower() + ".config";
					if (System.IO.File.Exists(filePath))
					{
						string[]? dFolder = path.Src.Split("/");
						filePath = "cSync\\Media\\" + _media.Name?.Replace(" ", "-").ToLower() + "_" + dFolder[2] + ".config";
					}
					mediaDetail.Save(filePath);
				}
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError("MediaSerialize Serialize error {ex}", ex);
				return false;
			}
		}
	}
}
