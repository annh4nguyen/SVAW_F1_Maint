using Model.DataModel;
using PagedList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Dao
{
    public class SummaryEventDao
    {
        string Conn = System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString;
        AvaniDataContext db = null;

        int RUNNING_EVENT = 1, STOPPING_EVENT = 2, ALARM_EVENT = 3, DISCONNECT_EVENT = 11;

        public static int TimeBySecond = 0;
        public static int TimeByMinute = 1;
        public static int TimeByHour = 2;
        public static int TimeByDay = 3;
        public static int TimeByMonth = 4;
        public static int TimeByYear = 5;

        public SummaryEventDao()
        {
            if (!string.IsNullOrEmpty(Conn))
            {
                db = new AvaniDataContext(Conn);
            }else
            {
                db = new AvaniDataContext();
            }
        }

        public List<tblSummaryEventReport> ListAll(int iYear, int iMonth, int iDay, int NumberOfDays, int WorkShift = 0, int Hour2UpdateReportDaily = 6)
        {
            List<tblSummaryEventReport> lst = new List<tblSummaryEventReport>();
            DateTime FinishDate = new DateTime(iYear, iMonth, iDay);
            //DateTime StartDate = FinishDate.AddDays(0 - NumberOfDays); //= 0 thì chỉ tính này hôm nay

            //string Conn = ConfigurationManager.ConnectionStrings["ConStr"].ToString(); ;
            //SqlConnection con = new SqlConnection(Conn);
            //con.Open();
            //string strSQL = "exec [CreateEventReport] @Year = '" + FinishDate.Year + "'";
            //SqlCommand cmd = new SqlCommand(strSQL, con);
            //cmd.ExecuteNonQuery();

            //for (int i = 0; i <= NumberOfDays; i++)
            //{
            //    DateTime d = FinishDate.AddDays(0 - i);

            //    //strSQL = " exec [CreateSummaryEventReport] @Year = " + d.Year + ", @Month = " + d.Year + ", @Day = " + d.Day;
            //    //cmd = new SqlCommand(strSQL, con);
            //    //cmd.ExecuteNonQuery();

            //    lst.AddRange(db.SummaryEventReports.Where(x => x.Year == d.Year && x.Month == d.Month && x.Day == d.Day).ToList());
            //}

            
            using (SqlConnection con = new SqlConnection(Conn))
            {
                con.Open();

                for (int i = 0; i <= NumberOfDays; i++)
                {
                    DateTime d = FinishDate.AddDays(0 - i);

                    if (d.Date == DateTime.Now.Date)
                    {
                        //Nếu là ngày hôm nay thì cho nó cập nhật lại phần sự kiện trước khi lấy.
                        //exec [CreateEventReport] @Year = 2019, @Month = 7, @Day = 23, @Hour2Update = 6
                        string query = "exec [CreateEventReport] @Year = " + d.Year + ", @Month = " + d.Month + ", @Day = " + d.Day + ", @Hour2Update = " + Hour2UpdateReportDaily;

                        SqlCommand CreateCmd = new SqlCommand(query, con);
                        CreateCmd.ExecuteNonQuery();
                    }


                    string strSQL = " exec [GetSummaryEventReport] @Year = " + d.Year + ", @Month = " + d.Month + ", @Day = "+d.Day;
                    strSQL += ",@WorkShift=" + WorkShift;
                    SqlCommand cmd = new SqlCommand(strSQL, con);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dtSource = new DataTable();

                    da.Fill(dtSource);

                    foreach (DataRow dr in dtSource.Rows)
                    {
                        tblSummaryEventReport _form = new tblSummaryEventReport();

                        _form = GetEachEvent(dr);
                        lst.Add(_form);
                    }
                    //lst.AddRange(db.SummaryEventReports.Where(x => x.Year == d.Year && x.Month == d.Month && x.Day == d.Day).ToList());
                }
            }
            

            return lst.OrderBy(x => x.NodeTypeId).ToList();
        }

        private tblSummaryEventReport GetEachEvent(DataRow dr)
        {
            tblSummaryEventReport form = new tblSummaryEventReport();

            form.Year = int.Parse(dr["Year"] == DBNull.Value ? "0" : dr["Year"].ToString());
            form.Month = int.Parse(dr["Month"] == DBNull.Value ? "0" : dr["Month"].ToString());
            form.Day = int.Parse(dr["Day"] == DBNull.Value ? "0" : dr["Day"].ToString());
            form.NodeId = int.Parse(dr["NodeId"].ToString());
            form.NodeName = dr["NodeName"].ToString();
            form.NodeTypeId = int.Parse(dr["NodeTypeId"].ToString());
            form.NodeTypeName = dr["NodeTypeName"].ToString();
            form.NumberOfRunning = int.Parse(dr["NumberOfRunning"] == DBNull.Value ? "0" : dr["NumberOfRunning"].ToString());
            form.RunningDuration = double.Parse(dr["RunningDuration"] == DBNull.Value ? "0" : dr["RunningDuration"].ToString());
            form.NumberOfStop = int.Parse(dr["NumberOfStop"] == DBNull.Value ? "0" : dr["NumberOfStop"].ToString());
            form.StopDuration = double.Parse(dr["StopDuration"] == DBNull.Value ? "0" : dr["StopDuration"].ToString());
            form.PlanDuration = long.Parse(dr["PlanDuration"] == DBNull.Value ? "0" : dr["PlanDuration"].ToString());

            return form;
        }

        //2023-11-05: Sửa lại các hàm dùng LINQ, không truy vấn từ DB trực tiếp

        public List<tblSummaryEventReport> GetListAll(int iYear, int iMonth, int iDay, int NumberOfDays, int WorkShift = 0, int Hour2UpdateReportDaily = 6)
        {
            try
            {
                List<tblSummaryEventReport> model = new List<tblSummaryEventReport>();

                DateTime _now = DateTime.Now;
                if (_now.Hour < Hour2UpdateReportDaily) _now = _now.AddDays(-1);

                DateTime _startDate = new DateTime(iYear, iMonth, iDay, Hour2UpdateReportDaily, 0, 0);
                DateTime _finishDate = _startDate.AddDays(NumberOfDays);
                for (int i = 0; i <= NumberOfDays; i++)
                {
                    DateTime _reportDate = _startDate.AddDays(i);
                    //Nếu chưa đến ngày ngày thì bỏ qua
                    if (_reportDate.Date > _now.Date)
                    {
                        break;
                    }

                    //Kiểm tra xem ngày đó đã được tạo chưa?
                    long iReportDate = Time2Num(_reportDate, TimeByDay);

                    tblCreatedSummaryReport tblCreated = db.tblCreatedSummaryReports.FirstOrDefault(x=>x.Id == iReportDate);
                    //Nếu ngày đó chưa được chạy báo cáo thì chạy
                    if (tblCreated == null)
                    {
                        UpdateSummaryReport(_reportDate.Year, _reportDate.Month, _reportDate.Day, 1);
                    }

                    //Nếu trùng vào ngày hôm nay thì cũng update
                    if (_reportDate.Date == _now.Date)
                    {
                        UpdateSummaryReport(_reportDate.Year, _reportDate.Month, _reportDate.Day, 1);
                    }

                    List<tblSummaryEventReport> lst = db.tblSummaryEventReports.Where(r => (r.Year == _reportDate.Year) && (r.Month == _reportDate.Month) && (r.Day == _reportDate.Day)).ToList();
                    //List<tblSummaryEventReport> lst = db.tblSummaryEventReports.Where(r => r.Id== iReportDate).ToList();

                    ////Nếu ngày này chưa có thì khả năng do chưa chạy - bị thủng
                    //if (lst.Count == 0)
                    //{
                    //    //Chạy lại
                    //    UpdateSummaryReport(_reportDate.Year, _reportDate.Month, _reportDate.Day, 1);
                    //    //Rồi lấy tiếp
                    //    lst = db.tblSummaryEventReports.Where(r => (r.Year == _reportDate.Year) && (r.Month == _reportDate.Month) && (r.Day == _reportDate.Day)).ToList();
                    //}


                    lst = lst.Where(r => (r.WorkShiftId == WorkShift)).ToList();
                    model.AddRange(lst);
                }

                return model;
            }

            catch (Exception ex)
            {
            }

            return null;

        }
        public void UpdateSummaryReport(int iYear, int iMonth, int iDay, int NumberOfDays, int WorkShift = 0, int Hour2UpdateReportDaily = 6)
        {
            try
            {
                DateTime LastUpdated = DateTime.MinValue.Date;

                List<tblNodeDef> tblNodeDefs = db.tblNodeDefs.Where(n => (bool)n.Active).ToList();
                DateTime _startDate = new DateTime(iYear, iMonth, iDay, Hour2UpdateReportDaily, 0, 0);
                DateTime _finishDate = _startDate.AddDays(NumberOfDays);
                List<tblEvent> lstEvents = new EventDao().GetListByTime(_startDate, _finishDate);
                List<tblWorkingPlan> lstWorkingPlans = db.tblWorkingPlans.Where(r => (r.Year == _startDate.Year) && (r.Month == _startDate.Month) && (r.Day >= _startDate.Day) && (r.Day <= _finishDate.Day)).ToList();
                
                List<tblRunningPlan> lstRunningPlans = new RunningPlanDao().Cast(lstWorkingPlans);

                List<tblWorkingShift> shifts = new WorkingShiftDao().listAll();

                List<tblSummaryEventReport> reports = new List<tblSummaryEventReport>();
                List<long> UpdateDates = new List<long>();

                for (int i = 0; i < NumberOfDays; i++)
                {
                    DateTime _reportDate = _startDate.AddDays(i);
                    //Kiểm tra tồn tại thì xóa đi
                    DeleteRange(_reportDate.Year, _reportDate.Month, _reportDate.Day, WorkShift);

                    //Chuẩn bị kế hoạch của ngày
                    List<tblRunningPlan> runningPlans = lstRunningPlans.Where(r => (r.Year == _reportDate.Year) && (r.Month == _reportDate.Month) && (r.Day == _reportDate.Day)).ToList();

                    foreach (tblNodeDef nodeDef in tblNodeDefs)
                    {
                        //Lấy phần báo cáo cho cả ngày
                        tblSummaryEventReport _item = GetSummaryByNode(nodeDef, lstEvents, runningPlans, _reportDate.Year, _reportDate.Month, _reportDate.Day, WorkShift, Hour2UpdateReportDaily);
                        reports.Add(_item);

                        if (WorkShift == 0)
                        {
                            //Làm thêm các ca nữa
                            foreach (tblWorkingShift shift in shifts)
                            {
                                _item = GetSummaryByNode(nodeDef, lstEvents, runningPlans, _reportDate.Year, _reportDate.Month, _reportDate.Day, shift.Id, Hour2UpdateReportDaily);
                                reports.Add(_item);
                            }
                        }
                    }

                    LastUpdated = _reportDate.Date;
                    long _iLastUpdated = Time2Num(LastUpdated, TimeByDay);
                    UpdateDates.Add(_iLastUpdated);
                }

                //Check for TODAY, Chỉ đánh dấu cùng lắm đến ngày hôm qua thôi
                DateTime _now = DateTime.Now;
                if (_now.Hour < Hour2UpdateReportDaily) _now = _now.AddDays(-1);
                if (LastUpdated >= _now.Date)
                {
                    LastUpdated = _now.AddDays(-1).Date;
                }
                //Save to DB
                db.CommandTimeout = 120;
                db.tblSummaryEventReports.InsertAllOnSubmit(reports);
                foreach (long _updateDate in UpdateDates)
                {
                    tblCreatedSummaryReport tblCreated = db.tblCreatedSummaryReports.FirstOrDefault(x => x.Id == _updateDate);

                    if (tblCreated == null)
                    {
                        tblCreated = new tblCreatedSummaryReport();
                        tblCreated.Id = _updateDate;
                        db.tblCreatedSummaryReports.InsertOnSubmit(tblCreated);
                    }
                }

                db.SubmitChanges();
            }

            catch (Exception ex)
            {
            }
        }

        private tblSummaryEventReport GetSummaryByNode(tblNodeDef node, List<tblEvent> lstEvents, List<tblRunningPlan> runningPlans, int iYear, int iMonth, int iDay, int WorkShift = 0, int Hour2UpdateReportDaily = 6)
        {
            //Lấy Event của ngày đó và của Node đó
            List<tblEvent> subEvents = new EventDao().GetListEvent(lstEvents, iYear, iMonth, iDay, WorkShift, node.NodeId, Hour2UpdateReportDaily);
            List<tblRunningPlan> subPlans = runningPlans.Where(p => (p.Year == iYear) && (p.Month == iMonth) && (p.Day == iDay) && (p.NodeId == node.NodeId)).ToList();
            if (WorkShift > 0)
            {
                subPlans = subPlans.Where(p => p.ShiftId == WorkShift).ToList();
            }

            tblSummaryEventReport form = new tblSummaryEventReport();

            form.Year = iYear;
            form.Month = iMonth;
            form.Day = iDay;
            form.NodeId = node.NodeId;
            form.NodeName = node.NodeName;
            form.NodeTypeId = node.NodeTypeId;
            form.NodeTypeName = node.NodeTypeName;
            form.WorkShiftId = WorkShift;

            //Tổng kế hoạch: Lọc theo cả ngày hoặc theo ca
            form.PlanDuration = subPlans.Sum(e => e.TotalMinute);
            form.PlanDuration = form.PlanDuration * 60;
            form.NumberOfRunning = subEvents.Where(e => e.EventDefId == RUNNING_EVENT).Count();
            form.NumberOfStop = subEvents.Where(e => e.EventDefId == STOPPING_EVENT).Count();
            form.RunningDuration = (long)subEvents.Where(e => e.EventDefId == RUNNING_EVENT).Sum(e => ((DateTime)e.T1 - (DateTime)e.T3).TotalSeconds);
            form.StopDuration = (long)subEvents.Where(e => e.EventDefId == STOPPING_EVENT).Sum(e => ((DateTime)e.T1 - (DateTime)e.T3).TotalSeconds);
            //form.OtherDuration = (long)subEvents.Where(e => e.EventDefId == DISCONNECT_EVENT).Sum(e => ((DateTime)e.T1 - (DateTime)e.T3).TotalSeconds);

            form.RunningRate = 0;
            if (form.PlanDuration != 0)
            {
                form.RunningRate = (1.0 * form.RunningDuration) / (1.0 * form.PlanDuration);
            }

            return form;
        }

        private void DeleteRange(int iYear, int iMonth, int iDay, int WorkShift = 0)
        {
            List<tblSummaryEventReport> eventReports = db.tblSummaryEventReports.Where(r => (r.Year == iYear) && (r.Month == iMonth) && (r.Day == iDay)).ToList();

            if (WorkShift > 0)
            {
                eventReports = eventReports.Where(r => r.WorkShiftId == WorkShift).ToList();
            }

            db.tblSummaryEventReports.DeleteAllOnSubmit(eventReports);
            db.SubmitChanges();
        }

   

        #region SupportFunction
        public static DateTime Num2Time(long num, int type)
        {
            if (type == TimeByMinute) return new DateTime(int.Parse($"{num:0}".Substring(0, 4)), int.Parse($"{num:0}".Substring(4, 2)), int.Parse($"{num:0}".Substring(6, 2)), int.Parse($"{num:0}".Substring(8, 2)), int.Parse($"{num:0}".Substring(10, 2)), 0);
            if (type == TimeByHour) return new DateTime(int.Parse($"{num:0}".Substring(0, 4)), int.Parse($"{num:0}".Substring(4, 2)), int.Parse($"{num:0}".Substring(6, 2)), int.Parse($"{num:0}".Substring(8, 2)), 0, 0);
            if (type == TimeByDay) return new DateTime(int.Parse($"{num:0}".Substring(0, 4)), int.Parse($"{num:0}".Substring(4, 2)), int.Parse($"{num:0}".Substring(6, 2)), 0, 0, 0);
            if (type == TimeByMonth) return new DateTime(int.Parse($"{num:0}".Substring(0, 4)), int.Parse($"{num:0}".Substring(4, 2)), 1, 0, 0, 0);
            return new DateTime(int.Parse($"{num:0}".Substring(0, 4)), 1, 1, 0, 0, 0);
        }

        public static long Time2Num(DateTime time, int type)
        {
            if (type == TimeByYear) return long.Parse($"{time:yyyy}");
            if (type == TimeByMonth) return long.Parse($"{time:yyyyMM}");
            if (type == TimeByDay) return long.Parse($"{time:yyyyMMdd}");
            if (type == TimeByHour) return long.Parse($"{time:yyyyMMddHH}");
            if (type == TimeByMinute) return long.Parse($"{time:yyyyMMddHHmm}");
            return 0;
        }
        #endregion

    }
}
