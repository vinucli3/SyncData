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
    public class MemberGroupSerialize : IMemberGroupSerialize
	{
		private readonly ILogger<MemberGroupSerialize> _logger;
		private IMemberGroupService _memberGroupService;

		public MemberGroupSerialize(
			ILogger<MemberGroupSerialize> logger,
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
				IEnumerable<IMemberGroup>? memberGroups = _memberGroupService.GetAll();
				foreach (IMemberGroup memberGroup in memberGroups)
				{
					XElement contentDetail = new XElement("MemberGroup",
						new XAttribute("Key", memberGroup.Key),
						new XAttribute("Alias", memberGroup.Name));

					string folder = "cSync\\MemberGroups";
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
							if (memberGroup.Key == new Guid(keyVal))
							{
								System.IO.File.Delete(file); break;
							}
						}
					}
					string path = "cSync\\MemberGroups\\" + memberGroup.Name.ToLower() + ".config";
					contentDetail.Save(path);
				}
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError("MemberGroupSerialize Serialize error {ex}", ex);
				return false;
			}
		}
	}
}
