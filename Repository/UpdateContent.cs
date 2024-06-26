﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SyncData.Interface;
using SyncData.Interface.Deserializers;
using SyncData.Interface.Serializers;
using SyncData.Model;
using SyncData.Repository.Deserializers;
using SyncData.Repository.Serializers;
using System.Xml.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;
using static Lucene.Net.Search.FieldValueHitQueue;
using static Umbraco.Cms.Core.Collections.TopoGraph;
using static Umbraco.Cms.Core.Constants.Conventions;


namespace SyncData.Repository
{
	public class UpdateContent : IUpdateContent
	{
		private IContentSerialize _contentSerialize;
		private readonly ILogger<UpdateContent> _logger;
		private readonly IPublishedContentQuery _publishedContent;
		private readonly IMediaService _mediaService;
		private readonly IWebHostEnvironment _webHostEnvironment;
		private IContentService _contentService;
		private readonly MediaFileManager _mediaFileManager;
		private readonly MediaUrlGeneratorCollection _mediaUrlGeneratorCollection;
		private readonly IShortStringHelper _shortStringHelper;
		private readonly IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider;
		private readonly IScopeProvider _scopeProvider;
		private IContentDeserialize _contentDeserialize;
		List<DiffObject> diffArray = new List<DiffObject>();
		public UpdateContent(IContentSerialize contentSerialize,
			IPublishedContentQuery publishedContent,
			IMediaService mediaService,
			IWebHostEnvironment webHostEnvironment,
			 IContentService contentService,
			MediaFileManager mediaFileManager,
			IShortStringHelper shortStringHelper,
			ILogger<UpdateContent> logger,
			IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
			IContentDeserialize contentDeserialize,
			MediaUrlGeneratorCollection mediaUrlGenerators,
			IScopeProvider scopeProvider)
		{
			_logger = logger;
			_publishedContent = publishedContent;
			_mediaService = mediaService;
			_webHostEnvironment = webHostEnvironment;
			_contentService = contentService;
			_mediaFileManager = mediaFileManager;
			_mediaUrlGeneratorCollection = mediaUrlGenerators;
			_shortStringHelper = shortStringHelper;
			_contentTypeBaseServiceProvider = contentTypeBaseServiceProvider;
			_scopeProvider = scopeProvider;
			_contentSerialize = contentSerialize;
			_contentDeserialize = contentDeserialize;
		}
		public async Task<MediaNameKey> ImageProcess(Guid id)
		{
			MediaNameKey imageNameKey = new MediaNameKey();
			try
			{
				var content = _publishedContent.Content(id);
				MediaWithCrops? imgProp = content.Value("image") as MediaWithCrops;
				if (imgProp == null)
				{
					return null;
				}
				var medias = _mediaService.GetRootMedia().Where(x => x.Key == imgProp.Key).FirstOrDefault();
				if (imgProp != null)
				{
					var ds = _mediaService.GetById(imgProp.Key);
					if (ds != null)
					{
						var umbracoFile = ds.GetValue<string>(Constants.Conventions.Media.File);
						var sds = JsonConvert.DeserializeObject<MediaNameKey>(umbracoFile);
						umbracoFile = Path.Combine(this._webHostEnvironment.WebRootPath + sds.Src);
						byte[] imageArray = System.IO.File.ReadAllBytes(umbracoFile);
						string base64ImageRepresentation = Convert.ToBase64String(imageArray);
						if (base64ImageRepresentation != null)
						{
							imageNameKey.Key = imgProp.Key;
							imageNameKey.SortOrder = imgProp.SortOrder;
							imageNameKey.Level = imgProp.Level;
							imageNameKey.Parent = imgProp.Parent != null ? imgProp.Parent.Key : Guid.Empty;
							imageNameKey.Name = imgProp.Name;
							imageNameKey.Path = JsonConvert.DeserializeObject<MediaNameKey>(medias.Properties.FirstOrDefault().Values.FirstOrDefault().EditedValue.ToString()).Src;
							imageNameKey.Src = base64ImageRepresentation;
							imageNameKey.ContentType = imgProp.ContentType.Alias.ToString();
							imageNameKey.NodeID = content.Key;
						}
					}
				}
				_logger.LogInformation("Image Process Successfull");
			}
			catch (Exception ex)
			{
				_logger.LogError("Image Processing Error with exception {ex}", ex);
			}
			return imageNameKey;
		}
		public async Task ImageUpdate(MediaNameKey imageSrc)//, int id)
		{
			string? mediaPath = _webHostEnvironment.MapPathWebRoot("~/media");
			IMedia? _media = _mediaService.GetRootMedia().Where(x => x.Key == imageSrc.Key).FirstOrDefault();

			if (_media == null)
			{
				_logger.LogError("create image");
				bool success = await SaveImage(imageSrc.Src, imageSrc.Name, imageSrc.Path.Remove(0, 1).Replace("/", "\\"));
				if (success)
				{
					string pth = Path.Combine(this._webHostEnvironment.WebRootPath, imageSrc.Path.Remove(0, 1).Replace("/", "\\"));
					using (Stream stream = System.IO.File.OpenRead(pth))
					{
						var parentMedia = _mediaService.GetRootMedia().Where(x => x.Key == imageSrc.Parent).FirstOrDefault();
						int parent = -1;
						if (parentMedia != null)
						{
							parent = parentMedia.Id;
						}

						IMedia media = _mediaService.CreateMedia(imageSrc.Name + ".jpg", parent, imageSrc.ContentType);
						media.SetValue(Constants.Conventions.Media.File, imageSrc.Path);
						media.Key = imageSrc.Key;
						media.Level = Convert.ToInt32(imageSrc.Level);
						media.SortOrder = Convert.ToInt32(imageSrc.SortOrder);
						var result = _mediaService.Save(media);

						var node = _contentService.GetById(imageSrc.NodeID);
						List<ImageProp> newMedia = new List<ImageProp>();
						IProperty? a = node.Properties.Where(x => x.PropertyType.Name == "Image").FirstOrDefault();
						ImageProp? b = JsonConvert.DeserializeObject<ImageProp>(a.Values.FirstOrDefault().EditedValue.ToString().Replace("[", "").Replace("]", ""));
						if (b == null)
						{
							b = new ImageProp();
							b.Key = a.Key.ToString();
						}
						b.MediaKey = media.Key.ToString();
						newMedia.Add(b);
						if (parent != null)
						{
							node?.SetValue("Image", JsonConvert.SerializeObject(newMedia));
						}
						_contentService.SaveAndPublish(node);
					}
					_logger.LogInformation("Image Update Successfull");
				}
				else
				{
					_logger.LogError("Create image error");
				}
			}
			else
			{

				//var content = _publishedContent.Content(imageSrc.NodeID);
				//MediaWithCrops? tr = content.Value("image") as MediaWithCrops;
				//if (tr?.Name == imageSrc.Name)
				//{
				//	return;
				//}
				_logger.LogError("image exist");
				IContent? node = _contentService.GetById(imageSrc.NodeID);
				IMedia? media = _mediaService.GetById(_media.Id);
				List<ImageProp> newMedia = new List<ImageProp>();
				IProperty? a = node.Properties.Where(x => x.PropertyType.Name == "Image").FirstOrDefault();
				ImageProp? b = new ImageProp();
				_logger.LogInformation("qweqwew {a}", a.Values.FirstOrDefault().EditedValue.ToString());

				b = JsonConvert.DeserializeObject<ImageProp>(a.Values.FirstOrDefault().EditedValue.ToString().Replace("[", "").Replace("]", ""));
				if (b == null)
				{
					b = new ImageProp();
				}

				b.Key = a.Key.ToString();
				b.MediaKey = media.Key.ToString();
				newMedia.Add(b);
				_logger.LogInformation("Here {newMedia}", newMedia[0].Key);

				if (node != null)
				{
					node?.SetValue("Image", JsonConvert.SerializeObject(newMedia));
					_contentService.SaveAndPublish(node);
				}
				else
				{
					_logger.LogError("no content");
				}

			}
		}
		public async Task<bool> SaveImage(string ImgStr, string ImgName, string path)
		{
			var pathSpl = path.Split("\\");
			String srcpath = Path.Combine(this._webHostEnvironment.WebRootPath, pathSpl[0], pathSpl[1]);
			//Check if directory exist
			if (!Directory.Exists(srcpath))
			{
				Directory.CreateDirectory(srcpath); //Create directory if it doesn't exist
			}
			string imageName = ImgName + ".jpg";
			//set the image path
			string imgPath = Path.Combine(srcpath, pathSpl[2]);
			byte[] imageBytes = Convert.FromBase64String(ImgStr);
			System.IO.File.WriteAllBytes(imgPath, imageBytes);
			return true;
		}
		public async Task<bool> UpdateTitle(string Title, Guid id)
		{
			var parent = _contentService.GetById(id);
			if (parent != null)
			{
				parent?.SetValue("title", Title);
				_contentService.SaveAndPublish(parent);
				return true;
			}
			else
				return false;
		}
		public async Task<List<ContentDto>> CollectExistingNodes()
		{
			var allPubUnPubContent = new List<IContent>();
			var rootNodes = _contentService.GetRootContent();

			var query = new Query<IContent>(_scopeProvider.SqlContext).Where(x => x.Published || x.Trashed);

			foreach (var c in rootNodes)
			{
				allPubUnPubContent.Add(c);
				var descendants = _contentService.GetPagedDescendants(c.Id, 0, int.MaxValue, out long totalNodes, query);
				allPubUnPubContent.AddRange(descendants);
			}

			List<ContentDto> listCont = new List<ContentDto>();
			foreach (var c in allPubUnPubContent)
			{
				ContentDto n = new ContentDto() { Id = c.Id, Key = c.Key, Name = c.Name, Selected = false };
				listCont.Add(n);
			}
			return listCont;
		}
		public async Task<List<DiffObject>> FindDiffNodes(DiffXelements nodes)
		{
			XElement X1 = nodes.X1; // XElement.Parse(stringWithXmlGoesHere)
			XElement X2 = nodes.X2;
			//var children1 = X1.Elements().Elements().ToList();
			//var children2 = X2.Elements().Elements().ToList();
			var children1 = X1.Elements().ToList();
			var infoChild1 = children1[0].Elements().ToList();
			var propChild1 = children1[1].Elements().ToList();
			var children2 = X2.Elements().ToList();
			if (children2.Count == 0)
			{
				diffArray = new List<DiffObject>();
				for (int i = 0; i < infoChild1.Count; i++)
				{
					var eatr1 = infoChild1[i].Value;

					DiffObject newDiff = new DiffObject
					{
						PropName = infoChild1[i].Name.ToString(),
						PropOldValue = "",
						PropCurrValue = eatr1.ToString(),
						PropAction = "Create"
					};
					diffArray.Add(newDiff);
				}
				for (int i = 0; i < propChild1.Count; i++)
				{
					var eatr1 = propChild1[i]?.Element("Value")?.Value;
					DiffObject newDiff = new DiffObject
					{
						PropName = "Property-" + propChild1[i].Name.ToString(),
						PropOldValue = "",
						PropCurrValue = eatr1?.ToString()!,
						PropAction = "Create"
					};
					diffArray.Add(newDiff);
				}
				return diffArray;
			}
			var infoChild2 = children2[0].Elements().ToList();
			var propChild2 = children2[1].Elements().ToList();

			if (children1.Count != children2.Count)
			{
				DiffObject newDiff = new DiffObject
				{
					PropName = ""
				};
				diffArray.Add(newDiff);
			}
			else
			{
				for (int i = 0; i < infoChild1.Count; i++)
				{
					CompareElements(infoChild1[i], infoChild2[i], "Info");
				}
				for (int i = 0; i < propChild1.Count; i++)
				{
					CompareElements(propChild1[i], propChild2[i], "Property");
				}

				var createDate = diffArray.FindIndex(x => x.PropName.Contains("CreateDate"));

				if (createDate != -1)
				{
					diffArray.RemoveAt(createDate);
				}
			}
			return diffArray;
		}
		public async Task<List<DiffObject>> CompareElements(XElement element1, XElement element2, string type)
		{
			var eatr1 = element1.Attributes().FirstOrDefault();
			var eatr2 = element2.Attributes().FirstOrDefault();
			if (eatr1 != null)
			{
				if (eatr1.Value != eatr2.Value)
				{
					DiffObject newDiff = new DiffObject
					{
						PropName = type == "Property" ? "Property-" + element1.Name.ToString() : element1.Name.ToString(),
						PropOldValue = eatr2.Value.ToString(),
						PropCurrValue = eatr1.Value.ToString(),
						PropAction = "Update"
					};
					diffArray.Add(newDiff);
				}
			}
			// Compare element values
			if (element1.Value != element2.Value)
			{

				if (element1.Name != "Value")
				{

					if (element1.Value.Contains("mediaKey"))
					{
						var e1 = JsonConvert.DeserializeObject<List<ImageProp>>(element1.Value);
						var e2 = JsonConvert.DeserializeObject<List<ImageProp>>(element2.Value);
						if (e1[0].MediaKey == e2[0].MediaKey)
						{
							return diffArray;
						}
					}

					DiffObject newDiff = new DiffObject
					{
						PropName = type == "Property" ? "Property-" + element1.Name.ToString() : element1.Name.ToString(),
						PropOldValue = element2.Value.ToString(),
						PropCurrValue = element1.Value.ToString(),
						PropAction = "Update"
					};
					diffArray.Add(newDiff);

				}
			}
			return diffArray;
		}
		public async Task<bool> SolveDifference(XElement source)
		{
			XElement? root = new XElement(source.Name, source.Attributes());
			string? keyVal = root?.Attribute("Key")?.Value;

			string mainPath = "cSync\\Content";
			string[] files = Directory.GetFiles(mainPath);
			foreach (string file in files)
			{
				XElement response = XElement.Load(file);
				XElement? desroot = new XElement(response.Name, response.Attributes());

				string? deskeyVal = root?.Attribute("Key")?.Value;
				if (keyVal == deskeyVal)
				{
					System.IO.File.Delete(file);
					var nodeName = source?.Element("Info")?.Element("NodeName")?.Attribute("Default")?.Value;
					response = source!;
					string path = "cSync\\Content\\" + nodeName?.Replace(" ", "-").ToLower() + ".config";
					response?.Save(path);
					break;
				}

			}
			await _contentDeserialize.Handler();
			return true;
		}
		public async Task<string> ReadNode(Guid id)
		{
			var allPubUnPubContent = new List<IContent>();
			var rootNodes = _contentService.GetRootContent();

			var query = new Query<IContent>(_scopeProvider.SqlContext).Where(x => x.Published || x.Trashed);

			foreach (var c in rootNodes)
			{
				allPubUnPubContent.Add(c);
				var descendants = _contentService.GetPagedDescendants(c.Id, 0, int.MaxValue, out long totalNodes, query);
				allPubUnPubContent.AddRange(descendants);
			}

			var node = allPubUnPubContent.Where(x => x.Key == id).FirstOrDefault();
			string path = "";
			string folder = "cSync\\Content";
			if (!Directory.Exists(folder)) { };
			string[] fyles = Directory.GetFiles(folder);

			foreach (string file in fyles)
			{
				XElement response = XElement.Load(file);
				XElement? root = new XElement(response.Name, response.Attributes());

				string? keyVal = root.Attribute("Key").Value;
				if (id == new Guid(keyVal))
				{
					path = file; break;
				}
			}
			//string path = "cSync\\Content\\" + node.Name?.Replace(" ", "-").ToLower() + ".config";

			return path;

		}
		public async Task<bool> UpdateNode()
		{
			return true;
		}
		public bool DeleteNode()
		{
			return true;
		}
		public bool CreateNode()
		{
			return true;
		}
	}
}
