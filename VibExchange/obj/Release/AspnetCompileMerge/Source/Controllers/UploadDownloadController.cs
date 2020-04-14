using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using VibExchange.Filters;
using VibExchange.Models;


namespace VibExchange.Controllers
{
    public class UploadDownloadController : Controller
    {
       
        [InitializeSimpleMembership]
        public ActionResult ClientDtl()
        {
            
             return View();             
        }
        int PlanID, UserID = 0;
        string UserName, Balance_point, RegistrationDate, ExpiryDate, UserRole = null;
        /// <summary>
        /// code for per point data uploaded to here
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ClientDtl(FormCollection form)
        {
            if (ModelState.IsValid)
            {
                using (DBClass context = new DBClass())
                {
                    PlanID = 0; UserID = 0;
                    UserName = null; Balance_point = null; RegistrationDate = null; ExpiryDate = null; UserRole = null;
                    //context.AddParameter("@id",Session["id"].ToString());

                    if (form["AnalysisType"].ToString() == "Indicative Analysis")
                        //context.AddParameter("@PlanID", "6");
                        PlanID = 6;
                    else
                        //context.AddParameter("@PlanID", "7");
                        PlanID = 7;
                    UserName = Session["UserName"].ToString();
                    UserRole = Session["UserRole"].ToString();

                    DataSet ds = context.ExecuteDataSet("Select [AutoId] from [UserDetail] where [UserName]='" + UserName + "'", CommandType.Text);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        UserID = (int)ds.Tables[0].Rows[0]["AutoId"];
                    }
                    else
                    {
                        DataSet dsEmp = context.ExecuteDataSet("Select [AutoId] from [Emp_Detail] where [LoginID]='" + UserName + "'", CommandType.Text);
                        UserID = (int)dsEmp.Tables[0].Rows[0]["AutoId"];
                    }

                    Balance_point = form["NoOfPoints"].ToString();
                    RegistrationDate = DateTime.Now.ToString();
                    ExpiryDate = DateTime.Now.AddDays(1).ToString();
                    context.ExecuteNonQuery("Insert into tblUserPlans([PlanID],[UserID],[Balance_point],[RegistrationDate],[ExpiryDate],[UserRole]) values(" + PlanID + "," + UserID + ",'" + Balance_point + "','" + RegistrationDate + "','" + ExpiryDate + "','" + UserRole + "')", CommandType.Text);
            
                }


                string amount = Convert.ToString(form["Amount"]);
                
                return RedirectToAction("Payment1", "Clients", new { Amount = amount });
                //if (Convert.ToString(form["status"]) == "success") { message = "Your payment has been done successfully."; }
                //if (Convert.ToString(form["status"]) == "failure") { message = "Your payment has not been done successfully. Please try again !"; }
                // Payment(int id, string amt)
            }
            return View();
        }
        /// <summary>
        /// code for plans
        /// </summary>
        /// <param name="PlanName"></param>
        /// <returns></returns>

        public ActionResult Buy(string PlanName)
        {
            string amount = null;
            if (ModelState.IsValid)
            {
                using (DBClass context = new DBClass())
                {
                    int PlanID = 0;
                    int UserID = 0;
                    string UserName, TimeDuration, AnalysisMethod, Points, Price, UserRole = null;
                    //context.AddParameter("@id", Session["id"].ToString());
                    DataSet ds = context.ExecuteDataSet("Select [PlanId],[Points] ,[Price],[TimeDuration] ,[AnalysisMethod] from [tblPlans] where [PlanName]='" + PlanName + "'", CommandType.Text);
                    TimeDuration = ds.Tables[0].Rows[0]["TimeDuration"].ToString();
                    AnalysisMethod = ds.Tables[0].Rows[0]["AnalysisMethod"].ToString();
                    Points = ds.Tables[0].Rows[0]["Points"].ToString();
                    Price = ds.Tables[0].Rows[0]["Price"].ToString();
                    PlanID = (int)ds.Tables[0].Rows[0]["PlanId"];

                    UserName = System.Web.HttpContext.Current.User.Identity.Name.ToString();
                    UserRole = System.Web.HttpContext.Current.User.Identity.Name.ToString();
                    UserName = Session["UserName"].ToString();
                    UserRole = Session["UserRole"].ToString();
                   
                    ds = context.ExecuteDataSet("Select * from [UserDetail] where [UserName]='" + UserName + "'", CommandType.Text);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        UserID = (int)ds.Tables[0].Rows[0]["AutoId"];
                        UserRole = "Client";
                    }
                    else
                    {
                        DataSet dsEmp = context.ExecuteDataSet("Select * from [Emp_Detail] where [LoginID]='" + UserName + "'", CommandType.Text);
                        UserID = (int)dsEmp.Tables[0].Rows[0]["AutoId"];
                        UserRole = "Consultant";
                    }
                    amount = Price;
                    context.ExecuteNonQuery("Insert into tblUserPlans([PlanID],[UserID],[Balance_point],[RegistrationDate],[ExpiryDate],[UserRole]) values(" +
                        PlanID + "," + UserID + "," + Points + ",'" + DateTime.Now.ToString() + "','" + DateTime.Now.AddDays(Convert.ToInt32(TimeDuration)).ToString() + "','" + UserRole + "')", CommandType.Text);
                }

                return RedirectToAction("Payment1", "Clients", new { Amount = amount });
            }

