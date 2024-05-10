using Microsoft.Extensions.Logging;
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
	public class BlueprintSerialize : IBlueprintSerialize
	{
		public void Dispose()
		{

		}

		private readonly ILogger<BlueprintSerialize> _logger;

		private IContentService _contentService;
		private readonly IScopeProvider _scopeprovider;
		private IFileService _fileService;
		public BlueprintSerialize(ILoggerFactory loggerFactory,
			ILogger<BlueprintSerialize> logger,
			IContentService contentService,
			IScopeProvider scopeProvider,
			IFileService fileService
			)
		{
			_logger = logger;
			_contentService = contentService;
			_scopeprovider = scopeProvider;
			_fileService = fileService;
		}
		public async Task<bool> HandlerAsync()
		{
			try
			{
				List<AcknowDTO> listAcknowDTO = new List<AcknowDTO>();
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

				var bluePrints = _contentService.GetBlueprintsForContentTypes(
					allPubUnPubContent.Select(x => x.ContentTypeId).ToArray()).ToList();
				if (!bluePrints.IsCollectionEmpty())
				{

					foreach (IContent blueprint in bluePrints)
					{
						AcknowDTO acknowDTO=new AcknowDTO();
						
						XElement blueprintDetail = new XElement("Content",
												new XAttribute("Key", blueprint.Key),
												new XAttribute("Level", blueprint.Level),
												new XAttribute("Alias", blueprint.Name));

						IContent? parentCont = _contentService.GetById(blueprint.ParentId);
						ContentScheduleCollection? schDetail = _contentService.GetContentScheduleByContentId(blueprint.Id);
						ContentSchedule? fullSch = schDetail?.FullSchedule?.FirstOrDefault();

						ITemplate template = _fileService.GetTemplate(blueprint.TemplateId.Value);

						XElement info =
							new XElement("Info",
							new XElement("Parent",
								new XAttribute("Key", parentCont != null ? parentCont.Key : Guid.Empty), parentCont != null ? parentCont.Name : ""),
							new XElement("Path", "/" + blueprint.Name?.Replace(" ", "")),
							new XElement("Trashed", blueprint.Trashed),
							new XElement("ContentType", blueprint.ContentType.Alias),
							new XElement("CreateDate", blueprint.CreateDate),
							new XElement("NodeName", new XAttribute("Default", blueprint?.Name)),
							new XElement("SortOrder", blueprint.SortOrder),
							new XElement("Published", new XAttribute("Default", blueprint.Published)),
							new XElement("Schedule", fullSch != null ? new XElement("ContentSchedule",
							new XElement("Culture", fullSch?.Culture),
							new XElement("Action", fullSch?.Action),
							new XElement("Date", fullSch?.Date)) : null),
							new XElement("Template", new XAttribute("Key", template.Key), template.Name),
							new XElement("IsBlueprint", "true")
							);

						blueprintDetail.Add(info);
						XElement properties = new XElement("Properties");
						foreach (IProperty prop in blueprint.Properties)
						{
							properties.Add(
								new XElement(prop.Alias,
								new XElement("Value",
								new XCData(prop.GetValue() != null ? prop.GetValue().ToString() : "")
								)));
						}

						blueprintDetail.Add(properties);
						string folder = "cSync\\Blueprints";

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
								if (blueprint.Key == new Guid(keyVal))
								{
									System.IO.File.Delete(file); break;
								}
							}
						}

						string path = "cSync\\Blueprints\\" + blueprint.Name?.Replace(" ", "-").ToLower() + ".config";
						blueprintDetail.Save(path);
					}
				}
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError("BlueprintSerialize error {ex}", ex);
				return false;
			}
		}
	}



}
