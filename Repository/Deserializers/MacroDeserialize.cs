using Microsoft.Extensions.Logging;
using SyncData.Interface.Deserializers;
using System.Xml.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace SyncData.Repository.Deserializers
{
    public class MacroDeserialize : IMacroDeserialize
	{
		private readonly ILogger<MacroDeserialize> _logger;

		private IMacroService _macroService;
		private readonly IShortStringHelper _shortStringHelper;
		public MacroDeserialize(
			 ILogger<MacroDeserialize> logger,
			 IMacroService macroService,
			 IShortStringHelper shortStringHelper
			 )
		{
			_logger = logger;
			_macroService = macroService;
			_shortStringHelper = shortStringHelper;
		}

		public async Task<bool> HandlerAsync()
		{
			try
			{
				string folder = "cSync\\Macros";
				if (!Directory.Exists(folder)) return false;
				string[] files = Directory.GetFiles(folder);
				foreach (string file in files)
				{
					XElement readFile = XElement.Load(file); // XElement.Parse(stringWithXmlGoesHere)
					XElement? root = new XElement(readFile.Name, readFile.Attributes());

					string? keyVal = root?.Attribute("Key")?.Value ?? "";
					string? aliasVal = root?.Attribute("Alias")?.Value ?? "";
					string? levelVal = root?.Attribute("Level")?.Value ?? "";

					string? name = readFile.Element("Name")?.Value ?? "";
					string? macroSource = readFile.Element("MacroSource")?.Value ?? "";
					string? useInEditor = readFile.Element("UseInEditor")?.Value ?? "";
					string? dontRender = readFile.Element("DontRender")?.Value ?? "";
					string? cachedByMember = readFile.Element("CachedByMember")?.Value ?? "";
					string? cachedByPage = readFile.Element("CachedByPage")?.Value ?? "";
					string? cachedDuration = readFile.Element("CachedDuration")?.Value ?? "";

					IEnumerable<XElement>? properties = readFile.Element("Properties").Elements();

					IMacro? alreadyCreatedMacro = _macroService.GetByAlias(name);
					if (alreadyCreatedMacro == null)
					{
						Macro? newMacro = new Macro(_shortStringHelper,
							aliasVal,
							name,
							macroSource,
							Convert.ToBoolean(cachedByPage),
							Convert.ToBoolean(cachedByMember),
							Convert.ToBoolean(dontRender),
							Convert.ToBoolean(useInEditor),
							Convert.ToInt16(cachedDuration)
							);
						newMacro.Key = new Guid(keyVal);

						foreach (XElement property in properties)
						{
							string? propName = property.Element("Name")?.Value ?? "";
							string? propAlias = property.Element("Alias")?.Value ?? "";
							string? propSortOrder = property.Element("SortOrder")?.Value ?? "";
							string? propEditorAlias = property.Element("EditorAlias")?.Value ?? "";
							MacroProperty? macroProperty = new MacroProperty()
							{
								Name = propName,
								Alias = propAlias,
								SortOrder = Convert.ToInt16(propSortOrder),
								EditorAlias = propEditorAlias,
							};
							newMacro.Properties.Add(macroProperty);
						}
						_macroService.Save(newMacro);
					}
				}
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError("MacroDeserialize {ex}", ex);
				return false;
			}
		}
	}
}
