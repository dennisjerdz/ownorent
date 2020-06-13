using Newtonsoft.Json;
using Ownorent.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Ownorent
{
    public static class OwnorentHelper
    {
        /*  Paypal App and Accounts
            Paypal Login
            ownorent@gmail.com // Ownorent@123

            Paypal Sandbox Accounts
            ownorent-customer@gmail.com // customer@123
            ownorent-seller@gmail.com // seller@123
            
            Sandbox App Credentials
            APP ID: AUCuk2p4EuTkCYzgtGr8RoAU8UquesIKb9_SGpoMLbHuxx4boSGXX7GCtR1B9cEtgQlIp75IBnD9ex0B
            SECRET: EP8uJsRnr5oAv7LHdlc_Dlrev-D76R6CfxzmtVg5soCM4cC_1QDL348Rst92hMmI5WbHR1RAq7ECevvh

            Cards
            Card No: 4032032774064916
            Card Type: VISA
            Expiration Date: 07/2025

            Paypal Return URL (Paypal will redirect here w/ the parameters after successful payment)
            http://localhost:54620/?token=5GC374179T785545R&PayerID=VB57ANGC3H3YW
        */

        /* Paypal Sample Order Request / Http Request Body
            
            {
	            "intent":"CAPTURE",
	            "application_context":{
		            "brand_name":"Ownorent PH",	
		            "landing_page":"BILLING",
		            "user_action":"PAY_NOW",
		            "return_url":"http://localhost:54620/"
	            },
	            "purchase_units": [
	                {
	    	            "amount": {
		                    "currency_code": "PHP",
		                    "value": "1507999.5",
		                    "breakdown":{
		        	            "item_total":{
		        		            "currency_code":"PHP",
		        		            "value":"1507999.5"
		        	            }
		                    }
	    	            },
	    	            "items":[
				            {
		    		            "name":"Toyota CHR",
		    		            "quantity":"1",
		    		            "sku":"1",
		    		            "unit_amount":{
		    			            "currency_code":"PHP",
		    			            "value":"1479999.75"
		    		            }
		    	            },
		    	            {
		    		            "name":"i9 9900ks",
		    		            "quantity":"1",
		    		            "sku":"2",
		    		            "unit_amount":{
		    			            "currency_code":"PHP",
		    			            "value":"27999.75"
		    		            }
		    	            }
			            ]
		            }
	            ]
            }
        */

        public static string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // Paypal doesn't need access token IF you pass basic authentication header for every request
        public static async Task<string> GetAccessToken()
        {
            using (var client = new HttpClient())
            {
                // force SSL, since url we're requesting from is HTTPS, need to have SSL certificate
                if (ServicePointManager.SecurityProtocol != SecurityProtocolType.Tls12)
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                }

                var uri = new Uri("https://api.sandbox.paypal.com/v1/oauth2/token");

                string username = "AUCuk2p4EuTkCYzgtGr8RoAU8UquesIKb9_SGpoMLbHuxx4boSGXX7GCtR1B9cEtgQlIp75IBnD9ex0B";
                string password = "EP8uJsRnr5oAv7LHdlc_Dlrev-D76R6CfxzmtVg5soCM4cC_1QDL348Rst92hMmI5WbHR1RAq7ECevvh";
                var authToken = Encoding.ASCII.GetBytes($"{username}:{password}");

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                        Convert.ToBase64String(authToken));

                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials")
                });

                var result = await client.PostAsync(uri, content);
                string resultContent = await result.Content.ReadAsStringAsync();
                var getAccessToken = JsonConvert.DeserializeObject<PaypalAccessTokenModel>(resultContent);
                return getAccessToken.access_token;
            }
        }
        
        private const string AppId = "AUCuk2p4EuTkCYzgtGr8RoAU8UquesIKb9_SGpoMLbHuxx4boSGXX7GCtR1B9cEtgQlIp75IBnD9ex0B";
        private const string Secret = "EP8uJsRnr5oAv7LHdlc_Dlrev-D76R6CfxzmtVg5soCM4cC_1QDL348Rst92hMmI5WbHR1RAq7ECevvh";
        private static byte[] BasicAuthenticationHeaderByte {
            get
            {
                return Encoding.ASCII.GetBytes($"{AppId}:{Secret}");
            }
        }

        // Use this on; client.DefaultRequestHeaders.Authorization = this property
        public static AuthenticationHeaderValue BasicAuthenticationHeader {
            get {
                return new AuthenticationHeaderValue("Basic", Convert.ToBase64String(BasicAuthenticationHeaderByte));
            }    
        }
    }
}