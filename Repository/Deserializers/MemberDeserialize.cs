using Microsoft.Extensions.Logging;
using SyncData.Interface.Deserializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace SyncData.Repository.Deserializers
{
    public class MemberDeserialize : IMemberDeserialize
	{
		private readonly ILogger<MemberDeserialize> _logger;
		private IMemberTypeService _memberTypeServices;
		private IMemberService _memberService;
		public MemberDeserialize(
			 ILogger<MemberDeserialize> logger,
			IMemberTypeService memberTypeService,
			IMemberService memberService
			 )
		{
			_logger = logger;
			_memberTypeServices = memberTypeService;
			_memberService = memberService;
		}

		public async Task<bool> Handler()
		{
			try
			{
				string folder = "cSync\\Members";
				if (!Directory.Exists(folder)) return false;
				string[] fyles = Directory.GetFiles(folder);

				foreach (string file in fyles)
				{
					XElement response = XElement.Load(file);
					XElement? root = new XElement(response.Name, response.Attributes());

					string? key = root.Attribute("Key").Value;
					string? alias = root.Attribute("Alias").Value;
					string? level = root.Attribute("Level").Value;

					string? parent = response.Element("Info").Element("Parent").Attribute("Key").Value;
					string? path = response.Element("Info").Element("Path").Value;
					string? trashed = response.Element("Info").Element("Trashed").Value;
					string? contentType = response.Element("Info").Element("ContentType").Value;
					string? nodeName = response.Element("Info").Element("NodeName").Attribute("Default").Value;
					string? sortOrder = response.Element("Info").Element("SortOrder").Value;
					string? email = response.Element("Info").Element("Email").Value;
					string? username = response.Element("Info").Element("Username").Value;
					string? memberType = response.Element("Info").Element("MemberType").Value;
					string? rawPassword = response.Element("Info").Element("RawPassword").Value;
					IMemberType? existMemberType = _memberTypeServices.Get(memberType);
					Member newMember = new Member(existMemberType)
					{
						Key = new Guid(key),
						Level = Convert.ToInt16(level),
						Trashed = Convert.ToBoolean(trashed),
						Name = nodeName,
						SortOrder = Convert.ToInt16(sortOrder),
						Email = email,
						Username = username,
						RawPasswordValue = rawPassword,
					};

					IEnumerable<XElement>? properties = response.Element("Properties").Elements();
					foreach (XElement prop in properties)
					{
						string? editorAlias = prop.Name.LocalName;
						string? value = prop.Value;
						newMember.Properties.Where(x => x.Alias == editorAlias).FirstOrDefault().SetValue(value);
					}

					_memberService.Save(newMember);
					IEnumerable<XElement>? groups = response.Element("Groups").Elements();
					foreach (XElement group in groups)
					{
						string? groupName = group.Value;
						_memberService.AssignRole(newMember.Id, groupName);
					}
				}
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError("MemberDeserialize {ex}", ex);
				return false;
			}
		}
	}

}
