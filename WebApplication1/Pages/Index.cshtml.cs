using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using RestSharp;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace WebApplication1.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }


        //AddSubscriptionContractREquest
        public static string publickey = "1UaZrQae9oaihB9mKeoF";
        public static string privatekey = "tRe6qc3nanziawsBNqkQ";

        public static string CalculateDigest(string publicKey, string privateKey, string message)
        {
            var digest = "";
            var hash = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(privateKey));
            var correctHash = string.Join(string.Empty, hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(message)).Select(b => b.ToString("x2")));
            digest = publicKey + ":" + correctHash;
            return digest;
        }

        public IActionResult OnPostSubscribeRequest(string msisdn, string operatorCode)
        {

            AddSubscriptionContract SubscriptionContract = new AddSubscriptionContract
            {
                signature = "",
                customerAccountNumber = "1",
                msisdn = msisdn.ToString(),
                operatorCode = operatorCode.ToString(),
                subscriptionPlanId = 81912,
                initialPaymentproductId = "Game_cards_etisalat",
                initialPaymentDate = DateTime.UtcNow.ToString("u"),
                executeInitialPaymentNow = false,
                executeRecurringPaymentNow = false,
                recurringPaymentproductId = "Game_cards_etisalat",
                productCatalogName = "Games-Etisalat-Egypt",
                autoRenewContract = true,
                sendVerificationSMS = true,
                allowMultipleFreeStartPeriods = false,
                contractStartDate = DateTime.UtcNow.ToString("u"),
                contractEndDate = DateTime.UtcNow.AddYears(1).ToString("u"),
                language = 1,
                headerEnrichmentReferenceCode = "",
                smsId = ""

            };
            string theDigest = CalculateDigest(publickey, privatekey, SubscriptionContract.Message());

            var client = new RestClient("http://staging.tpay.me/api/TPAYSubscription.svc/json/AddSubscriptionContractRequest");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");


            request.AddParameter("application/json", "{\r\n    \"signature\": \""+theDigest.ToString()+"\",\r\n    \"customerAccountNumber\": \""+SubscriptionContract.customerAccountNumber.ToString()+"\",\r\n    " +
                "\"msisdn\": \""+msisdn.ToString()+"\",\r\n    \"operatorCode\": \""+operatorCode.ToString()+"\",\r\n    \"subscriptionPlanId\": "+SubscriptionContract.subscriptionPlanId+",\r\n    \"initialPaymentproductId\": \""+SubscriptionContract.initialPaymentproductId.ToString()+"\",\r\n    " +
                "\"initialPaymentDate\": \""+SubscriptionContract.initialPaymentDate.ToString()+ "\",\r\n    \"executeInitialPaymentNow\": "+SubscriptionContract.executeInitialPaymentNow.ToString().ToLower()+",\r\n    \"executeRecurringPaymentNow\": "+ SubscriptionContract.executeRecurringPaymentNow.ToString().ToLower() + ",\r\n    " +
                "\"recurringPaymentproductId\": \""+SubscriptionContract.recurringPaymentproductId.ToString()+"\",\r\n    \"productCatalogName\": \""+SubscriptionContract.productCatalogName.ToString()+"\",\r\n    \"autoRenewContract\": "+ SubscriptionContract.autoRenewContract.ToString().ToLower() + ",\r\n    \"sendVerificationSMS\": "+SubscriptionContract.sendVerificationSMS.ToString().ToLower()+",\r\n    " +
                "\"allowMultipleFreeStartPeriods\": "+SubscriptionContract.allowMultipleFreeStartPeriods.ToString().ToLower()+",\r\n    \"contractStartDate\": \""+SubscriptionContract.contractStartDate.ToString()+ "\",\r\n    \"contractEndDate\": \""+SubscriptionContract.contractEndDate.ToString()+"\",\r\n    " +
                "\"language\": "+SubscriptionContract.language+",\r\n    \"headerEnrichmentReferenceCode\": \"\",\r\n    \"smsId\": \"\"\r\n}", ParameterType.RequestBody);
            
            IRestResponse response = client.Execute(request);
            System.Diagnostics.Trace.WriteLine(response.Content);


            if (response.ErrorMessage == null)
            {
                var data = (JObject)JsonConvert.DeserializeObject(response.Content.ToString());
                var operatorcode = operatorCode.ToString();

                string returnsubscriptionContractId = data["subscriptionContractId"].Value<string>();

                string returnmsisdn = data["msisdn"].Value<string>();
                return new RedirectToPageResult("/VerifySubscriptionContract", new {returnsubscriptionContractId , returnmsisdn , operatorcode });

            }

            return Page();
        }


        

        

        

    }
    public class AddSubscriptionContract
    {
        public string signature { get; set; }
        public string customerAccountNumber { get; set; }
        public string msisdn { get; set; }
        public string operatorCode { get; set; }
        public int subscriptionPlanId { get; set; }
        public string initialPaymentproductId { get; set; }
        public string initialPaymentDate { get; set; }
        public bool executeInitialPaymentNow { get; set; }
        public bool executeRecurringPaymentNow { get; set; }
        public string recurringPaymentproductId { get; set; }
        public string productCatalogName { get; set; }
        public bool autoRenewContract { get; set; }
        public bool sendVerificationSMS { get; set; }
        public bool allowMultipleFreeStartPeriods { get; set; }
        public string contractStartDate { get; set; }
        public string contractEndDate { get; set; }
        public int language { get; set; }
        public string headerEnrichmentReferenceCode { get; set; }
        public string smsId { get; set; }
        public string Message()
        {
            return customerAccountNumber + msisdn + operatorCode + subscriptionPlanId +
            initialPaymentproductId + initialPaymentDate + executeInitialPaymentNow.ToString().ToLower() +
            recurringPaymentproductId + productCatalogName +
            executeRecurringPaymentNow.ToString().ToLower() + contractStartDate + contractEndDate +
            autoRenewContract.ToString().ToLower() +
            language.ToString() + sendVerificationSMS.ToString().ToLower() +
            allowMultipleFreeStartPeriods.ToString().ToLower() + headerEnrichmentReferenceCode + smsId;
        }


    }
}
