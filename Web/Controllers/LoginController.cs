using Model.Dao;
using avSVAW.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Model.DataModel;
using System.IO;
using Common;
using avSVAW.App_Start;
using System.Threading;
using System.Globalization;

namespace avSVAW.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Index()
        {
            
            var sessionLang = Session[GlobalConstants.LANG_SESSION];
            string culture = "vi-VN";
            if (sessionLang != null)
            {
                string lang = Convert.ToString(sessionLang);
                culture = string.Empty;
                if (lang.ToLower().CompareTo("vi") == 0 || string.IsNullOrEmpty(culture))
                {
                    culture = "vi-VN";
                }
                if (lang.ToLower().CompareTo("en") == 0 || string.IsNullOrEmpty(culture))
                {
                    culture = "en-US";
                }

            }
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(culture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
            
            try
            {
                if (Session[GlobalConstants.USER_SESSION] != null)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return View();
                }
            }
            catch (Exception e){
                return View();
            }

        }
        public ActionResult Logout()
        {
            //reset old session (method logout)
            Session[GlobalConstants.USER_SESSION] = null;
            return RedirectToAction("Index");
        }

        public ActionResult Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                //Check quyen user cho phep login
                var dao = new UserDao();
                tblUser user = dao.GetBytblUserName(model.UserName);

                string passMd5 = Encryptor.MD5Hash(model.Password);
                var result = dao.Login(model.UserName, passMd5);
                if (result == 1)
                {
                    if (!string.IsNullOrEmpty(user.Avatar))
                    {
                        //update avatar url
                        var baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
                        string imagePath = "";//GlobalConstants.AVATAR_LOCATION.Replace("~", string.Empty);
                        string imgAddress = string.Format("{0}{1}{2}", baseUrl, imagePath, user.Avatar);
                        user.Avatar = imgAddress;
                    }
                   
                    Session.Add(GlobalConstants.USER_SESSION, user);
                    return RedirectToAction("Index", "Home");
                }
                else if (result == 0)
                {
                    ModelState.AddModelError("", "Tài khoản không tồn tại.");
                }
                else if (result == -1)
                {
                    ModelState.AddModelError("", "Tài khoản đang bị khoá.");
                }
                else if (result == -2)
                {
                    ModelState.AddModelError("", "Mật khẩu không đúng.");
                }
                else if (result == -3)
                {
                    ModelState.AddModelError("", "Tài khoản của bạn không có quyền đăng nhập.");
                }
                else
                {
                    ModelState.AddModelError("", "đăng nhập không đúng.");
                }
            }
            return View("Index");
        }
    }
}