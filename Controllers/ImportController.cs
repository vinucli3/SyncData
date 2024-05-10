using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncData.Interface.Deserializers;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Controllers;

namespace SyncData.Controllers
{
	[PluginController("ImportContent")]
	public class ImportController : UmbracoAuthorizedApiController
	{
		private IBlueprintDeserialize _blueprintDeserialize;
		private IContentDeserialize _contentDeserialize;
		private IDataTypeDeserialize _dataTypeDeserialize;
		private IDictionaryDeserialize _dictionaryDeserialize;
		private IDocTypeDeserialize _docTypeDeserialize;
		private IDomainDeserialize _domainDeserialize;
		private ILanguageDeserialize _languageDeserialize;
		private IMacroDeserialize _macroDeserialize;
		private IMediaDeserialize _mediaDeserialize;
		private IMediaTypeDeserialize _mediaTypeDeserialize;
		private IMemberTypeDeserialize _memberTypeDeserialize;
		private IRelationDeserialize _relationDeserialize;
		private ITemplateDeserialize _templateDeserialize;
		private IUsersDeserialize _usersDeserialize;
		private IUserGroupDeserialize _userGroupDeserialize;
		private IMemberDeserialize _memberDeserialize;
		private IMemberGroupDeserialize _memberGroupDeserialize;

		public ImportController(IBlueprintDeserialize blueprintDeserialize,
			IContentDeserialize contentDeserialize,
			IDataTypeDeserialize dataTypeDeserialize,
			IDictionaryDeserialize dictionaryDeserialize,
			IDocTypeDeserialize docTypeDeserialize,
			IDomainDeserialize domainDeserialize,
			ILanguageDeserialize languageDeserialize,
			IMacroDeserialize macroDeserialize,
			IMediaDeserialize mediaDeserialize,
			IMediaTypeDeserialize mediaTypeDeserialize,
			IMemberTypeDeserialize memberTypeDeserialize,
			IRelationDeserialize relationDeserialize,
			ITemplateDeserialize templateDeserialize,
			IUsersDeserialize usersDeserialize,
			IUserGroupDeserialize userGroupDeserialize,
			IMemberDeserialize memberDeserialize,
			IMemberGroupDeserialize memberGroupDeserialize
			)
		{
			_blueprintDeserialize = blueprintDeserialize;
			_contentDeserialize = contentDeserialize;
			_dataTypeDeserialize = dataTypeDeserialize;
			_dictionaryDeserialize = dictionaryDeserialize;
			_docTypeDeserialize = docTypeDeserialize;
			_domainDeserialize = domainDeserialize;
			_languageDeserialize = languageDeserialize;
			_macroDeserialize = macroDeserialize;
			_mediaDeserialize = mediaDeserialize;
			_mediaTypeDeserialize = mediaTypeDeserialize;
			_memberTypeDeserialize = memberTypeDeserialize;
			_relationDeserialize = relationDeserialize;
			_templateDeserialize = templateDeserialize;
			_usersDeserialize = usersDeserialize;
			_userGroupDeserialize = userGroupDeserialize;
			_memberDeserialize = memberDeserialize;
			_memberGroupDeserialize = memberGroupDeserialize;
		}

		[HttpGet]
		public IActionResult TestCall()
		{
			return Ok();
		}

		[AllowAnonymous]
		[HttpGet]
		public async Task<IActionResult> HeartBeatAsync()
		{
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ImportBlueprintAsync()
		{
			await _blueprintDeserialize.HandlerAsync();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ImportContentAsync()
		{
			await _contentDeserialize.HandlerAsync();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ImportDataTypeAsync()
		{
			await _dataTypeDeserialize.HandlerAsync();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ImportDictionaryAsync()
		{
			await _dictionaryDeserialize.HandlerAsync();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ImportDocTypeAsync()
		{
			await _docTypeDeserialize.HandlerAsync();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ImportDomainAsync()
		{
			await _domainDeserialize.HandlerAsync();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ImportLanguageAsync()
		{
			await _languageDeserialize.HandlerAsync();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ImportMacroAsync()
		{
			await _macroDeserialize.HandlerAsync();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ImportMediaAsync()
		{
			await _mediaDeserialize.HandlerAsync();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ImportMediaTypeAsync()
		{
			await _mediaTypeDeserialize.HandlerAsync();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ImportMemberTypeAsync()
		{
			await _memberTypeDeserialize.HandlerAsync();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ImportRelationAsync()
		{
			await _relationDeserialize.HandlerAsync();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ImportTemplateAsync()
		{
			await _templateDeserialize.HandlerAsync();
			return new OkObjectResult(1);
		}
		
		[HttpGet]
		public async Task<IActionResult> ImportUsersAsync()
		{
			await _usersDeserialize.HandlerAsync();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ImportUserGroupsAsync()
		{
			await _userGroupDeserialize.HandlerAsync();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ImportMembersAsync()
		{
			await _memberDeserialize.HandlerAsync();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ImportMemberGroupsAsync()
		{
			await _memberGroupDeserialize.HandlerAsync();
			return new OkObjectResult(1);
		}
	}
}
