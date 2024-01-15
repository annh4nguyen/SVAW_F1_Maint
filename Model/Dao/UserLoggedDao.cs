using Model.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Dao
{
    public class UserLoggedDao
    {
        AvaniDataContext db = null;
        public UserLoggedDao()
        {
            string con = System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString;
            if (!string.IsNullOrEmpty(con))
            {
                db = new AvaniDataContext(con);
            }
            else
            {
                db = new AvaniDataContext();
            }
        }
        public int UserLoggedToday()
        {
            int result = 0;
            DateTime dtToday = DateTime.Today;
            var list = db.tblUserLoggeds.Where(x=>x.TimeLogged.Year== dtToday.Year && x.TimeLogged.Month== dtToday.Month && x.TimeLogged.Day==dtToday.Day).ToList();
            if (list != null)
            {
                result = list.Count;
            }
            return result;
        }
        public void InserOrUpdateUser(string username)
        {
            try
            {
                var userLogObj = db.tblUserLoggeds.Where(x => x.UserName.Equals(username)).SingleOrDefault();
                if (userLogObj == null)
                {
                    userLogObj = new tblUserLogged();
                    userLogObj.UserName = username;
                    userLogObj.TimeLogged = DateTime.Now;
                    db.tblUserLoggeds.InsertOnSubmit(userLogObj);
                    db.SubmitChanges();
                }
                else
                {
                    userLogObj.TimeLogged = DateTime.Now;
                    db.SubmitChanges();
                }
            }
            catch { }
        }
    }
}
