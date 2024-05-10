using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncData.Interface.Serializers;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Controllers;

namespace SyncData.Controllers
{
	[PluginController("ExportContent")]
	public class ExportController : UmbracoAuthorizedApiController
	{
		private IBlueprintSerialize _blueprintSerialize;
		private IContentSerialize _contentSerialize;
		private IDataTypeSerialize _dataTypeSerialize;
		private IDictionarySerialize _dictionarySerialize;
		private IDocTypeSerialize _docTypeSerialize;
		private IDomainSerialize _domainSerialize;
		private ILanguageSerialize _languageSerialize;
		private IMacroSerialize _macroSerialize;
		private IMediaSerialize _mediaSerialize;
		private IMediaTypeSerialize _mediaTypeSerialize;
		private IMemberTypeSerialize _memberTypeSerialize;
		private IRelationSerialize _relationSerialize;
		private ITemplateSerialize _templateSerialize;
		private IUsersSerialize _userSerialize;
		private IUserGroupSerialize _userGroupsSerialize;
		private IMemberSerialize _membersSerialize;
		private IMemberGroupSerialize _memberGroupsSerialize;

		public ExportController(
			IBlueprintSerialize blueprintSerialize,
			IContentSerialize contentSerialize,
			IDataTypeSerialize dataTypeSerialize,
			IDictionarySerialize dictionarySerialize,
			IDocTypeSerialize docTypeSerialize,
			IDomainSerialize domainSerialize,
			ILanguageSerialize languageSerialize,
			IMacroSerialize macroSerialize,
			IMediaSerialize mediaSerialize,
			IMediaTypeSerialize mediaTypeSerialize,
			IMemberTypeSerialize memberTypeSerialize,
			IRelationSerialize relationSerialize,
			ITemplateSerialize templateSerialize,
			IUsersSerialize usersSerialize,
			IUserGroupSerialize userGroupSerialize,
			IMemberSerialize memberSerialize,
			IMemberGroupSerialize memberGroupSerialize
			)
		{
			_blueprintSerialize = blueprintSerialize;
			_contentSerialize = contentSerialize;
			_dataTypeSerialize = dataTypeSerialize;
			_dictionarySerialize = dictionarySerialize;
			_docTypeSerialize = docTypeSerialize;
			_domainSerialize = domainSerialize;
			_languageSerialize = languageSerialize;
			_macroSerialize = macroSerialize;
			_mediaSerialize = mediaSerialize;
			_mediaTypeSerialize = mediaTypeSerialize;
			_memberTypeSerialize = memberTypeSerialize;
			_relationSerialize = relationSerialize;
			_templateSerialize = templateSerialize;
			_userSerialize = usersSerialize;
			_userGroupsSerialize = userGroupSerialize;
			_membersSerialize = memberSerialize;
			_memberGroupsSerialize = memberGroupSerialize;
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
		public async Task<IActionResult> ExportBlueprintAsync()
		{
			await _blueprintSerialize.HandlerAsync();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ExportContentAsync()
		{
			await _contentSerialize.HandlerAsync();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ExportDataTypeAsync()
		{
			await _dataTypeSerialize.HandlerAsync();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ExportDictionaryAsync()
		{
			await _dictionarySerialize.HandlerAsync();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ExportDocTypeAsync()
		{
			await _docTypeSerialize.HandlerAsync();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ExportDomainAsync()
		{
			await _domainSerialize.HandlerAsync();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ExportLanguageAsync()
		{
			await _languageSerialize.HandlerAsync();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ExportMacroAsync()
		{
			await _macroSerialize.HandlerAsync();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ExportMediaAsync()
		{
			await _mediaSerialize.HandlerAsync();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ExportMediaTypeAsync()
		{
			await _mediaTypeSerialize.HandlerAsync();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ExportMemberTypeAsync()
		{
			await _memberTypeSerialize.HandlerAsync();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ExportRelationAsync()
		{
			await _relationSerialize.HandlerAsync();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ExportTemplateAsync()
		{
			await _templateSerialize.HandlerAsync();
			return new OkObjectResult(1);
		}
		/// <summary>
		/// ///////
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public async Task<IActionResult> ExportUsersAsync()
		{
			await _userSerialize.HandlerAsync();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ExportUserGroupsAsync()
		{
			await _userGroupsSerialize.HandlerAsync();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ExportMembersAsync()
		{
			await _membersSerialize.HandlerAsync();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ExportMemberGroupsAsync()
		{
			await _memberGroupsSerialize.HandlerAsync();
			return new OkObjectResult(1);
		}

	}
}
