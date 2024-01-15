using Model.Dao;
using avSVAW.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Model.DataModel;
using System.IO;
using Common;
using avSVAW.App_Start;
using System.Configuration;
using System.Web.Services;
using System.Data;
using System.Data.SqlClient;
using ClosedXML.Excel;

namespace avSVAW.Controllers
{
    public class ServiceReportController : Controller
    {
        string strConStr = ConfigurationManager.ConnectionStrings["ConStr"].ToString();
        int Day2Close = int.Parse(ConfigurationManager.AppSettings["Day2Close"]);
        int Hour2UpdateReportDaily = int.Parse(ConfigurationManager.AppSettings["UpdateReportDaily"]);
        string ConfigPath = ConfigurationManager.AppSettings["ConfigPath"].ToString();
        bool Is_Sumiden = Convert.ToBoolean(ConfigurationManager.AppSettings["Is_Sumiden"].ToString());

        public ActionResult ChartEffReport(string FromDate, string ToDate, string NodeType, string WorkShift)
        {
            int iWorkShift = 0;
            int iNodeType = 0;
            DateTime startDate = DateTime.Today.AddDays(0 - Day2Close);
            DateTime endDate = DateTime.Today.AddDays(0 - Day2Close); ; //WebConstants.avMaxValue;

            GetWorkShift(WorkShift, out iWorkShift);
            GetNodeType(NodeType, out iNodeType);
            GetFormatFromDate(FromDate, out startDate);
            GetFormatToDate(ToDate, out endDate);

            List<tblNodeOnline> lstNode = new NodeOnlineDao().listAll(true).OrderBy(x => x.NodeId).ToList();

            List<SummaryEventForm> model = new List<SummaryEventForm>();

            int NumberOfDays = (int)(endDate - startDate).TotalDays;

            List<tblSummaryEventReport> lstReport = new SummaryEventDao().ListAll(endDate.Year, endDate.Month, endDate.Day, NumberOfDays, iWorkShift);
            string NodeIds = "";
            if (iNodeType == 0 || iNodeType == -1) //Lấy dạng tổng hợp hoặc máy B
            {
                List<tblNodeType> lst = GetNodeTypeLists(iNodeType, out NodeIds);
                foreach (var nt in lst)
                {
                    SummaryEventForm _form = new SummaryEventForm();
                    // lay tong so may
                    var CounterNodeType = lstNode.Where(x => x.NodeTypeId == nt.Id).Count();
                    _form.NodeId = nt.Id;
                    _form.NodeName = nt.Name + "<br/>(" + CounterNodeType + " máy)";
                    _form.NodeTypeId = nt.Id;
                    _form.NodeTypeName = nt.Name;
                    _form.WorkingPercent = 0;

                    List<tblSummaryEventReport> subList = lstReport.Where(x => x.NodeTypeId == _form.NodeTypeId).ToList();
                    foreach (var _item in subList)
                    {
                        _form.PlanDuration += (double)_item.PlanDuration;
                        _form.NumberOfRunning += (int)_item.NumberOfRunning;
                        _form.RunningDuration += (double)_item.RunningDuration;
                        _form.NumberOfStop += (int)_item.NumberOfStop;
                        _form.StopDuration += (double)_item.StopDuration;
                    }

                    if (_form.PlanDuration != 0)
                    {
                        _form.WorkingPercent = (float)_form.RunningDuration / _form.PlanDuration;
                    }
                    _form.WorkingPercent = Math.Round(100 * _form.WorkingPercent, 1);
                    _form.PlanDurationInHour = ConvertMin2Hour(_form.PlanDuration);
                    _form.RunningDurationInHour = ConvertMin2Hour(_form.RunningDuration);
                    _form.StopDurationInHour = ConvertMin2Hour(_form.StopDuration);

                    model.Add(_form);
                }

            }

            else//Lấy dạng chi tiết
            {
                foreach (var _node in lstNode)
                {
                    if (_node.NodeTypeId == iNodeType)
                    {

                        SummaryEventForm _form = new SummaryEventForm();
                        _form.NodeId = _node.NodeId;
                        _form.NodeName = _node.NodeName;
                        _form.NodeTypeId = _node.NodeTypeId;
                        _form.NodeTypeName = _node.NodeTypeName;
                        _form.WorkingPercent = 0;
                        List<tblSummaryEventReport> subList = lstReport.Where(x => x.NodeId == _node.NodeId).ToList();
                        foreach (var _item in subList)
                        {
                            _form.PlanDuration += (double)_item.PlanDuration;
                            _form.NumberOfRunning += (int)_item.NumberOfRunning;
                            _form.RunningDuration += (double)_item.RunningDuration;
                            _form.NumberOfStop += (int)_item.NumberOfStop;
                            _form.StopDuration += (double)_item.StopDuration;
                        }

                        if (_form.PlanDuration != 0)
                        {
                            _form.WorkingPercent = (float)_form.RunningDuration / _form.PlanDuration;
                        }
                        _form.WorkingPercent = Math.Round(100 * _form.WorkingPercent, 1);
                        _form.PlanDurationInHour = ConvertMin2Hour(_form.PlanDuration);
                        _form.RunningDurationInHour = ConvertMin2Hour(_form.RunningDuration);
                        _form.StopDurationInHour = ConvertMin2Hour(_form.StopDuration);

                        model.Add(_form);
                    }
                }

            }
            model = model.OrderBy(x => x.NodeTypeId).ThenBy(x => x.NodeName).ToList();
            ViewBag.model = model;

            return View("ChartEffReport");
        }

