using RaSed.Domain.Entities;
using RaSed.Domain.Interfaces;
using RaSed.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly AppDbContext _context;

        // هنا بتعرفي الريبوزاتوري الخاصة بكل كيان
        public AdminRepository _adminReposatory { get; private set; }
        // ممكن تضيفي كمان Product, Category, Request ... الخ

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            _adminReposatory = new AdminRepository(_context);
        }

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
