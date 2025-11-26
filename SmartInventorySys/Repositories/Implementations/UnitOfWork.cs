using Microsoft.EntityFrameworkCore.Storage;
using SmartInventorySys.Data;
using SmartInventorySys.Models;
using SmartInventorySys.Repositories.Interfaces;

namespace SmartInventorySys.Repositories.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;
        // Fix: Make this nullable (?) because we don't always have a transaction running
        private IDbContextTransaction? _currentTransaction;

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            Category = new Repository<Category>(_db);
            SubCategory = new Repository<SubCategory>(_db);
            Product = new Repository<Product>(_db);
            Order = new Repository<Order>(_db);
            OrderDetail = new Repository<OrderDetail>(_db);
            Supplier = new Repository<Supplier>(_db);
            Purchase = new Repository<Purchase>(_db);
        }

        public IRepository<Category> Category { get; private set; }

        public IRepository<SubCategory> SubCategory { get; private set; }
        public IRepository<Product> Product { get; private set; }
        public IRepository<Order> Order { get; private set; }
        public IRepository<OrderDetail> OrderDetail { get; private set; }
        public IRepository<Supplier> Supplier { get; private set; }
        public IRepository<Purchase> Purchase { get; private set; }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }

        public async Task<IDbContextTransaction?> BeginTransactionAsync()
        {
            if (_currentTransaction != null) return null;
            _currentTransaction = await _db.Database.BeginTransactionAsync();
            return _currentTransaction;
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await _db.SaveChangesAsync();
                if (_currentTransaction != null)
                {
                    await _currentTransaction.CommitAsync();
                }
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            try
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.RollbackAsync();
                }
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }
    }
}