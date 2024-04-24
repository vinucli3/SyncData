using SyncData.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SyncData.Interface
{
    public interface IUpdateContent
    {
        public Task<MediaNameKey> ImageProcess(Guid id);
        public Task ImageUpdate(MediaNameKey nameKey);//, int id);
        //public bool SaveImage(string ImgStr, string ImgName, string Path);
        //public bool UpdateTitle(string Title, Guid id);
		public Task<List<ContentDto>> CollectExistingNodes();
		public Task<string> ReadNode(Guid id);
		public Task<List<DiffObject>> FindDiffNodes(DiffXelements nodes);
        public Task<bool> SolveDifference(XElement source);
		public Task<bool> UpdateNode(XElement source);
	}
}
