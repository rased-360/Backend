using RaSed.Application.Interfaces;
using RaSed.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Infrastructure.Services.Authantication
{
    public class IdentityService : IIdentityService
    {
        private readonly IUnitOfWork _unitOfWork;
        public IdentityService(IUnitOfWork _unitOfWork)
        {
            this._unitOfWork = _unitOfWork;
        }


    }
}
