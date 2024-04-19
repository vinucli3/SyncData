using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncData.Interface.Serializers;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Controllers;

namespace SyncData.Controllers
{
	[PluginController("ExportContent")]
	public class ExportController : UmbracoApiController
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
		public async Task<IActionResult> HeartBeat()
		{
			return new OkObjectResult(1);
		}

		[HttpGet]
		public async Task<IActionResult> ExportBlueprint()
		{
			await _blueprintSerialize.Handler();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ExportContent()
		{
			await _contentSerialize.Handler();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ExportDataType()
		{
			await _dataTypeSerialize.Handler();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ExportDictionary()
		{
			await _dictionarySerialize.Handler();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ExportDocType()
		{
			await _docTypeSerialize.Handler();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ExportDomain()
		{
			await _domainSerialize.Handler();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ExportLanguage()
		{
			await _languageSerialize.Handler();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ExportMacro()
		{
			await _macroSerialize.Handler();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ExportMedia()
		{
			await _mediaSerialize.Handler();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ExportMediaType()
		{
			await _mediaTypeSerialize.Handler();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ExportMemberType()
		{
			await _memberTypeSerialize.Handler();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ExportRelation()
		{
			await _relationSerialize.Handler();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ExportTemplate()
		{
			await _templateSerialize.Handler();
			return new OkObjectResult(1);
		}
		/// <summary>
		/// ///////
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public async Task<IActionResult> ExportUsers()
		{
			await _userSerialize.Handler();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ExportUserGroups()
		{
			await _userGroupsSerialize.Handler();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ExportMembers()
		{
			await _membersSerialize.Handler();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ExportMemberGroups()
		{
			await _memberGroupsSerialize.Handler();
			return new OkObjectResult(1);
		}

	}
}
