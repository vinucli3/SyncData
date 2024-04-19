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
    public class DictionarySerialize : IDictionarySerialize
	{
		private readonly ILogger<DictionarySerialize> _logger;
		private readonly ILocalizationService _localizationService;

		public DictionarySerialize(
			ILogger<DictionarySerialize> logger,
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
				IEnumerable<IDictionaryItem>? dictionaryItems = _localizationService.GetRootDictionaryItems();

				foreach (IDictionaryItem dictionaryItem in dictionaryItems)
				{
					XElement dictionaryDetail = new XElement("Dictionary",
						new XAttribute("Key", dictionaryItem.Key),
						new XAttribute("Alias", dictionaryItem.ItemKey),
						new XAttribute("Level", "0"));
					XElement info =
						new XElement("Info",
						new XElement("Identity", dictionaryItem.HasIdentity), "");

					dictionaryDetail.Add(info);

					XElement translation =
						new XElement("Translations");

					IEnumerable<IDictionaryTranslation>? transl = dictionaryItem.Translations;
					foreach (IDictionaryTranslation translationItem in transl)
					{
						XElement? translationKey = new XElement("Translation",
							new XAttribute("Language", translationItem.LanguageIsoCode), translationItem.Value);
						translation.Add(translationKey);
					}
					dictionaryDetail.Add(translation);



					string folder = "cSync\\Dictionary";
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
							if (dictionaryItem.Key == new Guid(keyVal))
							{
								System.IO.File.Delete(file); break;
							}
						}
					}
					string path = "cSync\\Dictionary\\" + dictionaryItem.ItemKey?.Replace(" ", " -").ToLower() + ".config";
					dictionaryDetail.Save(path);
				}
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError("Dictionary Serialize error {ex}", ex);
				return false;
			}
		}
	}
}
