namespace SyncData.Interface.Deserializers
{
    public interface IDataTypeDeserialize
    {
        public Task<bool> Handler();
    }
}