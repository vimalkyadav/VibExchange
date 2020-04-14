using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VibExchange.Models;

namespace VibExchange.Controllers
{
    public class AdminController : Controller
    {
        //
        // GET: /Admin/
        public ActionResult Index()
        {
            return View();
        }

        //
        // GET: /Admin/Details/5
        public ActionResult GetUserDetail()
        {
            UserRegisterViewModel usermodel = new UserRegisterViewModel();
            List<UserRegisterViewModel> lstUser = new List<UserRegisterViewModel>();
            try
            {
                using (DBClass context = new DBClass())
                {
                    DataTable dtUser = context.getData("getAllUserList", CommandType.StoredProcedure);
                    if (dtUser.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dtUser.Rows)
                        {
                            lstUser.Add(new UserRegisterViewModel
                            {
                                uName = Convert.ToString(dr["Name"]),
                                UserName = Convert.ToString(dr["UserName"]),
                                Email = Convert.ToString(dr["Email_ID"]),
                                uMobile = Convert.ToString(dr["Phone"]),
                                uCompany = Convert.ToString(dr["Company"]),
                                CreateDate = Convert.ToString(dr["CreationDate"]),
                                UserRole = Convert.ToString(dr["UserRole"])
                            });
                        }
                        usermodel.lstUser = lstUser;
                    }
                }
            }
            catch (Exception ex) { throw ex; }
            return PartialView("Alluser", usermodel.lstUser);
        }

        //
        // GET: /Admin/Create
        public ActionResult GetEnquiryDetail()
        {
            Enquiry userenquiry= new Enquiry();
            List<Enquiry> enquiryList = new List<Enquiry>();
            try
            {
                using (DBClass context = new DBClass())
                {
                    DataTable dtUser = context.getData("Alladminenquiry", CommandType.StoredProcedure);
                    if (dtUser.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dtUser.Rows)
                        {
                            enquiryList.Add(new Enquiry
                            {
                        EnquiryID = Convert.ToString(dr["EnquiryID"]),
                        Enq_Category = Convert.ToString(dr["EnquiryCategory"]),
                        UserName = Convert.ToString(dr["UserName"]),
                        Enq_Subject = Convert.ToString(dr["EnquirySubject"]),
                        EnquiryDetail1 = Convert.ToString(dr["EnquiryDetail"]),
                        Enq_CreateDate = Convert.ToString(dr["CreateDate"]),
                        Enquiry_Cost = Convert.ToString(dr["Enquiry_Cost"]),
                        Buyer_Name = Convert.ToString(dr["Buyer_Name"]),
                        DateOfBuying = Convert.ToString(dr["DateOfBuying"])
                
                   });
                         }
                        userenquiry.enquiryList = enquiryList;
                    }
                }
                    }
                catch (Exception ex) { throw ex; }
            return PartialView("Allenq", userenquiry.enquiryList);
                  }

        //
        // POST: /Admin/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Admin/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Admin/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Admin/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Admin/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
