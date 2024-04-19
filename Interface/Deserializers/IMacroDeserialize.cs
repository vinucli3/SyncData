namespace SyncData.Interface.Deserializers
{
    public interface IMacroDeserialize
    {
		public Task<bool> Handler();
    }
}