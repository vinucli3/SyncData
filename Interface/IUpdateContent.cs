using Microsoft.AspNetCore.Mvc;
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
        //public Task<MediaNameKey> ImageProcess(Guid id);
        //public Task ImageUpdate(MediaNameKey nameKey);//, int id);
        //public bool SaveImage(string ImgStr, string ImgName, string Path);
        //public bool UpdateTitle(string Title, Guid id);
		public Task<List<ContentDto>> CollectExistingNodesAsync();
		public Task<XElement> ReadNodeAsync(Guid id);
		public Task<List<DiffObject>> FindDiffNodesAsync(UpdateDTO nodes);
        public Task<bool> SolveDifferenceAsync(UpdateDTO source);
		public Task<bool> UpdateNodeAsync(XElement source);
		public Task<bool> CreateNodeAsync(XElement source);
		public Task<bool> DeleteNodeAsync(UpdateDTO source);
		public Task<XElement> RemoteReadNodeAsync(UpdateDTO updateDTO);
		public Task<List<Guid>> CollectAllNodes(Guid id);
		public Task<bool> DeleteRemoteAsync( List<Guid> nodekeys);
	}
	
}
