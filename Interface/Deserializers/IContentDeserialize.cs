﻿namespace SyncData.Interface.Deserializers
{
    public interface IContentDeserialize
    {
        public Task<bool> Handler();
    }
}