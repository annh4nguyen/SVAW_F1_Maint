using Model.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace avSVAW.Models
{
    public class UserForm : tblUser
    {
        public List<SelectListItem> Roles { get; set; }
        public tblUser GetBaseUser(UserForm entity)
        {
            tblUser user = new tblUser();
            user.ID = entity.ID;
            user.UserName = entity.UserName;
            user.Password = entity.Password;
            user.Role = entity.Role;
            user.FullName = entity.FullName;
            user.Email = entity.Email;
            user.Phone = entity.Phone;
            user.Avatar = entity.Avatar;
            user.CreatedDate = entity.CreatedDate;
            user.Status = entity.Status;

            return user;
        }

        public UserForm GetUserForm (tblUser entity)
        {
            UserForm user = new UserForm();
            user.ID = entity.ID;
            user.UserName = entity.UserName;
            user.Password = entity.Password;
            user.Role = entity.Role;
            user.FullName = entity.FullName;
            user.Email = entity.Email;
            user.Phone = entity.Phone;
            user.Avatar = entity.Avatar;
            user.CreatedDate = entity.CreatedDate;
            user.Status = entity.Status;

            return user;
        }
    }
}