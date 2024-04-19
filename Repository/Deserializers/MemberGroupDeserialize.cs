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
    public class MemberGroupDeserialize : IMemberGroupDeserialize
	{
		private readonly ILogger<MemberGroupDeserialize> _logger;
		private IMemberGroupService _memberGroupService;
		public MemberGroupDeserialize(
			 ILogger<MemberGroupDeserialize> logger,
			 IMemberGroupService memberGroupService
			 )
		{
			_logger = logger;
			_memberGroupService = memberGroupService;
		}

		public async Task<bool> Handler()
		{
			try
			{
				string folder = "cSync\\MemberGroups";
				if (!Directory.Exists(folder)) return false;
				string[] fyles = Directory.GetFiles(folder);
				foreach (string file in fyles)
				{
					XElement response = XElement.Load(file);
					XElement? root = new XElement(response.Name, response.Attributes());

					string? keyVal = root.Attribute("Key").Value;
					string? aliasVal = root.Attribute("Alias").Value;

					MemberGroup memberGroup = new MemberGroup() { Name = aliasVal, Key = new Guid(keyVal) };
					_memberGroupService.Save(memberGroup);
				}
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError("MemberGroupDeserialize {ex}", ex);
				return false;
			}
		}
	}
}
