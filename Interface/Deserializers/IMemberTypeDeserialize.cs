namespace SyncData.Interface.Deserializers
{
    public interface IMemberTypeDeserialize
    {
        public Task<bool> HandlerAsync();
    }
}