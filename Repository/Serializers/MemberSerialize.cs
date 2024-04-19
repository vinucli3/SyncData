using Microsoft.Extensions.Logging;
using SyncData.Interface.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace SyncData.Repository.Serializers
{
    public class MemberSerialize : IMemberSerialize
	{
		private readonly ILogger<MemberSerialize> _logger;
		private IMemberService _memberService;
		private IMemberGroupService _memberGroupService;
		public MemberSerialize(
			ILogger<MemberSerialize> logger,
			IMemberService memberService,
			IMemberGroupService memberGroupService
		   )
		{
			_logger = logger;
			_memberService = memberService;
			_memberGroupService = memberGroupService;
		}


		public async Task<bool> Handler()
		{
			try
			{
				long count = 0;
				IEnumerable<IMember>? members = _memberService.GetAll(0, 10, out count);
				foreach (IMember member in members)
				{
					XElement contentDetail = new XElement(member.ContentType.Alias,
							new XAttribute("Key", member.Key),
							new XAttribute("Level", member.Level),
							new XAttribute("Alias", member.Email));

					string? pathVal = "/" + member?.Name;

					XElement info =
							new XElement("Info",
							new XElement("Parent",
							new XAttribute("Key", new Guid())),
							new XElement("Path", pathVal),
							new XElement("Trashed", member?.Trashed),
							new XElement("ContentType", member?.ContentType.Alias),
							new XElement("CreateDate", member?.CreateDate),
							new XElement("NodeName", new XAttribute("Default", member?.Name)),
							new XElement("SortOrder", member.SortOrder),
							new XElement("Email", member.Email),
							new XElement("Username", member.Username),
							new XElement("MemberType", member.ContentType.Alias),
							new XElement("RawPassword", member.RawPasswordValue)
							);
					XElement properties = new XElement("Properties");
					foreach (IProperty prop in member.Properties)
					{
						properties.Add(
							new XElement(prop.Alias,
							new XElement("Value",
							new XCData(prop.GetValue() != null ? prop.GetValue().ToString() : "")
							)));

					}
					XElement groups = new XElement("Groups");
					IEnumerable<int>? memberGroupsIds = _memberService.GetAllRolesIds(member.Id);
					foreach (int memberGroupsId in memberGroupsIds)
					{
						IMemberGroup? memberGroup = _memberGroupService.GetById(memberGroupsId);
						groups.Add(
							new XElement("Group", memberGroup.Name));
					}

					contentDetail.Add(info);
					contentDetail.Add(properties);
					contentDetail.Add(groups);
					string folder = "cSync\\Members";
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
							if (member.Key == new Guid(keyVal))
							{
								System.IO.File.Delete(file); break;
							}
						}
					}
					string path = "cSync\\Members\\" + member.Key.ToString().ToLower() + ".config";
					contentDetail.Save(path);
				}
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError("MemberSerialize Serialize error {ex}", ex);
				return false;
			}
		}
	}
}
