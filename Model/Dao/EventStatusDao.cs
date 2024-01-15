using Model.DataModel;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Dao
{
    public class EventStatusDao
    {
        AvaniDataContext db = null;
        public EventStatusDao()
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

        public long Insert(tblEventStatus entity)
        {
            try
            {
                db.tblEventStatus.InsertOnSubmit(entity);
                db.SubmitChanges();
            }
            catch { }
            return entity.Id;
        }

        public bool Update(tblEventStatus entity)
        {
            try
            {
                var tblEventStatus = db.tblEventStatus.SingleOrDefault(x => x.Id == entity.Id);
                tblEventStatus.Name = entity.Name;
                db.SubmitChanges();
                return true;
            }
            catch (Exception ex)
            {
                //logging
                return false;
            }

        }

        public List<tblEventStatus> listAll()
        {
            return db.tblEventStatus.ToList();
        }
        public IEnumerable<tblEventStatus> ListAllPaging(string searchString)
        {
            IQueryable<tblEventStatus> model = db.tblEventStatus;
            if (!string.IsNullOrEmpty(searchString))
            {
                model = model.Where(x => x.Name.Contains(searchString));
            }

            return model.OrderByDescending(x => x.Name).ToList();
        }

        public tblEventStatus ViewDetail(int id)
        {
            try
            {
                return db.tblEventStatus.SingleOrDefault(x => x.Id == id);
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
                var _es = db.tblEventStatus.SingleOrDefault(x => x.Id == id);
                db.tblEventStatus.DeleteOnSubmit(_es);
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
