using Overland.Models.Enums;

namespace Overland.Models;

public class Customer {
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public PhoneType PhoneType { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Zip { get; set; }
    public string? MonthlyIncome { get; set; }
    public string? EstimatedDebt { get; set; }
    public string? OfferAmount { get; set; }
    public DirectMail? DirectMail { get; set; }

    public string? AccessCode { get; set; }
}