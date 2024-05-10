namespace SyncData.Interface.Deserializers
{
    public interface IDictionaryDeserialize
    {
        public Task<bool> HandlerAsync();
    }
}