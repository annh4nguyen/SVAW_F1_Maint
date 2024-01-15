using Model.DataModel;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Dao
{
    public class LineDao
    {
        AvaniDataContext db = null;
        public LineDao()
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

        public long Insert(tblLine entity)
        {
            try
            {
                db.tblLines.InsertOnSubmit(entity);
                db.SubmitChanges();
            }
            catch { }
            return entity.Id;
        }

        public bool Update(tblLine entity)
        {
            try
            {
                var tblLine = db.tblLines.SingleOrDefault(x => x.Id == entity.Id);
                tblLine.Code = entity.Code;
                tblLine.Name = entity.Name;
                tblLine.Description = entity.Description;
                tblLine.FactoryId = entity.FactoryId;
                tblLine.nOrder = entity.nOrder;
                db.SubmitChanges();
                return true;
            }
            catch (Exception ex)
            {
                //logging
                return false;
            }

        }

        public List<tblLine> listAll()
        {
            return db.tblLines.OrderBy(x => x.nOrder).ToList();
        }
        public IEnumerable<tblLine> ListAllPaging(string searchString)
        {
            IQueryable<tblLine> model = db.tblLines;
            if (!string.IsNullOrEmpty(searchString))
            {
                model = model.Where(x => x.Code.Contains(searchString) || x.Name.Contains(searchString));
            }

            return model.OrderByDescending(x => x.Name).ToList();
        }

        public tblLine GetByCode(string code)
        {
            return db.tblLines.SingleOrDefault(x => x.Code == code);
        }
        public tblLine ViewDetail(int id)
        {
            try
            {
                return db.tblLines.SingleOrDefault(x => x.Id == id);
            }
            catch
            {
                return null;
            }
        }
        public string GettblLineNameById(int? id)
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
                var line = db.tblLines.SingleOrDefault(x => x.Id == id);
                db.tblLines.DeleteOnSubmit(line);
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
