using Model.DataModel;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Dao
{
    public class WorkingShiftDao
    {
        AvaniDataContext db = null;
        public WorkingShiftDao()
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

        public long Insert(tblWorkingShift entity)
        {
            try
            {
                CalculateTotalTime(ref entity);
                db.tblWorkingShifts.InsertOnSubmit(entity);
                db.SubmitChanges();
            }
            catch (Exception e) { }
            return entity.Id;
        }

        private void CalculateTotalTime(ref tblWorkingShift entity)
        {
            DateTime StartTime = new DateTime(2019, 1, 1, entity.StartHour, entity.StartMinute, 0);
            DateTime FinishTime = new DateTime(2019, 1, 1, entity.FinishHour, entity.FinishMinute, 0);
            if (entity.FinishHour < entity.StartHour)
            {
                FinishTime = new DateTime(2019, 1, 2, entity.FinishHour, entity.FinishMinute, 0);
            }

            entity.TotalMinute = (int) (FinishTime - StartTime).TotalMinutes;
        }

        public bool Update(tblWorkingShift entity)
        {
            try
            {
                CalculateTotalTime(ref entity);

                var tblWorkingShift = db.tblWorkingShifts.SingleOrDefault(x => x.Id == entity.Id);
                tblWorkingShift.Name = entity.Name;
                tblWorkingShift.StartHour = entity.StartHour;
                tblWorkingShift.StartMinute = entity.StartMinute;
                tblWorkingShift.FinishHour = entity.FinishHour;
                tblWorkingShift.FinishMinute = entity.FinishMinute;
                tblWorkingShift.TotalMinute = entity.TotalMinute;

                db.SubmitChanges();
                return true;
            }
            catch (Exception ex)
            {
                //logging
                return false;
            }

        }

        public List<tblWorkingShift> listAll()
        {
            return db.tblWorkingShifts.ToList();
        }
     
        public tblWorkingShift ViewDetail(int id)
        {
            try
            {
                return db.tblWorkingShifts.SingleOrDefault(x => x.Id == id);
            }
            catch
            {
                return null;
            }
        }

        public string GetNameById(int id)
        {
            try
            {
                return db.tblWorkingShifts.SingleOrDefault(x => x.Id == id).Name;
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
                var line = db.tblWorkingShifts.SingleOrDefault(x => x.Id == id);
                db.tblWorkingShifts.DeleteOnSubmit(line);
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
