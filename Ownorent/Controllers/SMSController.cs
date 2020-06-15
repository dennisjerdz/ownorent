using GlobeLabs.Api;
using Ownorent.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Ownorent.Controllers
{
    public class SMSController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        
        public ActionResult Register(string access_token, string subscriber_number)
        {
            Trace.TraceInformation("Received! Parameters: "+access_token+", "+subscriber_number);

            string subscriber_number_p = "0" + subscriber_number;

            var user = db.Users.FirstOrDefault(u => u.MobileNumber == subscriber_number_p);
            
            if (user != null)
            {
                user.MobileNumberCode = access_token;
                db.SaveChanges();

                Sms sms = new Sms(OwnorentHelper.ShortCode, access_token);

                dynamic response = sms
                    .SetReceiverAddress("+63" + subscriber_number)
                    .SetMessage("Hello, " + user.FirstName + ", your Mobile Number has been verified. You will now receive both SMS and Email notifications.")
                    .SendMessage()
                    .GetDynamicResponse();

                Trace.TraceInformation(subscriber_number);
            }
            else
            {
                Trace.TraceInformation("Mobile Number doesn't exist.");
            }

            return new HttpStatusCodeResult(201);
        }

        public ActionResult Receive()
        {
            return new HttpStatusCodeResult(201);
        }
    }
}