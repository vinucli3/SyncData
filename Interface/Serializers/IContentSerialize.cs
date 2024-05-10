namespace SyncData.Interface.Serializers
{
    public interface IContentSerialize
    {
		public Task<bool> HandlerAsync();
    }
}