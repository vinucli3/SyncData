namespace SyncData.Interface.Serializers
{
    public interface IRelationSerialize
    {
		public Task<bool> Handler();
    }
}