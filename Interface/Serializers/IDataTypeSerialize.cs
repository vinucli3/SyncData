namespace SyncData.Interface.Serializers
{
    public interface IDataTypeSerialize
    {
        public Task<bool> HandlerAsync();
    }
}