        public ActionResult OperationReport(string ReportDate, string NodeType, string WorkShift)
        {
            int iWorkShift = 0;
            int iNodeType = 0;
            DateTime startDate = DateTime.Today.AddDays(0 - Day2Close);

            GetWorkShift(WorkShift, out iWorkShift);
            GetNodeType(NodeType, out iNodeType);
            GetFormatFromDate(ReportDate, out startDate);

            List<OperationForm> model = new List<OperationForm>();

            using (SqlConnection con = new SqlConnection(strConStr))
            {

                con.Open();
                if (startDate.Date == DateTime.Now.Date)
                {
                    //Nếu là ngày hôm nay thì cho nó cập nhật lại phần sự kiện trước khi lấy.
                    //exec [CreateEventReport] @Year = 2019, @Month = 7, @Day = 23, @Hour2Update = 6
                    string query = "exec [CreateEventReport] @Year = " + startDate.Year + ", @Month = " + startDate.Month + ", @Day = " + startDate.Day + ", @Hour2Update = " + Hour2UpdateReportDaily;

                    SqlCommand CreateCmd = new SqlCommand(query, con);
                    CreateCmd.ExecuteNonQuery();
                }
                // kiem tra la cong ty sumiden
                int iNodetypeClone = iNodeType;
                if (Is_Sumiden)
                {
                    iNodetypeClone = iNodeType == -1 ? 0 : iNodeType; // loai B se lay tat ca roi loai tru
                }

                string strQuery = "exec [ViewOperationEvent] @Year = " + startDate.Year + ", @Month = " + startDate.Month + ", @Day = " + startDate.Day + ", @NodeType = " + iNodetypeClone;
                strQuery += ",@WorkShift=" + iWorkShift;
                DataTable dt = new DataTable();
                SqlCommand cmd = new SqlCommand(strQuery, con);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                int iCount = 0;

                string NodeIds = "";
                List<tblNodeType> lstNodeType = GetNodeTypeLists(iNodeType, out NodeIds);


                foreach (DataRow dr in dt.Rows)
                {
                    OperationForm _form = new OperationForm();
                    _form = GetEachEvent(dr);

                    bool isShow = NodeIds.Contains(_form.NodeTypeId + ";");

                    if (isShow)
                    {
                        model.Add(_form);
                        iCount++;
                    }
                }
            }
            ViewBag.model = model;
            return View("OperationReport");
        }

