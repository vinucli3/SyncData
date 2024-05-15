using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SyncData.Interface;
using SyncData.Model;
using SyncData.PublishServer;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Cms.Web.Common.Controllers;


namespace SyncData.Controllers
{
	
	[PluginController("PublishContent")]
	public class PublishController : UmbracoApiController
	{
		private static readonly JsonSerializerOptions _options =
		new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

		private ILoggerFactory _loggerFactory;
		private readonly ILogger<PublishController> _logger;
		private IContentService _contentService;
		private readonly IUpdateContent _updateContent;
		private readonly UmbracoHelper _umbracoHelper;
		private IFileService _template;
		private readonly IVariationContextAccessor _variationContextAccessor;
		private readonly IMediaService _mediaService;
		private IContentTypeService _contentTypeService;
		private readonly IPublishedContentQuery _publishedContent;
		private readonly IDataTypeService _dataTypeService;
		private readonly ILanguageRepository _languageRepository;
		private readonly IScopeProvider _scopeProvider;
		private ILocalizationService _localizationService;
		private IMediaTypeService _mediaTypeService;
		private readonly IMemberTypeService _memberTypeServices;
		private IDomainService _domainService;
		private IHttpClientFactory _httpFactory { get; set; }

		public PublishController(
			 IContentService contentService,
			IUpdateContent updateContent,
			 ILoggerFactory loggerFactory,
			 ILogger<PublishController> logger,
			 UmbracoHelper umbracoHelper,
			 IFileService template,
			 IVariationContextAccessor variationContextAccessor,
			 IMediaService mediaService,
			 IContentTypeService contentTypeService,
			 IPublishedContentQuery publishedContent,
			 IDataTypeService dataTypeService,
			 ILanguageRepository languageRepository,
			 IScopeProvider scopeProvider,
			 ILocalizationService localizationService,
			 IMediaTypeService mediaTypeService,
			 IMemberTypeService memberTypeService,
			 IDomainService domainService,
			 IHttpClientFactory httpClientFactory
			 )
		{
			_contentService = contentService;
			_updateContent = updateContent;
			this._logger = logger;
			this._loggerFactory = loggerFactory;
			_umbracoHelper = umbracoHelper;
			_template = template;
			_variationContextAccessor = variationContextAccessor;
			_mediaService = mediaService;
			_contentTypeService = contentTypeService;
			_publishedContent = publishedContent;
			_dataTypeService = dataTypeService;
			_languageRepository = languageRepository;
			_scopeProvider = scopeProvider;
			_localizationService = localizationService;
			_mediaTypeService = mediaTypeService;
			_memberTypeServices = memberTypeService;
			_domainService = domainService;
			_httpFactory = httpClientFactory;
		}

		//[Authorize]
		[HttpGet]
		public async Task<List<HeartBeatDTO>> HeartBeat()
		{
			List<HeartBeatDTO> heartBeatDTO = new List<HeartBeatDTO>();
			
				using (var scope = _scopeProvider.CreateScope(autoComplete: true))
				{
					// build a query to select everything the people table
					var sql = scope.SqlContext.Sql().Select("*").From("serverModel");
					var host = HttpContext.Request.Host;
					// fetch data from the database with the query and map to the Person class
					List<ServerModel>? allServers = scope.Database.Fetch<ServerModel>(sql);
				foreach (var item in allServers)
				{
					HeartBeatDTO beatDTO = new HeartBeatDTO();
					beatDTO.Server = item.Url;
					beatDTO.Name = item.Name;
					//_logger.LogInformation(url);
					//Creating the HttpWebRequest
					try
					{
						HttpWebRequest request = WebRequest.Create(item.Url.Replace("https","http")) as HttpWebRequest;
						//Setting the Request method HEAD, you can also use GET too.
						request.Method = "checkConnection";
						//Getting the Web Response.
						HttpWebResponse response = request.GetResponse() as HttpWebResponse;
						//Returns TRUE if the Status code == 200
						response.Close();
						beatDTO.Status = 0;
					}
					catch
					{
						//Any exception will returns false.
						beatDTO.Status = 1;
					}
					heartBeatDTO.Add(beatDTO);
				}
				return heartBeatDTO;
			}
			
		}
		public async Task<IActionResult> checkConnection()
		{
			return Ok();
		}
		[HttpGet]
		public async Task<IActionResult> CollectNodeDetailAsync()
		{
			return Ok(await _updateContent.CollectExistingNodesAsync());
		}
		[HttpPost]
		public async Task<IActionResult> FindDifferencesAsync([FromBody] UpdateDTO idKey)
		{
			return Ok(JsonConvert.SerializeObject(await _updateContent.FindDiffNodesAsync(idKey)));
		}
		[HttpPost]
		public async Task<IActionResult> NodeUpdateAsync([FromBody] UpdateDTO idKey)
		{
			
			bool g = await _updateContent.SolveDifferenceAsync(idKey);
			return Ok(JsonConvert.SerializeObject(g));
		}
		[HttpGet]
		public async Task<IActionResult> GetNodeAsync([FromQuery] string id)
		{
			var element= await _updateContent.ReadNodeAsync(new Guid(id));
			if (element != null)
			{
				return Ok(JsonConvert.SerializeObject(element));
			}
			else
			{
				return Ok("New Node");
			}

		}
		[HttpPost]
		public async Task<IActionResult> CreateNodeAsync(XElement source)
		{
			bool g = await _updateContent.CreateNodeAsync(source);
			return Ok();
		}
		[HttpPut]
		public async Task<IActionResult> UpdateNodeAsync([FromBody] XElement xElement)
		{
			_logger.LogInformation("======================%%%");
			bool g = await _updateContent.UpdateNodeAsync(xElement);
			_logger.LogInformation("C {g}",g);
			return Ok();
		}
		[HttpPost]
		public async Task<IActionResult> DeleteNodeAsync(UpdateDTO source)
		{
			bool g = await _updateContent.DeleteNodeAsync(source);
			return Ok();
		}
		[HttpDelete]
		public async Task<IActionResult> DeleteNodeRemoteAsync([FromBody] List<Guid> source)
		{
			bool g = await _updateContent.DeleteRemoteAsync(source);
			return Ok();
		}
		[HttpGet]
		public async Task<List<Guid>> CollectNodes([FromQuery] string id)
		{
			return await _updateContent.CollectAllNodes(new Guid(id));
		}
	}
}

