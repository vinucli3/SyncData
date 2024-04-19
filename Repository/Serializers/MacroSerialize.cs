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
using static Umbraco.Cms.Core.Constants.DataTypes;

namespace SyncData.Repository.Serializers
{
    public class MacroSerialize : IMacroSerialize
	{
		private readonly ILogger<MacroSerialize> _logger;
		private readonly IMacroService _macroService;
		public MacroSerialize(
			 ILogger<MacroSerialize> logger,
			  IMacroService macroService
			)
		{
			_logger = logger;
			_macroService = macroService;
		}
		public async Task<bool> Handler()
		{
			try
			{
				IEnumerable<IMacro>? macros = _macroService.GetAll();
				foreach (IMacro macro in macros)
				{
					XElement macroDetail = new XElement("Macro",
									new XAttribute("Key", macro.Key),
									new XAttribute("Level", "0"),
									new XAttribute("Alias", macro.Alias),
									new XElement("Name", macro.Name),
									new XElement("MacroSource", macro.MacroSource),
									new XElement("UseInEditor", macro.UseInEditor),
									new XElement("DontRender", macro.DontRender),
									new XElement("CachedByMember", macro.CacheByMember),
									new XElement("CachedByPage", macro.CacheByPage),
									new XElement("CachedDuration", macro.CacheDuration)
									);
					MacroPropertyCollection? properties = macro.Properties;
					XElement? propElement = new XElement("Properties");
					foreach (IMacroProperty property in properties)
					{
						XElement proVal = new XElement("Property",
							new XElement("Name", property.Name),
							new XElement("Alias", property.Alias),
							new XElement("SortOrder", property.SortOrder),
							new XElement("EditorAlias", property.EditorAlias)
							);
						propElement.Add(proVal);
					}

					macroDetail.Add(propElement);
					string folder = "cSync\\Macros";
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
							if (macro.Key == new Guid(keyVal))
							{
								System.IO.File.Delete(file); break;
							}
						}
					}
					string path = "cSync\\Macros\\" + macro.Name?.Replace(" ", "-").ToLower() + ".config";
					macroDetail.Save(path);
				}
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError("MacroSerialize Serialize error {ex}", ex);
				return false;
			}
		}
	}
}
