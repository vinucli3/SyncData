namespace SyncData.Interface.Deserializers
{
    public interface IMediaDeserialize
    {
        public Task<bool> Handler();
    }
}