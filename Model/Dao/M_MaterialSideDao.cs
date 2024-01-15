using Model.DataModel;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Dao
{
    public class M_MaterialSideDao
    {
        AvaniDataContext db = null;
        public M_MaterialSideDao()
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

        public long Insert(tblM_MaterialSide entity)
        {
            try
            {
                db.tblM_MaterialSides.InsertOnSubmit(entity);
                db.SubmitChanges();
            }
            catch { }
            return entity.Id;
        }

        public bool Update(tblM_MaterialSide entity)
        {
            try
            {
                var tblM_MaterialSide = db.tblM_MaterialSides.SingleOrDefault(x => x.Id == entity.Id);
                tblM_MaterialSide.Name = entity.Name;
                tblM_MaterialSide.Description = entity.Description;
                tblM_MaterialSide.nOrder = entity.nOrder;
                db.SubmitChanges();
                return true;
            }
            catch (Exception ex)
            {
                //logging
                return false;
            }

        }

        public List<tblM_MaterialSide> listAll()
        {
            return db.tblM_MaterialSides.OrderBy(x => x.nOrder).ToList();
        }
        public tblM_MaterialSide ViewDetail(int id)
        {
            try
            {
                return db.tblM_MaterialSides.SingleOrDefault(x => x.Id == id);
            }
            catch
            {
                return null;
            }
        }
        public string GettblM_MaterialSideNameById(int? id)
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
                var line = db.tblM_MaterialSides.SingleOrDefault(x => x.Id == id);
                db.tblM_MaterialSides.DeleteOnSubmit(line);
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
