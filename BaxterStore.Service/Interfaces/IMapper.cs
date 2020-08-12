using BaxterStore.Data.POCOs;

namespace BaxterStore.Service.Interfaces
{
    public interface IMapper<Resource, TDataEntity> where TDataEntity : DataEntity
    {
        TDataEntity MapToEntity(Resource resource);
        Resource MapToResource(TDataEntity dataEntity);
    }
}
