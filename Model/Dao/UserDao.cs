using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PagedList;
using System.Configuration;
using Common;
using Model.DataModel;

namespace Model.Dao
{
    public class UserDao
    {
        AvaniDataContext db = null;
        public UserDao()
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
        public int GetMaxId()
        {
            return db.tblUsers.OrderByDescending(x => x.ID).FirstOrDefault().ID;

        }
        public long Insert(tblUser entity)
        {
            try
            {
                entity.ID = GetMaxId() + 1;
                db.tblUsers.InsertOnSubmit(entity);
                db.SubmitChanges();
            }
            catch (Exception e)
            {
            }
            return entity.ID;
        }

        public bool Update(tblUser entity, bool SelfEdit = false)
        {
            try
            {
                var user = db.tblUsers.SingleOrDefault(x => x.ID == entity.ID);
                if (SelfEdit)
                {
                    user.FullName = entity.FullName;
                    if (!string.IsNullOrEmpty(entity.Password))
                    {
                        user.Password = entity.Password;
                    }
                    user.Email = entity.Email;
                }
                else
                {
                    user.FullName = entity.FullName;
                    user.Role = entity.Role;
                    if (!string.IsNullOrEmpty(entity.Password))
                    {
                        user.Password = entity.Password;
                    }
                    user.Phone = entity.Phone;
                    user.Email = entity.Email;
                    if (entity.Avatar != null)
                    {
                        user.Avatar = entity.Avatar;
                    }

                    user.Status = entity.Status;
                }
                db.SubmitChanges();
                return true;
            }
            catch (Exception ex)
            {
                //logging
                return false;
            }

        }

        public IEnumerable<tblUser> ListAll()
        {
            return db.tblUsers.Where(x => x.Role != "CONFIG").ToList();
        }

       
        public tblUser GetBytblUserName(string userName)
        {
            try
            {
                return db.tblUsers.SingleOrDefault(x => x.UserName == userName);
            }
            catch
            {
                return null;
            }
        }

        public tblUser ViewDetail(int id)
        {
            try
            {
                return db.tblUsers.SingleOrDefault(x => x.ID == id);
            }
            catch
            {
                return null;
            }
        }
        public string GetFullNameById(int? id)
        {
            string result = "";
            if (id != null)
            {
                var obj = ViewDetail(id.GetValueOrDefault());
                if (obj != null)
                {
                    result = obj.FullName != null ? obj.FullName : "";
                }
            }
            return result;
        }
        public string GetUserNameById(int? id)
        {
            string result = "";
            if (id != null)
            {
                var obj = ViewDetail(id.GetValueOrDefault());
                if (obj != null)
                {
                    result = obj.UserName != null ? obj.UserName : "";
                }
            }
            return result;
        }

        public int Login(string userName, string passWord)
        {
            try
            {
                var result = db.tblUsers.SingleOrDefault(x => x.UserName == userName);

                if (result == null)
                {
                    return 0;
                }
                else
                {
                    if (result.Status == false)
                    {
                        return -1;
                    }
                    else
                    {
                        if (result.Password == passWord)
                        {
                            //update bo dem user login trong ngay
                            new UserLoggedDao().InserOrUpdateUser(userName);
                            return 1;
                        }
                        else
                            return -2;
                    }
                }
            }
            catch (Exception e)
            {
                return -2;
            }
        }

        public bool ChangeStatus(long id)
        {
            try
            {
                var user = db.tblUsers.SingleOrDefault(x => x.ID == id);
                user.Status = !user.Status;
                db.SubmitChanges();
                return user.Status;
            }
            catch
            {
                return false;
            }
        }
        public bool Delete(int id)
        {
            try
            {
                var user = db.tblUsers.SingleOrDefault(x => x.ID == id);
                db.tblUsers.DeleteOnSubmit(user);
                db.SubmitChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public bool ChecktblUserName(string userName)
        {
            return db.tblUsers.Count(x => x.UserName.Equals(userName)) > 0;
        }
        public bool CheckEmail(string email)
        {
            return db.tblUsers.Count(x => x.Email.Equals(email)) > 0;
        }
    }
}
