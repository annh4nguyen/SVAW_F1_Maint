using Model.DataModel;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Dao
{
    public class M_MaterialDao
    {
        AvaniDataContext db = null;
        public M_MaterialDao()
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

        public long Insert(tblM_Material entity)
        {
            try
            {
                db.tblM_Materials.InsertOnSubmit(entity);
                db.SubmitChanges();
            }
            catch { }
            return entity.Id;
        }

        public bool Update(tblM_Material entity)
        {
            try
            {
                var tblM_Material = db.tblM_Materials.SingleOrDefault(x => x.Id == entity.Id);
                tblM_Material.Code = entity.Code;
                tblM_Material.Name = entity.Name;
                tblM_Material.Hour = entity.Hour;
                tblM_Material.WarningLevel = entity.WarningLevel;
                tblM_Material.Type = entity.Type;
                tblM_Material.Slide = entity.Slide;
                tblM_Material.ApplyFor = entity.ApplyFor;

                db.SubmitChanges();
                return true;
            }
            catch (Exception ex)
            {
                //logging
                return false;
            }

        }

        public List<tblM_Material> listAll()
        {
            return db.tblM_Materials.OrderBy(x => x.Id).ToList();
        }
        public tblM_Material ViewDetail(int id)
        {
            try
            {
                return db.tblM_Materials.SingleOrDefault(x => x.Id == id);
            }
            catch
            {
                return null;
            }
        }
        public string GettblM_MaterialNameById(int? id)
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
                var line = db.tblM_Materials.SingleOrDefault(x => x.Id == id);
                db.tblM_Materials.DeleteOnSubmit(line);
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
