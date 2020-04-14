using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VibExchange.Filters;
using VibExchange.Models;
using WebMatrix.WebData;
using System.Web.Security;
using System.Net.Mail;
using System.Text;
using System.Security.Cryptography;
using System.Configuration;
namespace VibExchange.Controllers
{

    public class OrderController : Controller
    {
        ClientsController client;
        public ActionResult Index()
        {
            return View("GetConsultant");
        }


        [HttpGet]
        public ActionResult getEnquiry()
        {
            List<SelectListItem> CategoryList = new List<SelectListItem>();
            CategoryList.Add(new SelectListItem { Text = "Vibration Analysis", Value = "Vibration" });
            CategoryList.Add(new SelectListItem { Text = "Balancing", Value = "Balancing" });
            //CategoryList.Add(new SelectListItem { Text = "Leak Detection", Value = "Sound" });
            //CategoryList.Add(new SelectListItem { Text = "Thermal Analysis", Value = "Temprature" });
            ViewBag.categoryList = CategoryList;
            return PartialView("_EnquiryPartial");
        }


        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public ActionResult getEnquiry(FormCollection form, buyEnquiry order)
        {
            //string sts = form["status"].ToString();
            string[] user = form["EmailID"].ToString().Split('@');
            string username = Convert.ToString(user[0]);
            try
            {
                using (DBClass context = new DBClass())
                {
                    if (Session["UserName"] != null)
                    {
                        context.AddParameter("@UserName", Session["UserName"].ToString());
                        context.AddParameter("@EnquirySubject", form["EnquirySubject"]);
                        context.AddParameter("@EnquiryCategory", form["CategoryList"]);
                        context.AddParameter("@EnquiryDetail", form["EnquiryDetail"]);
                        context.AddParameter("@Country", form["Country"]);
                        context.AddParameter("@City", form["City"]);
                        context.AddParameter("@State", form["State"]);
                        context.AddParameter("@CreateDate", context.GetDateTime());
                        if (context.ExecuteNonQuery("addEnquiry", CommandType.StoredProcedure) > 0)
                        {
                            return RedirectToAction("showEnquiry", "Order");
                        }
                    }
                    else
                    {
                        DataTable dtCheckUser = context.getData("Select UserName,Email_ID from UserDetail where Email_ID = '" + form["EmailID"].ToString() + "'", CommandType.Text);
                        if (dtCheckUser.Rows.Count > 0)
                        {
                            return RedirectToAction("Login", "Home");
                        }
                        else
                        {
                            context.AddParameter("@Name", form["fullName"]);
                            context.AddParameter("@UserName", username);
                            context.AddParameter("@Password", form["Password"].ToString());
                            context.AddParameter("@EmailID", form["emailID"]);
                            context.AddParameter("@Phone", form["phone"].ToString());
                            context.AddParameter("@Country", form["Country"]);
                            context.AddParameter("@City", form["City"]);
                            context.AddParameter("@State", form["State"]);
                            context.AddParameter("@EnquiryCategory", form["CategoryList"]);
                            context.AddParameter("@EnquiryDetail", form["EnquiryDetail"]);
                            context.AddParameter("@CreateDate", context.GetDateTime());
                            if (context.ExecuteNonQuery("addEnquiryfromHome", CommandType.StoredProcedure) > 0)
                            {
                                WebSecurity.CreateUserAndAccount(username, order.password);
                                WebSecurity.Login(username, order.password);
                                Session["UserRole"] = "Client";
                                Session["UserName"] = username;
                                Session["Department"] = null;
                                Session["IsActiveSession"] = true;
                                Session["ImagePath"] = "";
                                Session["FullName"] = form["fullName"];
                                return RedirectToAction("EditUserProfile", "Home");
                            }
                            return RedirectToAction("Home", "Home");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Home", "Home");
            }
            return RedirectToAction("Home", "Home");
        }

        public ActionResult showEnquiry()
        {
            return View();
        }

        public ActionResult showEnquiry1()
        {
            var Enq_List = Json(Enquiry.getAllEnquiry(Convert.ToString(Session["UserRole"])), JsonRequestBehavior.AllowGet);
            return Enq_List;
        }

        public ActionResult showEnquiryData(Enquiry enq)
        {
            enq.enquiryList = enq.getAllEnquiryData();
            return PartialView("_getEnquiryData", enq.enquiryList);
        }


        //[InitializeSimpleMembership]

        public ActionResult buyEnquiry(string Id)
        {
            try
            {
                using (DBClass context = new DBClass())
                {
                    var uname = Session["UserName"].ToString();
                    SessionClass.Username = uname;
                    context.AddParameter("@Username", Session["UserName"].ToString());
                    DataSet ds = context.ExecuteDataSet("getUserRole", CommandType.StoredProcedure);
                    if (Convert.ToString(ds.Tables[0].Rows[0]["UserRole"]) != "Client")
                    {
                        DataTable dt = new DataTable();
                        context.AddParameter("@UserName", Session["UserName"].ToString());
                        context.AddParameter("@EnquiryID", Convert.ToInt32(Id));
                        dt = context.getData("getExpertBalance", CommandType.StoredProcedure);
                        //int CurrentBalance = Convert.ToInt32(dt.Rows[0]["Current_PointBalance"]);
                        int enquiryCost = Convert.ToInt32(dt.Rows[0]["Enquiry_Cost"]);

                        DataSet dsEmp = context.ExecuteDataSet("Select * from [Emp_Detail] where [LoginID]='" + uname + "'", CommandType.Text);
                        int UserID = (int)dsEmp.Tables[0].Rows[0]["AutoId"];
                        context.ExecuteNonQuery("INSERT INTO [dbo].[Enquiry_Buyer] ([EnquiryID] ,[BuyerID] ,[Buyer_Name] ,[Enquiry_Cost] ,[DateofBuying]) VALUES(" +
                            Convert.ToInt32(Id) + ",'" + UserID + "','" + uname + "'," + enquiryCost + ",'" + DateTime.Now.ToLocalTime().ToString() + "')", CommandType.Text);

                        SendPurchaseMail(uname, Convert.ToInt32(Id), Convert.ToInt32(UserID), enquiryCost);
                        //Payment(enquiryCost.ToString());
                        //return Json(new { status = "Success", message = "hello !" });



                        return RedirectToAction("Payment", new { Amount = enquiryCost, EnquiryId = Id.ToString() });
                    }
                    else
                    {
                        //return View();
                        return Json(new { status = "Client", message = "You are not autherized to buy an enquiry !" });
                    }
                }
            }
            catch (Exception ex)
            {

                return RedirectToAction("Errormessage");
            }
        }

        public ActionResult SendEnquiryDetailToClient(string EnquiryId)
        {
            try
            {
                using (DBClass context = new DBClass())
                {
                    context.AddParameter("@UserName", Session["UserName"].ToString());
                    context.AddParameter("@EnquiryId", Convert.ToInt32(EnquiryId));
                    DataTable dtEnquiry = context.getData("EnquiryDetailByID", CommandType.StoredProcedure);
                    if (dtEnquiry.Rows.Count > 0)
                    {
                        MailMessage msg = new MailMessage();
                        SmtpClient smtp = new SmtpClient();
                        MailAddress from = new MailAddress("support@vibration-service.com");
                        StringBuilder sb = new StringBuilder();
                        msg.IsBodyHtml = true;
                        smtp.Host = "smtp.zoho.com";
                        smtp.Port = 587;
                        msg.To.Add(Session["ConsultantEmailID"].ToString());
                        msg.Bcc.Add("deepakbhat@iadeptmarketing.com");
                        msg.From = from;
                        msg.Subject = "Lead Detail";
                        msg.Body += " <html>";
                        msg.Body += "<body>";
                        msg.Body += "<table class='table' >";

                        msg.Body += "<tr>";
                        msg.Body += "<td>Dear Sir ,Thank you for purchasing lead from Our portal www.vibration-service.com . Your lead details is mentioned below</td>";
                        msg.Body += "<td></td>";
                        msg.Body += "</tr>";

                        msg.Body += "<tr>";
                        msg.Body += "<td>customer Email Address:</td><td>" + dtEnquiry.Rows[0]["EnquiryEmailID"].ToString() + "</td>";
                        msg.Body += "<td></td>";
                        msg.Body += "</tr>";

                        msg.Body += "<tr>";
                        msg.Body += "<td>Contact No:</td><td>" + dtEnquiry.Rows[0]["Phone"].ToString() + "</td>";
                        msg.Body += "<td></td>";
                        msg.Body += "</tr>";

                        msg.Body += "<tr>";
                        msg.Body += "<td>Country:</td><td>" + dtEnquiry.Rows[0]["Country"].ToString() + "</td>";
                        msg.Body += "<td></td>";
                        msg.Body += "</tr>";

                        msg.Body += "<tr>";
                        msg.Body += "<td>City:</td><td>" + dtEnquiry.Rows[0]["City"].ToString() + "</td>";
                        msg.Body += "<td></td>";
                        msg.Body += "</tr>";

                        msg.Body += "<tr>";
                        msg.Body += "<td>State:</td><td>" + dtEnquiry.Rows[0]["State"].ToString() + "</td>";
                        msg.Body += "<td></td>";
                        msg.Body += "</tr>";

                        msg.Body += "<tr>";
                        msg.Body += "<td>Subject:</td><td>" + dtEnquiry.Rows[0]["EnquiryDetail"].ToString() + "</td>";
                        msg.Body += "<td></td>";
                        msg.Body += "</tr>";
                        msg.Body += "</table>";
                        msg.Body += "</body>";
                        msg.Body += "</html>";
                        smtp.UseDefaultCredentials = false;
                        smtp.EnableSsl = true;
                        smtp.Credentials = new System.Net.NetworkCredential("support@vibration-service.com", "Pass@123");
                        smtp.Send(msg);
                        msg.Dispose();
                    }

                }
            }
            catch (Exception ex) { throw ex; }

            return RedirectToAction("Home", "Home");
        }


        public ActionResult BoughtEnquiry(string Id)
        {
            using (DBClass context = new DBClass())
            {
                var uname = Session["UserName"].ToString();
                SessionClass.Username = uname;
                context.AddParameter("@Username", Session["UserName"].ToString());
                DataSet ds = context.ExecuteDataSet("getUserRole", CommandType.StoredProcedure);
                if (Convert.ToString(ds.Tables[0].Rows[0]["UserRole"]) != "Client")
                {
                    DataTable dt = new DataTable();
                    context.AddParameter("@UserName", Session["UserName"].ToString());
                    context.AddParameter("@EnquiryID", Convert.ToInt32(Id));
                    dt = context.getData("getExpertBalance", CommandType.StoredProcedure);
                    //int CurrentBalance = Convert.ToInt32(dt.Rows[0]["Current_PointBalance"]);
                    int enquiryCost = Convert.ToInt32(dt.Rows[0]["Enquiry_Cost"]);


                    return Json(new { status = "Success", message = "hello !" });

                }
                else
                {
                    //return View();
                    return Json(new { status = "Client", message = "You are not autherized to buy an enquiry !" });
                }
            }
        }


        public ActionResult Payment(string Amount, string EnquiryId)
        {
            //bool sts = false;
            //string firstName = Session["UserName"].ToString();
            string firstName = SessionClass.Username;
            string amount = Amount;
            string productInfo = Convert.ToString(1);
            string email = "";
            string phone = "";
            RemotePost myremotepost = new RemotePost();
            string key = "HKi5X9";
            string salt = "hUtCMrYk";
            myremotepost.Url = "https://secure.payu.in/_payment";
            myremotepost.Add("key", "HKi5X9");
            string txnid = Generatetxnid();
            myremotepost.Add("txnid", txnid);
            myremotepost.Add("amount", amount);
            myremotepost.Add("productinfo", productInfo);
            myremotepost.Add("firstname", firstName);
            myremotepost.Add("phone", phone);
            myremotepost.Add("email", email);
            myremotepost.Add("surl", "http://www.vibration-service.com/Order/SendEnquiryDetailToClient?EnquiryId=" + EnquiryId);//Change the success url here depending upon the port number of your local system.
            myremotepost.Add("furl", "http://www.vibration-service.com/Order/fail");//Change the failure url here depending upon the port number of your local system.
            myremotepost.Add("service_provider", "payu_paisa");
            string hashString = key + "|" + txnid + "|" + amount + "|" + productInfo + "|" + firstName + "|" + email + "|||||||||||" + salt;
            string hash = Generatehash512(hashString);
            myremotepost.Add("hash", hash);
            myremotepost.Post();
            return Json(new { status = "Success", message = "Success" }, JsonRequestBehavior.AllowGet);
        }


        //////////This method for convert all type of currency///////////////////////////

        public string[] CurrencyConvert(string Amount, string convertFrom, string ConvertTo)
        {
            string[] finalData = new string[2];
            //string convertRate = getConversionRate(convertFrom, ConvertTo);


            return finalData;
        }

        [HttpPost]
        public ActionResult Return(FormCollection form)
        {
            try
            {
                string[] merc_hash_vars_seq;
                string merc_hash_string = string.Empty;
                string merc_hash = string.Empty;
                string order_id = string.Empty;
                string hash_seq = "key|txnid|amount|productinfo|firstname|email|udf1|udf2|udf3|udf4|udf5|udf6|udf7|udf8|udf9|udf10";
                if (form["status"].ToString() == "success")
                {
                    merc_hash_vars_seq = hash_seq.Split('|');
                    Array.Reverse(merc_hash_vars_seq);
                    merc_hash_string = ConfigurationManager.AppSettings["SALT"] + "|" + form["status"].ToString();
                    foreach (string merc_hash_var in merc_hash_vars_seq)
                    {
                        merc_hash_string += "|";
                        merc_hash_string = merc_hash_string + (form[merc_hash_var] != null ? form[merc_hash_var] : "");
                    }
                    Response.Write(merc_hash_string);
                    merc_hash = Generatehash512(merc_hash_string).ToLower();
                    if (merc_hash != form["hash"])
                    {
                        using (DBClass context = new DBClass())
                        {
                            int i = context.ExecuteNonQuery("Update PaymentDetail set Status = 'True' Where FileID =" + form["productinfo"].ToString() + " ", CommandType.Text);
                        }
                        return RedirectToAction("showEnquiry", "Order");

                    }
                    else
                    {
                        order_id = Request.Form["txnid"];
                        using (DBClass context = new DBClass())
                        {
                            int i = context.ExecuteNonQuery("Update PaymentDetail set Status = 'True' Where FileID =" + form["productinfo"].ToString() + " ", CommandType.Text);
                        }
                        TempData["Error"] = "Your Payment has been done successfully.";
                        return RedirectToAction("showEnquiry", "Order");
                    }
                }
                else
                {
                    TempData["Error"] = "Your Payment has not been done. Please try again !";
                    return RedirectToAction("showEnquiry", "Order");
                }
            }
            catch
            {
                // return RedirectToAction("ClientList", "Clients");
            }

            return RedirectToAction("showEnquiry", "Order");
        }

        public class RemotePost
        {
            private System.Collections.Specialized.NameValueCollection Inputs = new System.Collections.Specialized.NameValueCollection();
            public string Url = "";
            public string Method = "post";
            public string FormName = "form1";

            public void Add(string name, string value)
            {
                Inputs.Add(name, value);
            }

            public void Post()
            {
                System.Web.HttpContext.Current.Response.Clear();

                System.Web.HttpContext.Current.Response.Write("<html><head>");

                System.Web.HttpContext.Current.Response.Write(string.Format("</head><body onload=\"document.{0}.submit()\">", FormName));
                System.Web.HttpContext.Current.Response.Write(string.Format("<form name=\"{0}\" method=\"{1}\" action=\"{2}\" >", FormName, Method, Url));
                for (int i = 0; i < Inputs.Keys.Count; i++)
                {
                    System.Web.HttpContext.Current.Response.Write(string.Format("<input name=\"{0}\" type=\"hidden\" value=\"{1}\">", Inputs.Keys[i], Inputs[Inputs.Keys[i]]));
                }
                System.Web.HttpContext.Current.Response.Write("</form>");
                System.Web.HttpContext.Current.Response.Write("</body></html>");

                System.Web.HttpContext.Current.Response.End();
            }
        }

        public string Generatehash512(string text)
        {

            byte[] message = Encoding.UTF8.GetBytes(text);

            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] hashValue;
            SHA512Managed hashString = new SHA512Managed();
            string hex = "";
            hashValue = hashString.ComputeHash(message);
            foreach (byte x in hashValue)
            {
                hex += String.Format("{0:x2}", x);
            }
            return hex;
        }


        public string Generatetxnid()
        {
            Random rnd = new Random();
            string strHash = Generatehash512(rnd.ToString() + DateTime.Now);
            string txnid1 = strHash.ToString().Substring(0, 20);

            return txnid1;
        }

        public ActionResult SomeAction()
        {
            // ...
            return Json(
                new { Message = "Success Message!" },
                JsonRequestBehavior.AllowGet
            );
        }



        public ActionResult getEnquiryStatus(string Id)
        {
            using (DBClass context = new DBClass())
            {
                context.AddParameter("@EnquiryID", Id);
                context.AddParameter("@UserName", Session["UserName"].ToString());
                DataTable dt = context.getData("getEnquiryStatus", CommandType.StoredProcedure);
                if (dt.Rows.Count > 0)
                {
                    return Json(new { sts = "True", message = "True" });
                }
                else
                {
                    return Json(new { sts = "False", message = "False" });
                }
            }

        }

        public ActionResult showEnquiryDetail(string EnquiryID)
        {
            Enquiry enquiry = new Enquiry();
            using (DBClass context = new DBClass())
            {
                context.AddParameter("@EnquiryID", EnquiryID);
                DataTable dt = context.getData("getEnquiryDetailByID", CommandType.StoredProcedure);
                if (dt.Rows.Count > 0)
                {

                    enquiry.UserName = Convert.ToString(dt.Rows[0]["uName"]);
                    enquiry.company = Convert.ToString(dt.Rows[0]["uCompanyName"]);
                    enquiry.phone = Convert.ToString(dt.Rows[0]["uMobile_No"]);
                    enquiry.emailid = Convert.ToString(dt.Rows[0]["Email_ID"]);
                    enquiry.EnquiryID = Convert.ToString(dt.Rows[0]["ID"]);
                    enquiry.Country = Convert.ToString(dt.Rows[0]["Country"]);
                    enquiry.City = Convert.ToString(dt.Rows[0]["City"]);
                    enquiry.State = Convert.ToString(dt.Rows[0]["State"]);
                    enquiry.Enq_Category = Convert.ToString(dt.Rows[0]["EnquiryCategory"]);
                    enquiry.Enq_Subject = Convert.ToString(dt.Rows[0]["EnquirySubject"]);
                    enquiry.EnquiryDetail1 = Convert.ToString(dt.Rows[0]["EnquiryDetail"]);
                }
                else
                {
                    ModelState.AddModelError("", "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.");
                }
            }
            return PartialView("_showEnquiryDetail", enquiry);
        }

        [HttpGet]
        public ActionResult getExpertQuery()
        {
            return PartialView("_getExpertQuery");
        }

        [HttpPost]
        public ActionResult getExpertQuery(Contact query, FormCollection form)
        {
            MailMessage msg = new MailMessage();
            SmtpClient smtp = new SmtpClient();
            MailAddress from = new MailAddress("support@vibration-service.com");
            StringBuilder sb = new StringBuilder();
            msg.IsBodyHtml = true;
            smtp.Host = "smtp.zoho.com";
            smtp.Port = 587;
            msg.To.Add("info@vibration-service.com");
            msg.To.Add("deepakbhat@iadeptmarketing.com");
            msg.Bcc.Add("laxmi@iadept.in");
            msg.From = from;
            msg.Subject = "User's Query on VibExchange !";
            msg.Body += " <html>";
            msg.Body += "<body>";
            msg.Body += "<table class='table' >";

            msg.Body += "<tr>";
            msg.Body += "<td>User Name:</td><td>" + form["Name"].ToString() + "</td>";
            msg.Body += "<td></td>";
            msg.Body += "</tr>";

            msg.Body += "<tr>";
            msg.Body += "<td>Email Address:</td><td>" + form["Email"].ToString() + "</td>";
            msg.Body += "<td></td>";
            msg.Body += "</tr>";

            msg.Body += "<tr>";
            msg.Body += "<td>Phone Number:</td><td>" + form["Phone"].ToString() + "</td>";
            msg.Body += "<td></td>";
            msg.Body += "</tr>";

            msg.Body += "<tr>";
            msg.Body += "<td>Subject:</td><td>" + form["Subject"].ToString() + "</td>";
            msg.Body += "<td></td>";
            msg.Body += "</tr>";

            msg.Body += "<tr>";
            msg.Body += "<td>Description:</td><td>" + form["Message"].ToString() + "</td>";
            msg.Body += "<td></td>";
            msg.Body += "</tr>";

            msg.Body += "</table>";
            msg.Body += "</body>";
            msg.Body += "</html>";
            smtp.UseDefaultCredentials = false;
            smtp.EnableSsl = true;
            smtp.Credentials = new System.Net.NetworkCredential("support@vibration-service.com", "Pass@123");
            smtp.Send(msg);
            msg.Dispose();
            return RedirectToAction("Home", "Home");
        }


        public ActionResult buy(string amount)
        {
            return RedirectToAction("Payment", "Order", new { Amount = amount });
        }

        private void SendPurchaseMail(string name, int EnquiryID, int BuyerID, int EnquiryCost)
        {
            MailMessage msg = new MailMessage();
            SmtpClient smtp = new SmtpClient();
            MailAddress from = new MailAddress("support@vibration-service.com");
            StringBuilder sb = new StringBuilder();
            msg.IsBodyHtml = true;
            smtp.Host = "smtp.zoho.com";
            smtp.Port = 587;
            msg.To.Add("kajol@iadeptmarketing.com");
            msg.To.Add("laxmi@iadept.in");
            msg.To.Add("deepakbhat@iadeptmarketing.com");
            msg.From = from;
            msg.Subject = "Enquiry purchased by consultant on VibExchange !";
            msg.Body += " <html>";
            msg.Body += "<body>";
            msg.Body += "<table class='table' >";

            msg.Body += "<tr>";
            msg.Body += "<td>Consultant Name:</td><td>" + name + "</td>";
            msg.Body += "<td></td>";
            msg.Body += "</tr>";

            msg.Body += "<tr>";
            msg.Body += "<td>EnquiryID:</td><td>" + EnquiryID.ToString() + "</td>";
            msg.Body += "<td></td>";
            msg.Body += "</tr>";

            msg.Body += "<tr>";
            msg.Body += "<td>BuyerID:</td><td>" + BuyerID.ToString() + "</td>";
            msg.Body += "<td></td>";
            msg.Body += "</tr>";

            msg.Body += "<tr>";
            msg.Body += "<td>EnquiryCost:</td><td>" + EnquiryCost.ToString() + "</td>";
            msg.Body += "<td></td>";
            msg.Body += "</tr>";


            msg.Body += "</table>";
            msg.Body += "</body>";
            msg.Body += "</html>";
            smtp.UseDefaultCredentials = false;
            smtp.EnableSsl = true;
            smtp.Credentials = new System.Net.NetworkCredential("support@vibration-service.com", "Pass@123");
            smtp.Send(msg);
            msg.Dispose();
        }

        public ActionResult fail()
        {
            //TODO: Database entries on successful payment
            using (DBClass context = new DBClass())
            {
                var uname = Session["UserName"].ToString();
                SessionClass.Username = uname;

                DataSet dsEmp = context.ExecuteDataSet("Select * from [Emp_Detail] where [LoginID]='" + uname + "'", CommandType.Text);
                int UserID = (int)dsEmp.Tables[0].Rows[0]["AutoId"];
                context.ExecuteNonQuery("Delete from Enquiry_Buyer where DateofBuying in (Select  top 1 (DateofBuying) from Enquiry_Buyer where buyerid=" + UserID + "  order by DateofBuying desc)", CommandType.Text);

                return View();
            }
        }

        public ActionResult ErrorMessage()
        {
            return View();
        }

        //public ActionResult getEnquiryDetail(string EnquiryId, string amount)
        //{
        //    Enquiry enq = new Enquiry();
        //    using (DBClass context = new DBClass())
        //    {
        //        //context.AddParameter("@EnquiryID", EnquiryId);
        //        //context.AddParameter("@UserName", Session["UserName"].ToString());
        //        //DataTable dt = context.getData("getEnquiryDetail", CommandType.StoredProcedure);
        //        enq.EnquiryID = EnquiryId;
        //        enq.Enq_Subject = "sUBJECT";


        //        //return RedirectToAction("Payment", "Order", new { Amount = amount });

        //        //if (dt.Rows.Count > 0)
        //        //{
        //        return View();
        //        //}
        //        //else
        //        //{
        //        //    return View(enq);
        //        //}
        //    }         
        //}
    }
}