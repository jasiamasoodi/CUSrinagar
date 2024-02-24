using CaptchaMvc.HtmlHelpers;
using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CUSrinagar.WebApp.Controllers
{
    public class AccountController : Controller
    {
        #region SignIn
        [HttpGet]
        public ActionResult SignIn(string R = null)
        {
            ViewBag.ReturnUrl = R + "";
            return View();
        }

        [HttpGet]
        [OAuthorize]
        public ActionResult SignOut()
        {
            List<AppRoles> roles = AppUserHelper.AppUsercompact?.UserRoles;
            if (System.Web.HttpContext.Current.User.Identity.IsAuthenticated)
            {
                new AuthenticationManager().SignOut();
            }
            if (roles.Any(x => x == AppRoles.Student))
                return RedirectToAction("StudentZone", "Account", new { area = string.Empty });
            else
            {
                //AUTO Admission cancel
                new ServiceManager().AutoAdmissionCancelAsync();
                return RedirectToAction("SignIn", "Account", new { area = string.Empty });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignIn(AppLogin appLogin)
        {
            ViewBag.ReturnUrl = appLogin.ReturnUrl;

            Tuple<SignInStatus, string> response = new AuthenticationManager().SignIn(appLogin);
            if (response.Item1 == SignInStatus.Success)
            {
                if (string.IsNullOrWhiteSpace(appLogin.ReturnUrl) || !Url.IsLocalUrl(appLogin.ReturnUrl))
                {
                    if (response.Item2.Contains(AppRoles.College.ToString()))
                        return RedirectToAction("Index", "Dashboard", new { area = "CUCollegeAdminPanel" });
                    else
                        return RedirectToAction("Index", "Dashboard", new { area = "CUSrinagarAdminPanel" });
                }
                else
                {
                    return Redirect(appLogin.ReturnUrl);
                }

            }
            else
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><a href='#' class='alert-link'>{response.Item2}</a></div>";
            return View();

        }

        [HttpGet]
        public ActionResult Forbidden()
        {
            return View();
        }

        #endregion



        #region ForgetPassword
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(AppLogin model)
        {
            if (new AccountManager().ForwardPasswordLink(model.UserName))
            {
                TempData["response"] = $"<div class='alert alert-success alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><a href='#' class='alert-link'>Password Reset link Sent Succesfully on your Email</a></div>";
            }
            else
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'>UserName Or Email does not exist</a></div>";
            }
            return View(model);
        }

        [HttpGet]
        public ActionResult ReSetPassword(string generatedToken)
        {
            if (!string.IsNullOrWhiteSpace(generatedToken))
            {
                ForgotPassword AppUser = new AccountManager().GetUserByToken(generatedToken);
                if (AppUser != null)
                {
                    return View(AppUser);
                }
            }
            return View("SignIn");
        }


        [HttpPost]
        [AllowAnonymous]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        [ValidateAntiForgeryToken]
        public ActionResult ReSetPassword(ForgotPassword model)
        {
            if (!string.IsNullOrEmpty(model.Password) && !string.IsNullOrEmpty(model.ConfirmPassword) && model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Password and Confirm Password should be same");
            }
            if (ModelState.IsValid)
            {
                if (new AccountManager().ChangePassword(model))
                {
                    TempData["response"] = $"<div class='alert alert-success alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><a href='#' class='alert-link'>Password changed Succesfully</a></div>";

                }
                else
                {
                    TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong>Oh snap!</strong> <a href='#' class='alert-link'>Reasons: <br/>1. UserName Or Email does not exist <br/>2. Previous and new passwords should not be same.</a></div>";
                }

            }

            return View(model);

        }
        #endregion



        #region StudentZone-SignIn
        [HttpGet]
        public ActionResult StudentZone(string R = null)
        {
            ViewBag.IsLocalHost = !HttpContext.Request.Url.Host.Contains("cusrinagar.edu.in");
            var selectListItems = Helper.GetSelectList<PrintProgramme>().ToList();
            selectListItems.Insert(0, new SelectListItem() { Selected = true, Text = " Select Programme " });
            ViewBag.PrintProgrammes = selectListItems;

            ViewBag.PrintProgrammeOption = Helper.GetSelectList<PrintProgramme>().OrderBy(x => x.Value);
            ViewBag.ReturnUrl = "";
            if (!string.IsNullOrWhiteSpace(R))
                ViewBag.ReturnUrl = R;

            return View();
        }


        public JsonResult GetStudentsForLogin(PrintProgramme? programme)
        {
            ViewBag.IsLocalHost = !HttpContext.Request.Url.Host.Contains("cusrinagar.edu.in");
            return Json(new StudentManager().GetStudentsForLogin(programme), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetParticularStudentForLogin(Guid College_ID, Guid Course_ID, int? Batch)
        {
            ViewBag.IsLocalHost = !HttpContext.Request.Url.Host.Contains("cusrinagar.edu.in");
            return Json(new StudentManager().GetParticularStudentForLogin(College_ID, Course_ID, Batch), JsonRequestBehavior.DenyGet);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StudentZone(ARGReprint aRGReprint, string R = null)
        {
            bool isProduction = HttpContext.Request.Url.Host.Contains("cusrinagar.edu.in");
            ViewBag.IsLocalHost = !isProduction;

            ViewBag.ReturnUrl = "";
            if (!string.IsNullOrWhiteSpace(R))
                ViewBag.ReturnUrl = R;

            ViewBag.PrintProgrammeOption = Helper.GetSelectList<PrintProgramme>().OrderBy(x => x.Value);

            if (!(this?.IsCaptchaValid("") ?? false) && isProduction)
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><a href='#' class='alert-link'>Invalid Verification Code</a></div>";
                return RedirectToAction("StudentZone");
            }
            else
            {
                if (!isProduction)
                {
                    ModelState.Remove("CaptchaInputText");
                }

                if (!ModelState.IsValid)
                    return View();

                Tuple<SignInStatus, string> response = new AuthenticationManager().SignIn(aRGReprint);

                if (response.Item1 == SignInStatus.Success)
                {
                    if (string.IsNullOrWhiteSpace(R))
                        return RedirectToAction("GetInfo", "StudentInfo", new { area = "CUStudentZone" });
                    else
                    {
                        if (!string.IsNullOrEmpty(R)) //(Url.IsLocalUrl(R))
                            return Redirect(R);
                        else
                            return RedirectToAction("GetInfo", "StudentInfo", new { area = "CUStudentZone" });
                    }
                }
                else
                    TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><a href='#' class='alert-link'>{response.Item2}</a></div>";
                return View();
            }
        }

        #endregion


        [OAuthorize]
        [HttpGet]
        public ActionResult ResetUserPassword()
        {
            return View();
        }

        [HttpPost]
        [OAuthorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        [ValidateAntiForgeryToken]
        public ActionResult ResetUserPassword(ForgotPassword model)
        {
            if (!string.IsNullOrEmpty(model.Password) && !string.IsNullOrEmpty(model.ConfirmPassword) && model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Password and Confirm Password should be same");
            }
            ModelState.Remove(nameof(model.UserName));

            if (ModelState.IsValid)
            {
                if (new AccountManager().ChangeUserPassword(model))
                {
                    Tuple<SignInStatus, string> response = new AuthenticationManager().SignIn(new AppLogin { UserName = model.UserName, Password = model.Password });

                    if (response.Item2.Contains(AppRoles.College.ToString()))
                        return RedirectToAction("Index", "Dashboard", new { area = "CUCollegeAdminPanel" });
                    else
                        return RedirectToAction("Index", "Dashboard", new { area = "CUSrinagarAdminPanel" });
                }
                else
                {
                    TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> Previous and new passwords should not be same.</a></div>";
                }

            }

            return View(model);

        }
    }
}