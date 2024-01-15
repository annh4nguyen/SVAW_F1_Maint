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
    public class EventModel : tblEvent
    {
        public double Duration { get; set; }
        public void Cast(tblEvent e)
        {
            this.Id = e.Id;
            this.NodeId = e.NodeId;
            this.EventDefId = e.EventDefId;
            this.T3 = e.T3;
            this.T1 = e.T1;
            this.Duration = ((DateTime)e.T3 - (DateTime)e.T1).TotalSeconds;

        }
    }

    public class EventDao
    {
        string Conn = System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString;
        AvaniDataContext db = null;
        public EventDao()
        {
            if (!string.IsNullOrEmpty(Conn))
            {
                db = new AvaniDataContext(Conn);
            }
            else
            {
                db = new AvaniDataContext();
            }
        }

        public List<tblEvent> ListAll(DateTime fromDate, DateTime toDate, int WorkShift = 0, int EventDefId = 0, int NodeId = 0)
        {
            List<tblEvent> lst = new List<tblEvent>();

            int _startHour = 6, _startMinute = 0, _finishHour = 6, _finishMinute = 0;

            //Get Shift
            List<tblWorkingShift> shifts = new WorkingShiftDao().listAll();
            if (WorkShift > 0)
            {
                tblWorkingShift shift = shifts.FirstOrDefault(x => x.Id == WorkShift);
                _startHour = shift.StartHour;
                _startMinute = shift.StartMinute;
                _finishHour = shift.FinishHour;
                _finishMinute = shift.FinishMinute;
            }

            int NumberOfDays = (int)(toDate - fromDate).TotalDays;

            using (SqlConnection con = new SqlConnection(Conn))
            {
                con.Open();

                for (int i = 0; i <= NumberOfDays; i++)
                {
                    DateTime dateTime = fromDate.AddDays(i);

                    DateTime StartTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, _startHour, _startMinute, 0);
                    DateTime FinishTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, _finishHour, _finishMinute, 0);


                    if (StartTime >= FinishTime)
                    {
                        FinishTime = FinishTime.AddDays(1);
                    }

                    if (FinishTime > DateTime.Now)
                    {
                        FinishTime = DateTime.Now;
                    }


                    string strSQL = " Select Id, NodeId, EventDefId, T3, T1  ";
                    strSQL += " FROM tblEvent ";

                    //strSQL += $" where T1 >= '{StartTime.ToString("yyyy-MM-dd HH:mm:ss")}' and T3 <= '{FinishTime.ToString("yyyy-MM-dd HH:mm:ss")}' ";
                    strSQL += $" where ( (T1 >= '{StartTime.ToString("yyyy-MM-dd HH:mm:ss")}') OR (T1 is null)  )  and (T3 <= '{FinishTime.ToString("yyyy-MM-dd HH:mm:ss")}' ) ";
                    if (EventDefId > 0)
                    {
                        strSQL += " and EventDefId = " + EventDefId.ToString();
                    }
                    if (NodeId > 0)
                    {
                        strSQL += " AND NodeId = " + NodeId.ToString();
                    }

                    strSQL += " ORDER BY NodeId, T1 ";

                    SqlCommand cmd = new SqlCommand(strSQL, con);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dtSource = new DataTable();
                    cmd.CommandTimeout = 120;
                    da.Fill(dtSource);

                    foreach (DataRow dr in dtSource.Rows)
                    {
                        tblEvent _form = new tblEvent();

                        _form = GetEachEvent(dr, StartTime, FinishTime);
                        lst.Add(_form);
                    }
                    //lst.AddRange(db.EventReports.Where(x => x.Year == d.Year && x.Month == d.Month && x.Day == d.Day).ToList());
                }
            }

            return lst.OrderBy(x => x.NodeId).ToList();
        }

        public List<tblEvent> ListAllByTime(DateTime StartTime, DateTime FinishTime, int EventDefId = 0, int NodeId = 0)
        {
            List<tblEvent> lst = new List<tblEvent>();

            using (SqlConnection con = new SqlConnection(Conn))
            {
                con.Open();

                if (StartTime >= FinishTime)
                {
                    FinishTime = FinishTime.AddDays(1);
                }

                if (FinishTime > DateTime.Now)
                {
                    FinishTime = DateTime.Now;
                }

                string strSQL = " Select Id, NodeId, EventDefId, T3, T1  ";
                strSQL += " FROM tblEvent ";
                //strSQL += $" where (T1 >= '{StartTime.ToString("yyyy-MM-dd HH:mm:ss")}' and T3 <= '{FinishTime.ToString("yyyy-MM-dd HH:mm:ss")}' ) OR (T1 is null) ";
                strSQL += $" where ( (T1 >= '{StartTime.ToString("yyyy-MM-dd HH:mm:ss")}') OR (T1 is null)  )  and (T3 <= '{FinishTime.ToString("yyyy-MM-dd HH:mm:ss")}' ) ";
                if (EventDefId > 0)
                {
                    strSQL += " and EventDefId = " + EventDefId.ToString();
                }
                if (NodeId > 0)
                {
                    strSQL += " AND NodeId = " + NodeId.ToString();
                }

                strSQL += " ORDER BY NodeId, T1 ";

                SqlCommand cmd = new SqlCommand(strSQL, con);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dtSource = new DataTable();

                da.Fill(dtSource);

                foreach (DataRow dr in dtSource.Rows)
                {
                    tblEvent _form = new tblEvent();

                    _form = GetEachEvent(dr, StartTime, FinishTime);
                    lst.Add(_form);
                }
                //lst.AddRange(db.EventReports.Where(x => x.Year == d.Year && x.Month == d.Month && x.Day == d.Day).ToList());
            }

            return lst.OrderBy(x => x.NodeId).ToList();
        }

        private tblEvent GetEachEvent(DataRow dr, DateTime StartTime, DateTime FinishTime)
        {
            tblEvent form = new tblEvent();

            form.Id = long.Parse(dr["Id"] == DBNull.Value ? "0" : dr["Id"].ToString());
            form.NodeId = int.Parse(dr["NodeId"].ToString());
            form.EventDefId = int.Parse(dr["EventDefId"].ToString());
            form.T3 = DateTime.Parse(dr["T3"].ToString());
            form.T1 = dr["T1"] == DBNull.Value ? FinishTime : DateTime.Parse(dr["T1"].ToString());

            if (form.T1 > FinishTime)
            {
                form.T1 = FinishTime;
            }
            if (form.T3 < StartTime)
            {
                form.T3 = StartTime;
            }

            return form;
        }

        public List<tblEvent> GetListAll(DateTime fromDate, DateTime toDate, int WorkShift = 0, int EventDefId = 0, int NodeId = 0)
        {
            DateTime StartTime = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 6, 0, 0);
            DateTime FinishTime = new DateTime(toDate.Year, toDate.Month, toDate.Day, 6, 0, 0);
            FinishTime = FinishTime.AddDays(1);
            if (WorkShift > 0)
            {
                tblWorkingShift shift = new WorkingShiftDao().ViewDetail(WorkShift);
                StartTime = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, shift.StartHour, shift.StartMinute, 0);
                FinishTime = new DateTime(toDate.Year, toDate.Month, toDate.Day, shift.FinishHour, shift.FinishMinute, 0);
                if (shift.StartHour < 6) StartTime = StartTime.AddDays(1);
                if (shift.FinishHour <= 6) FinishTime = FinishTime.AddDays(1);
            }

            db.CommandTimeout = 120;

            List<tblEvent> lst = db.tblEvents.Where(e => (e.T3 <= FinishTime) && ((e.T1 >= StartTime) || (e.T1 == null))).ToList();
            if (EventDefId > 0)
            {
                lst = lst.Where(e => e.EventDefId == EventDefId).ToList();
            }
            if (NodeId > 0)
            {
                lst = lst.Where(e => e.NodeId == NodeId).ToList();
            }
            List<tblEvent> model = new List<tblEvent>();
            foreach (tblEvent item in lst)
            {
                if (item.T3 < StartTime) item.T3 = StartTime;
                if (item.T1 == null) item.T1 = FinishTime;
                if (item.T1 > DateTime.Now) item.T1 = DateTime.Now;

                model.Add(item);
            }

            return model.OrderBy(x => x.NodeId).ThenBy(x => x.T3).ToList();
        }
        public List<tblEvent> GetListByTime(DateTime StartTime, DateTime FinishTime, int EventDefId = 0, int NodeId = 0)
        {
            List<tblEvent> lst = db.tblEvents.Where(e => (e.T3 <= FinishTime) && ((e.T1 >= StartTime) || (e.T1 == null))).ToList();

            return lst.OrderBy(x => x.NodeId).ThenBy(x => x.T3).ToList();
        }

        public List<tblEvent> GetListByTimeWithFixMargin(DateTime _startTime, DateTime _finishTime, int EventDefId = 0, int NodeId = 0)
        {
            DateTime _now = DateTime.Now;
            if (_finishTime > _now) _finishTime = _now;
            //if (_startTime > _now) _startTime = new DateTime(_now.Year, _now.Month, _now.Day, 0, 0, 0) ;

            List<tblEvent> lst = db.tblEvents.Where(e => (e.T3 <= _finishTime) && ((e.T1 >= _startTime) || (e.T1 == null)) && ((e.EventDefId == EventDefId) || (EventDefId == 0)) && ((e.NodeId == NodeId) || (NodeId == 0))).ToList();

            List<tblEvent> model = new List<tblEvent>();

            foreach (tblEvent tblEvent in lst)
            {
                if (tblEvent.T1 == null) tblEvent.T1 = _finishTime;
                if (tblEvent.T1 > _finishTime) tblEvent.T1 = _finishTime;

                if (tblEvent.T3 < _startTime) tblEvent.T3 = _startTime;
                model.Add(tblEvent);
            }
            return model.OrderBy(x => x.NodeId).ThenBy(x => x.T3).ToList();
        }

        public List<tblEvent> GetListEvent(List<tblEvent> lstEvents, int iYear, int iMonth, int iDay, int WorkShift = 0, int NodeId = 0, int Hour2UpdateReportDaily = 6)
        {
            List<tblEvent> model = new List<tblEvent>();

            DateTime _startTime = new DateTime(iYear, iMonth, iDay, Hour2UpdateReportDaily, 0, 0);
            DateTime _finishTime = _startTime.AddDays(1);
            if (WorkShift > 0)
            {
                tblWorkingShift shift = new WorkingShiftDao().ViewDetail(WorkShift);
                _startTime = new DateTime(iYear, iMonth, iDay, shift.StartHour, shift.StartMinute, 0);
                _finishTime = new DateTime(iYear, iMonth, iDay, shift.FinishHour, shift.FinishMinute, 0);
                if (_startTime > _finishTime) _finishTime = _finishTime.AddDays(1);
            }

            if (_finishTime > DateTime.Now) _finishTime = DateTime.Now;
            //Lấy list các event trong khoảng thời gian thỏa mãn

            List<tblEvent> tblEvents = lstEvents.Where(e => (e.T3 <= _finishTime) && ((e.T1 >= _startTime) || (e.T1 == null))).ToList();
            if (NodeId > 0)
            {
                tblEvents = tblEvents.Where(e => e.NodeId == NodeId).ToList();
            }

            foreach (tblEvent tblEvent in tblEvents)
            {
                if (tblEvent.T1 == null) tblEvent.T1 = _finishTime;
                if (tblEvent.T1 > _finishTime) tblEvent.T1 = _finishTime;
                if (tblEvent.T3 < _startTime) tblEvent.T3 = _startTime;
                model.Add(tblEvent);
            }
            return model;
        }


    }
}
