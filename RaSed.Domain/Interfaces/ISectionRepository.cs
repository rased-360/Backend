using RaSed.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Domain.Interfaces
{
    public interface ISectionRepository : IGenericRepository<Section>
    {

        Task<List<Section>> GetAllAsync();
        Task<bool> ExistsByIdAsync(int id);
    }
}
