using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Umbraco.Cms.Core.Services;
using SyncData.Interface.Serializers;
using Umbraco.Cms.Core.Models;
using static Umbraco.Cms.Core.Collections.TopoGraph;

namespace SyncData.Repository.Serializers
{
	public class DataTypeSerialize : IDataTypeSerialize
	{
		private readonly ILogger<DataTypeSerialize> _logger;

		private readonly IDataTypeService _dataTypeService;
		private static readonly JsonSerializerOptions _options =
		new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

		public DataTypeSerialize(
			 ILogger<DataTypeSerialize> logger,
			 IDataTypeService dataTypeService
			)
		{
			_logger = logger;
			_dataTypeService = dataTypeService;
		}

		public async Task<bool> Handler()
		{
			try
			{
				IEnumerable<IDataType>? allDataType = _dataTypeService.GetAll();

				foreach (IDataType dataType in allDataType)
				{
					string? jsonString = JsonSerializer.Serialize(dataType.Configuration, _options);
					XElement dataTypeDetail = new XElement("Content",
						new XAttribute("Key", dataType.Key),
						new XAttribute("Level", dataType.Level));

					XElement info =
						new XElement("Info",
						new XElement("Name", dataType.Name),
						new XElement("EditorAlias", dataType.EditorAlias),
						new XElement("DatabaseType", dataType.DatabaseType));
					dataTypeDetail.Add(info);
					XElement config = new XElement("Config", new XCData(jsonString));
					dataTypeDetail.Add(config);
					string fileName = Regex.Replace(dataType.Name, "[^A-Za-z0-9 ]", "");
					string folder = "cSync\\DataTypes";
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
							if (dataType.Key == new Guid(keyVal))
							{
								System.IO.File.Delete(file); break;
							}
						}
					}
					string path = "cSync\\DataTypes\\" + fileName.Replace(" ", "") + ".config";
					dataTypeDetail.Save(path);
				}
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError("DataType Serialize error {ex}", ex);
				return false;
			}

		}
	}
}
