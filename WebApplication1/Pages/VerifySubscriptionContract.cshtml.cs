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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NPoco.Expressions;
using Polly;

namespace WebApplication1.Pages
{
    public class VerifySubscriptionContract : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public VerifySubscriptionContract(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }

        //VerifySubscriptionContract
        public static string CalculateDigest(string publicKey, string privateKey, string message)
        {
            var digest = "";
            var hash = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(privateKey));
            var correctHash = string.Join(string.Empty, hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(message)).Select(b => b.ToString("x2")));
            digest = publicKey + ":" + correctHash;
            return digest;
        }

        public static string publickey = "1UaZrQae9oaihB9mKeoF";
        public static string privatekey = "tRe6qc3nanziawsBNqkQ";

        public IActionResult OnPostVerifyContract(string verifycode)
        {

            var urll = Request.Headers["referer"];
            Uri uri = new Uri(urll);
            string subscriptionContractIdd = HttpUtility.ParseQueryString(uri.Query).Get("returnsubscriptionContractId");
            int subscriptionContractId = int.Parse(subscriptionContractIdd);
            VerifyContract Verify = new VerifyContract
            {
                signature = "",
                pinCode = verifycode.ToString(),
                subscriptionContractId = subscriptionContractId

            };
            var digestverify = CalculateDigest(publickey, privatekey, Verify.Message());

            var client = new RestClient("http://staging.TPAY.me/api/TPAYSubscription.svc/Json/VerifySubscriptionContract");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");

            request.AddParameter("application/json", "{\r\n    \"signature\": \"" + digestverify.ToString() + "\",\r\n    " +
                "\"pinCode\": \"" + Verify.pinCode + "\",\r\n    \"subscriptionContractId\": " + Verify.subscriptionContractId + "\r\n}", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            System.Diagnostics.Trace.WriteLine(response.Content);
            var dataresponseCode = (JObject)JsonConvert.DeserializeObject(response.Content.ToString());
            string responseCode = dataresponseCode["responseCode"].Value<string>();

            var data = (JObject)JsonConvert.DeserializeObject(response.Content.ToString());
            string subscriptionContractID = data["subscriptionContractId"].Value<string>();

           


            if (responseCode == 0.ToString()) //means the user enter the correct PINCODE
            {
                OnPostSendFreeMTmessage();         //Fire Our SubscriptionWelcomeMessage to the user
                return new RedirectToPageResult("/Welcome", new { subscriptionContractID });
            }
            else if (responseCode == 51.ToString())
            {
                    TempData["Message"] = "Sorry we have encountered a problem from our Side please try again later. Or contact support team at example@support.com";
            }
            else if (responseCode == 302.ToString())
            {
                string returnsubscriptionContractId = HttpUtility.ParseQueryString(uri.Query).Get("returnsubscriptionContractId");

                TempData["Message"] = "Sorry, the Pin Code is invalid";
                return new RedirectToPageResult("/VerifySubscriptionContract", new { returnsubscriptionContractId,  });

            }
            else if (responseCode == 305.ToString())
            {
                TempData["Message"] = "Sorry, you have exceeded the number of attempts please try again in few minutes";
            }
            return Page();
            
        }

        private void IndexModel()
        {
            throw new NotImplementedException();
        }

        // Resend Verification PIN Code
        public IActionResult OnPostResendVerification()
        {
            var urll = Request.Headers["referer"];
            Uri uri = new Uri(urll);
            string subscriptionContractIdd = HttpUtility.ParseQueryString(uri.Query).Get("returnsubscriptionContractId");
            int subscriptionContractId = int.Parse(subscriptionContractIdd);

            ResendVerificationPIN ResendVerification = new ResendVerificationPIN
            {
                signature = "",
                subscriptionContractId = subscriptionContractId
             };

            var digestsms = CalculateDigest(publickey, privatekey, ResendVerification.messagesms());

            
        var client = new RestClient("http://staging.TPAY.me/api/TPAYSubscription.svc/Json/SendSubscriptionContractVerificationSMS");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", "{\r\n    \"signature\": \"" + digestsms.ToString() + "\",\r\n    " +
                "\"subscriptionContractId\": " + ResendVerification.subscriptionContractId.ToString() + "\r\n}", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            System.Diagnostics.Trace.WriteLine(response.Content);

            return Page();
        }
        // Send Free MT message
        public IActionResult OnPostSendFreeMTmessage()
        {
            var urll = Request.Headers["referer"];
            Uri uri = new Uri(urll);
            string msisdn = HttpUtility.ParseQueryString(uri.Query).Get("returnmsisdn");
            string operatorcode = HttpUtility.ParseQueryString(uri.Query).Get("operatorcode");

            SendFreeMTmessage SendFreeMT = new SendFreeMTmessage
            {

                signature = "",
                messageBody = "Welcome to our platform",
                msisdn = msisdn,
                operatorCode = operatorcode

            };
            var digest = CalculateDigest(publickey, privatekey, SendFreeMT.freemessage());

            var client = new RestClient("http://staging.TPAY.me/api/TPAY.svc/json/SendFreeMTMessage");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", "{\r\n    \"signature\": \"" + digest.ToString() + "\",\r\n    " +
                "\"messageBody\": \"" + SendFreeMT.messageBody.ToString() + "\",\r\n    \"msisdn\": \"" + SendFreeMT.msisdn.ToString() + "\",\r\n    \"operatorCode\": \"" + SendFreeMT.operatorCode.ToString() + "\"\r\n}",
                ParameterType.RequestBody);

            IRestResponse response = client.Execute(request);
            System.Diagnostics.Trace.WriteLine(response.Content);

            return Page();
        }



    }
    public class VerifyContract
    {
        public string signature { get; set; }
        public string pinCode { get; set; }
        public int subscriptionContractId { get; set; }
        public string Message()
        {
            return subscriptionContractId.ToString() + pinCode;
        }
    }

    public class ResendVerificationPIN
    {
        public string signature { get; set; }
        public int subscriptionContractId { get; set; }

        public string messagesms()
        {
            return subscriptionContractId.ToString();
        }
    }

    public class SendFreeMTmessage
    {
        public string signature { get; set; }
        public string messageBody { get; set; }
        public string msisdn { get; set; }
        public string operatorCode { get; set; }
        public string freemessage()
        {
            return messageBody + msisdn + operatorCode;
        }

    }
}
