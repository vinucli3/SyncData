namespace SyncData.Interface.Serializers
{
    public interface IMemberSerialize
    {
		public Task<bool> Handler();
    }
}