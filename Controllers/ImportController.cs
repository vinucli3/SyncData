using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncData.Interface.Deserializers;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Controllers;

namespace SyncData.Controllers
{
	[PluginController("ImportContent")]
	public class ImportController : UmbracoApiController
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
		public async Task<IActionResult> HeartBeat()
		{
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ImportBlueprint()
		{
			await _blueprintDeserialize.Handler();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ImportContent()
		{
			await _contentDeserialize.Handler();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ImportDataType()
		{
			await _dataTypeDeserialize.Handler();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ImportDictionary()
		{
			await _dictionaryDeserialize.Handler();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ImportDocType()
		{
			await _docTypeDeserialize.Handler();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ImportDomain()
		{
			await _domainDeserialize.Handler();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ImportLanguage()
		{
			await _languageDeserialize.Handler();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ImportMacro()
		{
			await _macroDeserialize.Handler();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ImportMedia()
		{
			await _mediaDeserialize.Handler();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ImportMediaType()
		{
			await _mediaTypeDeserialize.Handler();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ImportMemberType()
		{
			await _memberTypeDeserialize.Handler();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ImportRelation()
		{
			await _relationDeserialize.Handler();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ImportTemplate()
		{
			await _templateDeserialize.Handler();
			return new OkObjectResult(1);
		}
		
		[HttpGet]
		public async Task<IActionResult> ImportUsers()
		{
			await _usersDeserialize.Handler();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ImportUserGroups()
		{
			await _userGroupDeserialize.Handler();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ImportMembers()
		{
			await _memberDeserialize.Handler();
			return new OkObjectResult(1);
		}
		[HttpGet]
		public async Task<IActionResult> ImportMemberGroups()
		{
			await _memberGroupDeserialize.Handler();
			return new OkObjectResult(1);
		}
	}
}
