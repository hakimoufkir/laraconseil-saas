﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MultiTenantStripeAPI.Infrastructure.GenericRepo;
using MultiTenantStripeAPI.Application.IUnitOfWork;
using MultiTenantStripeAPI.Application.IGenericRepo;
using MultiTenantStripeAPI.Infrastructure.Persistence.Context;
using Microsoft.Extensions.Configuration;
using MultiTenantStripeAPI.Application.IRepositories;
using MultiTenantStripeAPI.Infrastructure.Repositories;
using MultiTenantStripeAPI.Infrastructure.UnitOfWork;

namespace MultiTenantStripeAPI.Infrastructure
{
    public static class DependencyInjectionInfra
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add DbContext with SQL Server
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultSQLConnection"),
                    sqlOptions => sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorCodesToAdd: null)
                ));

            // Register Generic Repository and Unit of Work
            services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddTransient<IUnitOfWork, UnitOfWork.UnitOfWork>();
            services.AddTransient<ITenantRepository, TenantRepository>();
            services.AddTransient<IRolePermissionRepository, RolePermissionRepository>();
            services.AddTransient<IUserRoleRepository, UserRoleRepository>();
            services.AddTransient<IRoleRepository, RoleRepository>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddHttpContextAccessor();


            return services;
        }
    }
}