        public ActionResult StopReportByType(string FromDate, string ToDate, string WorkShift)
        {
            int iWorkShift = 0;
            int iNodeType = 0;
            DateTime startDate = DateTime.Today.AddDays(0 - Day2Close);
            DateTime endDate = DateTime.Today.AddDays(0 - Day2Close);
            GetWorkShift(WorkShift, out iWorkShift);
            GetFormatFromDate(FromDate, out startDate);
            GetFormatToDate(ToDate, out endDate);

            List<TotalStopTypeForm> model = new List<TotalStopTypeForm>();

            using (SqlConnection con = new SqlConnection(strConStr))
            {

                con.Open();
                if (endDate.Date >= DateTime.Now.Date)
                {
                    //Nếu là ngày hôm nay thì cho nó cập nhật lại phần sự kiện trước khi lấy.
                    string query = "exec [CreateEventReport] @Year = " + endDate.Year + ", @Month = " + endDate.Month + ", @Day = " + endDate.Day + ", @Hour2Update = " + Hour2UpdateReportDaily;
                    SqlCommand CreateCmd = new SqlCommand(query, con);
                    CreateCmd.ExecuteNonQuery();

                    //Nếu là ngày hôm nay thì cho nó cập nhật lại phần sự kiện trước khi lấy.
                    string querySum = " exec [CreateSummaryEventReport] @Year = " + endDate.Year + ", @Month = " + endDate.Month + ", @Day = " + endDate.Day;
                    querySum += ",@WorkShift=" + iWorkShift;

                    SqlCommand SumCmd = new SqlCommand(querySum, con);
                    SumCmd.ExecuteNonQuery();

                }

                string strQuery = " exec [CreateStopTypeReport] @FromYear = " + startDate.Year + ", @FromMonth = " + startDate.Month + ", @FromDay = " + startDate.Day + ", @ToYear = " + endDate.Year + ", @ToMonth = " + endDate.Month + ", @ToDay = " + endDate.Day;

                strQuery += ", @Hour2Update = " + Hour2UpdateReportDaily;

                strQuery += ",@WorkShift = " + iWorkShift;

                DataTable dt = new DataTable();
                SqlCommand cmd = new SqlCommand(strQuery, con);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                dt = ds.Tables[ds.Tables.Count - 1];
                int iCount = 0;
                foreach (DataRow dr in dt.Rows)
                {
                    TotalStopTypeForm _form = new TotalStopTypeForm();
                    _form = GetEachTotalStopTypeEvent(dr);

                    model.Add(_form);

                    iCount++;
                }
            }

            return View("StopTypeReport", model);
        }

        public ActionResult EffReport(string FromDate, string ToDate, string NodeType, string WorkShift)
        {
            int iWorkShift = 0;
            int iNodeType = 0;
            DateTime startDate = DateTime.Today.AddDays(0 - Day2Close);
            DateTime endDate = DateTime.Today.AddDays(0 - Day2Close); ; //WebConstants.avMaxValue;

            GetWorkShift(WorkShift, out iWorkShift);
            GetNodeType(NodeType, out iNodeType);
            GetFormatFromDate(FromDate, out startDate);
            GetFormatToDate(ToDate, out endDate);

            List<tblNodeOnline> lstNode = new NodeOnlineDao().listAll(true).OrderBy(x => x.NodeId).ToList();
            List<SummaryEventForm> model = new List<SummaryEventForm>();

            int NumberOfDays = (int)(endDate - startDate).TotalDays;

            List<tblSummaryEventReport> lstReport = new SummaryEventDao().ListAll(endDate.Year, endDate.Month, endDate.Day, NumberOfDays, iWorkShift, Hour2UpdateReportDaily);

            string NodeIds = "";
            List<tblNodeType> lstNodeType = GetNodeTypeLists(iNodeType, out NodeIds);

            foreach (var _node in lstNode)
            {
                bool isShow = NodeIds.Contains(_node.NodeTypeId + ";");
                if (isShow)
                {
                    SummaryEventForm _form = new SummaryEventForm();
                    _form.NodeId = _node.NodeId;
                    _form.NodeName = _node.NodeName;
                    _form.NodeTypeId = _node.NodeTypeId;
                    _form.NodeTypeName = _node.NodeTypeName;
                    _form.WorkingPercent = 0;
                    List<tblSummaryEventReport> subList = lstReport.Where(x => x.NodeId == _node.NodeId).ToList();
                    foreach (var _item in subList)
                    {
                        _form.PlanDuration += (double)_item.PlanDuration;
                        _form.NumberOfRunning += (int)_item.NumberOfRunning;
                        _form.RunningDuration += (double)_item.RunningDuration;
                        _form.NumberOfStop += (int)_item.NumberOfStop;
                        _form.StopDuration += (double)_item.StopDuration;
                    }

                    if (_form.PlanDuration != 0)
                    {
                        _form.WorkingPercent = (float)_form.RunningDuration / _form.PlanDuration;
                    }
                    _form.WorkingPercent = Math.Round(100 * _form.WorkingPercent, 1);
                    _form.PlanDurationInHour = ConvertMin2Hour(_form.PlanDuration);
                    _form.RunningDurationInHour = ConvertMin2Hour(_form.RunningDuration);
                    _form.StopDurationInHour = ConvertMin2Hour(_form.StopDuration);

                    model.Add(_form);
                }
            }
            model = model.OrderBy(x => x.NodeTypeId).ThenBy(x => x.NodeName).ToList();
            return View("EffectivenessReport", model);
        }

