using BaxterStore.Data.POCOs;

namespace BaxterStore.Data.Interfaces
{
    public interface ICacheHandler<TDataEntity> where TDataEntity : DataEntity
    {
        bool TryGetValue(string entityId, out TDataEntity dataEntity);

        void AddToCache(TDataEntity dataEntity);

        void DropFromCache(string entityId);
    }
}
