using Microsoft.AspNetCore.Mvc;

namespace LighthouseKeeper.Controllers;

[ApiController]
[Route("")]
public class CommandsController : ControllerBase
{
    private readonly BtWatcher _btWatcher;
    
    public CommandsController(BtWatcher watcher) => _btWatcher = watcher;
    
    [HttpGet("off")]
    public async Task<ActionResult> GetOff()
    {
        foreach (var lighthouse in _btWatcher.Lighthouses)
        {
            await lighthouse.ConnectAsync();
            await lighthouse.PowerOffAsync();
            lighthouse.Disconnect();
        }

        return Ok();
    }
    
    [HttpGet("on")]
    public async Task<ActionResult> GetOn()
    {
        foreach (var lighthouse in _btWatcher.Lighthouses)
        {
            await lighthouse.ConnectAsync();
            await lighthouse.PowerOnAsync();
            lighthouse.Disconnect();
        }

        return Ok();
    }
}