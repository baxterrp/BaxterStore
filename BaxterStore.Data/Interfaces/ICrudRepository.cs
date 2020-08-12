using BaxterStore.Data.POCOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BaxterStore.Data.Interfaces
{
    public interface ICrudRepository<TDataEntity> where TDataEntity : DataEntity
    {
        Task<TDataEntity> Add(TDataEntity dataEntity);
        Task<TDataEntity> Update(TDataEntity dataEntity);
        Task<IEnumerable<TDataEntity>> Search(IEnumerable<SearchParameter> searchParameters);
        Task<TDataEntity> FindById(string entityId);
        Task Delete(string entityId);
    }
}
