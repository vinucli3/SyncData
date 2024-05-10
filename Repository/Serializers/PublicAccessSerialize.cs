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
using static Umbraco.Cms.Core.Collections.TopoGraph;

namespace SyncData.Repository.Serializers
{
    public class PublicAccessSerialize : IPublicAccessSerialize
	{
		private readonly ILogger<MemberTypeSerialize> _logger;
		private readonly IPublicAccessService _publicAccessService;
		private IContentService _contentService;
		public PublicAccessSerialize(ILogger<MemberTypeSerialize> logger, IPublicAccessService publicAccessService, IContentService contentService)
		{
			_logger = logger;
			_publicAccessService = publicAccessService;
			_contentService = contentService;
		}
		public async Task<bool> HandlerAsync()
		{
			try
			{

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError("MemberTypeSerialize Serialize error {ex}", ex);
				return false;
			}
		}
		public async Task<bool> SingleHandlerAsync(PublicAccessEntry publicAccessEntry, IContent node)
		{
			try
			{
				XElement protectDetail = new XElement("protect",
						new XAttribute("Key", node.Key),
						new XAttribute("Alias", node.Name));
				var loginNod = _contentService.GetById(publicAccessEntry.LoginNodeId);
				var noAccessNod = _contentService.GetById(publicAccessEntry.NoAccessNodeId);
				XElement infoElement =
						new XElement("Info",
						new XElement("Path", FindPath(node)),
						new XElement("LoginNode", new XAttribute("Key", loginNod != null ? loginNod.Key : Guid.Empty), loginNod != null ? FindPath(loginNod) : ""),
						new XElement("NoAccessNode", new XAttribute("Key", noAccessNod != null ? noAccessNod.Key : Guid.Empty), noAccessNod != null ? FindPath(noAccessNod) : ""));
				protectDetail.Add(infoElement);

				var rules = publicAccessEntry.Rules;
				XElement rulesElement =
						new XElement("Rules", "");
				foreach (var rule in rules)
				{
					XElement ruleElement =
						new XElement("Rule",
						new XElement("RuleType", rule.RuleType),
						new XElement("RuleValue", rule.RuleValue)
						);
					rulesElement.Add(ruleElement);
				}
				protectDetail.Add(rulesElement);
				string folder = "cSync\\PublicAccess";
				if (!Directory.Exists(folder))
				{
					Directory.CreateDirectory(folder);
				}
				string path = "cSync\\PublicAccess\\" + node.Name?.Replace(" ", "-").ToLower() + ".config";
				protectDetail.Save(path);
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError("MemberTypeSerialize Serialize error {ex}", ex);
				return false;
			}
		}

		public string FindPath(IContent node)
		{
			string oPath = "";
			var pathSplit = node.Path.Split(",");
			foreach (var d in pathSplit)
			{
				if (d == "-1")
				{
					oPath += "/";
				}
				else if (d == "-20")
				{
					oPath += "[-20]/";
				}
				else
				{
					var Pnode = _contentService.GetById(Convert.ToInt16(d));
					oPath += Pnode.Name.Replace(" ", "") + (pathSplit.IndexOf(d) != pathSplit.Length - 1 ? "/" : "");
				}
			}
			return oPath;
		}
	}
}
