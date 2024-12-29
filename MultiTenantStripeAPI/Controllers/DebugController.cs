using Microsoft.AspNetCore.Mvc;
using MultiTenantStripeAPI.Data;
using MultiTenantStripeAPI.Models;
using System;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class DebugController : ControllerBase
{
    private readonly TenantDbContext _context;

    public DebugController(TenantDbContext context)
    {
        _context = context;
    }

    // GET: api/debug/tenants
    [HttpGet("tenants")]
    public IActionResult GetTenants()
    {
        try
        {
            var tenants = _context.Tenants.ToList();
            return Ok(tenants);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // GET: api/debug/tenants/{id}
    [HttpGet("tenants/{id}")]
    public IActionResult GetTenantById(int id)
    {
        try
        {
            var tenant = _context.Tenants.FirstOrDefault(t => t.Id == id);
            if (tenant == null)
                return NotFound($"Tenant with ID {id} not found.");

            return Ok(tenant);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // POST: api/debug/tenants
    [HttpPost("tenants")]
    public IActionResult CreateTenant([FromBody] Tenant tenant)
    {
        try
        {
            if (tenant == null)
                return BadRequest("Tenant object is null.");

            _context.Tenants.Add(tenant);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetTenantById), new { id = tenant.Id }, tenant);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // PUT: api/debug/tenants/{id}
    [HttpPut("tenants/{id}")]
    public IActionResult UpdateTenant(int id, [FromBody] Tenant updatedTenant)
    {
        try
        {
            var tenant = _context.Tenants.FirstOrDefault(t => t.Id == id);
            if (tenant == null)
                return NotFound($"Tenant with ID {id} not found.");

            tenant.TenantName = updatedTenant.TenantName;
            tenant.Email = updatedTenant.Email;
            tenant.SubscriptionStatus = updatedTenant.SubscriptionStatus;

            _context.SaveChanges();

            return Ok(tenant);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // DELETE: api/debug/tenants/{id}
    [HttpDelete("tenants/{id}")]
    public IActionResult DeleteTenant(int id)
    {
        try
        {
            var tenant = _context.Tenants.FirstOrDefault(t => t.Id == id);
            if (tenant == null)
                return NotFound($"Tenant with ID {id} not found.");

            _context.Tenants.Remove(tenant);
            _context.SaveChanges();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
