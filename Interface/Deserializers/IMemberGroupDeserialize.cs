namespace SyncData.Interface.Deserializers
{
    public interface IMemberGroupDeserialize
    {
        public Task<bool> Handler();
    }
}