using Model.DataModel;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Dao
{
    public class M_MaintHistoryDao
    {
        AvaniDataContext db = null;
        public M_MaintHistoryDao()
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

        public long Insert(tblM_MaintHistory entity)
        {
            try
            {
                db.tblM_MaintHistories.InsertOnSubmit(entity);
                db.SubmitChanges();
            }
            catch { }
            return entity.Id;
        }

        public bool Update(tblM_MaintHistory entity)
        {
            try
            {
                var tblM_MaintHistory = db.tblM_MaintHistories.SingleOrDefault(x => x.Id == entity.Id);
                tblM_MaintHistory.NodeId = entity.NodeId;
                tblM_MaintHistory.MaterialId = entity.MaterialId;
                tblM_MaintHistory.ActionDate = entity.ActionDate;
                tblM_MaintHistory.ActionId = entity.ActionId;
                tblM_MaintHistory.ActionUserName  = entity.ActionUserName;
                tblM_MaintHistory.RunningHour = entity.RunningHour;
                tblM_MaintHistory.NextAction = entity.NextAction;
                tblM_MaintHistory.Description = entity.Description;

                db.SubmitChanges();
                return true;
            }
            catch (Exception ex)
            {
                //logging
                return false;
            }

        }

        public List<tblM_MaintHistory> listAll()
        {
            return db.tblM_MaintHistories.OrderBy(x => x.Id).ToList();
        }
        public tblM_MaintHistory ViewDetail(int id)
        {
            try
            {
                return db.tblM_MaintHistories.SingleOrDefault(x => x.Id == id);
            }
            catch
            {
                return null;
            }
        }
    
        public bool Delete(int id)
        {
            try
            {
                var line = db.tblM_MaintHistories.SingleOrDefault(x => x.Id == id);
                db.tblM_MaintHistories.DeleteOnSubmit(line);
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
