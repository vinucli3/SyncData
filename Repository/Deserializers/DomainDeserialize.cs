using Microsoft.Extensions.Logging;
using SyncData.Interface.Deserializers;
using System.Xml.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace SyncData.Repository.Deserializers
{
    public class DomainDeserialize : IDomainDeserialize
	{
		private readonly ILogger<DomainDeserialize> _logger;

		private IDomainService _domainService;
		private IContentService _contentService;
		private ILocalizationService _localizationService;
		public DomainDeserialize(
			 ILogger<DomainDeserialize> logger,
			 IDomainService domainService,
			 IContentService contentService,
			 ILocalizationService localizationService
			 )
		{
			_logger = logger;
			_domainService = domainService;
			_contentService = contentService;
			_localizationService = localizationService;
		}

		public async Task<bool> HandlerAsync()
		{
			try
			{
				string folder = "cSync\\Domain";
				if (!Directory.Exists(folder)) return false;
				string[] files = Directory.GetFiles(folder);
				List<IDomain>? domains = _domainService.GetAll(true).ToList();
				foreach (var item in domains)
				{
					_domainService.Delete(item);
				}
				foreach (string file in files)
				{
					XElement readFile = XElement.Load(file); // XElement.Parse(stringWithXmlGoesHere)
					XElement? response = new XElement(readFile.Name, readFile.Attributes());

					string? keyVal = response?.Attribute("Key")?.Value ?? "";
					string? aliasVal = response?.Attribute("Alias")?.Value ?? "";

					IDomain? domainExist = _domainService.GetAll(true).Where(x => x.DomainName == aliasVal).FirstOrDefault();
					if (domainExist != null)
					{
						continue;
					}

					string? isWildcard = readFile.Element("Info").Element("IsWildcard")?.Value ?? "";
					string? language = readFile.Element("Info").Element("Language")?.Value ?? "";
					string? root = readFile.Element("Info").Element("Root")?.Value ?? "";
					string? rootKey = readFile.Element("Info").Element("Root")?.Attribute("Key").Value ?? "";
					string? sortOrder = readFile.Element("Info").Element("SortOrder")?.Value ?? "";

					ILanguage? langVal = _localizationService.GetLanguageByIsoCode(language);
					IContent? rootContent = _contentService.GetById(new Guid(rootKey));
					UmbracoDomain? domain = new UmbracoDomain("mydomainname ")
					{
						Key = new Guid(keyVal),
						DomainName = aliasVal,
						LanguageId = langVal?.Id,
						RootContentId = rootContent?.Id,
						SortOrder = Convert.ToInt16(sortOrder)
					};

					_domainService.Save(domain);
				}
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError("DomainDeserialize {ex}", ex);
				return false;
			}
		}
	}
}
