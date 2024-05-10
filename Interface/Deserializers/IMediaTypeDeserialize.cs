namespace SyncData.Interface.Deserializers
{
    public interface IMediaTypeDeserialize
    {
        public Task<bool> HandlerAsync();
    }
}