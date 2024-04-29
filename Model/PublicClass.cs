using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SyncData.Model
{
	public class MediaNameKey
	{
		public Guid Key { get; set; }
		public int Level { get; set; }
		public int SortOrder { get; set; }
		public Guid Parent { get; set; }
		public string Src { get; set; }
		public string Name { get; set; }
		public string Path { get; set; }
		public string ContentType { get; set; }
		public Guid NodeID { get; set; }
	}
	public class ImageProp
	{
		public string Key { get; set; }
		public string MediaKey { get; set; }
	}
	public class TitleDto
	{
		public Guid Key { get; set; }
		public string Value { get; set; }
	}

	public class ContentDto
	{
		public Guid Key { get; set; }
		public int Id { get; set; }
		public string Name { get; set; }
		public bool Selected { get; set; }
	}
	public class DiffXelements
	{
		public XElement X1 { get; set; }
		public XElement X2 { get; set; }
	}

	public class DiffObject
	{
		public string PropName { get; set; }
		public string PropOldValue { get; set; }
		public string PropCurrValue {  get; set; }
		public string PropAction { get; set; }
		public string PropType { get; set; }
	}
	
	public class AcknowDTO
	{
		public string Item { get; set; }
		public int ItemCount { get; set; }
		public int ItemChanges { get; set; }
		public int ItemErrors { get; set; }

	}
}
