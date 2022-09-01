using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;

namespace Shorten.API.Controllers;

[ApiController]
[Route("[controller]")]
public class ShortenController : ControllerBase
{
    private static readonly IDictionary<string, string> _data = new ConcurrentDictionary<string, string>();
    private readonly ILogger<ShortenController> _logger;

    public ShortenController(ILogger<ShortenController> logger)
    {
        _logger = logger;
    }

    [HttpPost(Name = "CreateShorten")]
    public IActionResult CreateShorten([FromBody] ShortenViewModel viewModel) => _data.TryAdd(viewModel.Alias, viewModel.Link) 
        ? CreatedAtAction(nameof(GetShorten), new { alias = viewModel.Alias }, viewModel)
        : Conflict();

    [Cache(duration: 30, argument: "alias")]
    [HttpGet("{alias}", Name = "GetShorten")]
    public IActionResult GetShorten(string alias)
    {
        _logger.LogInformation("Resolving Shorten \"{alias}\"", alias);
        return _data.TryGetValue(alias, out var link) ? Redirect(link) : NotFound();
    }
}

public class ShortenViewModel
{
    [Required]
    public string Alias { get; set; }

    [Required, Url]
    public string Link { get; set; }
}
