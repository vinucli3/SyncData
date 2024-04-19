namespace SyncData.Interface.Deserializers
{
    public interface ITemplateDeserialize
    {
        public Task<bool> Handler();
    }
}