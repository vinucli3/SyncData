namespace SyncData.Interface.Deserializers
{
    public interface IMemberDeserialize
    {
        public Task<bool> HandlerAsync();
    }
}