        public ActionResult ChartIncReport(string FromDate, string ToDate, string NodeType, string WorkShift)
        {
            int iWorkShift = 0;
            int iNodeType = 0;
            DateTime startDate = DateTime.Today.AddDays(0 - Day2Close);
            DateTime endDate = DateTime.Today.AddDays(0 - Day2Close); ; //WebConstants.avMaxValue;

            GetWorkShift(WorkShift, out iWorkShift);
            GetNodeType(NodeType, out iNodeType);
            GetFormatFromDate(FromDate, out startDate);
            GetFormatToDate(ToDate, out endDate);
            List<tblNodeType> lstNodeType = new NodeTypeDao().listAll(true);

            int NumberOfDays = (int)(endDate - startDate).TotalDays;

            List<tblSummaryEventReport> lstReport = new SummaryEventDao().ListAll(endDate.Year, endDate.Month, endDate.Day, NumberOfDays);

            List<IncForm> model = new List<IncForm>();
            IncForm initInc = new IncForm();
            initInc.stt = 0;
            initInc.ActualDuration = 0;
            initInc.PlanDuration = 0;
            model.Add(initInc);
            using (SqlConnection con = new SqlConnection(strConStr))
            {

                con.Open();

                string strQuery = " exec [CreateReportInc] @FromYear = " + startDate.Year + ", @FromMonth = " + startDate.Month + ", @FromDay = " + startDate.Day + ", @ToYear = " + endDate.Year + ", @ToMonth = " + endDate.Month + ", @ToDay = " + endDate.Day;
                if (iNodeType != 0)
                {
                    strQuery += ", @NodeType = " + iNodeType;
                }
                strQuery += ", @WorkShift=" + iWorkShift;
                DataTable dt = new DataTable();
                SqlCommand cmd = new SqlCommand(strQuery, con);
                SqlDataAdapter da = new SqlDataAdapter(cmd);

                da.Fill(dt);
                //for (var day = startDate.Date; day.Date <= endDate.Date; day = day.AddDays(1))
                //{
                double incActualDuration = 0;
                double incPlanDuration = 0;
                int i = 0;
                foreach (DataRow dr in dt.Rows)
                {
                    i++;

                    IncForm _formDr = GetEachInc(dr);
                    _formDr.stt = i;
                    incPlanDuration = incPlanDuration + Math.Round(_formDr.PlanDuration / 60, 0);
                    _formDr.PlanDuration = incPlanDuration;
                    _formDr.ActualDuration = incActualDuration = incActualDuration + Math.Round(_formDr.ActualDuration / 60, 0);
                    model.Add(_formDr);
                }
            }
            ViewBag.model = model;
            return View("ChartIncReport");
        }


        public ActionResult Index()
        {
            HomeModel model = new HomeModel();

            model.SoUserTrongNgay = new UserLoggedDao().UserLoggedToday();


            return View(model);
        }

