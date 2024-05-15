using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NUglify;
using SyncData.Interface;
using SyncData.Interface.Deserializers;
using SyncData.Interface.Serializers;
using SyncData.Model;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Scoping;


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
		private IFileService _fileService;
		private IMemberGroupService _memberGroupService;
		List<DiffObject> diffArray = new List<DiffObject>();
		private IRelationService _relationService;
		private IHttpClientFactory _httpFactory { get; set; }
		public UpdateContent(
			IHttpClientFactory httpClientFactory,
			IContentSerialize contentSerialize,
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
			IScopeProvider scopeProvider,
			IFileService fileService,
			IMemberGroupService memberGroupService,
			IRelationService relationService)
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
			_fileService = fileService;
			_memberGroupService = memberGroupService;
			_relationService = relationService;
			_httpFactory = httpClientFactory;
		}

		public async Task<List<ContentDto>> CollectExistingNodesAsync()
		{
			var allPubUnPubContent = new List<IContent>();
			var rootNodes = _contentService.GetRootContent();

			var recycledContent = _contentService.GetPagedContentInRecycleBin(0, 100, out long total).ToList();

			var query = new Query<IContent>(_scopeProvider.SqlContext).Where(x => x.Published && x.Trashed);

			foreach (var c in rootNodes)
			{
				allPubUnPubContent.Add(c);
				var descendants = _contentService.GetPagedDescendants(c.Id, 0, int.MaxValue, out long totalNodes, null);
				allPubUnPubContent.AddRange(descendants);
			}
			allPubUnPubContent.AddRange(recycledContent);

			List<ContentDto> listCont = new List<ContentDto>();
			foreach (var c in allPubUnPubContent)
			{
				ContentDto n = new ContentDto() { Id = c.Id, Key = c.Key, Name = c.Name, Selected = false };
				listCont.Add(n);
			}
			return listCont;
		}
		public async Task<List<DiffObject>> FindDiffNodesAsync(UpdateDTO idKey)
		{
			try
			{
				XElement source = await ReadNodeAsync(new Guid(idKey.Id));
				_logger.LogInformation("A1");
				XElement remote = await RemoteReadNodeAsync(idKey);
				_logger.LogInformation("A2");
				XElement X1 = source; // XElement.Parse(stringWithXmlGoesHere)
				XElement X2 = remote;
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
							PropAction = "Create",
							PropType = "Info"
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
							PropAction = "Create",
							PropType = "Property"
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
						await CompareElementsAsync(infoChild1[i], infoChild2[i], "Info");
					}
					for (int i = 0; i < propChild1.Count; i++)
					{
						await CompareElementsAsync(propChild1[i], propChild2[i], "Property");
					}

					var createDate = diffArray.FindIndex(x => x.PropName.Contains("CreateDate"));

					if (createDate != -1)
					{
						diffArray.RemoveAt(createDate);
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError("Compare error {ex}", ex);
			}
			return diffArray;
		}
		public async Task<List<DiffObject>> CompareElementsAsync(XElement element1, XElement element2, string type)
		{
			var eatr1 = element1.Attributes().FirstOrDefault();
			var eatr2 = element2.Attributes().FirstOrDefault();
			if (eatr1 != null && eatr2 != null)
			{
				if (eatr1.Value != eatr2.Value)
				{
					DiffObject newDiff = new DiffObject
					{
						PropName = type == "Property" ? "Property-" + element1.Name.ToString() : element1.Name.ToString(),
						PropOldValue = eatr2.Value.ToString(),
						PropCurrValue = eatr1.Value.ToString(),
						PropAction = "Update",
						PropType = type
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
						PropAction = "Update",
						PropType = type
					};
					diffArray.Add(newDiff);

				}
			}
			return diffArray;
		}
		public async Task<bool> SolveDifferenceAsync(UpdateDTO idKey)
		{
			var source = await ReadNodeAsync(new Guid(idKey.Id));
			string jsonData = JsonConvert.SerializeObject(source);
			using (HttpClient client = new HttpClient())
			{
				HttpResponseMessage response = new HttpResponseMessage();
				if (idKey.Action == "Update")
				{
					string url = idKey.Url + "umbraco/publishcontent/publish/UpdateNode";
					HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, url);
					request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");

					response = await client.SendAsync(request);
				}
				else if (idKey.Action == "Create")
				{
					string url = idKey.Url + "umbraco/publishcontent/publish/CreateNode";
					HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
					request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");

					response = await client.SendAsync(request);

				}
				// Handling response
				if (response.IsSuccessStatusCode)
					return true;
				else
					return false;
			}
		}
		public async Task<XElement> ReadNodeAsync(Guid id)
		{
			XElement response = null;
			var allPubUnPubContent = new List<IContent>();
			var rootNodes = _contentService.GetRootContent();

			var recycledContent = _contentService.GetPagedContentInRecycleBin(0, 100, out long total).ToList();

			var query = new Query<IContent>(_scopeProvider.SqlContext).Where(x => x.Published && x.Trashed);

			foreach (var c in rootNodes)
			{
				allPubUnPubContent.Add(c);
				var descendants = _contentService.GetPagedDescendants(c.Id, 0, int.MaxValue, out long totalNodes, null);
				allPubUnPubContent.AddRange(descendants);
			}
			allPubUnPubContent.AddRange(recycledContent);

			var node = allPubUnPubContent.Where(x => x.Key == id).FirstOrDefault();
			string path = "";
			string folder = "cSync\\Content";
			if (!Directory.Exists(folder)) { };
			string[] fyles = Directory.GetFiles(folder);

			foreach (string file in fyles)
			{
				XElement fileExist = XElement.Load(file);
				XElement? root = new XElement(fileExist.Name, fileExist.Attributes());

				string? keyVal = root.Attribute("Key").Value;
				if (id == new Guid(keyVal))
				{
					path = file; break;
				}
			}
			//string path = "cSync\\Content\\" + node.Name?.Replace(" ", "-").ToLower() + ".config";
			//var elementPath = await _updateContent.ReadNodeAsync(new Guid(id));
			if (path != "")
			{
				response = XElement.Load(path);
			}
			return response;

		}
		public async Task<XElement> RemoteReadNodeAsync(UpdateDTO idKey)
		{
			XElement? data = null;
			var client = _httpFactory.CreateClient("HttpWeb");
			client.BaseAddress = new Uri(idKey.Url + "umbraco/publishcontent/publish/");
			var response = await client.GetStringAsync("GetNode?id=" + idKey.Id).ConfigureAwait(false);
			if (response == "New Node")
			{
				response = "{ \"Content\": \"\" }";
			}
			try
			{
				XmlDocument doc = (XmlDocument)JsonConvert.DeserializeXmlNode(response);
				data = doc.DocumentElement.GetXElement();
			}
			catch (Exception ex)
			{
				_logger.LogError("RemoteReadNodeAsync {ex}", ex);
			}
			return data;

		}
		public async Task<bool> CreateNodeAsync(XElement source)
		{
			var nodeName = source?.Element("Info")?.Element("NodeName")?.Attribute("Default")?.Value;
			string path = "cSync\\Content\\" + nodeName?.Replace(" ", "-").ToLower() + ".config";

			source?.Save(path);
			_logger.LogInformation("Save {file}", path);


			await _contentDeserialize.creatContentAsync(path);
			return true;
		}
		public async Task<bool> UpdateNodeAsync(XElement source)
		{
			string? keyVal = source?.Attribute("Key")?.Value;

			var children = source.Elements().ToList();
			var infoChild = children[0].Elements().ToList();
			var propChild = children[1].Elements().ToList();
			string? parentKeyVal = source.Element("Info").Element("Parent").Attribute("Key").Value;
			string? trashed = source.Element("Info").Element("Trashed").Value;
			string? nodeName = source.Element("Info").Element("NodeName").Attribute("Default").Value;
			string? sortOrder = source.Element("Info").Element("SortOrder").Value;
			string? publishedNode = source.Element("Info")?.Element("Published").Attribute("Default").Value;
			var existTempl = source.Element("Info")?.Element("Template").Value;

			var node = _contentService.GetById(new Guid(keyVal));
			if (node.Trashed == false && Convert.ToBoolean(trashed) == true)
			{
				_logger.LogInformation("move to recyclebin");
				_contentService.MoveToRecycleBin(node);
			}
			if (node.Trashed == true && Convert.ToBoolean(trashed) == false)
			{
				List<IContent>? recycledContent = _contentService.GetPagedContentInRecycleBin(0, 100, out long total).ToList();
				_logger.LogInformation("restore A1");
				foreach (var content in recycledContent)
				{
					_logger.LogInformation("restore A2");
					if (node.Key == content.Key)
					{
						_logger.LogInformation("restore A3");
						var relation = _relationService.GetByChildId(content.Id).FirstOrDefault();
						if (relation != null)
						{
							_logger.LogInformation("restore node");
							_contentService.Move(node, relation.ParentId);
						}
					}

				}
			}

			var nodeProp = node.Properties.ToList();
			foreach (var prop in propChild)
			{
				var nodpr = node.Properties.Where(x => x.Alias == prop.Name.ToString()).FirstOrDefault();
				if (nodpr.PropertyType.PropertyEditorAlias == "Umbraco.MemberGroupPicker")
				{
					var memberGRp = _memberGroupService.GetByName(prop.Value);

					if (memberGRp != null)
						nodpr.SetValue(memberGRp.Id);
				}
				else
					nodpr.SetValue(prop.Value);
			}

			if (new Guid(parentKeyVal) != Guid.Empty)
			{
				IContent? parentNode = _contentService.GetById(new Guid(parentKeyVal));
				node.ParentId = parentNode.Id;

			}
			node.Name = nodeName;
			node.SortOrder = Convert.ToInt16(sortOrder);
			if (!existTempl.IsNullOrWhiteSpace())
			{
				XElement? templateNode = source.Element("Info")?.Element("Template");
				string? templateKey = templateNode.Attribute("Key").Value;
				string? templateValue = templateNode.Value;

				ITemplate? template = _fileService.GetTemplate(new Guid(templateKey));
				node.TemplateId = template?.Id;
			}
			if (Convert.ToBoolean(publishedNode))
				_contentService.SaveAndPublish(node);
			else
				_contentService.Save(node);

			ContentScheduleCollection? contentScheduleCollection = new ContentScheduleCollection();
			List<XElement>? schedule = source.Element("Info").Element("Schedule").Elements().ToList();
			if (schedule.Count != 0)
			{

				foreach (XElement item in schedule)
				{
					string? culture = item.Element("Culture").Value;
					string? action = item.Element("Action").Value;
					string? date = item.Element("Date").Value;
					DateTime actualDate = DateTime.Parse(date).ToUniversalTime();
					ContentSchedule sched = new ContentSchedule(culture,
						actualDate,
						(ContentScheduleAction)Enum.Parse(typeof(ContentScheduleAction), action));

					contentScheduleCollection.Add(sched);

				}
				_contentService.Save(node, contentSchedule: contentScheduleCollection);
			}



			return true;
		}
		public async Task<bool> DeleteNodeAsync(UpdateDTO idKey)
		{
			var client = _httpFactory.CreateClient("HttpWeb");
			client.BaseAddress = new Uri(idKey.Url + "umbraco/publishcontent/publish/");
			var guids = await client.GetStringAsync("CollectNodes?id=" + idKey.Id).ConfigureAwait(false);
			var remote = JsonConvert.DeserializeObject<List<Guid>>(guids);
			var local = await CollectAllNodes(new Guid(idKey.Id));
			var list3 = remote.Except(local).ToList();

			string jsonData = JsonConvert.SerializeObject(list3);
			using (HttpClient client1 = new HttpClient())
			{
				HttpResponseMessage response = new HttpResponseMessage();
				string url = idKey.Url + "umbraco/publishcontent/publish/DeleteNodeRemote";
				HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, url);
				request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");
				response = await client1.SendAsync(request);

			}

			return true;
		}
		public async Task<bool> DeleteRemoteAsync([FromBody] List<Guid> nodekeys)
		{
			foreach (var nodekey in nodekeys)
			{
				var node = _contentService.GetById(nodekey);
				if (node != null)
				{
					_contentService.MoveToRecycleBin(node);
				}
			}

			return true;
		}
		public async Task<List<Guid>> CollectAllNodes(Guid id)
		{
			List<Guid> nodes = new List<Guid>();
			var cont = _contentService.GetById(id);
			var allChildNodes = _contentService.GetPagedChildren(cont.Id, 0, 100, out long count);
			foreach (var child in allChildNodes)
			{
				nodes.Add(child.Key);
			}
			return nodes;
		}
	}
}
