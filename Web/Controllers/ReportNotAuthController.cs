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
    public class ReportNotAuthController : Controller
    {
        string strConStr = ConfigurationManager.ConnectionStrings["ConStr"].ToString();
        int Day2Close = int.Parse(ConfigurationManager.AppSettings["Day2Close"]);
        int Hour2UpdateReportDaily = int.Parse(ConfigurationManager.AppSettings["UpdateReportDaily"]);
        string ConfigPath = ConfigurationManager.AppSettings["ConfigPath"].ToString();
        bool Is_Sumiden = Convert.ToBoolean(ConfigurationManager.AppSettings["Is_Sumiden"].ToString());

      
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
            
            lst = lst.Where(x => x.Id == iNodeType).ToList();

            
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
            int totalTimeWorkShift = 24 * 60;  // tong thoi gian cua ca lam viec 24h - ngay  hoac 8h - theo ca don vi phut
            iWorkShift = 0;
            if (!string.IsNullOrEmpty(WorkShift))
            {
                try
                {
                    iWorkShift = Convert.ToInt32(WorkShift);
                }
                catch { }
            }
            ViewBag.WorkShift = iWorkShift;

            List<tblWorkingShift> lstWorkShift = new WorkingShiftDao().listAll().Where(x => x.Id != 1).ToList(); // 1 ~ id Ca HC
            List<SelectListItem> WorkShifts = new List<SelectListItem>();
            foreach (var _workShift in lstWorkShift)
            {
                if (_workShift.Id == iWorkShift)
                {
                    totalTimeWorkShift = _workShift.TotalMinute;
                }
                WorkShifts.Add(new SelectListItem() { Value = _workShift.Id.ToString(), Text = _workShift.Name.ToString() });
            }
            WorkShifts.Insert(0, new SelectListItem { Text = "-- Tất cả --", Value = "0" });
            ViewBag.WorkShifts = WorkShifts;
            ViewBag.totalTimeWorkShift = totalTimeWorkShift;
        }
        public void GetNodeType(string NodeType, out int iNodeType)
        {
            iNodeType = 0;
            if (!string.IsNullOrEmpty(NodeType))
            {
                iNodeType = Convert.ToInt32(NodeType);
            }
            ViewBag.NodeType = iNodeType;

            List<SelectListItem> NodeTypes = new List<SelectListItem>();
            List<tblNodeType> lst = new NodeTypeDao().listAll(true);

            
                foreach (var nt in lst)
                {
                    NodeTypes.Add(new SelectListItem() { Value = nt.Id.ToString(), Text = nt.Name.ToString() });
                }
            
            NodeTypes.Insert(0, new SelectListItem { Text = "-- Tất cả --", Value = "" });

            ViewBag.NodeTypes = NodeTypes;
        }
    }
}