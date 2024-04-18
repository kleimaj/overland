using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using FluentValidation;
using FluentValidation.AspNetCore;
using FormHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MimeKit;
using Newtonsoft.Json;
using PhoneNumbers;
using Overland.Models;
using Overland.Models.Const;
using Overland.Models.Enums;
using Overland.Models.Jsn;
using Overland.Models.Settings;
using Overland.Services;
using RestSharp;

namespace Overland.Controllers;

public class VerifyController : Controller {
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<VerifyController> _logger;
    private readonly IMartenService _martenService;
    private readonly IValidator<Customer> _validator;
    private readonly IWebHostEnvironment _hostingEnvironment;

    public VerifyController(IValidator<Customer> validator, ILogger<VerifyController> logger,
        IMartenService martenService, IOptions<EmailSettings> emailSettings, IWebHostEnvironment hostingEnvironment) {
        _validator = validator;
        _logger = logger;
        _martenService = martenService;
        _emailSettings = emailSettings.Value;
        _hostingEnvironment = hostingEnvironment;
    }

    [HttpGet]
    public IActionResult Index(Guid customerGid, string accessCode) {
        var directMail = new DirectMail();

          var verifyCustomer = new Customer { Id = customerGid };
               if (Guid.Empty == customerGid) {
                   customerGid = Guid.NewGuid();
                   var promocode = new Promocode
                       { Id = customerGid, Code = accessCode ?? "Missing", Referer = "QR" };
                   _martenService.CreatePromocode(promocode);
               }
               else {
                     var promocode = _martenService.GetPromocode(customerGid).Result;
                     if (promocode == null) {
                          promocode = new Promocode
                            { Id = customerGid, Code = accessCode ?? "Missing", Referer = "QR" };
                          _martenService.CreatePromocode(promocode);
                     }
               }

        if (accessCode != "Missing") {
            directMail = _martenService.GetDirectMail(accessCode).Result;
            if (directMail != null)
                verifyCustomer = new Customer {
                    Id = customerGid,
                    Address = directMail.Address,
                    FirstName = directMail.FirstName,
                    LastName = directMail.LastName,
                    City = directMail.City,
                    Zip = directMail.Zip,
                    DirectMail = directMail
                };
        }

        ViewBag.AccessCode = accessCode;
        ViewBag.CustomerGid = customerGid;
        ViewBag.StateId = States.stateList.FindIndex(x => directMail != null && x.Value == directMail.StateCode);

        return View(verifyCustomer);
    }
    
    [HttpGet]
    [Route("/Customer")]
    public IActionResult Index(string PromoCode) {
        var customerGid = Guid.NewGuid();
       return Index(customerGid, PromoCode);
    }


