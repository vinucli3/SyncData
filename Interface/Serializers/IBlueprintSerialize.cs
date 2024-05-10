using SyncData.Model;

namespace SyncData.Interface.Serializers
{
    public interface IBlueprintSerialize : IDisposable
	{
        public Task<bool> HandlerAsync();
    }
}