using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using NUglify.Helpers;
using SyncData.Interface.Deserializers;
using SyncData.Model;
using System.Xml.Linq;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;

namespace SyncData.Repository.Deserializers
{
	public class DataTypeDeserialize : IDataTypeDeserialize
	{
		private readonly ILogger<DataTypeDeserialize> _logger;

		private readonly IConfigurationEditorJsonSerializer _configurationEditorJsonSerializer;
		private IDataTypeService _dataTypeService;
		private readonly PropertyEditorCollection _propertyEditorCollection;
		private IArtifact _artifact;
		public DataTypeDeserialize(
			 ILogger<DataTypeDeserialize> logger,
			 IConfigurationEditorJsonSerializer configurationEditorJsonSerializer,
			 IDataTypeService dataTypeService,
			 PropertyEditorCollection propertyEditorCollection
			 )
		{
			_logger = logger;
			_configurationEditorJsonSerializer = configurationEditorJsonSerializer;
			_dataTypeService = dataTypeService;
			_propertyEditorCollection = propertyEditorCollection;
		}

		public async Task<bool> Handler()
		{
			try
			{
				string folder = "cSync\\Datatypes";
				if (!Directory.Exists(folder)) return false;
				string[] files = Directory.GetFiles(folder);
				var allDataType = _dataTypeService.GetAll();

				foreach (string file in files)
				{
					XElement readFile = XElement.Load(file); // XElement.Parse(stringWithXmlGoesHere)
					XElement? root = new XElement(readFile.Name, readFile.Attributes());


					string? keyVal = root.Attribute("Key").Value ?? "";
					string? nameVal = readFile.Element("Info").Element("Name").Value ?? "";
					string? editorAlias = readFile.Element("Info").Element("EditorAlias").Value ?? "";
					string? databaseType = readFile.Element("Info").Element("DatabaseType").Value ?? "";
					string? configVal = readFile.Element("Config").Value ?? "";
					/*******************************************/
					var existDataType = allDataType.Where(x => x.Key == new Guid(keyVal)).FirstOrDefault();
					IDataEditor? dataTypeName = _propertyEditorCollection.Where(x => x.Alias == editorAlias).FirstOrDefault();


					if (existDataType == null)
					{
						existDataType = new DataType(dataTypeName, _configurationEditorJsonSerializer, -1) { Id = existDataType != null ? existDataType.Id : 0 };
						existDataType.Key = new Guid(keyVal);
						existDataType.Name = nameVal;
						string? configSer = _configurationEditorJsonSerializer.Serialize(dataTypeName.Name);
						existDataType.DatabaseType = (ValueStorageType)Enum.Parse(typeof(ValueStorageType), databaseType);
					}
					else if (existDataType.Name != nameVal)
					{

						existDataType.Name = nameVal;
					}
					if (existDataType.EditorAlias == "Umbraco.ContentPicker")
					{
						var config = JsonConvert.DeserializeObject<ContentPickerConfiguration>(configVal);
						if (config != null)
						{
							ContentPickerConfiguration prevalues = (ContentPickerConfiguration)existDataType.Configuration;
							prevalues.IgnoreUserStartNodes = config.IgnoreUserStartNodes;
							prevalues.ShowOpenButton = config.ShowOpenButton;
							prevalues.StartNodeId = config.StartNodeId;
						}
					}
					else if (existDataType.EditorAlias == "Umbraco.DateTime")
					{
						var config = JsonConvert.DeserializeObject<DateTimeConfiguration>(configVal);
						if (config != null)
						{
							DateTimeConfiguration prevalues = (DateTimeConfiguration)existDataType.Configuration;
							prevalues.Format = config.Format;
							prevalues.OffsetTime = config.OffsetTime;
						}
					}
					else if (existDataType.EditorAlias == "Umbraco.ColorPicker")
					{
						var config = JsonConvert.DeserializeObject<ColorPickerConfiguration>(configVal);
						if (config != null)
						{
							ColorPickerConfiguration prevalues = (ColorPickerConfiguration)existDataType.Configuration;
							prevalues.Items = config.Items;
							prevalues.UseLabel = config.UseLabel;
						}
					}
					else if (existDataType.EditorAlias == "Umbraco.CheckBoxList")
					{
						var config = JsonConvert.DeserializeObject<ValueListConfiguration>(configVal);
						if (config != null)
						{
							ValueListConfiguration? prevalues = (ValueListConfiguration)existDataType.Configuration;
							prevalues.Items = config.Items;
						}
					}
					else if (existDataType.EditorAlias == "Umbraco.DropDown.Flexible")
					{
						var config = JsonConvert.DeserializeObject<DropDownFlexibleConfiguration>(configVal);
						if (config != null)
						{
							DropDownFlexibleConfiguration prevalues = (DropDownFlexibleConfiguration)existDataType.Configuration;
							prevalues.Items = config.Items;
							prevalues.Multiple = config.Multiple;
						}
					}
					else if (existDataType.EditorAlias == "Umbraco.ImageCropper")
					{
						var config = JsonConvert.DeserializeObject<ImageCropperConfiguration>(configVal);
						if (config != null)
						{
							ImageCropperConfiguration prevalues = (ImageCropperConfiguration)existDataType.Configuration;
							prevalues.Crops = config.Crops;
						}
					}
					else if (existDataType.EditorAlias == "Umbraco.MediaPicker3")
					{
						var config = JsonConvert.DeserializeObject<MediaPicker3Configuration>(configVal);
						if (config != null)
						{
							MediaPicker3Configuration prevalues = (MediaPicker3Configuration)existDataType.Configuration;
							prevalues.Crops = config.Crops;
							prevalues.EnableLocalFocalPoint = config.EnableLocalFocalPoint;
							prevalues.Filter = config.Filter;
							prevalues.IgnoreUserStartNodes = config.IgnoreUserStartNodes;
							prevalues.Multiple = config.Multiple;
							prevalues.StartNodeId = config.StartNodeId;
							prevalues.ValidationLimit = config.ValidationLimit;
						}
					}
					else if (existDataType.EditorAlias == "Umbraco.Label")
					{
						var config = JsonConvert.DeserializeObject<LabelConfiguration>(configVal);
						if (config != null)
						{
							LabelConfiguration prevalues = (LabelConfiguration)existDataType.Configuration;
							prevalues.ValueType = config.ValueType;
						}
					}
					else if (existDataType.EditorAlias == "Umbraco.ListView")
					{
						var config = JsonConvert.DeserializeObject<ListViewConfiguration>(configVal);
						if (config != null)
						{
							ListViewConfiguration prevalues = (ListViewConfiguration)existDataType.Configuration;
							prevalues.BulkActionPermissions = config.BulkActionPermissions;
							prevalues.Icon = config.Icon;
							prevalues.IncludeProperties = config.IncludeProperties;
							prevalues.Layouts = config.Layouts;
							prevalues.OrderBy = config.OrderBy;
							prevalues.OrderDirection = config.OrderDirection;
							prevalues.PageSize = config.PageSize;
							prevalues.ShowContentFirst = config.ShowContentFirst;
							prevalues.TabName = config.TabName;
							prevalues.UseInfiniteEditor = config.UseInfiniteEditor;
						}
					}
					else if (existDataType.EditorAlias == "Umbraco.MemberPicker") //todo
					{
						var config = JsonConvert.DeserializeObject(configVal);
						if (config != null)
						{

						}
					}
					else if (existDataType.EditorAlias == "Umbraco.MultiUrlPicker")
					{
						var config = JsonConvert.DeserializeObject<MultiUrlPickerConfiguration>(configVal);
						if (config != null)
						{
							MultiUrlPickerConfiguration prevalues = (MultiUrlPickerConfiguration)existDataType.Configuration;
							prevalues.HideAnchor = config.HideAnchor;
							prevalues.IgnoreUserStartNodes = config.IgnoreUserStartNodes;
							prevalues.MaxNumber = config.MaxNumber;
							prevalues.MinNumber = config.MinNumber;
							prevalues.OverlaySize = config.OverlaySize;
						}

					}
					else if (existDataType.EditorAlias == "Umbraco.Integer")
					{
						var config = JsonConvert.DeserializeObject(configVal);
						if (config != null)
						{

						}
					}
					else if (existDataType.EditorAlias == "Umbraco.TinyMCE")
					{
						var config = JsonConvert.DeserializeObject<RichTextConfiguration>(configVal);
						if (config != null)
						{
							RichTextConfiguration prevalues = (RichTextConfiguration)existDataType.Configuration;
							prevalues.Editor = config.Editor;
							prevalues.HideLabel = config.HideLabel;
							prevalues.IgnoreUserStartNodes = config.IgnoreUserStartNodes;
							prevalues.MediaParentId = config.MediaParentId;
							prevalues.OverlaySize = config.OverlaySize;
						}
					}
					else if (existDataType.EditorAlias == "Umbraco.Tags")
					{
						var config = JsonConvert.DeserializeObject<TagConfiguration>(configVal);
						if (config != null)
						{
							TagConfiguration prevalues = (TagConfiguration)existDataType.Configuration;
							prevalues.Delimiter = config.Delimiter;
							prevalues.Group = config.Group;
							prevalues.StorageType = config.StorageType;
						}
					}
					else if (existDataType.EditorAlias == "Umbraco.TextBox")
					{
						var config = JsonConvert.DeserializeObject<TextboxConfiguration>(configVal);
						if (config != null)
						{
							TextboxConfiguration prevalues = (TextboxConfiguration)existDataType.Configuration;
							prevalues.MaxChars = config.MaxChars;
						}
					}
					else if (existDataType.EditorAlias == "Umbraco.TextArea")
					{
						var config = JsonConvert.DeserializeObject<TextAreaConfiguration>(configVal);
						if (config != null)
						{
							TextAreaConfiguration prevalues = (TextAreaConfiguration)existDataType.Configuration;
							prevalues.MaxChars = config.MaxChars;
							prevalues.Rows = config.Rows;
						}
					}
					else if (existDataType.EditorAlias == "Umbraco.TrueFalse")
					{
						var config = JsonConvert.DeserializeObject<TrueFalseConfiguration>(configVal);
						if (config != null)
						{
							TrueFalseConfiguration prevalues = (TrueFalseConfiguration)existDataType.Configuration;
							prevalues.Default = config.Default;
							prevalues.LabelOff = config.LabelOff;
							prevalues.LabelOn = config.LabelOn;
							prevalues.ShowLabels = config.ShowLabels;
						}
					}
					else if (existDataType.EditorAlias == "Umbraco.UploadField")
					{
						var config = JsonConvert.DeserializeObject<FileUploadConfiguration>(configVal);
						if (config != null)
						{
							FileUploadConfiguration prevalues = (FileUploadConfiguration)existDataType.Configuration;
							prevalues.FileExtensions = config.FileExtensions;
						}
					}
					else if (existDataType.EditorAlias == "Umbraco.MediaPicker")
					{
						var config = JsonConvert.DeserializeObject<MediaPickerConfiguration>(configVal);
						if (config != null)
						{
							MediaPickerConfiguration prevalues = (MediaPickerConfiguration)existDataType.Configuration;
							prevalues.DisableFolderSelect = config.DisableFolderSelect;
							prevalues.IgnoreUserStartNodes = config.IgnoreUserStartNodes;
							prevalues.Multiple = config.Multiple;
							prevalues.OnlyImages = config.OnlyImages;
							prevalues.StartNodeId = config.StartNodeId;
						}
					}
					else if (existDataType.EditorAlias == "Umbraco.RadioButtonList")
					{
						var config = JsonConvert.DeserializeObject<ValueListConfiguration>(configVal);
						if (config != null)
						{
							ValueListConfiguration prevalues = (ValueListConfiguration)existDataType.Configuration;
							prevalues.Items = config.Items;
						}
					}
					else if (existDataType.EditorAlias == "Umbraco.BlockGrid")
					{
						var config = JsonConvert.DeserializeObject<BlockGridConfiguration>(configVal);
						if (config != null)
						{
							BlockGridConfiguration prevalues = (BlockGridConfiguration)existDataType.Configuration;
							prevalues.BlockGroups = config.BlockGroups;
							prevalues.Blocks = config.Blocks;
							prevalues.CreateLabel = config.CreateLabel;
							prevalues.GridColumns = config.GridColumns;
							prevalues.LayoutStylesheet = config.LayoutStylesheet;
							prevalues.MaxPropertyWidth = config.MaxPropertyWidth;
							prevalues.UseLiveEditing = config.UseLiveEditing;
							prevalues.ValidationLimit = config.ValidationLimit;
						}
					}
					else if (existDataType.EditorAlias == "Umbraco.ColorPicker.EyeDropper")
					{
						var config = JsonConvert.DeserializeObject<EyeDropperColorPickerConfiguration>(configVal);
						if (config != null)
						{
							EyeDropperColorPickerConfiguration prevalues = (EyeDropperColorPickerConfiguration)existDataType.Configuration;
							prevalues.ShowAlpha = config.ShowAlpha;
							prevalues.ShowPalette = config.ShowPalette;
						}
					}
					else if (existDataType.EditorAlias == "Umbraco.MultiNodeTreePicker")
					{
						var config = JsonConvert.DeserializeObject<MultiNodePickerConfiguration>(configVal);
						if (config != null)
						{
							MultiNodePickerConfiguration prevalues = (MultiNodePickerConfiguration)existDataType.Configuration;
							prevalues.Filter = config.Filter;
							prevalues.IgnoreUserStartNodes = config.IgnoreUserStartNodes;
							prevalues.MaxNumber = config.MaxNumber;
							prevalues.MinNumber = config.MinNumber;
							prevalues.ShowOpen = config.ShowOpen;
							prevalues.TreeSource = config.TreeSource;
						}
					}
					else if (existDataType.EditorAlias == "Umbraco.Decimal")
					{

					}
					else if (existDataType.EditorAlias == "Umbraco.EmailAddress")
					{
						var config = JsonConvert.DeserializeObject<EmailAddressConfiguration>(configVal);
						if (config != null)
						{
							EmailAddressConfiguration prevalues = (EmailAddressConfiguration)existDataType.Configuration;
							prevalues.IsRequired = config.IsRequired;
						}
					}
					else if (existDataType.EditorAlias == "Umbraco.MarkdownEditor")
					{
						var config = JsonConvert.DeserializeObject<MarkdownConfiguration>(configVal);
						if (config != null)
						{
							MarkdownConfiguration prevalues = (MarkdownConfiguration)existDataType.Configuration;
							prevalues.DefaultValue = config.DefaultValue;
							prevalues.DisplayLivePreview = config.DisplayLivePreview;
							prevalues.OverlaySize = config.OverlaySize;
						}
					}
					else if (existDataType.EditorAlias == "Umbraco.Slider")
					{
						var config = JsonConvert.DeserializeObject<SliderConfiguration>(configVal);
						if (config != null)
						{
							SliderConfiguration prevalues = (SliderConfiguration)existDataType.Configuration;
							prevalues.EnableRange = config.EnableRange;
							prevalues.InitialValue = config.InitialValue;
							prevalues.InitialValue2 = config.InitialValue2;
							prevalues.MaximumValue = config.MaximumValue;
							prevalues.MinimumValue = config.MinimumValue;
							prevalues.StepIncrements = config.StepIncrements;
						}
					}
					else if (existDataType.EditorAlias == "Umbraco.BlockList")
					{
						var config = JsonConvert.DeserializeObject<BlockListConfiguration>(configVal);
						if (config != null)
						{
							BlockListConfiguration prevalues = (BlockListConfiguration)existDataType.Configuration;
							prevalues.Blocks = config.Blocks;
							prevalues.MaxPropertyWidth = config.MaxPropertyWidth;
							prevalues.UseInlineEditingAsDefault = config.UseInlineEditingAsDefault;
							prevalues.UseLiveEditing = config.UseLiveEditing;
							prevalues.UseSingleBlockMode = config.UseSingleBlockMode;
							prevalues.ValidationLimit = config.ValidationLimit;
						}
					}
					else if (existDataType.EditorAlias == "Umbraco.MultipleTextstring")
					{
						var config = JsonConvert.DeserializeObject<MultipleTextStringConfiguration>(configVal);
						if (config != null)
						{
							MultipleTextStringConfiguration prevalues = (MultipleTextStringConfiguration)existDataType.Configuration;
							prevalues.Maximum = config.Maximum;
							prevalues.Minimum = config.Minimum;
						}
					}
					else
					{
						if (existDataType.EditorAlias == "sds")
						{

						}
					}
					_dataTypeService.Save(existDataType);

				}
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError("DataTypeDeserialize 1001 {ex}", ex);
				return false;
			}
		}
	}
}
