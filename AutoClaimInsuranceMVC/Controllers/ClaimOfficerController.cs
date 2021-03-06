﻿using AutoClaimInsuranceMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Web.Helpers;

namespace AutoClaimInsuranceMVC.Controllers
{
    public class ClaimOfficerController : Controller
    {
        AutoClaimContext db = new AutoClaimContext();
        // GET: ClaimOfficer
        //[Authorize]
        [HttpGet]
        [Authorize]
        public ActionResult ClaimOfficerPage()
        {
            if ((Session["officerId"] != null) && Session["role"].ToString() == "Claim officer")
            {

                var progress = db.Claims.Where(p => p.status.Equals("progress")).Count();
                ViewBag.progress = progress;
                var evaluated = db.Claims.Where(e => e.status.Equals("evaluated")).Count();
                ViewBag.evaluated = evaluated;
                var claimed = db.Claims.Where(e => e.status.Equals("claimed")).Count();
                ViewBag.claimed = claimed;
                return View();
            }
            else
            {
                return RedirectToAction("OfficerLogin", "Officer");
            }
            
            
            
        }
        [Authorize]
        public ActionResult ClaimProgress()
        {

            if ((Session["officerId"] != null) && Session["role"].ToString() == "Claim officer")
            {
                var claim = db.Claims.Where(c => c.status.Equals("progress")).ToList();
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
            else
            {
                return RedirectToAction("OfficerLogin", "Officer");
            }
        }

           
            [Authorize]
        public ActionResult ClaimDetails(string claimId)
        {

            if ((Session["officerId"] != null) && Session["role"].ToString() == "Claim officer")
            {
                int claimid = int.Parse(claimId);
                var claimDetails = db.Claims.Where(c => c.claimId == claimid).FirstOrDefault();
                ViewBag.Report = db.Reports.Where(r => r.status.Equals("pending")).ToList();
                ViewBag.Assessor = db.Officers.Where(o => o.role.Equals("Assessor")).ToList();
                return View(claimDetails);
            }
            else
            {
                return RedirectToAction("OfficerLogin", "Officer");
            }


        }
        [HttpPost]
        [Authorize]
        public ActionResult Assign( string officerId,string claimId)
        {
            
                int claimID = int.Parse(claimId);
                Report report = new Report();
                report.officerId = officerId;
                report.claimId = claimID;
                report.status = "pending";
                report.content = "pending";
                report.reportDate = DateTime.Now;
                report.amount = 0.0;
                db.Reports.Add(report);
                db.SaveChanges();
                var claim = db.Claims.Where(c => c.claimId.Equals(report.claimId)).FirstOrDefault();
                claim.status = "assigned";
                db.Entry(claim).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("ClaimProgress");
         
        }
        [HttpGet]
        [Authorize]
        public ActionResult CompletedReport()
        {
            if ((Session["officerId"] != null) && Session["role"].ToString() == "Claim officer")
            {
                var completedreport = db.Reports.Where(c => c.status.Equals("completed")).ToList();
                ViewBag.completedreport = completedreport;
                return View(completedreport);
            }
            else
            {
                return RedirectToAction("OfficerLogin", "Officer");
            }
        }
       
        [Authorize]
        public ActionResult AcceptClaim(string claimId,string reportId)
        {
            if ((Session["officerId"] != null) && Session["role"].ToString() == "Claim officer")
            {
                int claimID = int.Parse(claimId);
                var acceptedclaim = db.Claims.Where(c => c.claimId == claimID).FirstOrDefault();
                acceptedclaim.status = "claimed";
                db.Entry(acceptedclaim).State = EntityState.Modified;
                db.SaveChanges();
                int reportID = int.Parse(reportId);
                var report = db.Reports.Where(r => r.reportId == reportID).FirstOrDefault();
                report.status = "claimed";
                db.Entry(report).State = EntityState.Modified;
                db.SaveChanges();

                var data = db.Claims.Where(c => c.claimId == claimID).FirstOrDefault();
                Mail obj = new Mail();
                obj.ToEmail = data.MailID;
                obj.EmailSubject = "Claim-status---This is an auto Generated Mail";
                obj.EMailBody = "Your insurance is claimed, claim authorizer will contact you soon";
                WebMail.SmtpServer = "smtp.gmail.com";
                WebMail.SmtpPort = 587;
                WebMail.SmtpUseDefaultCredentials = true;
                WebMail.EnableSsl = true;
                WebMail.UserName = "autoclaiminsurance";
                WebMail.Password = "Mail password";
                WebMail.From = "Your mailID here";
                WebMail.Send(to: obj.ToEmail, subject: obj.EmailSubject, body: obj.EMailBody, cc: obj.EmailCC, bcc: obj.EmailBCC, isBodyHtml: true);
                return RedirectToAction("CompletedReport");
            }
            else
            {
                return RedirectToAction("OfficerLogin", "Officer");
            }
        }
        [HttpGet]
        [Authorize]
        public ActionResult RejectClaim(string claimId, string reportId)
        {
            if ((Session["officerId"] != null) && Session["role"].ToString() == "Claim officer")
            {

                int claimID = int.Parse(claimId);
                var rejectclaim = db.Claims.Where(c => c.claimId == claimID).FirstOrDefault();
                rejectclaim.status = "rejected";
                db.Entry(rejectclaim).State = EntityState.Modified;
                db.SaveChanges();
                int reportID = int.Parse(reportId);
                var report = db.Reports.Where(r => r.reportId == reportID).FirstOrDefault();
                report.status = "rejected";
                db.Entry(report).State = EntityState.Modified;
                db.SaveChanges();

                var data = db.Claims.Where(c => c.claimId == claimID).FirstOrDefault();
                Mail obj = new Mail();
                obj.ToEmail = data.MailID;
                obj.EmailSubject = "Claim-status---This is an auto Generated Mail";
                obj.EMailBody = "Sorry, Your claim is rejected due to discrepancy found in details";
                WebMail.SmtpServer = "smtp.gmail.com";
                WebMail.SmtpPort = 587;
                WebMail.SmtpUseDefaultCredentials = true;
                WebMail.EnableSsl = true;
                WebMail.UserName = "autoclaiminsurance";
                WebMail.Password = "Mail Password";
                WebMail.From = "Your mailId here";
                WebMail.Send(to: obj.ToEmail, subject: obj.EmailSubject, body: obj.EMailBody, cc: obj.EmailCC, bcc: obj.EmailBCC, isBodyHtml: true);
                return RedirectToAction("CompletedReport");
            }
            else
            {
                return RedirectToAction("OfficerLogin", "Officer");
            }

        }
        [HttpGet]
        [Authorize]
        public ActionResult AcceptedClaim()
        {
            if ((Session["officerId"] != null) && Session["role"].ToString() == "Claim officer")
            {
                var acceptedclaim = db.Claims.Where(c => c.status.Equals("claimed")).ToList();
                ViewBag.acceptedclaim = acceptedclaim;
                return View(acceptedclaim);
            }
            else
            {
                return RedirectToAction("OfficerLogin", "Officer");
            }
        }
        public ActionResult RejectedClaim()
        {
            if ((Session["officerId"] != null) && Session["role"].ToString() == "Claim officer")
            {
                var rejectedclaim = db.Claims.Where(c => c.status.Equals("Rejected")).ToList();
                ViewBag.rejectedclaim = rejectedclaim;
                return View(rejectedclaim);
            }
            else
            {
                return RedirectToAction("OfficerLogin", "Officer");
            }

        }
        [Authorize]
        public ActionResult AcceptedClaimDetails(string claimId)
        {
            if ((Session["officerId"] != null) && Session["role"].ToString() == "Claim officer")
            {
                int claimid = int.Parse(claimId);
                var acceptedClaimDetails = db.Claims.Where(c => c.claimId == claimid).FirstOrDefault();
                return View(acceptedClaimDetails);
            }
            else
            {
                return RedirectToAction("OfficerLogin", "Officer");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        
    }

}
