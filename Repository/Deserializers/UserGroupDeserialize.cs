using Microsoft.Extensions.Logging;
using SyncData.Interface.Deserializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace SyncData.Repository.Deserializers
{
    public class UserGroupDeserialize : IUserGroupDeserialize
	{
		private readonly ILogger<UserGroupDeserialize> _logger;
		private ILocalizationService _localizationService;
		private IContentService _contentService;
		private readonly IMediaService _mediaService;
		private IUserService _userService;
		private IShortStringHelper _shortStringHelper;
		public UserGroupDeserialize(
			 ILogger<UserGroupDeserialize> logger,
			 ILocalizationService localizationService,
			IContentService contentService,
			IMediaService mediaService,
			IUserService userService,
			IShortStringHelper shortStringHelper
			 )
		{
			_logger = logger;
			_localizationService = localizationService;
			_contentService = contentService;
			_mediaService = mediaService;
			_userService = userService;
			_shortStringHelper = shortStringHelper;
		}

		public async Task<bool> HandlerAsync()
		{
			try
			{
				string folder = "cSync\\UserGroups";
				if (!Directory.Exists(folder)) return false;
				string[] fyles = Directory.GetFiles(folder);
				foreach (string file in fyles)
				{
					XElement response = XElement.Load(file);
					XElement? root = new XElement(response.Name, response.Attributes());

					string? key = root.Attribute("Key").Value;
					string? alias = root.Attribute("Alias").Value;
					string? sections = response.Element("Info").Element("Sections").Value;
					string? icon = response.Element("Info").Element("Icon").Value;
					string? name = response.Element("Info").Element("Name").Value;
					string? startContentId = response.Element("Info").Element("StartContentId").Value;
					string? startMediaId = response.Element("Info").Element("StartMediaId").Value;
					string? permission = response.Element("Info").Element("Permission").Value;

					var userGroup = _userService.GetUserGroupByAlias(alias) as UserGroup;
					if (userGroup is null)
					{
						userGroup = new UserGroup(_shortStringHelper) { HasAccessToAllLanguages = false }; ;
						userGroup.Id = 0;
					}

					userGroup.ClearAllowedSections();
					string[]? sectionsVal = sections.Split(",");
					foreach (string section in sectionsVal)
					{
						userGroup.AddAllowedSection(section);
					}

					userGroup.Name = name;
					userGroup.Alias = alias;
					userGroup.Icon = icon;
					if (new Guid(startContentId) != Guid.Empty)
					{
						IContent? node = _contentService.GetById(new Guid(startContentId));
						userGroup.StartContentId = node.Id;
					}

					if (new Guid(startMediaId) != Guid.Empty)
					{
						IMedia? node = _mediaService.GetById(new Guid(startMediaId));
					}

					IEnumerable<XElement>? languages = response.Element("Languages").Elements();
					string? allLnag = languages.FirstOrDefault().Value;
					if (allLnag == "false")
					{
						userGroup.ClearAllowedLanguages();
						userGroup.HasAccessToAllLanguages = false;
						foreach (XElement item in languages.Skip(1))
						{
							string? lang = item.Value;
							ILanguage? language = _localizationService.GetLanguageByIsoCode(lang);
							userGroup.AddAllowedLanguage(language.Id);
						}
					}

					_userService.Save(userGroup);
					char[]? permissionVal = permission.ToCharArray();
					foreach (char perm in permissionVal)
					{
						_userService.AssignUserGroupPermission(userGroup.Id, perm);
					}

					List<int> usrGrp = new List<int>() { userGroup.Id };
					IEnumerable<XElement>? assignedPermissions = response.Element("AssignedPermissions").Elements();
					foreach (XElement nodes in assignedPermissions)
					{
						string? nodeKey = nodes.Attribute("Key").Value;
						char[]? permissions = nodes.Value.ToCharArray();
						IContent? node = _contentService.GetById(new Guid(nodeKey));
						if (node == null) continue;

						EntityPermission? perm = _contentService.GetPermissions(node).FirstOrDefault();

						foreach (char item in permissions)
						{
							_contentService.SetPermission(node, item, usrGrp);
						}
					}
				}
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError("UserGroupDeserialize {ex}", ex);
				return false;
			}
		}
	}
}
