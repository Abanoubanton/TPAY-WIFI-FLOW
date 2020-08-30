using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;


namespace WebApplication1.Pages
{
    public class Welcome : PageModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        private readonly ILogger<ErrorModel> _logger;

        public Welcome(ILogger<ErrorModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }

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

        // Cancel existing subscription contract (Directly from service page)
        public IActionResult OnPostCancelSubscriptionContract()
        {
            var urll = Request.Headers["referer"];
            Uri uri = new Uri(urll);
            string subscriptionContractIdd = HttpUtility.ParseQueryString(uri.Query).Get("subscriptionContractID");
            int subscriptionContractId = int.Parse(subscriptionContractIdd);

            CancelSubscriptionContractRequest CancelSubscriptionContract = new CancelSubscriptionContractRequest
            {
                signature = "",
                subscriptionContractId = subscriptionContractId
            };
            var digest = CalculateDigest(publickey, privatekey, CancelSubscriptionContract.message());

            var client = new RestClient("http://staging.tpay.me/api/TPAYSubscription.svc/json/CancelSubscriptionContractRequest");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", "{\r\n    \"signature\": \"" + digest.ToString() + "\",\r\n    " +
                "\"subscriptionContractId\": " + CancelSubscriptionContract.subscriptionContractId + "\r\n}", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            System.Diagnostics.Trace.WriteLine(response.Content);

            return Page();
        }



        // Cancel existing subscription contract (Using PINCODE SMS)
        public IActionResult OnPostCancelSubscriptionContractpincode()
        {
            var urll = Request.Headers["referer"];
            Uri uri = new Uri(urll);
            string subscriptionContractIdd = HttpUtility.ParseQueryString(uri.Query).Get("subscriptionContractID");
            int subscriptionContractId = int.Parse(subscriptionContractIdd);

            CancelSubscriptionContractRequestpincode CancelSubscriptionContractpincode = new CancelSubscriptionContractRequestpincode
            {
                signature = "",
                subscriptionContractId = subscriptionContractId
            };

            var digest = CalculateDigest(publickey, privatekey, CancelSubscriptionContractpincode.message());

            var client = new RestClient("http://staging.tpay.me/api/TPAYSubscription.svc/json/SendSubscriptionCancellationPinSMS");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", "{\r\n    \"signature\": \"" + digest.ToString() + "\",\r\n    " +
                "\"subscriptionContractId\": " + CancelSubscriptionContractpincode.subscriptionContractId + "\r\n}", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            if (response.ErrorMessage == null)
            {
                var data = (JObject)JsonConvert.DeserializeObject(response.Content.ToString());
                string subscriptionContractID = data["subscriptionContractId"].Value<string>();
                return new RedirectToPageResult("/VerifySubscriptionCancellationPINCode", new { subscriptionContractID });
            }

            return Page();
        }

    }
    
    public class CancelSubscriptionContractRequest
    {
        public string signature { get; set; }
        public int subscriptionContractId { get; set; }
        public string message()
        {
            return subscriptionContractId.ToString();
        }
    }
    public class CancelSubscriptionContractRequestpincode
    {
        public string signature { get; set; }
        public int subscriptionContractId { get; set; }
        public string message()
        {
            return subscriptionContractId.ToString();
        }
    }
}
