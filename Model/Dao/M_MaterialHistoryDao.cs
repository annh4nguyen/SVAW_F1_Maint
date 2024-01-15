using Model.DataModel;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Dao
{
    public class M_MaterialHistoryDao
    {
        AvaniDataContext db = null;
        public M_MaterialHistoryDao()
        {
            string con = System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString;
            if (!string.IsNullOrEmpty(con))
            {
                db = new AvaniDataContext(con);
            }else
            {
                db = new AvaniDataContext();
            }
        }

        public long Insert(tblM_MaterialHistory entity)
        {
            try
            {
                db.tblM_MaterialHistories.InsertOnSubmit(entity);
                db.SubmitChanges();
            }
            catch { }
            return entity.Id;
        }

        public bool Update(tblM_MaterialHistory entity)
        {
            try
            {
                var tblM_MaterialHistory = db.tblM_MaterialHistories.SingleOrDefault(x => x.Id == entity.Id);
                tblM_MaterialHistory.Code = entity.Code;
                tblM_MaterialHistory.Name = entity.Name;
                tblM_MaterialHistory.Hour = entity.Hour;
                tblM_MaterialHistory.WarningLevel = entity.WarningLevel;
                tblM_MaterialHistory.Type = entity.Type;
                tblM_MaterialHistory.Slide = entity.Slide;
                tblM_MaterialHistory.ApplyFor = entity.ApplyFor;

                db.SubmitChanges();
                return true;
            }
            catch (Exception ex)
            {
                //logging
                return false;
            }

        }

        public List<tblM_MaterialHistory> listAll()
        {
            return db.tblM_MaterialHistories.OrderBy(x => x.Id).ToList();
        }
        public tblM_MaterialHistory ViewDetail(int id)
        {
            try
            {
                return db.tblM_MaterialHistories.SingleOrDefault(x => x.Id == id);
            }
            catch
            {
                return null;
            }
        }
        public string GettblM_MaterialHistoryNameById(int? id)
        {
            string result = "";
            if (id != null)
            {
                var obj = ViewDetail(id.GetValueOrDefault());
                if (obj != null)
                {
                    result = obj.Name ?? "";
                }
            }
            return result;
        }

        public bool Delete(int id)
        {
            try
            {
                var line = db.tblM_MaterialHistories.SingleOrDefault(x => x.Id == id);
                db.tblM_MaterialHistories.DeleteOnSubmit(line);
                db.SubmitChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }
    }
}
