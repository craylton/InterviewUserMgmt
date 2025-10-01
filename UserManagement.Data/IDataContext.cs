using System.Linq;

namespace UserManagement.Data;

public interface IDataContext
{
    IQueryable<TEntity> GetAll<TEntity>() where TEntity : class;

    void Create<TEntity>(TEntity entity) where TEntity : class;

    void UpdateAndSave<TEntity>(TEntity entity) where TEntity : class;

    void Delete<TEntity>(TEntity entity) where TEntity : class;

    TEntity? GetById<TEntity>(long id) where TEntity : class;
}
