namespace SyncData.Interface.Serializers
{
    public interface IDomainSerialize
    {
		public Task<bool> Handler();
    }
}