using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orders.WebAPI.ApplicationDbContext;

namespace UnitOfWork
{
    public class UnitOfWorkManager : IUnitOfWorkManager
    {
        private readonly AppDbContext _dbContext;

        public UnitOfWorkManager(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private bool _isUnitOfWorkStarted = false;

        public void StartUnitOfWork()
        {
            _isUnitOfWorkStarted = true;
        }

        public bool IsUnitOfWorkStarted => _isUnitOfWorkStarted;

        public async Task<int> SaveChangesAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }
    }
}
