namespace SyncData.Interface.Deserializers
{
    public interface IBlueprintDeserialize
    {
		public Task<bool> HandlerAsync();
    }
}