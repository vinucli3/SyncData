using Microsoft.Extensions.Logging;
using SyncData.Interface.Deserializers;
using System.Xml.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace SyncData.Repository.Deserializers
{
    public class LanguageDeserialize : ILanguageDeserialize
	{
		private readonly ILogger<LanguageDeserialize> _logger;

		private ILocalizationService _localizationService;
		public LanguageDeserialize(
			 ILogger<LanguageDeserialize> logger,
			 ILocalizationService localizationService
			 )
		{
			_logger = logger;
			_localizationService = localizationService;
		}

		public async Task<bool> Handler()
		{
			try
			{
				string folder = "cSync\\Languages";
				if (!Directory.Exists(folder)) return false;
				string[] files = Directory.GetFiles(folder);

				foreach (string file in files)
				{
					XElement readFile = XElement.Load(file); // XElement.Parse(stringWithXmlGoesHere)
					XElement? root = new XElement(readFile.Name, readFile.Attributes());

					string? keyVal = root.Attribute("Key").Value ?? "";
					string? aliasVal = root.Attribute("Alias").Value ?? "";
					string? levelVal = root.Attribute("Level").Value ?? "";

					string? nameVal = readFile.Element("Name").Value ?? "";
					string? isoCodeAlias = readFile.Element("IsoCode").Value ?? "";
					string? isMandType = readFile.Element("IsMandatory").Value ?? "";
					string? isDefault = readFile.Element("IsDefault").Value ?? "";

					ILanguage? language = _localizationService.GetLanguageByIsoCode(isoCodeAlias);
					if (language is null)
					{
						Language? newLang = new Language(isoCodeAlias, nameVal)
						{
							Key = new Guid(keyVal),
							CultureName = nameVal,
							IsoCode = isoCodeAlias,
							IsDefault = Convert.ToBoolean(isDefault),
							IsMandatory = Convert.ToBoolean(isMandType),
							FallbackLanguageId = 1
						};
						_localizationService.Save(newLang);
					}
				}
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError("LanguageDeserialize {ex}", ex);
				return false;
			}
		}
	}
}
