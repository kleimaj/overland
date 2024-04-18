using CsvHelper.Configuration.Attributes;
using Marten.Schema;

namespace Overland.Models;

public class DirectMail {
    [Identity]
    [Name("offer_code")]
    public string AccessCode { get; set; }
    [Name("con_first_name")]
    public string FirstName { get; set; }
    [Name("con_last_name")]
    public string LastName { get; set; }
    [Ignore]
    public string Email { get; set; }
    [Ignore]
    public string PrimaryPhone { get; set; }
    [Name("assembled_address")]
    public string Address { get; set; }
    [Name("city_name")]
    public string City { get; set; }
    [Name("state_cd")]
    public string StateCode { get; set; }
    [Name("zip_cd")]
    public string Zip { get; set; }
    [Name("dms_maildate")]
    public DateTime MailDate { get; set; }
    [Name("exp_date")]
    public DateTime? Exp_Date { get; set; }
    [Name("fileName")]
    public string? fileName { get; set; }
    [Name("pkgName")]
    public string? pkgName { get; set; }
    [Name("remailFlag")]
    public bool? remailFlag { get; set; }
}
