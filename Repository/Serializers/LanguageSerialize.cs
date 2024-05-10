using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using SyncData.Interface.Serializers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;

namespace SyncData.Repository.Serializers
{
    public class LanguageSerialize : ILanguageSerialize
	{
		private readonly ILogger<LanguageSerialize> _logger;
		private ILocalizationService _localizationService;
		public LanguageSerialize(
			 ILogger<LanguageSerialize> logger,
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
				string? culture = CultureInfo.CurrentCulture.Name;
				IEnumerable<ILanguage>? languages = _localizationService.GetAllLanguages();

				foreach (ILanguage language in languages)
				{
					CompareInfo compareInfo = language.CultureInfo.CompareInfo;
					string? num = compareInfo.Version.SortId.ToString("N")
						.Substring(compareInfo.Version.SortId.ToString("N").Length - 8);
					int a = Convert.ToInt32(num, 16); ;

					Guid guid = new Guid(a, 0, 0, new byte[8]);
					XElement languageDetail = new XElement("Language",
						new XAttribute("Key", guid),
						new XAttribute("Alias", language.IsoCode),
						new XAttribute("Level", 0),
						new XElement("Name", language.CultureName),
						new XElement("IsoCode", language.IsoCode),
						new XElement("IsMandatory", language.IsMandatory),
						new XElement("IsDefault", language.IsDefault));

					string folder = "cSync\\Languages";
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
							if (guid == new Guid(keyVal))
							{
								System.IO.File.Delete(file); break;
							}
						}
					}
					string path = "cSync\\Languages\\" + language.IsoCode.Replace(" ", "-") + ".config";
					languageDetail.Save(path);
				}
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError("LanguageSerialize Serialize error {ex}", ex);
				return false;
			}
		}
	}
}
