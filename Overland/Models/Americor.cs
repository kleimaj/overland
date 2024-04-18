using Newtonsoft.Json;
using RestSharp;

namespace Overland.Models; 

public class Americor {
      public class LeadResponse
    {
        [JsonProperty("id")]
        public long Id { get; set; }
    }
    public class tokenResponse {
        public string token { get; set; }
        public string lifetime { get; set; }
    }
    public class Lead {
        [JsonProperty("firstName")] public string FirstName { get; set; }

        [JsonProperty("lastName")] public string LastName { get; set; }

        [JsonProperty("middleName", NullValueHandling = NullValueHandling.Ignore)]
        public string MiddleName { get; set; }

        [JsonProperty("phoneMobile", NullValueHandling = NullValueHandling.Ignore)]
        public string PhoneMobile { get; set; }

        [JsonProperty("phoneHome", NullValueHandling = NullValueHandling.Ignore)]
        public string PhoneHome { get; set; }

        [JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
        public string Email { get; set; }

        [JsonProperty("noEmail", NullValueHandling = NullValueHandling.Ignore)]
        public bool NoEmail { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("ssn", NullValueHandling = NullValueHandling.Ignore)]
        public string Ssn { get; set; }

        [JsonProperty("address", NullValueHandling = NullValueHandling.Ignore)]
        public string Address { get; set; }

        [JsonProperty("city", NullValueHandling = NullValueHandling.Ignore)]
        public string City { get; set; }

        [JsonProperty("zip", NullValueHandling = NullValueHandling.Ignore)]
        public string Zip { get; set; }

        [JsonProperty("mailingAddress", NullValueHandling = NullValueHandling.Ignore)]
        public string MailingAddress { get; set; }

        [JsonProperty("mailingState", NullValueHandling = NullValueHandling.Ignore)]
        public string MailingState { get; set; }

        [JsonProperty("mailingCity", NullValueHandling = NullValueHandling.Ignore)]
        public string MailingCity { get; set; }

        [JsonProperty("mailingZip", NullValueHandling = NullValueHandling.Ignore)]
        public string MailingZip { get; set; }

        [JsonProperty("externalSalesRepId", NullValueHandling = NullValueHandling.Ignore)]
        public string ExternalSalesRepId { get; set; }

        [JsonProperty("accessCode", NullValueHandling = NullValueHandling.Ignore)]
        public string AccessCode { get; set; }

        [JsonProperty("clientEstimatedDebt", NullValueHandling = NullValueHandling.Ignore)]
        public double ClientEstimatedDebt { get; set; }
        
        [JsonProperty("source", NullValueHandling = NullValueHandling.Ignore)]
        public string Source { get; set; }
    }

 

}
internal static class AmericorApi {
    
    internal static object GetAmericorApiUser() {
        return new { username = "dms_overland_financial_group", publicKey = "Az1KaELTz4g188D6LbwoTnzFspyQNptm" };
    }
    internal static void AddStdHeaders(RestRequest restRequest, string apiToken) {
        restRequest.AddHeader("accept", "application/json");
        restRequest.AddHeader("authorization", $"Bearer {apiToken}");
    }
}