            return View();
        }

        
        //public ActionResult Buy(string PlanName)
        //{
        //    string amount = null;
        //    if (ModelState.IsValid)
        //    {
        //        using (DBClass context = new DBClass())
        //        {
        //            int PlanID = 0;
        //            int UserID = 0;
        //            string UserName, TimeDuration, AnalysisMethod, Points, Price, UserRole = null;
        //            context.AddParameter("@id", Session["id"].ToString());
        //            DataSet ds = context.ExecuteDataSet("Select [PlanId],[Points] ,[Price],[TimeDuration] ,[AnalysisMethod] from [tblPlans] where [PlanName]='" + PlanName + "'", CommandType.Text);
        //            TimeDuration = ds.Tables[0].Rows[0]["TimeDuration"].ToString();
        //            AnalysisMethod = ds.Tables[0].Rows[0]["AnalysisMethod"].ToString();
        //            Points = ds.Tables[0].Rows[0]["Points"].ToString();
        //            Price = ds.Tables[0].Rows[0]["Price"].ToString();
        //            PlanID = (int)ds.Tables[0].Rows[0]["PlanId"];

        //            UserName = System.Web.HttpContext.Current.User.Identity.Name.ToString();
        //            UserRole = System.Web.HttpContext.Current.User.Identity.Name.ToString();
        //            UserName = Session["UserName"].ToString();
        //            UserRole = Session["UserRole"].ToString();
        //            if (UserRole == "Client")
        //            {
        //                ds = context.ExecuteDataSet("Select [AutoId] from [UserDetail] where [UserName]='" + UserName + "'", CommandType.Text);
        //                if (ds.Tables[0].Rows.Count > 0)
        //                    UserID = (int)ds.Tables[0].Rows[0]["AutoId"];
        //            }
        //            else
        //            {
        //                DataSet dsEmp = context.ExecuteDataSet("Select [AutoId] from [Emp_Detail] where [LoginID]='" + UserName + "'", CommandType.Text);
        //                if (dsEmp.Tables[0].Rows.Count > 0)
        //                    UserID = (int)dsEmp.Tables[0].Rows[0]["AutoId"];
        //            }

        //            if (UserID != 0)
        //            {
        //                amount = Price;
        //                context.ExecuteNonQuery("Insert into tblUserPlans([PlanID],[UserID],[Balance_point],[RegistrationDate],[ExpiryDate],[UserRole]) values(" +
        //                    PlanID + "," + UserID + "," + Points + ",'" + DateTime.Now.ToString() + "','" + DateTime.Now.AddDays(Convert.ToInt32(TimeDuration)).ToString() + "','" + UserRole + "')", CommandType.Text);
        //            }
        //        }

        //        return RedirectToAction("Payment1", "Clients", new { Amount = amount });
        //    }

        //    return View();
        //}

        [HttpGet]
        public ActionResult UploadFile(string id, string flag, string ReturnUrl,FormCollection form)
        {
            //if (Convert.ToString(form["status"]) == "success") { }
            //if (Convert.ToString(form["status"]) == "failure") { message = "Your payment has not been done successfully. Please try again !"; }
            ViewBag.ReturnUrl = ReturnUrl;
            if (Session["UserName"] == null)
            {
                //TempData["MessageLogin"] = "Please login before upload or register if you are not registered !";
                return RedirectToAction("Login", "Home");
            }
            else
            {
                UploadFile file = new UploadFile();
                file.CostList = GetCostList();
                file.MachienDataList = GetMachineData();
                ViewBag.AnalysisMethod = flag;
                return View(file);
            }
        }

        public List<UploadFile> GetCostList()
        {
            List<UploadFile> allList = new List<UploadFile>();

            using (DBClass context = new DBClass())
            {
                DataTable dt = context.getData("select * from AnalysisCost", CommandType.Text);
                foreach (DataRow dr in dt.Rows)
                {
                    allList.Add(new UploadFile
                    {
                        sr = Convert.ToString(dr["ServiceType"]),
                        costperpoint = Convert.ToString(dr["CostPerPoint"]),
                        costpergraph = Convert.ToString(dr["CostPerGraphOrImage"]),
                        AnalysisTime = Convert.ToString(dr["AnalysisTime"])
                    });
                }
                return allList;
            }
        }

        public List<UploadFile> GetMachineData()
        {
            List<UploadFile> MachineData = new List<UploadFile>();
            try
            {
                using (DBClass context = new DBClass())
                {
                    DataTable dt = context.getData("Select mi.ID,mi.MachineName,mi.Point_Count ,dmd.DUName from MachineInfo mi inner join DriveMachineDetail dmd on mi.ID=dmd.MachineID ", CommandType.Text);
                    foreach (DataRow dr in dt.Rows)
                    {
                        string MacID = Convert.ToString(dr["ID"]);
                        string Train = Convert.ToString(dr["MachineName"]);
                        string DEUnit = Convert.ToString(dr["DUName"]);
                        string NDE1 = Convert.ToString(dr["MachineName"]);
                        string NDE2 = Convert.ToString(dr["MachineName"]);
                        string NDE3 = Convert.ToString(dr["MachineName"]);
                        int PointCount = Convert.ToInt32(dr["Point_Count"]);
                        MachineData.Add(new UploadFile
                        {
                            MachineID = Convert.ToInt32(MacID),
                            TrainName = Convert.ToString(Train),
                            DEUnitType = Convert.ToString(DEUnit),
                            NDEUnit1 = Convert.ToString(NDE1),
                            NDEUnit2 = Convert.ToString(NDE2),
                            NDEUnit3 = Convert.ToString(NDE3),
                            TotalPoint = Convert.ToInt32(PointCount)
                        });
                    }
                }
            }
            catch { }
            return MachineData;
        }

        List<SelectListItem> MachineSlot = new List<SelectListItem>();
        List<SelectListItem> AnalysisCost = new List<SelectListItem>();
        DataTable dtMachineList = new DataTable();

        private SelectList GetMachinesSlot()
        {
            using (DBClass context = new DBClass())
            {
                try
                {
                    DataTable dt = context.getData("Select * from AnalysisCost", CommandType.Text);
                    for (int i = 0; i < dtMachineList.Rows.Count; i++)
                    {
                        MachineSlot.Add(new SelectListItem { Text = Convert.ToString(dt.Rows[i]["MachineSlot"]), Value = Convert.ToString(dtMachineList.Rows[i]["ID"]) });
                    }
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                }
                return new SelectList(MachineSlot, "Value", "Text", "id");
            }
        }

        [HttpPost]
        [InitializeSimpleMembershipAttribute]
        public ActionResult UploadFile(string id, FormCollection formCollection, HttpPostedFileBase[] files)
        {
            bool sts = ModelState.IsValid;
            if (ModelState.IsValid)
            {
                var file = HttpContext.Request.Files["UploadedFile"];
                if (file == null)
                {
                    ModelState.AddModelError("File", "Please Upload Your file");
                }
                else if (file.ContentLength > 0)
                {
                    int MaxContentLength = 1024 * 1024 * 150; //150 MB
                    //string[] AllowedFileExtensions = new string[] { ".sdf", ".docx", ".dat", ".pdf", ".txt", ".doc" };
                    //string ext = file.FileName.Substring(file.FileName.LastIndexOf('.'));
                    //if (!AllowedFileExtensions.Contains(file.FileName.Substring(file.FileName.LastIndexOf('.'))))
                    //{
                    //    ModelState.AddModelError("File", "Please file of type: " + string.Join(", ", AllowedFileExtensions));

                    //}
                    if (file.ContentLength > MaxContentLength)
                    {
                        ModelState.AddModelError("File", "Your file is too large, maximum allowed size is: " + MaxContentLength + " MB. Please zip your file and upload agail.");
                    }
                    else
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        string fname = Session["UserName"].ToString() + "@" + fileName;
                        var path = Path.Combine(Server.MapPath("~/Files"), fname);

                        if (System.IO.File.Exists(path))
                        {
                            ModelState.AddModelError("File", "This file is already exist!");
                            ViewBag.errorsOccurred = "You have already upload this file before ! Please rename your file or upload another file.";
                            TempData["Error"] = "You have already upload this file before ! Please rename your file or upload another file.";
                            return View();
                            // return JavaScript(alert("Hello this is an alert"));
                            //return Content("<script language='javascript' type='text/javascript'>alert('This file is already exist!');</script>");
                        }
                        else
                        {
                            string analysisType = null;
                            using (DBClass context = new DBClass())
                            {
                                var analysis = formCollection["AnalysisType"];
                                DateTime AnalysisTime = DateTime.Now;
                                switch (analysis)
                                {
                                    case "Indicative":
                                        {
                                            analysisType = "Indicative Analysis";
                                            AnalysisTime = AnalysisTime.AddDays(1);
                                            break;

                                        }
                                    case "VibAnalyst":
                                        {
                                            analysisType = "VibAnalyst Analysis";
                                            AnalysisTime = AnalysisTime.AddDays(2);
                                            break;
                                        }
                                    case "Expert":
                                        {
                                            analysisType = "Expert Analysis";
                                            break;
                                        }
                                }
                                string dtime = DateTime.Now.Date.ToString("dd/MM/yyyy");
                                context.AddParameter("@UserName", Session["UserName"].ToString());
                                context.AddParameter("@InstrumentUsed", formCollection["Instrument"]);
                                context.AddParameter("@FileName", fileName);
                                context.AddParameter("@Description", formCollection["Description"]);
                                context.AddParameter("@UploadDate", DateTime.Now.Date.ToString("dd/MM/yyyy"));
                                context.AddParameter("@FileType", formCollection["FileType"]);
                                context.AddParameter("@AnalysisType", analysisType);
                                context.AddParameter("@AnalysisMethod", "Manually Analysis Method");
                                context.AddParameter("@AnalysisTime", AnalysisTime);
                                if (context.ExecuteNonQuery("addUploadData", CommandType.StoredProcedure) > 0)
                                {
                                    file.SaveAs(path);
                                    bool chk = sendMail();
                                    ModelState.Clear();
                                    ViewBag.Message = "File uploaded successfully";
                                    return RedirectToAction("MachineDetail", "UploadDownload");
                                }
                                else
                                {
                                    ModelState.AddModelError("", "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.");
                                }
                            }
                        }
                    }
                }
            }
            return View();
        }

        public bool sendMail()
        {
            string Name = null;
            string EmailID = null;
            string Phone = null;
            string Company = null;
            string UplaodDate = null;
            bool status = false;
            string FileName = null;
            try
            {
                using (DBClass context = new DBClass())
                {
                    context.AddParameter("@UserName", Session["UserName"].ToString());
                    context.AddParameter("@UserRole", Session["UserRole"].ToString());
                    DataTable dt = context.getData("getUserDetail", CommandType.StoredProcedure);
                    Name = Convert.ToString(dt.Rows[0]["uName"]);
                    EmailID = Convert.ToString(dt.Rows[0]["Email_ID"]);
                    Phone = Convert.ToString(dt.Rows[0]["uMobile_No"]);
                    Company = Convert.ToString(dt.Rows[0]["uCompanyName"]);
                    UplaodDate = DateTime.Now.ToString();
                }

                using (DBClass context = new DBClass())
                {
                    DataTable dt = context.getData("Select top 1 [FileName] from [UploadData] where [UserName]='" + Session["UserName"].ToString() + "' order by id desc", CommandType.Text);
                    FileName = Convert.ToString(dt.Rows[0]["FileName"]);
                }
                MailMessage msg = new MailMessage();
                SmtpClient smtp = new SmtpClient();
                MailAddress from = new MailAddress("info@vibration-service.com");
                StringBuilder sb = new StringBuilder();
                msg.IsBodyHtml = true;
                smtp.Host = "smtp.zoho.com";
                smtp.Port = 587;
                msg.To.Add("info@vibration-service.com");
                msg.To.Add("deepakbhat@iadeptmarketing.com");
                msg.To.Add("kajol@iadeptmarketing.com");
                msg.Bcc.Add("laxmi@iadept.in");
                msg.From = from;
                msg.Subject = "A new file has been uploaded to analysis on VibExchange ! ";
                msg.Body += " <html>";
                msg.Body += "<body>";
                msg.Body += "<table>";
                msg.Body += "<tr>";
                msg.Body += "<td>A new file has been uploaded to analysis on VibExchange ! Contact detail is : </td>";
                msg.Body += "</tr>";
                msg.Body += "<tr>";
                msg.Body += "<td>Customer Name : </td><td>" + Name + "</td>";
                msg.Body += "</tr>";
                msg.Body += "<tr>";
                msg.Body += "<td>Phone No : </td><td>" + Phone + "</td>";
                msg.Body += "</tr>";
                msg.Body += "<tr>";
                msg.Body += "<td>Email ID : </td><td>" + EmailID + "</td>";
                msg.Body += "</tr>";
                msg.Body += "<tr>";
                msg.Body += "<td>Comapny Name : </td><td>" + Company + "</td>";
                msg.Body += "</tr>";
                msg.Body += "<tr>";
                msg.Body += "<td>Uploaded Date : </td><td>" + UplaodDate + "</td>";
                msg.Body += "</tr>";
                msg.Body += "<tr>";
                msg.Body += "<td>FileName : </td><td>" + FileName + "</td>";
                msg.Body += "</tr>";
                msg.Body += "<tr>";
                msg.Body += "<td>for more information </td><td><a href='www.vibration-service.com'> Click Here </a></td>";
                msg.Body += "</tr>";
                msg.Body += "</table>";
                msg.Body += "</body>";
                msg.Body += "</html>";
                smtp.UseDefaultCredentials = false;
                smtp.EnableSsl = true;
                smtp.Credentials = new System.Net.NetworkCredential("info@vibration-service.com", "nishant78");
                smtp.Send(msg);
                msg.Dispose();
                status = true;

            }
            catch
            {
                status = false;
            }
            return status;
        }

        public ActionResult AnalysisMode(string id, AnalysisMethod am)
        {
            //if (User.Identity.Name == "")
            //{
            //    TempData["MessageLogin"] = "Please login before upload or register if you are not registered !";
            //    return RedirectToAction("Login", "Home");
            //}
            //else
            //{
            am.CostList = GetCostList();
            ViewBag.AnalysisMethod = id;
            return View(am);
            //}
        }

        public ActionResult AnalysisManually(FormCollection fcollection)
        {
            return PartialView("_uploadData");
        }

       
        public ActionResult AddDriveUnit(int id)
        {
            List<SelectListItem> MachineOrientation = new List<SelectListItem>();
            MachineOrientation.Add(new SelectListItem { Text = "Horizontal", Value = "Horizontal" });
            MachineOrientation.Add(new SelectListItem { Text = "Vertical", Value = "Vertical" });
            ViewBag.MOList = MachineOrientation;

            List<SelectListItem> MountingType = new List<SelectListItem>();
            MountingType.Add(new SelectListItem { Text = "Flexible", Value = "Flexible" });
            MountingType.Add(new SelectListItem { Text = "Rigid", Value = "Rigid" });
            ViewBag.MountingList = MountingType;

            //List<SelectListItem> TransmissionType = new List<SelectListItem>();
            //TransmissionType.Add(new SelectListItem { Text = "Direct Mountained", Value = "Direct Mountained" });
            //TransmissionType.Add(new SelectListItem { Text = "Coupled", Value = "Coupled" });
            //TransmissionType.Add(new SelectListItem { Text = "Belt Driven", Value = "Belt Driven" });
            //ViewBag.TransmissionList = TransmissionType;

            List<SelectListItem> DEUnit = new List<SelectListItem>();
            DEUnit.Add(new SelectListItem { Text = "Motor", Value = "Motor" });
            DEUnit.Add(new SelectListItem { Text = "Engine", Value = "Engine" });
            DEUnit.Add(new SelectListItem { Text = "Turbine", Value = "Turbine" });
            DEUnit.Add(new SelectListItem { Text = "Others", Value = "Others" });
            ViewBag.DEUnitList = DEUnit;

            List<SelectListItem> LubType = new List<SelectListItem>();
            LubType.Add(new SelectListItem { Text = "Oil", Value = "Oil" });
            LubType.Add(new SelectListItem { Text = "Grease", Value = "Grease" });
            ViewBag.LubType = LubType;

            return PartialView("_DriveUnit");
        }

        [HttpPost]
        public ActionResult AddDriveUnit(int id, FormCollection form)
        {
            int fileID = id;
            try
            {
                using (DBClass context = new DBClass())
                {
                    context.AddParameter("@MachineName", form["TrainName"]);
                    context.AddParameter("@MachineOrientation", form["MOType"]);
                    context.AddParameter("@MountingType", form["MountingType"]);
                    context.AddParameter("@UserName", Session["UserName"].ToString());
                    context.AddParameter("@DriveUnitName", form["DEUnitType"]);
                    context.AddParameter("@RPM", form["DURPM"]);
                    context.AddParameter("@Ber_Lubrication", form["BerLubrication"]);
                    context.AddParameter("@Ber_Manufacture", form["bearingMake"]);
                    context.AddParameter("@Ber_NumberDE", form["bearingNoDE"]);
                    context.AddParameter("@Ber_NumberNDE", form["bearingNoNDE"]);
                    context.AddParameter("@NoOfPoles", form["NoOfPoles"]);
                    context.AddParameter("@Point_Count", form["Point_Count_DE"]);
                    context.AddParameter("@FileID", fileID);
                    if (context.ExecuteNonQuery("AddDriveUnit", CommandType.StoredProcedure) > 0) 
                    {
                        ModelState.Clear();
                        //TempData["Status"] = "Drive Unit Added successfully";

                        return RedirectToAction("MachineDetail", "UploadDownload");
                        // return status;
                    }
                    else
                    {
                        ModelState.AddModelError("", "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.");
                    }
                }
            }
            catch { }
            return View();
            //return status;
        }

        public ActionResult EditDriveUnit(UploadFile edit, string MachineID)
        {
            using (DBClass context = new DBClass())
            {
                context.AddParameter("@MachineID", MachineID);
                DataTable dt = context.getData("Select * from MachineInfo mi inner join DriveMachineDetail de on mi.ID= de.MachineID where mi.ID='" + MachineID + "'", CommandType.Text);
                edit.TrainName = Convert.ToString(dt.Rows[0]["MachineName"]);
                edit.MachineOrientaion = Convert.ToString(dt.Rows[0]["MachineOrientation"]);
                edit.MountingType = Convert.ToString(dt.Rows[0]["MountingType"]);
                edit.DEUnitType = Convert.ToString(dt.Rows[0]["DUName"]);
                edit.DURPM = Convert.ToString(dt.Rows[0]["RPM"]);
                edit.BearingLubrication = Convert.ToString(dt.Rows[0]["Ber_Lubrication"]);
                edit.bearingMake = Convert.ToString(dt.Rows[0]["Ber_Manufacture"]);
                edit.bearingNoDE = Convert.ToString(dt.Rows[0]["Ber_NumberDE"]);
                edit.bearingNoNDE = Convert.ToString(dt.Rows[0]["Ber_NumberNDE"]);
                edit.NoOfPoles = Convert.ToInt32(dt.Rows[0]["PolesCount"]);
                edit.Point_Count_DE = Convert.ToInt32(dt.Rows[0]["Point_Count"]);
                return View("_EditDriveUnit", edit);
            }
        }

        public ActionResult DeleteDrive(string id)
        {
            using (DBClass context = new DBClass())
            {
                int i = context.ExecuteNonQuery("Delete from DriveMachineDetail where MachineID ='" + id + "' ;Delete from MachineInfo where ID='" + id + "' ; ", CommandType.Text);
                if (i > 0)
                {
                    return RedirectToAction("MachineDetail", "UploadDownload");
                }
            }
            return RedirectToAction("MachineDetail", "UploadDownload");
        }

        [HttpGet]
        public ActionResult AddNDEUnit(string MachineID)
        {
            UploadFile upload = new UploadFile();
            upload.DrivenUnitID = Convert.ToInt32(MachineID);
            TempData["MachineID"] = MachineID.ToString();
            ViewBag.DriveID = Convert.ToInt32(MachineID);
            List<SelectListItem> NDEUnit = new List<SelectListItem>();
            NDEUnit.Add(new SelectListItem { Text = "Gear Box Two Stage", Value = "Gear Box Two Stage" });
            NDEUnit.Add(new SelectListItem { Text = "Gear Box Three Stage", Value = "Gear Box Three Stage" });
            NDEUnit.Add(new SelectListItem { Text = "Fan", Value = "Fan" });
            NDEUnit.Add(new SelectListItem { Text = "Pump", Value = "Pump" });
            NDEUnit.Add(new SelectListItem { Text = "Crusher", Value = "Crusher" });
            NDEUnit.Add(new SelectListItem { Text = "Compressor ", Value = "Compressor" });

            ViewBag.NDEUnitList = NDEUnit;

            List<SelectListItem> NDEType = new List<SelectListItem>();
            NDEType.Add(new SelectListItem { Text = "Intermediate", Value = "Intermediate" });
            NDEType.Add(new SelectListItem { Text = "Driven", Value = "Driven" });
            ViewBag.NDETypeList = NDEType;

            List<SelectListItem> TransmissionType = new List<SelectListItem>();
            TransmissionType.Add(new SelectListItem { Text = "Direct Mountained", Value = "Direct Mountained" });
            TransmissionType.Add(new SelectListItem { Text = "Coupled", Value = "Coupled" });
            TransmissionType.Add(new SelectListItem { Text = "Belt Driven", Value = "Belt Driven" });
            ViewBag.TransmissionList = TransmissionType;

            List<SelectListItem> LubType = new List<SelectListItem>();
            LubType.Add(new SelectListItem { Text = "Oil", Value = "Oil" });
            LubType.Add(new SelectListItem { Text = "Grease", Value = "Grease" });
            ViewBag.LubType = LubType;

            List<SelectListItem> Belttype = new List<SelectListItem>();
            Belttype.Add(new SelectListItem { Text = "Timing Belt", Value = "Timing Belt" });
            Belttype.Add(new SelectListItem { Text = "V Belt", Value = "V Belt" });
            ViewBag.BeltTypeList = Belttype;

            return View("_NDEUnit", upload);
        }

        [HttpPost]
        public ActionResult AddNDEUnit(string id, FormCollection form)
        {
            try
            {
                int DriveUnitID;
                int TotalPoint;
                string id1 = form["DrivenUnitID"].ToString();
                int fileID = Convert.ToInt32(ViewBag.FileID);
                using (DBClass context = new DBClass())
                {
                    DataTable dt = context.getData("Select mi.Point_Count,de.ID,de.FileID from DriveMachineDetail de inner join MachineInfo mi on mi.ID= de.MachineID where mi.ID = '" + id1 + "'", CommandType.Text);
                    DriveUnitID = Convert.ToInt32(dt.Rows[0]["ID"]);
                    TotalPoint = Convert.ToInt32(dt.Rows[0]["Point_Count"]) + Convert.ToInt32(form["Point_Count_NDE"]);

                    context.AddParameter("@UserName", Session["UserName"].ToString());
                    context.AddParameter("@MachineID", DriveUnitID);
                    context.AddParameter("@DriveUnitID", DriveUnitID);
                    context.AddParameter("@Machine_Name", form["NDEUnit"]);
                    context.AddParameter("@Machine_Type", form["NDEType"]);
                    context.AddParameter("@Transmission_Type", form["TransmissionType"]);
                    context.AddParameter("@RPM", Convert.ToDouble(form["NDERPM"]));
                    context.AddParameter("@Ber_Lubrication", form["BerLubrication"]);
                    context.AddParameter("@Ber_Manufacture", form["bearingMake"]);
                    context.AddParameter("@Ber_DE", form["bearingNoDE"]);
                    context.AddParameter("@Ber_NDE", form["bearingNoNDE"]);
                    context.AddParameter("@Ber_DE_IN", form["bearingDEIn"]);
                    context.AddParameter("@Ber_NDE_IN", form["bearingNDEIn"]);
                    context.AddParameter("@Ber_DE_OUT", form["bearingDEOut"]);
                    context.AddParameter("@Ber_NDE_OUT", form["bearingNDEOut"]);
                    context.AddParameter("@Ber_DE_Idle", form["bearingDEIdler"]);
                    context.AddParameter("@Ber_NDE_Idle", form["bearingNDEIdler"]);
                    context.AddParameter("@Input_RPM", Convert.ToDouble(form["InputRPM"]));
                    context.AddParameter("@Output_RPM", Convert.ToDouble(form["OutputRPM"]));
                    context.AddParameter("@Gear_Ratio", form["GearRatio"]);
                    context.AddParameter("@Teeth_Input_Count", Convert.ToInt32(form["TeethCountInput"]));
                    context.AddParameter("@Teeth_Idler_Count", Convert.ToInt32(form["TeethCountIdle"]));
                    context.AddParameter("@Teeth_Output_Count", Convert.ToInt32(form["TeethCountOutput"]));
                    context.AddParameter("@Blades", form["BladesorFins"]);
                    context.AddParameter("@Belt_Type", form["BeltType"]);
                    context.AddParameter("@Belt_Count", Convert.ToInt32(form["BeltCount"]));
                    context.AddParameter("@Belt_Length", form["BeltLength"]);
                    context.AddParameter("@Pitch_Dia", Convert.ToDouble(form["PitchDia"]));
                    context.AddParameter("@Teeth_Count", Convert.ToInt32(form["TeethCount"]));
                    context.AddParameter("@Point_Count", Convert.ToInt32(form["Point_Count_NDE"]));
                    context.AddParameter("@FileID", Convert.ToInt32(dt.Rows[0]["FileID"]));
                    //context.AddParameter("@TotalPoint", TotalPoint);

                    if (context.ExecuteNonQuery("AddDrivenUnit", CommandType.StoredProcedure) > 0)
                    {

                        ModelState.Clear();
                        ViewBag.Message = "Driven Unit Added successfully";
                        return RedirectToAction("MachineDetail", "UploadDownload");
                    }
                    else
                    {
                        ModelState.AddModelError("", "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.");
                    }

                }
            }
            catch { }
            return View();
        }

        public ActionResult GetMachineData1(int fileID, UploadFile upl)
        {
            List<UploadFile> MachineData = new List<UploadFile>();
            try
            {
                using (DBClass context = new DBClass())
                {
                    context.AddParameter("@FileID", fileID);
                    context.AddParameter("@UserName", Session["UserName"].ToString());
                    DataTable dt = context.getData("getDriveUnit", CommandType.StoredProcedure);
                    foreach (DataRow dr in dt.Rows)
                    {
                        string MacID = Convert.ToString(dr["ID"]);
                        string Train = Convert.ToString(dr["MachineName"]);
                        string DEUnit = Convert.ToString(dr["DUName"]);
                        int PointCount = Convert.ToInt32(dr["Point_Count"]);
                        MachineData.Add(new UploadFile
                        {
                            MachineID = Convert.ToInt32(MacID),
                            TrainName = Convert.ToString(Train),
                            DEUnitType = Convert.ToString(DEUnit),
                            //NDEUnit1 = Convert.ToString(NDE1),
                            //NDEUnit2 = Convert.ToString(NDE2),
                            //NDEUnit3 = Convert.ToString(NDE3),
                            TotalPoint = Convert.ToInt32(PointCount)
                        });

                    }
                }
            }
            catch { }
            var machinedata = Json(MachineData, JsonRequestBehavior.AllowGet);
            return machinedata;
        }

        public JsonResult getDrivenUnit(string id)
        {
            //Preparing anonymous variable with json data
            List<UploadFile> MachineData = new List<UploadFile>();
            try
            {
                using (DBClass context = new DBClass())
                {
                    context.AddParameter("@MachineID", id);
                    context.AddParameter("@UserName", Session["UserName"].ToString());
                    DataTable dt = context.getData("getDrivenUnit", CommandType.StoredProcedure);
                    foreach (DataRow dr in dt.Rows)
                    {
                        string MacID = Convert.ToString(dr["ID"]);
                        string NDEName = Convert.ToString(dr["Machine_Name"]);
                        int PointCount = Convert.ToInt32(dr["Point_Count"]);
                        MachineData.Add(new UploadFile
                        {
                            MachineID = Convert.ToInt32(MacID),
                            NDEUnit1 = Convert.ToString(NDEName),
                            Point_Count_NDE = Convert.ToInt32(PointCount)
                        });

                    }
                }
            }
            catch { }

            //Returning json data
            return Json(MachineData, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult MachineDetail(UploadFile uploadMachine)
        {
            //bool filetype = false;
            using (DBClass context = new DBClass())
            {
                context.AddParameter("@UserName", Session["UserName"].ToString());
                DataTable dt = context.getData("getCurrentFileDetail", CommandType.StoredProcedure);

                //if (Convert.ToString(dt.Rows[0]["FileType"]) == "Data")
                //{
                //    filetype = true;
                //}
                uploadMachine.FileID = Convert.ToInt32(dt.Rows[0]["ID"]);
                uploadMachine.fileName1 = Convert.ToString(dt.Rows[0]["FileName"]);
                uploadMachine.Instrument = Convert.ToString(dt.Rows[0]["InstrumentUsed"]);
                uploadMachine.AnalysisType = Convert.ToString(dt.Rows[0]["AnalysisType"]);
                uploadMachine.Description = Convert.ToString(dt.Rows[0]["Description"]);
            }
            ViewBag.FileID = uploadMachine.FileID;
            return View(uploadMachine);
        }

        [HttpPost]
        public ActionResult MachineDetail(FormCollection form, UploadFile up)
        {

            int id = Convert.ToInt32(up.FileID);
            using (DBClass context = new DBClass())
            {
                int totalPoint = 0;
                context.AddParameter("@FileID", id);
                DataTable DEPoint = context.getData("getDEPointCountbyFileID", CommandType.StoredProcedure);
                foreach (DataRow dr in DEPoint.Rows)
                {
                    totalPoint += Convert.ToInt32(dr["Point_Count"]);
                }
                context.AddParameter("@FileID", id);
                DataTable NDEPoint = context.getData("getNDEPointCountbyFileID", CommandType.StoredProcedure);
                foreach (DataRow dr in NDEPoint.Rows)
                {
                    totalPoint += Convert.ToInt32(dr["Point_Count"]);
                }

                if (context.ExecuteNonQuery("Update UploadData set PointCount = '" + totalPoint + "' where ID = '" + id + "' ", CommandType.Text) > 0)
                {
                    DataTable dt1 = context.getData("Select AnalysisType from UploadData where ID = '" + id + "'", CommandType.Text);
                    double cost = 0;
                    if (Convert.ToString(dt1.Rows[0]["AnalysisType"]) == "Indicative Analysis")
                    {
                        cost = 140 * totalPoint;
                    }
                    else
                    {
                        cost = 680 * totalPoint;
                    }
                    context.ExecuteNonQuery("Update PaymentDetail set Amount = '" + cost + "' where FileID = '" + id + "' ", CommandType.Text);
                    ModelState.Clear();
                    return RedirectToAction("ClientList", "Clients");
                }
            }
            return View();
        }
        public FileStreamResult GetSampleReport(string id)
        {
            string fileName = null;
            if (id == "Indicative Analysis")
            {
                fileName = "SampleReport_Indicative.pdf";
            }
            else if (id == "VibAnalyst Analysis")
            {
                fileName = "Sample_report_VibAnalyst.pdf";
            }
            var path = Path.Combine(Server.MapPath("~/Images"), fileName);
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            return File(fs, "application/pdf");
        }


        public FileStreamResult GetReport(string id)
        {
            string fileName = null;
            if (id == "Indicative Analysis1")
            {
                fileName = "T123.pdf";
            }
            else if (id == "VibAnalyst Analysis1")
            {
                fileName = "Terms & conditions - VibExchange.pdf";
            }
            var path = Path.Combine(Server.MapPath("~/Images"), fileName);
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            return File(fs, "application/pdf");
        }
        public ActionResult GetVideo()
        {
            return new VideoResult();
        }

        /// <summary>
        /// Payment Failure case
        /// </summary>
        /// <returns></returns>
        public ActionResult View1()
        {
           
            //TODO: Database entries on successful payment
            using (DBClass context = new DBClass())
            {
                UserName = Session["UserName"].ToString();
                UserRole = Session["UserRole"].ToString();
                if (UserRole == "Client")
                {
                    DataSet ds = context.ExecuteDataSet("Select [AutoId] from [UserDetail] where [UserName]='" + UserName + "'", CommandType.Text);
                    if (ds.Tables[0].Rows.Count > 0)
                        UserID = (int)ds.Tables[0].Rows[0]["AutoId"];

                }
                else
                {
                    DataSet dsEmp = context.ExecuteDataSet("Select [AutoId] from [Emp_Detail] where [LoginID]='" + UserName + "'", CommandType.Text);
                    if (dsEmp.Tables[0].Rows.Count > 0)
                        UserID = (int)dsEmp.Tables[0].Rows[0]["AutoId"];
                }
                context.ExecuteNonQuery("Delete from tbluserplans where AutoID in (Select  top 1(AutoID) from tbluserplans where userid=" + UserID + " and userrole='" + UserRole + "' order by RegistrationDate desc)", CommandType.Text);
            
               
            }
            return View();
            //return RedirectToAction("UploadFile");
        }

        public string message { get; set; }
    }
}
