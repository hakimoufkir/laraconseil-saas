using MultiTenantStripeAPI.Application.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantStripeAPI.Application.IUnitOfService
{
    public interface IUnitOfService
    {
        ITenantService TenantService { get; }
    }
}