        public ActionResult AnotherLink()
        {
            return View("Index");
        }
        public ActionResult Summary()
        {
            ViewBag.TimeOut = 1000 * int.Parse(ConfigurationManager.AppSettings["TimeOut"]);
            int Day2Close = int.Parse(ConfigurationManager.AppSettings["Day2Close"]);


            DateTime Yesterday = GetLastestWorkingDay();
            int iYear = Yesterday.Year;
            int iMonth = Yesterday.Month;
            int iDay = Yesterday.Day;
            ViewBag.ReportDate = Yesterday.ToString("dd/MM/yyyy");

            List<tblNodeOnline> lstNode = new NodeOnlineDao().listAll(true);
            List<SummaryEventTypeForm> lstForm = new List<SummaryEventTypeForm>();
            List<tblNodeType> lstType = new NodeTypeDao().listAll(true);

            List<tblSummaryEventReport> lstReport = new SummaryEventDao().ListAll(iYear, iMonth, iDay, 0);

            double TotalPlan = 0, TotalActual = 0;
            int iCount = 0;
            foreach (var _type in lstType)
            {
                if (!_type.Name.Contains("NC"))
                {
                    int iNodeCount = 0;
                    SummaryEventTypeForm _form = new SummaryEventTypeForm();
                    foreach (var _item in lstReport)
                    {
                        if (_item.NodeTypeId == _type.Id)
                        {
                            _form.PlanDuration += (double)_item.PlanDuration;
                            _form.ActualDuration += (double)_item.RunningDuration;
                            iNodeCount++;
                        }
                    }

                    //  _form.NumberOfNodes = iNodeCount;
                    _form.NumberOfNodes = lstNode.Where(x => x.NodeTypeId == _type.Id).Count();

                    _form.NodeTypeId = _type.Id;
                    _form.NodeTypeName = _type.Name;
                    _form.Year = iYear;
                    _form.Month = iMonth;
                    _form.Day = iDay;




                    //_form.ActualDuration = Math.Round((float)_form.ActualDuration / 60, 1); //Tính về số phút

                    TotalPlan += _form.PlanDuration;
                    TotalActual += _form.ActualDuration;

                    if (_form.PlanDuration != 0)
                    {
                        _form.WorkingPercent = (float)_form.ActualDuration / _form.PlanDuration;
                    }
                    _form.WorkingPercent = Math.Round(100 * _form.WorkingPercent, 1);
                    lstForm.Add(_form);
                    iCount++;
                }
            }
            double _totalPercent = 0;
            if (TotalPlan != 0)
            {
                _totalPercent = 100 * (float)TotalActual / TotalPlan;
            }
            ViewBag.TotalPercent = Math.Round(_totalPercent, 1);
            ViewBag.NumberOfNodes = lstNode.Count;
            ViewBag.NumberOfTypes = iCount;
            return PartialView("_Summary", lstForm);
        }

        private DateTime GetLastestWorkingDay()
        {
            DateTime Yesterday = DateTime.Now;
            int iCount = 0;
            using (SqlConnection con = new SqlConnection(strConStr))
            {
                con.Open();
                do
                {
                    Yesterday = Yesterday.AddDays(-1);

                    string strQuery = "SELECT * FROM tblWorkingPlan WHERE [Year] = " + Yesterday.Year + " AND [Month] = " + Yesterday.Month + " AND [Day] = " + Yesterday.Day;

                    DataTable dt = new DataTable();
                    SqlCommand cmd = new SqlCommand(strQuery, con);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    dt = ds.Tables[ds.Tables.Count - 1];
                    iCount = dt.Rows.Count;
                }
                while (iCount <= 0);

            }

            return Yesterday;
        }








        [HttpGet]
        public FileContentResult OperationToExcel(string ReportDate, string NodeType, string WorkShift)
        {
            int iWorkShift = 0;
            GetWorkShift(WorkShift, out iWorkShift);
            // load nodetype
            int iNodeType = 0;
            GetNodeType(NodeType, out iNodeType);
            //huantn fix 2019-09-05
            int iNodeTypeClone = iNodeType;
            if (Is_Sumiden)
            {
                iNodeTypeClone = iNodeType == -1 ? 0 : iNodeType; // loai B se lay tat ca roi loai tru
            }

            DateTime reportDate = DateTime.Now.AddDays(0 - Day2Close);
            int iYear = reportDate.Year;
            int iMonth = reportDate.Month;
            int iDay = reportDate.Day;

            if (!string.IsNullOrEmpty(ReportDate))
            {
                try
                {
                    reportDate = Convert.ToDateTime(ReportDate.Trim());
                }
                catch { }
            }
            ViewBag.ReportDate = reportDate.ToString("dd/MM/yyyy");

            List<OperationForm> model = new List<OperationForm>();

            using (SqlConnection con = new SqlConnection(strConStr))
            {

                con.Open();
                if (reportDate.Date == DateTime.Now.Date)
                {
                    //Nếu là ngày hôm nay thì cho nó cập nhật lại phần sự kiện trước khi lấy.
                    //exec [CreateEventReport] @Year = 2019, @Month = 7, @Day = 23, @Hour2Update = 6
                    string query = "exec [CreateEventReport] @Year = " + reportDate.Year + ", @Month = " + reportDate.Month + ", @Day = " + reportDate.Day + ", @Hour2Update = " + Hour2UpdateReportDaily;
                    SqlCommand CreateCmd = new SqlCommand(query, con);
                    CreateCmd.ExecuteNonQuery();
                }


                string strQuery = "exec [ViewOperationEvent] @Year = " + reportDate.Year + ", @Month = " + reportDate.Month + ", @Day = " + reportDate.Day + ", @NodeType = " + iNodeTypeClone;
                strQuery += ",@WorkShift=" + iWorkShift;
                DataTable dtSource = new DataTable();
                SqlCommand cmd = new SqlCommand(strQuery, con);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dtSource);
                int iCount = 0;
                foreach (DataRow dr in dtSource.Rows)
                {
                    OperationForm _form = new OperationForm();

                    _form = GetEachEvent(dr);

                    //huantn fix 2019-09-05
                   
                        model.Add(_form);
                    
                    iCount++;
                }
            }

