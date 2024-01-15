using Model.DataModel;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Dao
{
    public class WorkingPlanDao
    {
        AvaniDataContext db = null;
        public WorkingPlanDao()
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

        public long Insert(tblWorkingPlan entity)
        {
            try
            {
                db.tblWorkingPlans.InsertOnSubmit(entity);
                db.SubmitChanges();
            }
            catch (Exception e) { }
            return entity.Id;
        }

     
        public bool Update(tblWorkingPlan entity)
        {
            try
            {
                var tblWorkingPlan = db.tblWorkingPlans.SingleOrDefault(x => x.Id == entity.Id);
                tblWorkingPlan.WorkingId = entity.WorkingId;
                tblWorkingPlan.Year = entity.Year;
                tblWorkingPlan.Month = entity.Month;
                tblWorkingPlan.Day = entity.Day;
                tblWorkingPlan.ShiftId = entity.ShiftId;
                tblWorkingPlan.NodeId = entity.NodeId;

                db.SubmitChanges();
                return true;
            }
            catch (Exception ex)
            {
                //logging
                return false;
            }

        }

        public List<tblWorkingPlan> ListAll()
        {
            return db.tblWorkingPlans.ToList();
        }
        public List<tblWorkingPlan> ListAll(int iYear, int iMonth, int iDay, int iShift)
        {
            List<tblWorkingPlan> lst = db.tblWorkingPlans.Where(x => x.Year == iYear && x.Month == iMonth).ToList();
            if (iDay != 0)
            {
                lst = lst.Where(x => x.Day == iDay).ToList();
            }
            if (iShift != 0)
            {
                lst = lst.Where(x => x.ShiftId == iShift).ToList();
            }

            return lst;
        }
        public List<tblWorkingPlan> ListAll(int iWorkingId)
        {
            List<tblWorkingPlan> lst = db.tblWorkingPlans.Where(x => x.WorkingId == iWorkingId).OrderBy(x => x.Day).ToList();
            return lst;
        }

        public int MaxWorkingId()
        {
            int result = 0;
            try
            {
                //result = db.Receipts.Count(x => x.ReceiptDate.Year.Equals(DateTime.Today.Year) &&
                //                                x.ReceiptDate.Month.Equals(DateTime.Today.Month));

                result = (int)db.tblWorkingPlans.Max(x => x.WorkingId);

            }
            catch
            {

            }
            return result;
        }

        public tblWorkingPlan ViewDetail(int id)
        {
            try
            {
                return db.tblWorkingPlans.SingleOrDefault(x => x.Id == id);
            }
            catch
            {
                return null;
            }
        }

        public bool DeleteWorking(int id)
        {
            try
            {
                var line = db.tblWorkingPlans.Where(x => x.WorkingId == id);
                db.tblWorkingPlans.DeleteAllOnSubmit(line);
                db.SubmitChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public bool CheckDuplicate(tblWorkingPlan entity)
        {
            try
            {
                var tblWorkingPlan = db.tblWorkingPlans.SingleOrDefault(x => x.Year == entity.Year && (x.Month == entity.Month) && (x.Day == entity.Day) && (x.ShiftId == entity.ShiftId) && (x.NodeId == entity.NodeId));
                return (tblWorkingPlan != null);
            }
            catch (Exception ex)
            {
                //logging
                return false;
            }

        }

        public bool Delete(int id)
        {
            try
            {
                var line = db.tblWorkingPlans.SingleOrDefault(x => x.Id == id);
                db.tblWorkingPlans.DeleteOnSubmit(line);
                db.SubmitChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }
    }

    public class tblRunningPlan: tblWorkingPlan
    {
        public int StartHour { get; set; }
        public int StartMinute { get; set; }
        public int FinishHour { get; set; }
        public int FinishMinute { get; set; }
        public int TotalMinute { get; set; }
    }

    public class RunningPlanDao
    {
        public tblRunningPlan Cast(tblWorkingPlan workingPlan)
        {
            tblRunningPlan ret = new tblRunningPlan();
            ret.Id = workingPlan.Id;
            ret.WorkingId = workingPlan.WorkingId;
            ret.NodeId = workingPlan.NodeId;
            ret.Year = workingPlan.Year;
            ret.Month = workingPlan.Month;
            ret.Day = workingPlan.Day;
            tblWorkingShift shift = new WorkingShiftDao().ViewDetail(workingPlan.ShiftId);
            ret.ShiftId = shift.Id;
            ret.StartHour = shift.StartHour;
            ret.StartMinute = shift.StartMinute;
            ret.FinishHour = shift.FinishHour;
            ret.FinishMinute = shift.FinishMinute;
            ret.TotalMinute = shift.TotalMinute;
            return ret;
        }

        public List<tblRunningPlan> Cast(List<tblWorkingPlan> workingPlans)
        {
            List<tblRunningPlan> model = new List<tblRunningPlan>();

            foreach (tblWorkingPlan workingPlan in workingPlans)
            { 
                model.Add(Cast(workingPlan));
            }

            return model;
        }
    }
}
