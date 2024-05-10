using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SyncData.Interface.Serializers;
using SyncData.Model;
using System.Drawing;
using System.Xml.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Scoping;
using static Umbraco.Cms.Core.Collections.TopoGraph;

namespace SyncData.Repository.Serializers
{
	public class ContentSerialize : IContentSerialize
	{
		private readonly ILogger<ContentSerialize> _logger;

		private IContentService _contentService;
		private readonly IScopeProvider _scopeprovider;
		private IFileService _fileService;
		private IMemberGroupService _memberGroupService;
		private IDomainService _domainService;
		private readonly IPublicAccessService _publicAccessService;
		public IDomainSerialize _domainSerialize;
		public IPublicAccessSerialize _publicAccessSerialize;
		public ContentSerialize(
			ILogger<ContentSerialize> logger,
			IContentService contentService,
			IScopeProvider scopeProvider,
			IFileService fileService,
			IMemberGroupService memberGroupService,
			IDomainService domainService,
			IPublicAccessService publicAccessService,
			IDomainSerialize domainSerialize,
			IPublicAccessSerialize publicAccessSerialize
			)
		{
			_logger = logger;
			_contentService = contentService;
			_scopeprovider = scopeProvider;
			_fileService = fileService;
			_memberGroupService = memberGroupService;
			_domainService = domainService;
			_publicAccessService = publicAccessService;
			_domainSerialize = domainSerialize;
			_publicAccessSerialize = publicAccessSerialize;
		}
		public async Task<bool> HandlerAsync()
		{
			try
			{
				//Collect all
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
				//content
				foreach (IContent node in allPubUnPubContent)
				{
					var b = await SinlgeHandlerAsync(node);
					if (b)
					{
						
					}
				}
				_logger.LogInformation("Content Serialize success");
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError("Content Serialize {ex}", ex);
				return false;
			}

		}
		public async Task<bool> SinlgeHandlerAsync(IContent node)
		{
			try
			{
				XElement contentDetail = new XElement("Content",
						new XAttribute("Key", node.Key),
						new XAttribute("Level", node.Level),
						new XAttribute("Alias", node.Name));
				ITemplate template = null;
				if (!node.TemplateId.HasValue)
				{
					var name = node.Name;
					_logger.LogError("No template available for the node {name}", name);

				}
				else template = _fileService.GetTemplate(node.TemplateId.Value).IfNull(null);
				IContent parentCont = _contentService.GetById(node.ParentId);
				ContentScheduleCollection? schDetail = _contentService.GetContentScheduleByContentId(node.Id);
				var allSchedule = schDetail?.FullSchedule?.ToList();
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
				var schX = new XElement("Schedule");
				foreach (var schedule in allSchedule)
				{
					var scd = new XElement("ContentSchedule",
						new XElement("Culture", schedule?.Culture),
						new XElement("Action", schedule?.Action),
						new XElement("Date", schedule?.Date));
					schX.Add(scd);
				}

				XElement info =
						new XElement("Info",
						new XElement("Parent",
							new XAttribute("Key", parentCont != null ? parentCont.Key : Guid.Empty), parentCont != null ? parentCont.Name : ""),
						new XElement("Path", oPath),
				new XElement("Trashed", node.Trashed),
						new XElement("ContentType", node.ContentType.Alias),
				new XElement("CreateDate", node.CreateDate),
						new XElement("NodeName", new XAttribute("Default", node?.Name)),
				new XElement("SortOrder", node.SortOrder),
						new XElement("Published", new XAttribute("Default", node.Published)),
						schX,
						new XElement("Template", template != null ? new XAttribute("Key", template.Key) : "", template != null ? template.Name : null)
						);

				XElement properties = new XElement("Properties");
				foreach (var prop in node.Properties)
				{
					if (prop.PropertyType.PropertyEditorAlias == "Umbraco.MemberGroupPicker")
					{
						var sds = Convert.ToInt16(prop.GetValue());
						var memberGrp = _memberGroupService.GetById(Convert.ToInt16(prop.GetValue()));

						properties.Add(
							new XElement(prop.Alias,
							new XElement("Value",
							new XCData(memberGrp.Name)
							)));
					}
					else
					{
						properties.Add(
							new XElement(prop.Alias,
							new XElement("Value",
							new XCData(prop.GetValue() != null ? prop.GetValue().ToString() : "")
							)));
					}
				}

				contentDetail.Add(info);
				contentDetail.Add(properties);
				string folder = "cSync\\Content";
				if (!Directory.Exists(folder))
				{
					Directory.CreateDirectory(folder);
				}
				else
				{
					string[] fyles = Directory.GetFiles(folder);
					_logger.LogInformation("Collect all file");
					foreach (string file in fyles)
					{
						_logger.LogInformation("Check Files");
						XElement response = XElement.Load(file);
						XElement? root = new XElement(response.Name, response.Attributes());

						string? keyVal = root.Attribute("Key").Value;
						if (node.Key == new Guid(keyVal))
						{
							_logger.LogInformation("Select file");
							System.IO.File.Delete(file);
							_logger.LogInformation("Exist file deleted");
							break;
						}
					}
				}
				string path = "cSync\\Content\\" + node.Name?.Replace(" ", "-").ToLower() + ".config";
				contentDetail.Save(path);
				var pubAccServ = _publicAccessService.GetEntryForContent(node);
				if(pubAccServ != null)
				{
					var b = await _publicAccessSerialize.SingleHandlerAsync(pubAccServ, node);
				}

				var domain = _domainService.GetAssignedDomains(node.Id, true).FirstOrDefault();
				if (domain != null)
				{
					var b = await _domainSerialize.SingleHandlerAsync(domain);
				}

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError("Content Serialize {ex}", ex);
				return false;
			}
		}
	}
}