            string SheetName = "" + reportDate.ToString("yyyy_MM_dd");
            //if (iNodeType != 0)
            //{
            //    SheetName += "-" + strNodeName;
            //}
            DataTable dt = new DataTable(SheetName);

            dt.Columns.AddRange(new DataColumn[6] { new DataColumn(Resources.Language.Node),
                                            new DataColumn(Resources.Language.NodeType),
                                            new DataColumn(Resources.Language.EventDef),
                                            new DataColumn(Resources.Language.Start),
                                            new DataColumn(Resources.Language.Finish),
                                            new DataColumn(Resources.Language.TotalDuration + "(" + Resources.Language.Minute + ")")
            });

            foreach (var _row in model)
            {
                dt.Rows.Add(_row.NodeName,
                    _row.NodeTypeName,
                    _row.EventDefName,
                    _row.StartTime,
                    _row.FinishTime,
                    Math.Round(_row.Duration / 60, 0)
                  );
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "OperationReport_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm") + ".xlsx");
                }
            }
        }
        [WebMethod]
        public JsonResult GetOperationReport(string ReportDate, string NodeType, string WorkShift)
        {
            // load workshift
            int iWorkShift = 0;
            GetWorkShift(WorkShift, out iWorkShift);
            // load nodetype
            int iNodeType = 0;
            GetNodeType(NodeType, out iNodeType);

            DateTime reportDate = DateTime.Now.AddDays(0 - Day2Close);
            int iYear = reportDate.Year;
            int iMonth = reportDate.Month;
            int iDay = reportDate.Day;

            if (!string.IsNullOrEmpty(ReportDate))
            {
                try
                {
                    reportDate = Convert.ToDateTime(ReportDate.Trim());
                }
                catch { }
            }
            ViewBag.ReportDate = reportDate.ToString("dd/MM/yyyy");

            List<OperationForm> model = new List<OperationForm>();

            using (SqlConnection con = new SqlConnection(strConStr))
            {

                con.Open();
                if (reportDate.Date == DateTime.Now.Date)
                {
                    //Nếu là ngày hôm nay thì cho nó cập nhật lại phần sự kiện trước khi lấy.
                    //exec [CreateEventReport] @Year = 2019, @Month = 7, @Day = 23, @Hour2Update = 6
                    string query = "exec [CreateEventReport] @Year = " + reportDate.Year + ", @Month = " + reportDate.Month + ", @Day = " + reportDate.Day + ", @Hour2Update = " + Hour2UpdateReportDaily;

                    SqlCommand CreateCmd = new SqlCommand(query, con);
                    CreateCmd.ExecuteNonQuery();
                }
                // kiem tra la cong ty sumiden
                int iNodetypeClone = iNodeType;
                

                string strQuery = "exec [ViewOperationEvent] @Year = " + reportDate.Year + ", @Month = " + reportDate.Month + ", @Day = " + reportDate.Day + ", @NodeType = " + iNodetypeClone;
                strQuery += ",@WorkShift=" + iWorkShift;
                DataTable dt = new DataTable();
                SqlCommand cmd = new SqlCommand(strQuery, con);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                int iCount = 0;

                string NodeIds = "";
                List<tblNodeType> lstNodeType = GetNodeTypeLists(iNodeType, out NodeIds);


                foreach (DataRow dr in dt.Rows)
                {
                    OperationForm _form = new OperationForm();
                    _form = GetEachEvent(dr);

                    bool isShow = NodeIds.Contains(_form.NodeTypeId + ";");

                    if (isShow)
                    {
                        model.Add(_form);
                        iCount++;
                    }




                    //if (Is_Sumiden)
                    //{
                    //    var session = (Model.DataModel.tblUser)Session[GlobalConstants.USER_SESSION];
                    //    if (
                    //        session.Role.Equals(GlobalConstants.AV_STAFF_PlASTIC_GROUP)
                    //        && _form.NodeTypeName.Equals("E")
                    //        )
                    //    {
                    //        model.Add(_form);
                    //    }
                    //    if (
                    //        session.Role.Equals(GlobalConstants.AV_STAFF_COPPER_GROUP)
                    //        && _form.NodeTypeName.StartsWith("B")
                    //        )
                    //    {
                    //        model.Add(_form);
                    //    }
                    //    if (
                    //       session.Role.Equals(GlobalConstants.AV_MANAGER_GROUP) ||
                    //       session.Role.Equals(GlobalConstants.AV_ADMIN_GROUP)
                    //        )
                    //    {
                    //        if ((iNodeType == -1 && _form.NodeTypeName.StartsWith("B")) || (iNodeType >= 0))
                    //        {
                    //            model.Add(_form);
                    //        }

                    //    }
                    //}
                    //else
                    //{

                    //    model.Add(_form);
                    //}


                    //iCount++;
                }
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        public OperationForm GetEachEvent(DataRow dr)
        {
            OperationForm form = new OperationForm();

            form.NodeId = int.Parse(dr["NodeId"].ToString());
            form.NodeName = dr["NodeName"].ToString();
            form.NodeTypeId = int.Parse(dr["NodeTypeId"].ToString());
            form.NodeTypeName = dr["NodeTypeName"].ToString();
            form.EventDefId = int.Parse(dr["EventDefId"].ToString());
            form.EventDefName = dr["EventDefName"].ToString();
            form.StartTime = ((DateTime)dr["T3"]).ToString("yyyy-MM-dd HH:mm:ss");// (dr["T3"] == DBNull.Value ? DateTime.MinValue : (DateTime)dr["T3"]); ;
            form.FinishTime = ((DateTime)dr["T1"]).ToString("yyyy-MM-dd HH:mm:ss"); //(dr["T1"] == DBNull.Value ? DateTime.MinValue : (DateTime)dr["T1"]);
            //form.PlanDuration = double.Parse(dr["PlanDuration"] == DBNull.Value ? "0" : dr["PlanDuration"].ToString());
            form.Duration = double.Parse(dr["ActualDuration"] == DBNull.Value ? "0" : dr["ActualDuration"].ToString());

            form.strDuration = ConvertMin2Time(form.Duration);

            return form;
        }

        public List<tblNodeType> GetNodeTypeLists(int iNodeType, out string NodeIds)
        {
            List<tblNodeType> lst = new NodeTypeDao().listAll(true);
            NodeIds = "";
            if (iNodeType == 0) //Lấy dạng tổng hợp
            {
                
            }
            else if (iNodeType == -1)
            { // lay loai B cua cong ty Sumiden
                lst = lst.Where(x => x.Name.StartsWith("B")).ToList();
            }
            else
            {
                lst = lst.Where(x => x.Id == iNodeType).ToList();

            }
            foreach (tblNodeType nodeType in lst)
            {
                NodeIds += nodeType.Id + ";";
            }

            return lst;
        }
        public string ConvertMin2Time(double TotalMinute)
        {
            string ret = "";
            long _hour = (long)TotalMinute / 60;
            long _min = (long)TotalMinute % 60;

            ret += (_hour < 10 ? "0" + _hour.ToString() : _hour.ToString());
            ret += ":" + (_min < 10 ? "0" + _min.ToString() : _min.ToString());

            return ret;
        }
        public double ConvertMin2Hour(double TotalMinute)
        {
            //double ret = "";
            //long _hour = (long)TotalMinute / 60;
            //long _min = (long)TotalMinute % 60;

            //ret += (_hour < 10 ? "0" + _hour.ToString() : _hour.ToString());
            //ret += ":" + (_min < 10 ? "0" + _min.ToString() : _min.ToString());
            return Math.Round((double)TotalMinute / 60, 2);
        }
        public void GetWorkShift(string WorkShift, out int iWorkShift)
        {
            iWorkShift = 0;
            if (!string.IsNullOrEmpty(WorkShift))
            {
                try
                {
                    iWorkShift = Convert.ToInt32(WorkShift);
                }
                catch { }
            }
            if(iWorkShift == 0)
            {
                ViewBag.WorkShift = "Tất cả ";
            }
            else
            {
                ViewBag.WorkShift = new WorkingShiftDao().GetNameById(iWorkShift);
            }
        }
        public void GetNodeType(string NodeType, out int iNodeType)
        {
            iNodeType = 0;
            if (!string.IsNullOrEmpty(NodeType))
            {
                iNodeType = Convert.ToInt32(NodeType);
            }
            if (iNodeType == 0)
            {
                ViewBag.NodeType = "Tất cả ";
            }
            else
            {
                ViewBag.NodeType = new NodeTypeDao().GettblNodeTypeNameById(iNodeType);
            }
        }
        public void GetFormatFromDate(string InDate, out DateTime OutDate)
        {
            OutDate = DateTime.Now;
            if (!string.IsNullOrEmpty(InDate))
            {
                try
                {
                    OutDate = Convert.ToDateTime(InDate.Trim());
                }
                catch { }
            }
            ViewBag.FromDate = OutDate.ToString("yyyy/MM/dd");
        }
        public void GetFormatToDate(string InDate, out DateTime OutDate)
        {
            OutDate = DateTime.Now;
            if (!string.IsNullOrEmpty(InDate))
            {
                try
                {
                    OutDate = Convert.ToDateTime(InDate.Trim());
                }
                catch { }
            }
            ViewBag.ToDate = OutDate.ToString("yyyy/MM/dd");
        }
        public TotalStopTypeForm GetEachTotalStopTypeEvent(DataRow dr)
        {
            TotalStopTypeForm form = new TotalStopTypeForm();

            form.NodeTypeId = int.Parse(dr["NodeTypeId"].ToString());
            form.NodeTypeName = dr["NodeTypeName"].ToString();
            form.NumberOfNodes = int.Parse(dr["NumberOfNodes"] == DBNull.Value ? "0" : dr["NumberOfNodes"].ToString());

            form.RunningDuration = double.Parse(dr["RunningDuration"] == DBNull.Value ? "0" : dr["RunningDuration"].ToString());
            form.StopDuration = double.Parse(dr["StopDuration"] == DBNull.Value ? "0" : dr["StopDuration"].ToString());
            form.PlanDuration = Math.Round(double.Parse(dr["PlanDuration"] == DBNull.Value ? "0" : dr["PlanDuration"].ToString()), 0);

            form.RunningDurationInHour = ConvertMin2Hour(form.RunningDuration);
            form.StopDurationInHour = ConvertMin2Hour(form.StopDuration);
            form.PlanDurationInHour = ConvertMin2Hour(form.PlanDuration);

            form.strRunningDuration = ConvertMin2Time(form.RunningDuration);
            form.strStopDuration = ConvertMin2Time(form.StopDuration);
            form.strPlanDuration = ConvertMin2Time(form.PlanDuration);

            form.WorkingPercent = 0;
            if (form.PlanDuration != 0)
            {
                form.WorkingPercent = (float)form.RunningDuration / form.PlanDuration;
            }
            form.WorkingPercent = Math.Round(100 * form.WorkingPercent, 1);

            return form;

        }
        public IncForm GetEachInc(DataRow dr)
        {
            IncForm form = new IncForm();
            form.Year = int.Parse(dr["Year"].ToString());
            form.Month = int.Parse(dr["Month"].ToString());
            form.Day = int.Parse(dr["Day"].ToString());
            form.ActualDuration = Math.Round(double.Parse(dr["ActualDuration"] == DBNull.Value ? "0" : dr["ActualDuration"].ToString()));
            form.PlanDuration = double.Parse(dr["PlanDuration"] == DBNull.Value ? "0" : dr["PlanDuration"].ToString());


            return form;
        }
    }
}