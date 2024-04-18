using Overland.Models;

namespace Overland.Services;

public interface IMartenService {
    internal Task<bool> CreatePromocode(Promocode promocode);
    internal Task<Promocode?> GetPromocode(Guid guid);
    internal Task<bool> CreateCustomer(Customer customer);
    internal Task<bool> CreateCustomerLoanInfo(CustomerLoanInfo customerLoanInfo);
    internal Task<bool> CreateDirectMails(List<DirectMail> directMails);
    internal Task<DirectMail?> GetDirectMail(string Promocode);
    internal Task<bool> CreateDirectMail(DirectMail directMail);

}