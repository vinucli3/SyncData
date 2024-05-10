namespace SyncData.Interface.Deserializers
{
    public interface IRelationDeserialize
    {
        public Task<bool> HandlerAsync();
    }
}