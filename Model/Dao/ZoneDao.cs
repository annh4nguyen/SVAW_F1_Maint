using Model.DataModel;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Dao
{
    public class ZoneDao
    {
        AvaniDataContext db = null;
        public ZoneDao()
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

        public long Insert(tblZone entity)
        {
            try
            {
                db.tblZones.InsertOnSubmit(entity);
                db.SubmitChanges();
            }
            catch { }
            return entity.Id;
        }

        public bool Update(tblZone entity)
        {
            try
            {
                var tblZone = db.tblZones.SingleOrDefault(x => x.Id == entity.Id);
                tblZone.Name = entity.Name;
                tblZone.Description = entity.Description;
                tblZone.Color = entity.Color;
                tblZone.FactoryId = entity.FactoryId;
                db.SubmitChanges();
                return true;
            }
            catch (Exception ex)
            {
                //logging
                return false;
            }

        }

        public List<tblZone> listAll()
        {
            return db.tblZones.ToList();
        }
        public IEnumerable<tblZone> ListAllPaging(string searchString)
        {
            IQueryable<tblZone> model = db.tblZones;
            if (!string.IsNullOrEmpty(searchString))
            {
                model = model.Where(x => x.Name.Contains(searchString));
            }

            return model.OrderByDescending(x => x.Name).ToList();
        }

        public tblZone ViewDetail(int id)
        {
            try
            {
                return db.tblZones.SingleOrDefault(x => x.Id == id);
            }
            catch
            {
                return null;
            }
        }
        public string GettblZoneNameById(int? id)
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
                var zone = db.tblZones.SingleOrDefault(x => x.Id == id);
                db.tblZones.DeleteOnSubmit(zone);
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
