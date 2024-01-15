using Model.DataModel;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Dao
{
    public class FactoryDao
    {
        AvaniDataContext db = null;
        public FactoryDao()
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

        public long Insert(tblFactory entity)
        {
            try
            {
                db.tblFactories.InsertOnSubmit(entity);
                db.SubmitChanges();
            }
            catch { }
            return entity.Id;
        }

        public bool Update(tblFactory entity)
        {
            try
            {
                var factory = db.tblFactories.SingleOrDefault(x => x.Id == entity.Id);
                factory.Name = entity.Name;
                factory.Description = entity.Description;
                factory.IP = entity.IP;
                factory.MaxWaitTime = entity.MaxWaitTime;
                factory.MaxProcessTime = entity.MaxProcessTime;
                db.SubmitChanges();
                return true;
            }
            catch (Exception ex)
            {
                //logging
                return false;
            }

        }

        public List<tblFactory> listAll()
        {
            return db.tblFactories.ToList();
        }
        public IEnumerable<tblFactory> ListAllPaging(string searchString)
        {
            IQueryable<tblFactory> model = db.tblFactories;
            if (!string.IsNullOrEmpty(searchString))
            {
                model = model.Where(x => x.Name.Contains(searchString));
            }

            return model.OrderByDescending(x => x.Name).ToList();
        }
        public tblFactory ViewDetail(int id)
        {
            try
            {
                return db.tblFactories.SingleOrDefault(x => x.Id == id);
            }
            catch
            {
                return null;
            }
        }
        public string GettblFactoryNameById(int? id)
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
                var area = db.tblFactories.SingleOrDefault(x => x.Id == id);
                db.tblFactories.DeleteOnSubmit(area);
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
