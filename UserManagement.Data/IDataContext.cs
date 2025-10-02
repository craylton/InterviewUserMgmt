using System.Linq;
using System.Threading.Tasks;

namespace UserManagement.Data;

public interface IDataContext
{
    IQueryable<TEntity> GetAll<TEntity>() where TEntity : class;

    Task CreateAsync<TEntity>(TEntity entity) where TEntity : class;

    Task UpdateAndSaveAsync<TEntity>(TEntity entity) where TEntity : class;

    Task DeleteAsync<TEntity>(TEntity entity) where TEntity : class;

    Task<TEntity?> GetByIdAsync<TEntity>(long id) where TEntity : class;

    Task<TEntity?> GetByIdNoTrackingAsync<TEntity>(long id) where TEntity : class;
}
