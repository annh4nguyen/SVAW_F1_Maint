using Model.DataModel;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Dao
{
    public class NodeTypeDao
    {
        AvaniDataContext db = null;
        public NodeTypeDao()
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

        public long Insert(tblNodeType entity)
        {
            try
            {
                db.tblNodeTypes.InsertOnSubmit(entity);
                db.SubmitChanges();
            }
            catch { }
            return entity.Id;
        }

        public bool Update(tblNodeType entity)
        {
            try
            {
                var tblNodeType = db.tblNodeTypes.SingleOrDefault(x => x.Id == entity.Id);
                tblNodeType.Code = entity.Code;
                tblNodeType.Name = entity.Name;
                tblNodeType.Description = entity.Description;
                tblNodeType.Width = entity.Width;
                tblNodeType.Height = entity.Height;
                tblNodeType.MaxStopTime = entity.MaxStopTime;
                db.SubmitChanges();
                return true;
            }
            catch (Exception ex)
            {
                //logging
                return false;
            }

        }

        public List<tblNodeType> listAll(bool RemoveNotExist = false)
        {
            if (!RemoveNotExist)
            {
                return db.tblNodeTypes.ToList();
            }
            else
            {
                return db.tblNodeTypes.Where(x => x.Code != "NC").ToList();
            }
        }
        public IEnumerable<tblNodeType> ListAllPaging(string searchString)
        {
            IQueryable<tblNodeType> model = db.tblNodeTypes;
            if (!string.IsNullOrEmpty(searchString))
            {
                model = model.Where(x => x.Code.Contains(searchString) || x.Name.Contains(searchString));
            }

            return model.OrderByDescending(x => x.Name).ToList();
        }

        public tblNodeType GetByCode(string code)
        {
            return db.tblNodeTypes.SingleOrDefault(x => x.Code == code);
        }
        public tblNodeType ViewDetail(int id)
        {
            try
            {
                return db.tblNodeTypes.SingleOrDefault(x => x.Id == id);
            }
            catch
            {
                return null;
            }
        }
        public string GettblNodeTypeNameById(int? id)
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
                var _type = db.tblNodeTypes.SingleOrDefault(x => x.Id == id);
                db.tblNodeTypes.DeleteOnSubmit(_type);
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
