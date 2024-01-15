using Model.DataModel;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Dao
{
    public class NodeOnlineDao
    {
        AvaniDataContext db = null;
        public NodeOnlineDao()
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

        public bool UpdateTimeOut(int iNodeTypeId, int iTimeOut)
        {
            try
            {
                var tblNodeOnline = db.tblNodeOnlines.Where(x => x.NodeTypeId == iNodeTypeId).ToList();
                foreach (var item in tblNodeOnline)
                {
                    item.TimeOut = iTimeOut;
                    db.SubmitChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                //logging
                return false;
            }

        }

        public List<tblNodeOnline> listAll(bool RemoveNotExist = false)
        {
            List<tblNodeOnline> lst = new List<tblNodeOnline>();
            if (!RemoveNotExist)
            {
                lst = db.tblNodeOnlines.ToList();
            }
            else
            {
                lst = db.tblNodeOnlines.Where(x => x.NodeName != "NC").ToList();
            }

            return lst.OrderBy(x=>x.LineId).ThenBy(x => x.ZoneId).ThenBy(x => x.nOrder).ToList();
        }
        public IEnumerable<tblNodeOnline> ListAllPaging(string searchString)
        {
            IQueryable<tblNodeOnline> model = db.tblNodeOnlines;
            if (!string.IsNullOrEmpty(searchString))
            {
                model = model.Where(x => x.NodeName.Contains(searchString));
            }

            return model.OrderByDescending(x => x.nOrder).ToList();
        }
        public tblNodeOnline ViewDetail(int id)
        {
            try
            {
                return db.tblNodeOnlines.SingleOrDefault(x => x.NodeId == id);
            }
            catch
            {
                return null;
            }
        }
        public string GetNameById(int id)
        {
            var obj = ViewDetail(id);
            return obj.NodeName;
        }

        public List<tblNodeOnline> ListByTypeId(int TypeId)
        {
            List<tblNodeOnline> model = db.tblNodeOnlines.ToList().Where(x => x.NodeTypeId == TypeId).ToList();

            return model;
        }
        public List<tblNodeOnline> ListByLineId(int LineId)
        {
            List<tblNodeOnline> model = db.tblNodeOnlines.ToList().Where(x => x.LineId == LineId).ToList();

            return model;
        }
        public List<tblNodeOnline> ListZoneId(int ZoneId)
        {
            List<tblNodeOnline> model = db.tblNodeOnlines.ToList().Where(x => x.ZoneId == ZoneId).ToList();

            return model;
        }
    }
}
