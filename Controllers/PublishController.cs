using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using NUglify;
using SyncData.Interface;
using SyncData.Model;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.Attributes;
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
			 IDomainService domainService
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

			//_memberSignInManager = memberSignInManager;
		}

		[AllowAnonymous]
		[HttpGet]
		public IActionResult HeartBeat()
		{
			return new OkObjectResult(1);
		}

		[HttpGet]
		public async Task<IActionResult> CollectNodeDetail()
		{
			return Ok(await _updateContent.CollectExistingNodes());
		}
		[HttpPost]
		public async Task<IActionResult> FindDifferences([FromBody] DiffXelements nodes)
		{
			return Ok(JsonConvert.SerializeObject(await _updateContent.FindDiffNodes(nodes)));
		}

		[HttpPost]
		public async Task<IActionResult> ClearDifferences(DiffXelements source)
		{
			bool g = await _updateContent.SolveDifference(source.X1);
			return Ok(JsonConvert.SerializeObject(g));
		}

		[HttpGet]
		public async Task<IActionResult> GetNode([FromQuery] string id)
		{
			var elementPath = await _updateContent.ReadNode(new Guid(id));
			if (elementPath != "")
			{
				XElement response = XElement.Load(elementPath);
				return Ok(JsonConvert.SerializeObject(response));
			}
			else
			{
				return Ok("New Node");
			}

		}

		[HttpPost]
		public async Task<IActionResult> CreateNode(XElement source)
		{
			bool g = await _updateContent.CreateNode(source);
			return Ok();
		}
		[HttpPut]
		public async Task<IActionResult> UpdateNode(XElement source)
		{
			bool g = await _updateContent.UpdateNode(source);
			return Ok();
		}
		[HttpDelete]
		public async Task<IActionResult> DeleteNode(XElement source)
		{
			bool g = await _updateContent.DeleteNode(source);
			return Ok();
		}

		[HttpGet]
		public IActionResult ImageProcess([FromQuery] Guid id)
		{
			try
			{
				var response = _updateContent.ImageProcess(id);
				if (response == null)
				{
					return NotFound("No image found");
				}
				this._logger.LogInformation("Image Process Successfully");
				return Ok(response);
			}
			catch (Exception ex)
			{
				_logger.LogError("Image Process got error from the controller {ex}", ex);
				return NotFound(ex);
			}

		}
		[HttpPost]
		public IActionResult ImageUpdate([FromBody] MediaNameKey imageSrc)
		{
			try
			{
				//var content = _contentService.GetRootContent().FirstOrDefault();
				_updateContent.ImageUpdate(imageSrc);//, content.Id);
				this._logger.LogInformation("Image updated Successfully");
				return Ok("Successfully updated");
			}
			catch (Exception ex)
			{
				_logger.LogError("Image Update got error from the controller {ex}", ex);
				return StatusCode(500, ex.InnerException.Message);
			}

		}
		[HttpPost]
		public IActionResult UpdateContent([FromBody] TitleDto newValue)
		{
			try
			{
				//_updateContent.UpdateTitle(newValue.Value, newValue.Key);
				this._logger.LogInformation("Title update Successfully");
				return Ok("Successfully updated");
			}
			catch (Exception ex)
			{
				_logger.LogError("Title update got error from the controller {ex}", ex);
				return NotFound();
			}

			//var parent = _contentService.GetById(1058);
			//if (parent != null)
			//{
			//	parent?.SetValue("title", newValue);
			//	_contentService.SaveAndPublish(parent);
			//	return Ok("Successfully updated");
			//}
			//else
			//	return NotFound();
		}

	}
}

