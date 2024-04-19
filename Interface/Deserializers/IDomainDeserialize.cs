namespace SyncData.Interface.Deserializers
{
    public interface IDomainDeserialize
    {
        public Task<bool> Handler();
    }
}