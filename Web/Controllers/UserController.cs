using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Common;
using Model.Dao;
using Model.DataModel;
using avSVAW.Common;
using avSVAW.Models;
using avSVAW.App_Start;
using System.Configuration;

namespace avSVAW.Controllers
{
    [SessionTimeout]
    public class UserController : BaseController
    {
        public static bool Is_Sumiden =Convert.ToBoolean(ConfigurationManager.AppSettings["Is_Sumiden"].ToString());
        public ActionResult Index()
        {
            var dao = new UserDao();
            var lstUser = dao.ListAll();
            var model = new List<UserForm>();
            foreach (tblUser u in lstUser)
            {
                model.Add(new UserForm().GetUserForm(u));
            }

            return View(model);
        }

        public ActionResult Create()
        {
            Models.UserForm userModel = EditOrCreate(null);
            return View(userModel);
        }

        public Models.UserForm EditOrCreate(tblUser UserObj = null)
        {
            Models.UserForm userModel =new UserForm();

            if (UserObj != null)
            {
                userModel = new UserForm().GetUserForm(UserObj);
            }
            else
            {
                userModel.Status = true; //default cho form create la kich hoat trang thai
            }


            userModel.CreatedDate = DateTime.Now;

            userModel.Roles = new List<SelectListItem>();
            userModel.Roles.Add(new SelectListItem() { Value = GlobalConstants.AV_MANAGER_GROUP, Text = GlobalConstants.AV_MANAGER_GROUP });
            userModel.Roles.Add(new SelectListItem() { Value = GlobalConstants.AV_ADMIN_GROUP, Text = GlobalConstants.AV_ADMIN_GROUP });
            userModel.Roles.Add(new SelectListItem() { Value = GlobalConstants.AV_STAFF_GROUP, Text = GlobalConstants.AV_STAFF_GROUP });
            if(Is_Sumiden)  // cong ty sumiden them ngoai le
            {
                userModel.Roles.Add(new SelectListItem() { Value = GlobalConstants.AV_STAFF_PlASTIC_GROUP, Text = GlobalConstants.AV_STAFF_PlASTIC_GROUP });
                userModel.Roles.Add(new SelectListItem() { Value = GlobalConstants.AV_STAFF_COPPER_GROUP, Text = GlobalConstants.AV_STAFF_COPPER_GROUP });
            }
            //userModel.RoleListItem.Add(new SelectListItem() { Value = GlobalConstants.MT_USER_GROUP, Text = "Người dùng" });


            return userModel;
        }


        public ActionResult SelfEdit(int id)
        {
            var userObj = new UserDao().ViewDetail(id);
            //ViewBag.DepartmentName = new DepartmentDao().GetDepartmentNameById(userObj.DepartmentId);
            //string RoleName = "";
            //switch (userObj.Role)
            //{
            //    case "MANAGER":
            //        RoleName = "Giám đốc";
            //        break;
            //    //case "ACCOUNTANT":
            //    //    RoleName = "Kế toán";
            //    //    break;
            //    case "STAFF":
            //        RoleName = "Giao nhận";
            //        break;
            //    case "ADMIN":
            //        RoleName = "Quản trị";
            //        break;
            //}
            ViewBag.RoleName = userObj.Role;

            return View(userObj);
        }

        [HttpPost]
        public ActionResult SelfEdit(tblUser userObj)
        {
            if (ModelState.IsValid)
            {

                var dao = new UserDao();
                if (!string.IsNullOrEmpty(userObj.Password))
                {
                    var encryptedMd5Pas = Encryptor.MD5Hash(userObj.Password);
                    userObj.Password = encryptedMd5Pas;
                }
                var result = dao.Update(userObj, true);
                if (result)
                {
                    SetAlert("Sửa user thành công", "success");
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Cập nhật user không thành công");
                }
            }
            return View(userObj);
        }

        [HttpPost]
        public ActionResult Create(UserForm userForm)
        {
            if (ModelState.IsValid)
            {
                tblUser userObj = new UserForm().GetBaseUser(userForm);
                //validate
                if(userObj != null)
                {
                    var userExist = new UserDao().GetBytblUserName(userObj.UserName);
                    if(userExist != null)
                    {
                        //da ton tai user nay
                        ModelState.AddModelError("", "Đã tồn tại User này");
                        Models.UserForm userModel = EditOrCreate(null);
                        userModel.UserName = userExist.UserName;
                        return View(userModel);
                    }
                }

   

                //string server_path = Server.MapPath(GlobalConstants.AVATAR_LOCATION);

                //userObj.Avatar = server_path + "/" + userObj.Role + ".png";                
                //userObj.Avatar = server_path + "/user.png";

                var dao = new UserDao();
                var encryptedMd5Pas = Encryptor.MD5Hash(userObj.Password);
                userObj.Password = encryptedMd5Pas;

                long id = dao.Insert(userObj);
                if (id > 0)
                {
                    SetAlert("Thêm user thành công", "success");
                    return RedirectToAction("Index", "User");
                }
                else
                {
                    ModelState.AddModelError("", "Thêm user không thành công");
                }
            }
            return View();
        }
        public ActionResult Edit(int id)
        {
            var userObj = new UserDao().ViewDetail(id);
            Models.UserForm userModel = EditOrCreate(userObj);

      
            return View(userModel);
        }


        [HttpPost]
        public ActionResult Edit(UserForm userForm, HttpPostedFileBase upload)
        {
            if (ModelState.IsValid)
            {
                tblUser userObj = new UserForm().GetBaseUser(userForm);
       

                var dao = new UserDao();
                if (!string.IsNullOrEmpty(userObj.Password))
                {
                    var encryptedMd5Pas = Encryptor.MD5Hash(userObj.Password);
                    userObj.Password = encryptedMd5Pas;
                }
                var result = dao.Update(userObj);
                if (result)
                {
                    SetAlert("Sửa user thành công", "success");
                    return RedirectToAction("Index", "User");
                }
                else
                {
                    ModelState.AddModelError("", "Cập nhật user không thành công");
                }
            }
            return View(userForm);
        }


        public ActionResult Delete(int id)
        {
            new UserDao().Delete(id);

            return RedirectToAction("Index");
        }
    }

}