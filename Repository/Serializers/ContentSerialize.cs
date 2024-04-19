using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SyncData.Interface.Serializers;
using SyncData.Model;
using System.Xml.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Scoping;

namespace SyncData.Repository.Serializers
{
	public class ContentSerialize : IContentSerialize
	{
		private readonly ILogger<ContentSerialize> _logger;

		private IContentService _contentService;
		private readonly IScopeProvider _scopeprovider;
		private IFileService _fileService;
		private IMemberGroupService _memberGroupService;
		public ContentSerialize(
			ILogger<ContentSerialize> logger,
			IContentService contentService,
			IScopeProvider scopeProvider,
			IFileService fileService,
			IMemberGroupService memberGroupService
			)
		{
			_logger = logger;
			_contentService = contentService;
			_scopeprovider = scopeProvider;
			_fileService = fileService;
			_memberGroupService = memberGroupService;
		}
		public async Task<bool> Handler()
		{
			try
			{
				List<IContent>? allPubUnPubContent = new List<IContent>();
				IEnumerable<IContent>? rootNodes = _contentService.GetRootContent();

				IQuery<IContent>? query = new Query<IContent>(_scopeprovider.SqlContext).Where(x => x.Published || x.Trashed);

				foreach (IContent c in rootNodes)
				{
					allPubUnPubContent.Add(c);
					IEnumerable<IContent>? descendants = _contentService.GetPagedDescendants(c.Id, 0, int.MaxValue, out long totalNodes, query);
					allPubUnPubContent.AddRange(descendants);
				}

				//content
				foreach (IContent node in allPubUnPubContent)
				{
					XElement contentDetail = new XElement("Content",
						new XAttribute("Key", node.Key),
						new XAttribute("Level", node.Level),
						new XAttribute("Alias", node.Name));
					if (!node.TemplateId.HasValue)
					{
						var name = node.Name;
						_logger.LogError("No template available for the node {name}", name);
						continue;
					}
					ITemplate template = _fileService.GetTemplate(node.TemplateId.Value);
					IContent parentCont = _contentService.GetById(node.ParentId);
					ContentScheduleCollection? schDetail = _contentService.GetContentScheduleByContentId(node.Id);
					ContentSchedule? fullSch = schDetail?.FullSchedule?.FirstOrDefault();
					string? pathVal = (parentCont != null ? "/" + parentCont.Name.Replace(" ", "") : "") + "/" + node.Name.Replace(" ", "");

					XElement info =
							new XElement("Info",
							new XElement("Parent",
								new XAttribute("Key", parentCont != null ? parentCont.Key : Guid.Empty), parentCont != null ? parentCont.Name : ""),
							new XElement("Path", pathVal),
							new XElement("Trashed", node.Trashed),
							new XElement("ContentType", node.ContentType.Alias),
								new XElement("CreateDate", node.CreateDate),
							new XElement("NodeName", new XAttribute("Default", node?.Name)),
								new XElement("SortOrder", node.SortOrder),
							new XElement("Published", new XAttribute("Default", node.Published)),
							new XElement("Schedule", fullSch != null ? new XElement("ContentSchedule",
							new XElement("Culture", fullSch?.Culture),
							new XElement("Action", fullSch?.Action),
							new XElement("Date", fullSch?.Date)) : null),
							new XElement("Template", new XAttribute("Key", template.Key), template.Name)
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

						foreach (string file in fyles)
						{
							XElement response = XElement.Load(file);
							XElement? root = new XElement(response.Name, response.Attributes());

							string? keyVal = root.Attribute("Key").Value;
							if (node.Key == new Guid(keyVal))
							{
								System.IO.File.Delete(file); break;
							}
						}
					}
					string path = "cSync\\Content\\" + node.Name?.Replace(" ", "-").ToLower() + ".config";
					contentDetail.Save(path);
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

	}
}
