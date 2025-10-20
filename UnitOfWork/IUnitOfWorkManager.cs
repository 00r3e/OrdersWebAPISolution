using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitOfWork
{
    public interface IUnitOfWorkManager
    {
        void StartUnitOfWork();

        bool IsUnitOfWorkStarted { get; }

        Task<int> SaveChangesAsync();
    }
}
