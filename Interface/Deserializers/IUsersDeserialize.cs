namespace SyncData.Interface.Deserializers
{
    public interface IUsersDeserialize
    {
        public Task<bool> HandlerAsync();
    }
}