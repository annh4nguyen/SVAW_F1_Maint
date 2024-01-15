using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace avSVAW.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Yêu cầu nhập user name")]
        public string UserName { set; get; }

        [Required(ErrorMessage = "Yêu cầu nhập password")]
        public string Password { set; get; }
        
    }
}