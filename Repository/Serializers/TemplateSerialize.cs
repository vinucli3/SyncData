using Microsoft.Extensions.Logging;
using NPoco;
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
    public class TemplateSerialize : ITemplateSerialize
	{
		private readonly ILogger<TemplateSerialize> _logger;
		private IFileService _fileService;
		public TemplateSerialize(
			 ILogger<TemplateSerialize> logger,
			 IFileService fileService
			)
		{
			_logger = logger;
			_fileService = fileService;
		}

		public async Task<bool> Handler()
		{
			try
			{
				IEnumerable<ITemplate>? templates = _fileService.GetTemplates();
				foreach (ITemplate template in templates)
				{
					XElement memberDetail = new XElement("MemberType",
						new XAttribute("Key", template.Key),
						new XAttribute("Alias", template.Alias),
						new XAttribute("Level", "1"),
						new XElement("Name", template.Name?.Trim(" ")),
						new XElement("Parent", template.MasterTemplateAlias)); ;

					string folder = "cSync\\Templates";
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
							if (template.Key == new Guid(keyVal))
							{
								System.IO.File.Delete(file); break;
							}
						}
					}
					string path = "cSync\\Templates\\" + template.Alias?.Replace(" ", " -").ToLower() + ".config";
					memberDetail.Save(path);
				}
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError("TemplateSerialize Serialize error {ex}", ex);
				return false;
			}
		}
	}
}
