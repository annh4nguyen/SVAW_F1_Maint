using Model.DataModel;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Dao
{
    public class M_MaterialTypeDao
    {
        AvaniDataContext db = null;
        public M_MaterialTypeDao()
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

        public long Insert(tblM_MaterialType entity)
        {
            try
            {
                db.tblM_MaterialTypes.InsertOnSubmit(entity);
                db.SubmitChanges();
            }
            catch { }
            return entity.Id;
        }

        public bool Update(tblM_MaterialType entity)
        {
            try
            {
                var tblM_MaterialType = db.tblM_MaterialTypes.SingleOrDefault(x => x.Id == entity.Id);
                tblM_MaterialType.Name = entity.Name;
                tblM_MaterialType.Description = entity.Description;
                tblM_MaterialType.nOrder = entity.nOrder;
                db.SubmitChanges();
                return true;
            }
            catch (Exception ex)
            {
                //logging
                return false;
            }

        }

        public List<tblM_MaterialType> listAll()
        {
            return db.tblM_MaterialTypes.OrderBy(x => x.nOrder).ToList();
        }
        public tblM_MaterialType ViewDetail(int id)
        {
            try
            {
                return db.tblM_MaterialTypes.SingleOrDefault(x => x.Id == id);
            }
            catch
            {
                return null;
            }
        }
        public string GettblM_MaterialTypeNameById(int? id)
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
                var line = db.tblM_MaterialTypes.SingleOrDefault(x => x.Id == id);
                db.tblM_MaterialTypes.DeleteOnSubmit(line);
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
