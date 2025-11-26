using Microsoft.EntityFrameworkCore.Storage;
using SmartInventorySys.Models;

namespace SmartInventorySys.Repositories.Interfaces
{
    public interface IUnitOfWork
    {
        IRepository<Category> Category { get; }

        IRepository<SubCategory> SubCategory { get; }
        IRepository<Product> Product { get; }
        IRepository<Order> Order { get; }
        IRepository<OrderDetail> OrderDetail { get; }
        IRepository<Supplier> Supplier { get; }
        IRepository<Purchase> Purchase { get; }

        Task SaveAsync();

        // Fix: Update return type to nullable
        Task<IDbContextTransaction?> BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}