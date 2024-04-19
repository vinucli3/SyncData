namespace SyncData.Interface.Serializers
{
    public interface IDocTypeSerialize
    {
		public Task<bool> Handler();
    }
}