    [HttpGet]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() {
        _logger.LogError("{HttpContextTraceIdentifier}", Activity.Current?.Id ?? HttpContext.TraceIdentifier);
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpPost]
    [FormValidator]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(Customer customer) {
        var result = await _validator.ValidateAsync(customer);
        
        if (!result.IsValid) {
            result.AddToModelState(ModelState);
            return View("Index", customer);
        }

        try {
            var promocode = _martenService.GetPromocode(customer.Id).Result;
            if (promocode == null) {
                promocode = new Promocode { Id = customer.Id, Code = "Missing", Referer = "QR" };
                _martenService.CreatePromocode(promocode);
            }
            var directMail = _martenService.GetDirectMail(promocode.Code).Result;
            if (directMail == null) {
                customer.DirectMail = new DirectMail {
                    AccessCode = "Missing",
                    MailDate = DateTime.MinValue
                };
            }
            else {
                customer.DirectMail = directMail;
            }
            await _martenService.CreateCustomer(customer);
            EmailLoanApplication(customer);
            //EmailCustomer(customer);
            PostCrm(customer);
            
            return FormResult.CreateSuccessResult("Customer saved.",
                Url.Action("", "ThankYou", new { customerGid = customer.Id }), 500);
        }
        catch {
            return FormResult.CreateErrorResult("An error occurred!");
        }
    }
    private void PostCrm(Customer customer) {
        var client = new RestClient("https://hosted.debttrakker.net");
        var request = new RestRequest("/leadimport/debtimport.aspx") {
            Method = Method.Post
        };
        request.RequestFormat = DataFormat.Json;
        
        /*var americorApiUser = AmericorApi.GetAmericorApiUser();
        request.AddJsonBody(americorApiUser);
        var response = client.Execute<Americor.tokenResponse>(request);
        var token = JsonConvert.DeserializeObject<Americor.tokenResponse>(response.Content).token;
        request = new RestRequest("/v1/lead") {
            Method = Method.Post
        };
        _logger.LogInformation("/v1/lead: {token}", response.Content);
        _logger.LogInformation("LeadResponseId: {token}", token);
        AmericorApi.AddStdHeaders(request,token);
        request.RequestFormat = DataFormat.Json;*/
       
                
        /*var lead = new Americor.Lead {
            FirstName = customer.FirstName!,
            LastName = customer.LastName!,
            Email = customer.Email!,
            NoEmail = false,
            State = customer.State!,
            Address = customer.Address!,
            City = customer.City!,
            Zip = customer.Zip.Replace("-",""),
            ClientEstimatedDebt =  Convert.ToDouble(customer.OfferAmount?.Replace("$","").Replace(",","")),
            AccessCode = customer.DirectMail.AccessCode,
            Source = "Mail offer - 1"
        };*/
        
        /*if(customer.PhoneType == PhoneType.Cell) {
            var util = PhoneNumberUtil.GetInstance();
            var number = util.Parse(customer.Phone, "US");
            lead.PhoneMobile = util.Format(number, PhoneNumberFormat.E164);
        }
        else {
            var util = PhoneNumberUtil.GetInstance();
            var number = util.Parse(customer.Phone, "US");
            lead.PhoneHome = util.Format(number, PhoneNumberFormat.E164);
        }*/
        request.AddJsonBody(new
        {
            pSID = "29e2ea6c-bd85-4f42-8c7e-658125c3757b",
            pOID = "UU", pVendorID = customer.Id, pFname = customer.FirstName, pLname = customer.LastName, pEmailAddress = customer.Email,
            pAddress = customer.Address, pHomePhone = customer.Phone,
            pCity = customer.City, pState = customer.State, pZipCode = customer.Zip
        });
        /*request.AddJsonBody(lead);*/
        var response = client.ExecutePost(request);
        /*var leadResponseId = leadResponse.Data.Id;*/
        _logger?.LogInformation("Post to CRM made with context {0}", response.Content);
        /*_logger.LogInformation("LeadResponseId: {leadResponseId}", leadResponseId);*/
        
    }
    private bool EmailLoanApplication(Customer customerResponse) {
       /* var promocode = _martenService.GetPromocode(customerResponse.Id).Result;
        var directMail = _martenService.GetDirectMail(promocode.Code).Result;
        if (directMail == null) {
            directMail = new DirectMail {
                AccessCode = "Missing",
                MailDate = DateTime.MinValue
            };
        }*/
        
        var body = "First Name: " + customerResponse.FirstName;
        body += "\nLast Name: " + customerResponse.LastName;
        body += "\nEmail: " + customerResponse.Email;
        body += "\nAddress: " + customerResponse.Address;
        body += "\nCity: " + customerResponse.City;
        body += "\nState: " + customerResponse.State;
        body += "\nZip Code: " + customerResponse.Zip;
        body += "\nPhone: " + customerResponse.Phone;
        body += "\nType Phone: " + customerResponse.PhoneType;
        body += "\nReservation Code: " + customerResponse.AccessCode;
        body += "\nLoan Amount: " + customerResponse.OfferAmount;
        body += "\nTime(PDT): " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
        if (customerResponse.DirectMail != null) {
            body += "\nMail Date: " + customerResponse.DirectMail.MailDate.ToString("MM/dd/yyyy");
            body += "\n\n------------------------\n";
            body += "Direct Mail Piece \n";
            body += "First Name: " + customerResponse.DirectMail.FirstName;
            body += "\nLast Name: " + customerResponse.DirectMail.LastName;
            body += "\nAddress: " + customerResponse.DirectMail.Address;
            body += "\nCity: " + customerResponse.DirectMail.City;
            body += "\nState: " + customerResponse.DirectMail.StateCode;
            body += "\nZip Code: " + customerResponse.DirectMail.Zip;

        }

        using var message = new MailMessage();

        var emails = _emailSettings.ToEmail.Split(',');
        foreach (var email in emails) {
            message.To.Add(email);
        }
       
        message.From = new MailAddress(_emailSettings.Username);
        message.Subject = _emailSettings.Subject;
        message.Body = body;

        using var smtp = new SmtpClient();
        smtp.EnableSsl = true;

        smtp.Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password);
        smtp.Host = _emailSettings.Host;
        smtp.Port = _emailSettings.Port;

        try {
            smtp.Send(message);
            return true;
        }
        catch (Exception ex) {
            _logger?.LogError("Unable to send email {ex}", ex);
            return false;
        }
    }
    private bool EmailCustomer(Customer customerResponse)
    {
        try
        {
            using (var message = new MimeMessage())
            {
                message.To.Add(MailboxAddress.Parse(customerResponse.Email));
                message.From.Add(new MailboxAddress("Support", _emailSettings.Username));
                message.Subject = "Welcome to Prospifi!";

                string htmlFilePath = Path.Combine(_hostingEnvironment.WebRootPath, "images/email", "index.html");
                string htmlString = System.IO.File.ReadAllText(htmlFilePath);
                string interpolatedString = htmlString.Replace("[Fname]", customerResponse.FirstName);
                message.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = interpolatedString };

                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    try
                    {
                        client.Connect(_emailSettings.Host, _emailSettings.Port,
                            MailKit.Security.SecureSocketOptions.StartTls);
                        client.Authenticate(_emailSettings.Username, _emailSettings.Password);
                        client.Send(message);
                        _logger?.LogInformation("Mailkit Success: Send Welcome Email for {Email}",
                            customerResponse.Email);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Failed sending email");
                        return false;
                    }

                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed sending email");
            return false;
        }
    }

}