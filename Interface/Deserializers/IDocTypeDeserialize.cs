namespace SyncData.Interface.Deserializers
{
    public interface IDocTypeDeserialize
    {
        public Task<bool> HandlerAsync();
    }
}