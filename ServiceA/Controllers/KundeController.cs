using kundeAPI.Models;
using kundeAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Diagnostics;

namespace kundeAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KundeController : ControllerBase
{
    private readonly ILogger<KundeController> _logger;
    private readonly KundeService _kundeService;
    private int retryCount = 3;
    private readonly TimeSpan delay = TimeSpan.FromSeconds(5);

    public KundeController(ILogger<KundeController> logger, KundeService kundeService)
    {
        _logger = logger;
        _logger.LogDebug(1, "NLog injected into KundeController");
        _kundeService = kundeService;
    }

    [HttpGet]
    public async Task<List<Kunde>> Get() =>
        await _kundeService.GetAsync();

    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<Kunde>> Get(string id)
    {
        var Kunde = await _kundeService.GetAsync(id);

        if (Kunde is null)
        {
            return NotFound();
        }

        return Kunde;
    }

    [HttpPost("")]
    public async Task<IActionResult> Post(Kunde newKunde)
    {
        await _kundeService.CreateAsync(newKunde);

        return CreatedAtAction(nameof(Get), new { id = newKunde.Id }, newKunde);
    }

    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(string id, Kunde updatedKunde)
    {
        var Kunde = await _kundeService.GetAsync(id);

        if (Kunde is null)
        {
            return NotFound();
        }

        updatedKunde.Id = Kunde.Id;

        await _kundeService.UpdateAsync(id, updatedKunde);

        return NoContent();
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var Kunde = await _kundeService.GetAsync(id);

        if (Kunde is null)
        {
            return NotFound();
        }

        await _kundeService.RemoveAsync(id);

        return NoContent();
    }

    [HttpGet("version")]
    public IEnumerable<string> GetVersion()
    {
        var properties = new List<string>();
        var assembly = typeof(Program).Assembly;
        foreach (var attribute in assembly.GetCustomAttributesData())
        {
            properties.Add($"{attribute.AttributeType.Name} - {attribute.ToString()}");
        }
        return properties;
    }

    [HttpPost("henrik")]
    public async Task PostCustomerData()
    {
        int currentRetry = 0;

        for (;;)
        {
            try
            {
            // Call external service.
            await TransientOperationAsync();

            // Return or break.
            break;
            }
            catch (Exception ex)
            {
            Trace.TraceError("Operation Exception");

            currentRetry++;

            // Check if the exception thrown was a transient exception
            // based on the logic in the error detection strategy.
            // Determine whether to retry the operation, as well as how
            // long to wait, based on the retry strategy.
            if (currentRetry > this.retryCount || !IsTransient(ex))
            {
                // If this isn't a transient error or we shouldn't retry,
                // rethrow the exception.
                throw;
            }
            }

            // Wait to retry the operation.
            // Consider calculating an exponential delay here and
            // using a strategy best suited for the operation and fault.
            await Task.Delay(delay);
        }
    }

    private async Task TransientOperationAsync()
    {
        Kunde kunde = new() { Navn = "Henrik" };
        _logger.LogInformation("Gemmer data");
        await Post(kunde);
    }

    private bool IsTransient(Exception ex)
    {
        // Determine if the exception is transient.
        // In some cases this is as simple as checking the exception type, in other
        // cases it might be necessary to inspect other properties of the exception.
        /*if (ex is OperationTransientException)
            return true;*/

        var webException = ex as WebException;
        if (webException != null)
        {
            // If the web exception contains one of the following status values
            // it might be transient.
            return new[] {WebExceptionStatus.ConnectionClosed,
                        WebExceptionStatus.Timeout,
                        WebExceptionStatus.RequestCanceled }.
                    Contains(webException.Status);
        }

        // Additional exception checking logic goes here.
        return false;
    }
}
