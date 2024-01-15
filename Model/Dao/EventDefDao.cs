using Model.DataModel;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Dao
{
    public class EventDefDao
    {
        AvaniDataContext db = null;
        public EventDefDao()
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

        public long Insert(tblEventDef entity)
        {
            try
            {
                db.tblEventDefs.InsertOnSubmit(entity);
                db.SubmitChanges();
            }
            catch { }
            return entity.Id;
        }

        public bool Update(tblEventDef entity)
        {
            try
            {
                var eventDef = db.tblEventDefs.SingleOrDefault(x => x.Id == entity.Id);
                eventDef.Name = entity.Name;
                eventDef.Description = entity.Description;
                eventDef.zOrder = entity.zOrder;
                eventDef.Color = entity.Color;
                eventDef.UsingSound = entity.UsingSound;
                eventDef.SoundFileName = entity.SoundFileName;
                db.SubmitChanges();
                return true;
            }
            catch (Exception ex)
            {
                //logging
                return false;
            }

        }

        public List<tblEventDef> listAll()
        {
            //List<tblEventDef> model = db.tblEventDefs.ToList();

            return db.tblEventDefs.OrderBy( x => x.zOrder).ToList();
        }
        public IEnumerable<tblEventDef> ListAllPaging(string searchString)
        {
            IQueryable<tblEventDef> model = db.tblEventDefs;
            if (!string.IsNullOrEmpty(searchString))
            {
                model = model.Where(x => x.Name.Contains(searchString));
            }

            return model.OrderByDescending(x => x.Name).ToList();
        }

        public tblEventDef GettblEventDefById(int Id)
        {
            return db.tblEventDefs.SingleOrDefault(x => x.Id == Id);
        }
        public tblEventDef ViewDetail(int id)
        {
            try
            {
                return db.tblEventDefs.SingleOrDefault(x => x.Id == id);
            }
            catch
            {
                return null;
            }
        }
        public string GettblEventDefNameById(int id)
        {
            string result = "";
            if (id != 0)
            {
                var obj = ViewDetail(id);
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
                var ev = db.tblEventDefs.SingleOrDefault(x => x.Id == id);
                db.tblEventDefs.DeleteOnSubmit(ev);
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
