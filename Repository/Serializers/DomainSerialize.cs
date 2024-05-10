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
	public class DomainSerialize : IDomainSerialize
	{
		private readonly ILogger<DomainSerialize> _logger;
		private IDomainService _domainService;
		private IContentService _contentService;
		public DomainSerialize(
			 ILogger<DomainSerialize> logger,
			 IDomainService domainService,
			 IContentService contentService
			)
		{
			_logger = logger;
			_domainService = domainService;
			_contentService = contentService;
		}

		public async Task<bool> HandlerAsync()
		{
			try
			{
				List<IDomain>? domains = _domainService.GetAll(true).ToList();
				foreach (IDomain domain in domains)
				{
					var b = await SingleHandlerAsync(domain);
				}
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError("DomainSerialize Serialize error {ex}", ex);
				return false;
			}
		}
		public async Task<bool> SingleHandlerAsync(IDomain domain)
		{
			try
			{
				IContent? rootContent = _contentService.GetById(domain.RootContentId.GetValueOrDefault());
				XElement domainsDetail = new XElement("Content",
					new XAttribute("Key", domain.Key),
					new XAttribute("Alias", domain.DomainName));

				XElement info =
						new XElement("Info",
						new XElement("IsWildcard", domain.IsWildcard),
						new XElement("Language", domain.LanguageIsoCode),
						new XElement("Root", rootContent != null ? new XAttribute("Key", rootContent.Key) : null, rootContent != null ? rootContent.Name : ""),
						new XElement("SortOrder", domain.SortOrder)
						);

				domainsDetail.Add(info);
				string folder = "cSync\\Domain";
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
						if (domain.Key == new Guid(keyVal))
						{
							System.IO.File.Delete(file); break;
						}
					}
				}
				string? fileName = domain.DomainName.Remove(0, 1);
				if (domain.SortOrder >= 0)
					fileName = fileName.Remove(fileName.Length - 1, 1).Replace("/", "-");
				string path = "cSync\\Domain\\" + fileName + "_" + domain.LanguageIsoCode + ".config";
				domainsDetail.Save(path);
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError("DomainSerialize Serialize error {ex}", ex);
				return false;
			}
		}
	}
}
