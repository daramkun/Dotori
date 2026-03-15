using Dotori.Registry.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Dotori.Registry.Api;

[ApiController]
[Route("/")]
public sealed class RegistryInfoController(IConfiguration config, OAuthProviderRegistry providers) : ControllerBase
{
    [HttpGet]
    public IActionResult GetInfo() => Ok(new
    {
        name = "dotori-registry",
        version = "1.0.0",
        mode = config["Registry:Mode"] ?? "standalone",
        providers = providers.EnabledProviders.ToList(),
        apiVersion = "v1",
    });
}
