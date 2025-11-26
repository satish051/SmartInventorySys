using System.Linq.Expressions;

namespace SmartInventorySys.Repositories.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync(string? includeProperties = null);

        // Fix: Return T?
        Task<T?> GetByIdAsync(int id);

        // Fix: Return T?
        Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> filter, string? includeProperties = null);

        void Add(T entity);
        void Update(T entity); // Added this
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
    }
}