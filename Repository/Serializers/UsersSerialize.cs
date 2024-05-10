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
using Umbraco.Cms.Core.Services;

namespace SyncData.Repository.Serializers
{
    public class UsersSerialize : IUsersSerialize
	{
		private readonly ILogger<UsersSerialize> _logger;
		private IUserService _userService;
		private IContentService _contentService;
		private readonly IMediaService _mediaService;
		public UsersSerialize(
			ILogger<UsersSerialize> logger,
			IUserService userService,
			IContentService contentService,
			IMediaService mediaService
		   )
		{
			_logger = logger;
			_userService = userService;
			_contentService = contentService;
			_mediaService = mediaService;
		}
		public async Task<bool> HandlerAsync()
		{
			try
			{
				long count = 0;
				IEnumerable<IUser>? users = _userService.GetAll(0, 100, out count);
				foreach (IUser user in users)
				{
					XElement contentDetail = new XElement("User",
						new XAttribute("Key", user.Key),
						new XAttribute("Alias", user.Email));

					XElement info =
							new XElement("Info",
							new XElement("Comments", user.Comments),
							new XElement("Name", user.Name),
							new XElement("Username", user.Username),
							new XElement("Email", user.Email),
							new XElement("EmailConfirmed", ""),
							new XElement("FailedAttempts", user.FailedPasswordAttempts),
							new XElement("Approved", user.IsApproved),
							new XElement("LockedOut", user.IsLockedOut),
							new XElement("Language", user.Language),
							new XElement("RawPassword", user.RawPasswordValue)
							);
					contentDetail.Add(info);
					XElement groups = new XElement("Groups");
					foreach (IReadOnlyUserGroup group in user.Groups)
					{
						XElement groupX = new XElement("Group",
							new XElement("Alias", group.Alias));
						groups.Add(groupX);
					}
					contentDetail.Add(groups);

					XElement startContentNodes = new XElement("StartContentNodes");
					foreach (int item in user.StartContentIds)
					{
						IContent? content = _contentService.GetById(item);
						XElement? node = new XElement("Node", content?.Key);
						startContentNodes.Add(node);
					}
					XElement startMediaNodes = new XElement("StartMediaNodes");
					foreach (int item in user.StartMediaIds)
					{
						IMedia? media = _mediaService.GetById(item);
						XElement? mediaNode = new XElement("Node", media.Key);
						startMediaNodes.Add(mediaNode);
					}
					contentDetail.Add(startContentNodes);
					contentDetail.Add(startMediaNodes);

					string folder = "cSync\\Users";
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
							if (user.Key == new Guid(keyVal))
							{
								System.IO.File.Delete(file); break;
							}
						}
					}
					string path = "cSync\\Users\\" + user.Email.Replace("@", "-").ToLower() + ".config";
					contentDetail.Save(path);
				}
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError("UsersSerialize Serialize error {ex}", ex);
				return false;
			}
		}
	}
}
