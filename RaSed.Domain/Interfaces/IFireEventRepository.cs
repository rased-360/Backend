using RaSed.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Domain.Interfaces
{
    public interface IFireEventRepository : IGenericRepository<FireEvent>
    {
        /// <summary>
        /// Gets the active fire event (Status = "Active") for a device.
        /// Returns null if no active fire.
        /// </summary>
        Task<FireEvent?> GetActiveFireEventAsync(string deviceId);
    }
}
