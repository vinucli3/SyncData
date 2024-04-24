using Microsoft.Extensions.Logging;
using SyncData.Interface.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Scoping;
using static NPoco.SqlBuilder;

namespace SyncData.Repository.Serializers
{
    public class UserGroupSerialize : IUserGroupSerialize
	{
		private readonly ILogger<UserGroupSerialize> _logger;
		private readonly ILocalizationService _localizationService;
		private IUserService _userService;
		private IContentService _contentService;
		private readonly IScopeProvider _scopeprovider;
		private readonly IMediaService _mediaService;
		public UserGroupSerialize(
			ILogger<UserGroupSerialize> logger,
			ILocalizationService localizationService,
			IUserService userService,
			IContentService contentService,
			IScopeProvider scopeProvider,
			IMediaService mediaService
		   )
		{
			_logger = logger;
			_localizationService = localizationService;
			_userService = userService;
			_contentService = contentService;
			_scopeprovider = scopeProvider;
			_mediaService = mediaService;
		}
		public async Task<bool> Handler()
		{
			try
			{
				IEnumerable<IUserGroup>? userGroups = _userService.GetAllUserGroups();

				/*****get all content*///////////

				var allPubUnPubContent = new List<IContent>();
				var rootNodes = _contentService.GetRootContent();

				var recycledContent = _contentService.GetPagedContentInRecycleBin(0, 100, out long total).ToList();

				var query = new Query<IContent>(_scopeprovider.SqlContext).Where(x => x.Published && x.Trashed);

				foreach (var c in rootNodes)
				{
					allPubUnPubContent.Add(c);
					var descendants = _contentService.GetPagedDescendants(c.Id, 0, int.MaxValue, out long totalNodes, null);
					allPubUnPubContent.AddRange(descendants);
				}
				allPubUnPubContent.AddRange(recycledContent);


				foreach (IUserGroup userGroup in userGroups)
				{

					XElement contentDetail = new XElement("UserGroup",
						new XAttribute("Key", userGroup.Key),
						new XAttribute("Alias", userGroup.Alias));

					string? sections = "";
					string? permission = "";
					if (userGroup.AllowedSections.Count() > 0)
					{
						foreach (string item in userGroup.AllowedSections)
						{
							sections += item + ",";
						}
						sections = sections.Remove(sections.Length - 1);
					}
					if (userGroup.Permissions.Count() > 0)
					{
						foreach (string item in userGroup.Permissions)
						{
							permission += item + ",";
						}
						permission = permission.Remove(permission.Length - 1);
					}

					XElement info =
							new XElement("Info",
							new XElement("Sections", sections),
							new XElement("Icon", userGroup.Icon),
					new XElement("Name", userGroup.Name),
							new XElement("StartContentId", userGroup.StartContentId != -1 && userGroup.StartContentId != null ? _contentService.GetById((int)userGroup.StartContentId).Key : new Guid()),
							new XElement("StartMediaId", userGroup.StartMediaId != -1 && userGroup.StartMediaId != null ? _mediaService.GetById((int)userGroup.StartMediaId).Key : new Guid()),
							new XElement("Permission", new XCData(permission))
							);
					contentDetail.Add(info);
					sections = ""; permission = "";
					XElement assignedPermissions = new XElement("AssignedPermissions");
					foreach (IContent item in allPubUnPubContent)
					{
						EntityPermission? perm = _contentService.GetPermissions(item).FirstOrDefault();
						if (perm != null)
						{
							if (perm.UserGroupId == userGroup.Id)
							{
								foreach (string per in perm.AssignedPermissions)
								{
									sections += per + ",";
								}
								sections = sections.Remove(sections.Length - 1);
								XElement? contPerm = new XElement("Permission", new XAttribute("Key", item.Key), new XCData(sections));
								assignedPermissions.Add(contPerm);
							}
						}
					}

					contentDetail.Add(assignedPermissions);
					XElement languages = new XElement("Languages",
						new XElement("HasAccessToAllLanguages", userGroup.HasAccessToAllLanguages));
					foreach (int item in userGroup.AllowedLanguages)
					{
						XElement? lang = new XElement("AllowedLanguages", _localizationService.GetAllLanguages().Where(x => x.Id == item).Select(x => x.IsoCode));
						languages.Add(lang);
					}

					contentDetail.Add(languages);
					string folder = "cSync\\UserGroups";
					if (!Directory.Exists(folder))
					{
						Directory.CreateDirectory(folder);
					}
					else
					{
						string[] fyles = Directory.GetFiles(folder);

						foreach (string file in fyles)
						{
							XElement response = XElement.Load(file);
							XElement? root = new XElement(response.Name, response.Attributes());
							string? keyVal = root.Attribute("Key").Value;
							if (userGroup.Key == new Guid(keyVal))
							{
								System.IO.File.Delete(file); break;
							}
						}
					}
					string path = "cSync\\UserGroups\\" + userGroup.Alias.ToLower() + ".config";
					contentDetail.Save(path);
				}
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError("UserGroupSerialize Serialize error {ex}", ex);
				return false;
			}
		}
	}
}
