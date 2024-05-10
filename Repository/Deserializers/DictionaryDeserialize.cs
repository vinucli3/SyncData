using Microsoft.Extensions.Logging;
using SyncData.Interface.Deserializers;
using System.Xml.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace SyncData.Repository.Deserializers
{
    public class DictionaryDeserialize : IDictionaryDeserialize
	{
		private readonly ILogger<DictionaryDeserialize> _logger;
		private ILocalizationService _localizationService;
		public DictionaryDeserialize(
			 ILogger<DictionaryDeserialize> logger,
			 ILocalizationService localizationService
			 )
		{
			_logger = logger;
			_localizationService = localizationService;
		}

		public async Task<bool> HandlerAsync()
		{
			try
			{
				string folder = "cSync\\Dictionary";
				if (!Directory.Exists(folder)) return false;
				string[] files = Directory.GetFiles(folder);
				foreach (string file in files)
				{
					XElement readFile = XElement.Load(file);
					XElement? root = new XElement(readFile.Name, readFile.Attributes());

					string? keyVal = root?.Attribute("Key")?.Value ?? "";
					string? aliasVal = root?.Attribute("Alias")?.Value ?? "";
					string? levelVal = root?.Attribute("Level")?.Value ?? "";
					string? identity = readFile.Element("Info").Element("Identity")?.Value ?? "";

					IEnumerable<XElement>? translations = readFile.Element("Translations").Elements();

					List<IDictionaryTranslation> transltDict = new List<IDictionaryTranslation>();
					IEnumerable<ILanguage>? languages = _localizationService.GetAllLanguages();
					foreach (XElement translation in translations)
					{
						string? languageIsoCode = translation.Attribute("Language").Value ?? "";
						string? value = translation.Value ?? "";

						ILanguage? language = languages.Where(x => x.IsoCode == languageIsoCode).FirstOrDefault();

						DictionaryTranslation dictTrans = new DictionaryTranslation(language, value);
						transltDict.Add(dictTrans);
					}

					DictionaryItem? dictionaryItem = new DictionaryItem(aliasVal)
					{
						Key = new Guid(keyVal),
						ItemKey = aliasVal,
						Translations = transltDict
					};
					_localizationService.Save(dictionaryItem);
				}

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError("DictionaryDeserialize {ex}", ex);
				return false;
			}
		}
	}
}
