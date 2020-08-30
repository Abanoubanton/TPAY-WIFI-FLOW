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
using System.Web;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace WebApplication1.Pages
{
    public class VerifySubscriptionCancellationPINCode : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public VerifySubscriptionCancellationPINCode(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
        //verify subscription cancell PINCODE
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

        public IActionResult OnPostVerifyCancellation(string verifycode)
        {
            var urll = Request.Headers["referer"];
            Uri uri = new Uri(urll);
            string subscriptionContractIdd = HttpUtility.ParseQueryString(uri.Query).Get("subscriptionContractID");
            int subscriptionContractId = int.Parse(subscriptionContractIdd);

            VerifyCancellation Verifycancell = new VerifyCancellation
            {
                signature = "",
                subscriptionContractId = subscriptionContractId,
                pinCode = verifycode.ToString()
            };
            var digest = CalculateDigest(publickey, privatekey, Verifycancell.message());


            var client = new RestClient("http://staging.tpay.me/api/TPAYSubscription.svc/json/VerifySubscriptionCancellationPin");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");

            request.AddParameter("application/json", "{\r\n    \"signature\": \""+ digest.ToString()+"\",\r\n    " +
                "\"subscriptionContractId\": "+Verifycancell.subscriptionContractId+",\r\n    \"pinCode\": \""+ Verifycancell.pinCode+ "\"\r\n}", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            System.Diagnostics.Trace.WriteLine(response.Content);

            if (response.ErrorMessage == null)
            {
                return new RedirectToPageResult("/IndexModel");
            }

            return Page();
        }

    }
    public class VerifyCancellation
    {
        public string signature { get; set; }
        public int subscriptionContractId { get; set; }
        public string pinCode { get; set; }
        public string message()
        {
            return subscriptionContractId.ToString() + pinCode;
            ;
        }
    }
}
