using Model.DataModel;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Dao
{
    public class M_ActionDao
    {
        AvaniDataContext db = null;
        public M_ActionDao()
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

        public long Insert(tblM_Action entity)
        {
            try
            {
                db.tblM_Actions.InsertOnSubmit(entity);
                db.SubmitChanges();
            }
            catch { }
            return entity.Id;
        }

        public bool Update(tblM_Action entity)
        {
            try
            {
                var tblM_Action = db.tblM_Actions.SingleOrDefault(x => x.Id == entity.Id);
                tblM_Action.Name = entity.Name;
                tblM_Action.Description = entity.Description;
                tblM_Action.nOrder = entity.nOrder;
                db.SubmitChanges();
                return true;
            }
            catch (Exception ex)
            {
                //logging
                return false;
            }

        }

        public List<tblM_Action> listAll()
        {
            return db.tblM_Actions.OrderBy(x => x.nOrder).ToList();
        }
        public tblM_Action ViewDetail(int id)
        {
            try
            {
                return db.tblM_Actions.SingleOrDefault(x => x.Id == id);
            }
            catch
            {
                return null;
            }
        }
        public string GettblM_ActionNameById(int? id)
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
                var line = db.tblM_Actions.SingleOrDefault(x => x.Id == id);
                db.tblM_Actions.DeleteOnSubmit(line);
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
