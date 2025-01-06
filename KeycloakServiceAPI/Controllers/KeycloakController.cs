using KeycloakServiceAPI.Models;
using KeycloakServiceAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace KeycloakServiceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KeycloakController : ControllerBase
{
    private readonly ServiceBusPublisher _serviceBusPublisher;

    public KeycloakController(ServiceBusPublisher serviceBusPublisher)
    {
        _serviceBusPublisher = serviceBusPublisher;
    }

    [HttpPost("create-realm-user")]
    public async Task<IActionResult> CreateRealmAndUser([FromBody] KeycloakActionRequest request)
    {
        if (string.IsNullOrEmpty(request.RealmName) || string.IsNullOrEmpty(request.TenantEmail))
        {
            return BadRequest("Realm name and tenant email are required.");
        }

        var message = new KeycloakActionMessage
        {
            Action = "CreateRealmAndUser",
            Data = new KeycloakActionData
            {
                RealmName = request.RealmName,
                TenantEmail = request.TenantEmail
            }
        };

        await _serviceBusPublisher.PublishMessageAsync("keycloak-actions", message);
        return Ok($"Message for creating realm '{request.RealmName}' and user '{request.TenantEmail}' sent successfully.");
    }
}
