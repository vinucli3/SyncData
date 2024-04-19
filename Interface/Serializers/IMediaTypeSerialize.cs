namespace SyncData.Interface.Serializers
{
    public interface IMediaTypeSerialize
    {
		public Task<bool> Handler();
    }
}