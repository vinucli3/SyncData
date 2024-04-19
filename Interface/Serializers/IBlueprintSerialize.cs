namespace SyncData.Interface.Serializers
{
    public interface IBlueprintSerialize : IDisposable
	{
        public Task<bool> Handler();
    }
}