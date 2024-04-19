using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using SyncData.Interface.Deserializers;
using System.Globalization;
using System.Xml.Linq;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace SyncData.Repository.Deserializers
{
    public class TemplateDeserialize : ITemplateDeserialize
	{
		private readonly ILogger<TemplateDeserialize> _logger;

		private IFileService _fileService;
		private readonly IWebHostEnvironment _webHostEnvironment;
		private readonly IShortStringHelper _shortStringHelper;
		List<string> parentfiles = new List<string>();
		public TemplateDeserialize(
			 ILogger<TemplateDeserialize> logger,
			 IFileService fileService,
			 IWebHostEnvironment webHostEnvironment,
			 IShortStringHelper shortStringHelper
			 )
		{
			_logger = logger;
			_fileService = fileService;
			_webHostEnvironment = webHostEnvironment;
			_shortStringHelper = shortStringHelper;

		}
		public async Task<bool> Handler()
		{
			try
			{
				string folder = "cSync\\Templates";
				if (!Directory.Exists(folder)) return false;
				string[] files = Directory.GetFiles(folder);
				parentfiles = files.ToList();

				foreach (string file in files)
				{
					CreateParentTemplates(file);
				}
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError("TemplateDeserialize {ex}", ex);
				return false;
			}
		}

		void CreateParentTemplates(string? file)
		{
			XElement readFile = XElement.Load(file);
			XElement? root = new XElement(readFile.Name, readFile.Attributes());

			string? keyVal = root?.Attribute("Key")?.Value ?? "";
			if (_fileService.GetTemplate(new Guid(keyVal)) != null)
			{
				return;
			}
			string? aliasVal = root?.Attribute("Alias")?.Value ?? "";
			string? levelVal = root?.Attribute("Level")?.Value ?? "";

			string? nameVal = readFile.Element("Name")?.Value ?? "";
			string? parent = readFile.Element("Parent")?.Value ?? "";
			if (!parent.IsNullOrWhiteSpace())
			{
				string? parentFile = parentfiles.FirstOrDefault(item => item.ToString().ToLower().Contains(parent.ToLower())); ;
				if (parentFile.IsNullOrWhiteSpace()) { CreateParentTemplates(parentFile); }
			}

			try
			{
				string templatePath = string.Format(CultureInfo.InvariantCulture, "~/Views/{0}.cshtml", aliasVal);

				if (_fileService.GetTemplate(new Guid(keyVal)) == null)
				{
					string? localPath = _webHostEnvironment.MapPathContentRoot(templatePath);
					string? content = System.IO.File.ReadAllText(localPath);
					if (content is not null)
					{
						Template newTemplate = new Template(_shortStringHelper, nameVal, aliasVal);
						newTemplate.Key = new Guid(keyVal);
						newTemplate.Path = ViewPath(aliasVal);
						newTemplate.Content = content;
						if (!parent.IsNullOrWhiteSpace())
						{
							ITemplate? parentempl = _fileService.GetTemplate(parent);
							if (parentempl != null)
							{
								newTemplate.SetMasterTemplate(_fileService.GetTemplate(parent));
							}
						}
						_fileService.SaveTemplate(newTemplate);
					}
					else
					{
						_logger.LogError("No file found");
						return;
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError("Error template add {ex}", ex);
			}

		}
		public static string ViewPath(string alias)
		{
			return "../Views/" + alias.Replace(" ", "") + ".cshtml";
		}


	}
}