using RaSed.Application.DTOs.Authantication;
using RaSed.Application.Interfaces.Authantication;
using RaSed.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Infrastructure.Services.Authantication
{
    public class SectionService : ISectionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SectionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<SectionDto>> GetAllSectionsAsync()
        {
            var sections = await _unitOfWork._sectionRepository.GetAllAsync();
            return sections.Select(s => new SectionDto
            {
                Id = s.Id,
                Name = s.Name
            }).ToList();
        }
    }
}
