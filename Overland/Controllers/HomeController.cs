using System.Diagnostics;
using JSNLog;
using Microsoft.AspNetCore.Mvc;
using Overland.Models;
using Overland.Models.Jsn;
using Overland.Services;

namespace Overland.Controllers;

public class HomeController : Controller {
    private readonly ILogger<HomeController> _logger;
    private readonly IMartenService _martenService;

    public HomeController(ILogger<HomeController> logger, IMartenService martenService) {
        _logger = logger;
        _martenService = martenService;
    }

    [HttpGet]
    public IActionResult Index() {
        return View();
    }


    [HttpPost]
    [Route("Log")]
    public void JsnLogger([FromBody] JsnLogMessage obj) {
        foreach (var jsnError in obj.Payload) {
            var jsnErrorMessage = jsnError.Message.Replace("\r", "").Replace("\n", "");
            switch (jsnError.Level) {
                case (int)Level.TRACE:
                    _logger.LogTrace("{@LogMessage}", jsnErrorMessage);
                    break;
                case (int)Level.DEBUG:
                    _logger.LogDebug("{@LogMessage}", jsnErrorMessage);
                    break;
                case (int)Level.INFO:
                    _logger.LogInformation("{@LogMessage}", jsnErrorMessage);
                    break;
                case (int)Level.WARN:
                    _logger.LogWarning("{@LogMessage}", jsnErrorMessage);
                    break;
                case (int)Level.ERROR:
                    _logger.LogError("{@LogMessage}", jsnErrorMessage);
                    break;
                case (int)Level.FATAL:
                    _logger.LogCritical("{@LogMessage}", jsnErrorMessage);
                    break;
            }
        }
    }


    [HttpGet]
    public async Task<IActionResult> SubmitCode(string? accessCode = "Missing", string? referer = null) {
        var customerGid = Guid.NewGuid();

        var promocode = new Promocode
            { Id = customerGid, Code = accessCode ?? "Missing", Referer = referer ?? "Missing" };
        await _martenService.CreatePromocode(promocode);

        return RedirectToAction("", "Verify", new { customerGid, accessCode});
    }

    [HttpGet]
    public async Task<bool> CheckCode(string? accessCode) {
        if (accessCode == null) {
            return true; //allow blank code
        }
        var directMail = await _martenService.GetDirectMail(accessCode);
        if (directMail != null) {
            return true;
        }
        else {
            return false;
        }
    }
    
    [HttpGet]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() {
        _logger.LogError("{HttpContextTraceIdentifier}", Activity.Current?.Id ?? HttpContext.TraceIdentifier);
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}