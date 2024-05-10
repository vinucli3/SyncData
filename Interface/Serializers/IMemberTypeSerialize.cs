namespace SyncData.Interface.Serializers
{
    public interface IMemberTypeSerialize
    {
		public Task<bool> HandlerAsync();
    }
}