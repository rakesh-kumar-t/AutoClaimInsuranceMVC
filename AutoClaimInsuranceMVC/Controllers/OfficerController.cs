﻿using AutoClaimInsuranceMVC.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI.WebControls;
using System.Data.Entity;
namespace AutoClaimInsuranceMVC.Controllers
{
    public class OfficerController : Controller
    {
        AutoClaimContext db = new AutoClaimContext();
        // GET: Officer
        public ActionResult OfficerLogin()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult OfficerLogin(Officer officer)
        {
            officer.password = encrypt(officer.password);
            var user = db.Officers.Where(a => a.officerId.Equals(officer.officerId) && a.password.Equals(officer.password)).FirstOrDefault();
            if (user != null)
            {
                FormsAuthentication.SetAuthCookie(officer.officerId, false);
                Session["OfficerId"] = user.officerId.ToString();
                Session["Officername"] = (user.firstName + " " + user.lastName).ToString();
                Session["Role"] = user.role.ToString();
                if (user.role == "Insurance officer")
                    return RedirectToAction("InsuranceOfficerPage");
                else if (user.role == "Claim officer")
                    return RedirectToAction("ClaimOfficerPage","ClaimOfficer");
                else
                    return RedirectToAction("AssessorPage");

            }
            else
            {
                ModelState.AddModelError("", "Invalid login credentials");
            }

            return View(officer);
        }
        public static string encrypt(string clearText)
        {
            string EncryptionKey = "MAKV2SPBNI99212";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (System.IO.MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        [Authorize]
        [HttpGet]
        public ActionResult InsuranceOfficerPage()
        {
            var claim = db.Claims.Where(c => c.status.Equals("pending")).ToList();
            if (claim != null)
            {
                return View(claim);
            }
            else
            {
                ViewBag.Error = "Claim not exists";
            }
            return View();
        }
        [Authorize]
        [HttpGet]
        public ActionResult AssessorPage()
        {
            string OfficerId = Session["OfficerId"].ToString();
            var reports = db.Reports.Where(c => (c.officerId.Equals(OfficerId)) && (c.status.Equals("completed"))).ToList();
            if (reports != null)
            {
                return View(reports);
            }
            else
            {
                ViewBag.Error = "Not valid";
            }
            return View();


        }
        [Authorize]
        public ActionResult Verify(int claimId, string policyNumber)
        {
            string claimid = claimId.ToString();
            ViewBag.policyNumber = policyNumber;
            var insurance = db.Insurances.Where(c => c.policyNumber.Equals(policyNumber)).FirstOrDefault();
            if (insurance != null)
            {
                DateTime lastDate = insurance.endDate;
                int value = DateTime.Compare(DateTime.Now, lastDate);
                if (value < 0)
                {
                    ViewBag.Exists = "Policy Exists";
                    ViewBag.claimId = claimid;
                    return View(insurance);
                }
                else
                {
                    ViewBag.Exists = " Policy has expired";
                    ViewBag.claimId = claimid;
                    return View();
                }
            }
            else
            {
                ViewBag.check = "null";
                ViewBag.claimId = claimid;
                ViewBag.Exists = "No valid policy number for this insurer ";
                return View();
            }
        }
        [Authorize]
        public ActionResult Accept(string claimId)
        {
            int claimid = int.Parse(claimId);
            var claim = db.Claims.Where(c => c.claimId == claimid).FirstOrDefault();
            claim.status = "progress";
            db.Entry(claim).State = EntityState.Modified;
            db.SaveChanges();
            ViewBag.status = "Accepted";
            return RedirectToAction("InsuranceOfficerPage");
        }

        public ActionResult RejectInsurance(string claimId)
        {
            int claimid = int.Parse(claimId);
            var claim = db.Claims.Where(c => c.claimId == claimid).FirstOrDefault();
            claim.status = "rejected";
            db.Entry(claim).State = EntityState.Modified;
            db.SaveChanges();
            ViewBag.status = "Rejected";
            return RedirectToAction("InsuranceOfficerPage");
        }

        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("OfficerLogin");
        }

    }
}