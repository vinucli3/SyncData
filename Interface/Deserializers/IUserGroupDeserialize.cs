namespace SyncData.Interface.Deserializers
{
    public interface IUserGroupDeserialize
    {
        public Task<bool> Handler();
    }
}