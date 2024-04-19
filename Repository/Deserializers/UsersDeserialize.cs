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

namespace SyncData.Repository.Deserializers
{
	public class UsersDeserialize : IUsersDeserialize
	{
		private readonly ILogger<UsersDeserialize> _logger;
		private IContentService _contentService;
		private readonly IMediaService _mediaService;
		private IUserService _userService;
		public UsersDeserialize(
			 ILogger<UsersDeserialize> logger,
			 IContentService contentService,
			 IMediaService mediaService,
			 IUserService userService
			 )
		{
			_logger = logger;
			_contentService = contentService;
			_mediaService = mediaService;
			_userService = userService;
		}

		public async Task<bool> Handler()
		{
			try
			{
				string folder = "cSync\\Users";
				if (!Directory.Exists(folder)) return false;
				string[] fyles = Directory.GetFiles(folder);

				foreach (string file in fyles)
				{
					XElement response = XElement.Load(file);
					XElement? root = new XElement(response.Name, response.Attributes());

					string? key = root.Attribute("Key").Value;
					string? alias = root.Attribute("Alias").Value;
					if (_userService.GetByUsername(alias) != null)
					{
						continue;
					}

					string? comments = response.Element("Info").Element("Comments").Value;
					string? name = response.Element("Info").Element("Name").Value;
					string? username = response.Element("Info").Element("Username").Value;
					string? email = response.Element("Info").Element("Email").Value;
					string? emailConfirmed = response.Element("Info").Element("EmailConfirmed").Value;
					string? failedAttempts = response.Element("Info").Element("FailedAttempts").Value;
					string? approved = response.Element("Info").Element("Approved").Value;
					string? lockedOut = response.Element("Info").Element("LockedOut").Value;
					string? language = response.Element("Info").Element("Language").Value;
					string? rawPassword = response.Element("Info").Element("RawPassword").Value;

					IEnumerable<XElement>? startContentNodes = response.Element("StartContentNodes").Elements();
					List<int> startContentIds = new List<int>();
					foreach (XElement node in startContentNodes)
					{
						IContent? nodeId = _contentService.GetById(new Guid(node.Value));
						startContentIds.Add(nodeId.Id);
					}

					IEnumerable<XElement>? startMediaNodes = response.Element("StartMediaNodes").Elements();
					List<int> startMediaNodesIds = new List<int>();
					foreach (XElement node in startMediaNodes)
					{
						IMedia nodeId = _mediaService.GetById(new Guid(node.Value));
						startMediaNodesIds.Add(nodeId.Id);
					}

					IUser? user = _userService.CreateUserWithIdentity(username, email);
					user.Key = new Guid(key);
					user.Comments = comments;
					user.Name = name;
					user.FailedPasswordAttempts = Convert.ToInt16(failedAttempts);
					user.IsApproved = Convert.ToBoolean(approved);
					user.IsLockedOut = Convert.ToBoolean(lockedOut);
					user.Language = language;
					user.RawPasswordValue = rawPassword;
					user.StartContentIds = startContentIds.ToArray();
					user.StartMediaIds = startMediaNodesIds.ToArray();

					IEnumerable<XElement>? groups = response.Element("Groups").Elements();
					foreach (XElement group in groups)
					{
						user.AddGroup(_userService.GetUserGroupByAlias(group.Element("Alias").Value) as IReadOnlyUserGroup);
					}
					_userService.Save(user);
				}
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError("UsersDeserialize {ex}", ex);
				return false;
			}
		}
	}
}
