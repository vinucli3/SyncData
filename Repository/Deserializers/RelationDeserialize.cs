using Microsoft.Extensions.Logging;
using SyncData.Interface.Deserializers;
using System.Xml.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace SyncData.Repository.Deserializers
{
    public class RelationDeserialize : IRelationDeserialize
	{
		private readonly ILogger<RelationDeserialize> _logger;

		private IRelationService _relationService;
		public RelationDeserialize(
			 ILogger<RelationDeserialize> logger,
			 IRelationService relationService
			 )
		{
			_logger = logger;
			_relationService = relationService;
		}

		public async Task<bool> Handler()
		{
			try
			{
				string folder = "cSync\\RelationTypes";
				if (!Directory.Exists(folder)) return false;
				string[] files = Directory.GetFiles(folder);
				foreach (string file in files)
				{
					XElement readFile = XElement.Load(file); // XElement.Parse(stringWithXmlGoesHere)
					XElement? root = new XElement(readFile.Name, readFile.Attributes());

					string? keyVal = root?.Attribute("Key")?.Value ?? "";
					string? aliasVal = root?.Attribute("Alias")?.Value ?? "";
					string? levelVal = root?.Attribute("Level")?.Value ?? "";

					string? nameVal = readFile.Element("Info").Element("Name")?.Value ?? "";
					string? parentType = readFile.Element("Info").Element("ParentType")?.Value ?? "";
					string? childType = readFile.Element("Info").Element("ChildType")?.Value ?? "";
					string? isBidirectional = readFile.Element("Info").Element("Bidirectional")?.Value ?? "";
					string? isDependency = readFile.Element("Info").Element("IsDependency")?.Value ?? "";

					RelationType? relationType = new RelationType(aliasVal, nameVal,
						Convert.ToBoolean(isBidirectional),
						new Guid(parentType),
						new Guid(childType),
						Convert.ToBoolean(isDependency))
					{
						Key = new Guid(keyVal),
					};

					_relationService.Save(relationType);
				}
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError("RelationDeserialize {ex}", ex);
				return false;
			}
		}
	}
}
