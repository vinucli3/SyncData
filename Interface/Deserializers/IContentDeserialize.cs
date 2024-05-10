using System.Xml.Linq;

namespace SyncData.Interface.Deserializers
{
    public interface IContentDeserialize
    {
        public Task<bool> HandlerAsync();
		public Task<bool> SingleHandlerAsync(XElement source);
		public Task creatContentAsync(string file);
	}
}