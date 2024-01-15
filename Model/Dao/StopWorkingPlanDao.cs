using Model.DataModel;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Dao
{
    public class StopWorkingPlanDao
    {
        AvaniDataContext db = null;
        public StopWorkingPlanDao()
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

        public long Insert(tblStopWorkingPlan entity)
        {
            try
            {
                CalculateTotalMinute(ref entity);

                db.tblStopWorkingPlans.InsertOnSubmit(entity);
                db.SubmitChanges();
            }
            catch (Exception e) { }
            return entity.Id;
        }

     
        public bool Update(tblStopWorkingPlan entity)
        {
            try
            {
                var tblStopWorkingPlan = db.tblStopWorkingPlans.SingleOrDefault(x => x.Id == entity.Id);
                tblStopWorkingPlan.WorkingId = entity.WorkingId;
                tblStopWorkingPlan.Year = entity.Year;
                tblStopWorkingPlan.Month = entity.Month;
                tblStopWorkingPlan.Day = entity.Day;
                tblStopWorkingPlan.FromHour = entity.FromHour;
                tblStopWorkingPlan.FromMinute = entity.FromMinute;
                tblStopWorkingPlan.FromMinute = entity.FromMinute;
                tblStopWorkingPlan.ToHour = entity.ToHour;
                tblStopWorkingPlan.ToMinute = entity.ToMinute;
                CalculateTotalMinute(ref tblStopWorkingPlan);
                db.SubmitChanges();
                return true;
            }
            catch (Exception ex)
            {
                //logging
                return false;
            }

        }

        private void CalculateTotalMinute(ref tblStopWorkingPlan tblStopWorkingPlan)
        {
            DateTime _startDate = new DateTime(tblStopWorkingPlan.Year, tblStopWorkingPlan.Month, tblStopWorkingPlan.Day, tblStopWorkingPlan.FromHour, tblStopWorkingPlan.FromMinute, 0);
            DateTime _endDate = new DateTime(tblStopWorkingPlan.Year, tblStopWorkingPlan.Month, tblStopWorkingPlan.Day, tblStopWorkingPlan.ToHour, tblStopWorkingPlan.ToMinute, 0);
            if (tblStopWorkingPlan.ToHour < tblStopWorkingPlan.FromHour)//Kiểm tra nếu trượt sang ngày hôm sau 
            {
                _endDate = _endDate.AddDays(1);
            }

            TimeSpan span = _endDate - _startDate;
            
            tblStopWorkingPlan.TotalMinute = Convert.ToInt32(span.TotalMinutes);
        }

        public List<tblStopWorkingPlan> ListAll()
        {
            return db.tblStopWorkingPlans.ToList();
        }
        public List<tblStopWorkingPlan> ListAll(int iYear, int iMonth, int iDay, int iWorkingId)
        {
            List<tblStopWorkingPlan> lst = db.tblStopWorkingPlans.Where(x => x.Year == iYear && x.Month == iMonth).ToList();
            if (iDay != 0)
            {
                lst = lst.Where(x => x.Day == iDay).ToList();
            }
            if (iWorkingId != 0)
            {
                lst = lst.Where(x => x.WorkingId == iWorkingId).ToList();
            }

            return lst;
        }
        public List<tblStopWorkingPlan> ListAll(int iWorkingId)
        {
            List<tblStopWorkingPlan> lst = db.tblStopWorkingPlans.Where(x => x.WorkingId == iWorkingId).OrderBy(x => x.Day).ToList();
            return lst;
        }

        public int MaxWorkingId()
        {
            int result = 0;
            try
            {
                //result = db.Receipts.Count(x => x.ReceiptDate.Year.Equals(DateTime.Today.Year) &&
                //                                x.ReceiptDate.Month.Equals(DateTime.Today.Month));

                result = (int)db.tblStopWorkingPlans.Max(x => x.WorkingId);

            }
            catch
            {

            }
            return result;
        }

        public tblStopWorkingPlan ViewDetail(int id)
        {
            try
            {
                return db.tblStopWorkingPlans.SingleOrDefault(x => x.Id == id);
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
                var line = db.tblStopWorkingPlans.Where(x => x.WorkingId == id);
                db.tblStopWorkingPlans.DeleteAllOnSubmit(line);
                db.SubmitChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public bool CheckDuplicate(tblStopWorkingPlan entity)
        {
            try
            {
                var tblStopWorkingPlan = db.tblStopWorkingPlans.SingleOrDefault(x => x.Year == entity.Year && (x.Month == entity.Month) && (x.Day == entity.Day) && (x.FromHour == entity.FromHour) && (x.FromMinute == entity.FromMinute) && (x.ToHour == entity.ToHour) && (x.ToMinute == entity.ToMinute) && (x.NodeId == entity.NodeId));
                return (tblStopWorkingPlan != null);
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
                var line = db.tblStopWorkingPlans.SingleOrDefault(x => x.Id == id);
                db.tblStopWorkingPlans.DeleteOnSubmit(line);
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
