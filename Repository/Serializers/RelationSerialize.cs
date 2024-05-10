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
    public class RelationSerialize : IRelationSerialize
	{
		private readonly ILogger<RelationSerialize> _logger;
		private IRelationService _relationService;
		public RelationSerialize(
			 ILogger<RelationSerialize> logger,
			 IRelationService relationService
			)
		{
			_logger = logger;
			_relationService = relationService;
		}

		public async Task<bool> HandlerAsync()
		{
			try
			{
				IEnumerable<IRelationType>? relations = _relationService.GetAllRelationTypes();
				foreach (IRelationType relation in relations)
				{
					if (!relation.IsSystemRelationType())
					{
						IRelationTypeWithIsDependency? dependency = relation as IRelationTypeWithIsDependency;
						XElement relationDetail = new XElement("RelationType",
							new XAttribute("Key", relation.Key),
							new XAttribute("Level", "0"),
							new XAttribute("Alias", relation.Alias));

						XElement info = new XElement("Info",
							new XElement("Name", relation.Name),
							new XElement("ParentType", relation.ParentObjectType),
							new XElement("ChildType", relation.ChildObjectType),
							new XElement("Bidirectional", relation.IsBidirectional),
							new XElement("IsDependency", dependency?.IsDependency));

						relationDetail.Add(info);
						string folder = "cSync\\RelationTypes";
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
								if (relation.Key == new Guid(keyVal))
								{
									System.IO.File.Delete(file); break;
								}
							}
						}
						string path = "cSync\\RelationTypes\\" + relation.Name?.Replace(" ", "-").ToLower() + ".config";
						relationDetail.Save(path);
					}
				}
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError("RelationSerialize Serialize error {ex}", ex);
				return false;
			}
		}
	}
}
