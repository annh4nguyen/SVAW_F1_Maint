using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services;
using Common;
using Model.Dao;
using Model.DataModel;
using avSVAW.Common;
using avSVAW.Models;
using avSVAW.App_Start;
using System.Configuration;
using ClosedXML.Excel;
using System.IO;
using System.Data.SqlClient;
using SelectPdf;

namespace avSVAW.Controllers
{
    public class ReportController : BaseController
    {
        string strConStr = ConfigurationManager.ConnectionStrings["ConStr"].ToString();
        int Day2Close = int.Parse(ConfigurationManager.AppSettings["Day2Close"]);
        int Hour2UpdateReportDaily = int.Parse(ConfigurationManager.AppSettings["UpdateReportDaily"]);
        string ConfigPath = ConfigurationManager.AppSettings["ConfigPath"].ToString();
        bool Is_Sumiden = Convert.ToBoolean(ConfigurationManager.AppSettings["Is_Sumiden"].ToString());

        #region Code old

        public ActionResult EffReport(string FromDate, string ToDate, string NodeType, string WorkShift)
        {
            //Kiểm tra điều kiện tìm kiếm
            ViewBag.FromDate = FromDate;
            ViewBag.ToDate = ToDate;

            // load workshift 
            int iWorkShift = 0;
            GetWorkShift(WorkShift, out iWorkShift);

            // load nodetype
            int iNodeType = 0;
            GetNodeType(NodeType,out iNodeType);

            DateTime startDate = DateTime.Today.AddDays(0 - Day2Close); ;

            DateTime endDate = DateTime.Today.AddDays(0 - Day2Close); ; //WebConstants.avMaxValue;

            if (Session[GlobalConstants.STARTDATE_SESSION] != null)
            {
                startDate = (DateTime)Session[GlobalConstants.STARTDATE_SESSION];
                Session.Remove(GlobalConstants.STARTDATE_SESSION);
            }
            if (Session[GlobalConstants.ENDDATE_SESSION] != null)
            {
                endDate = (DateTime)Session[GlobalConstants.ENDDATE_SESSION];
                Session.Remove(GlobalConstants.ENDDATE_SESSION);
            }

            if (!string.IsNullOrEmpty(FromDate))
            {
                try
                {
                    startDate = Convert.ToDateTime(FromDate.Trim());
                }
                catch { }
            }

            ViewBag.FromDate = startDate.ToString("yyyy/MM/dd");

            if (!string.IsNullOrEmpty(ToDate))
            {
                try
                {
                    endDate = Convert.ToDateTime(ToDate.Trim());
                }
                catch { }
            }

            ViewBag.ToDate = endDate.ToString("yyyy/MM/dd");

            //Lưu vào Session
            Session.Add(GlobalConstants.STARTDATE_SESSION, startDate);
            Session.Add(GlobalConstants.ENDDATE_SESSION, endDate);

            int NumberOfDays = (int)(endDate - startDate).TotalDays;

            //???
            List<SummaryEventForm> model = GetEffReport(endDate.Year, endDate.Month, endDate.Day, NumberOfDays, iNodeType, iWorkShift);

            return View("EffectivenessReport", model);
        }

        public List<SummaryEventForm> GetEffReport(int iYear, int iMonth, int iDay, int NumberOfDays, int iNodeType, int iWorkShift)
        {
            List<tblNodeOnline> lstNode = new NodeOnlineDao().listAll(true).OrderBy(x => x.NodeId).ToList();
            List<SummaryEventForm> model = new List<SummaryEventForm>();

            List<tblSummaryEventReport> lstReport = new SummaryEventDao().ListAll(iYear, iMonth, iDay, NumberOfDays, iWorkShift, Hour2UpdateReportDaily);

            string NodeTypeIds = "";
            List<tblNodeType> lstNodeType = GetNodeTypeLists(iNodeType, out NodeTypeIds);

            foreach (var _node in lstNode)
            {
                bool isShow = NodeTypeIds.Contains(";" + _node.NodeTypeId + ";");
                //if (iNodeType == 0) {
                //    isShow = true;
                //}
                //else
                //{
                //    if (_node.NodeTypeId == iNodeType) {
                //        isShow = true;
                //    }
                //}

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
                    _form.RunningDurationInHour = ConvertSecond2Hour(_form.RunningDuration);
                    _form.StopDurationInHour = ConvertSecond2Hour(_form.StopDuration);

                    model.Add(_form);
                }
            }
            model = model.OrderBy(x => x.NodeTypeId).ThenBy(x => x.NodeName).ToList();

            return model;
        }

        public ActionResult ChartEffReport(string FromDate, string ToDate, string NodeType, string WorkShift)
        {
            //Kiểm tra điều kiện tìm kiếm
            ViewBag.FromDate = FromDate;
            ViewBag.ToDate = ToDate;

            // get view workshift 
            int iWorkShift = 0;
            GetWorkShift(WorkShift, out iWorkShift);

            // load nodetype
            int iNodeType = 0;
            GetNodeType(NodeType, out iNodeType);

            DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            DateTime endDate = DateTime.Today.AddDays(0 - Day2Close); ; //WebConstants.avMaxValue;
            if (endDate < startDate)
            {
                endDate = startDate;
            }

            if (Session[GlobalConstants.STARTDATE_SESSION] != null)
            {
                startDate = (DateTime)Session[GlobalConstants.STARTDATE_SESSION];
                Session.Remove(GlobalConstants.STARTDATE_SESSION);
            }
            if (Session[GlobalConstants.ENDDATE_SESSION] != null)
            {
                endDate = (DateTime)Session[GlobalConstants.ENDDATE_SESSION];
                Session.Remove(GlobalConstants.ENDDATE_SESSION);
            }

            if (!string.IsNullOrEmpty(FromDate))
            {
                try
                {
                    startDate = Convert.ToDateTime(FromDate.Trim());
                }
                catch { }
            }

            ViewBag.FromDate = startDate.ToString("yyyy/MM/dd");

            if (!string.IsNullOrEmpty(ToDate))
            {
                try
                {
                    endDate = Convert.ToDateTime(ToDate.Trim());
                }
                catch { }
            }

            ViewBag.ToDate = endDate.ToString("yyyy/MM/dd");

            //Lưu vào Session
            Session.Add(GlobalConstants.STARTDATE_SESSION, startDate);
            Session.Add(GlobalConstants.ENDDATE_SESSION, endDate);


            return View("ChartEffReport");
        }

        [WebMethod]
        public JsonResult GetChartEffReport(string FromDate, string ToDate, string NodeType, string WorkShift)
        {
            // load workshift
            int iWorkShift = 0;
            GetWorkShift(WorkShift, out iWorkShift);

            // load nodetype
            int iNodeType = 0;
            GetNodeType(NodeType, out iNodeType);

            DateTime startDate = DateTime.Today.AddDays(0 - Day2Close); ;

            DateTime endDate = DateTime.Today.AddDays(0 - Day2Close); ; //WebConstants.avMaxValue;

            if (Session[GlobalConstants.STARTDATE_SESSION] != null)
            {
                startDate = (DateTime)Session[GlobalConstants.STARTDATE_SESSION];
                Session.Remove(GlobalConstants.STARTDATE_SESSION);
            }
            if (Session[GlobalConstants.ENDDATE_SESSION] != null)
            {
                endDate = (DateTime)Session[GlobalConstants.ENDDATE_SESSION];
                Session.Remove(GlobalConstants.ENDDATE_SESSION);
            }

            if (!string.IsNullOrEmpty(FromDate))
            {
                try
                {
                    startDate = Convert.ToDateTime(FromDate.Trim());
                }
                catch { }
            }

            if (!string.IsNullOrEmpty(ToDate))
            {
                try
                {
                    endDate = Convert.ToDateTime(ToDate.Trim());
                }
                catch { }
            }
            //Lưu vào Session
            Session.Add(GlobalConstants.STARTDATE_SESSION, startDate);
            Session.Add(GlobalConstants.ENDDATE_SESSION, endDate);



            List<tblNodeOnline> lstNode = new NodeOnlineDao().listAll(true).OrderBy(x => x.NodeId).ToList();

            List<SummaryEventForm> model = new List<SummaryEventForm>();

            int NumberOfDays = (int)(endDate - startDate).TotalDays;
            List<tblSummaryEventReport> lstReport = new SummaryEventDao().GetListAll(startDate.Year, startDate.Month, startDate.Day, NumberOfDays, iWorkShift);

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
                    _form.NodeName = nt.Name + "\n(" + CounterNodeType + " " + Resources.Language.Machine+")";
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
                    _form.RunningDurationInHour = ConvertSecond2Hour(_form.RunningDuration);
                    _form.StopDurationInHour = ConvertSecond2Hour(_form.StopDuration);

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
                        _form.RunningDurationInHour = ConvertSecond2Hour(_form.RunningDuration);
                        _form.StopDurationInHour = ConvertSecond2Hour(_form.StopDuration);

                        model.Add(_form);
                    }
                }

            }

            model = model.OrderBy(x => x.NodeTypeId).ThenBy(x => x.NodeName).ToList();

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public List<tblNodeType> GetNodeTypeLists(int iNodeType, out string NodeIds)
        {
            List<tblNodeType> lst = new NodeTypeDao().listAll(true);
            NodeIds = ";";
            if (iNodeType == 0) //Lấy dạng tổng hợp
            {
                if (Is_Sumiden)
                {
                    var session = (Model.DataModel.tblUser)Session[GlobalConstants.USER_SESSION];
                    if (
                        session.Role.Equals(GlobalConstants.AV_STAFF_PlASTIC_GROUP)
                        )
                    {
                        lst = lst.Where(x => x.Name.Equals("E")).ToList();
                    }
                    else if (
                         session.Role.Equals(GlobalConstants.AV_STAFF_COPPER_GROUP)
                         )
                    {
                        lst = lst.Where(x => x.Name.StartsWith("B")).ToList();
                    }
                    else if (
                      session.Role.Equals(GlobalConstants.AV_MANAGER_GROUP) ||
                      session.Role.Equals(GlobalConstants.AV_STAFF_GROUP) ||
                      session.Role.Equals(GlobalConstants.AV_ADMIN_GROUP)
                      )
                    {
                        // admin thi lay toan bo
                    }
                    else
                    {
                        lst.Clear(); // role khac thi de trong
                    }


                }
            }
            else if (iNodeType == -1)
            { // lay loai B cua cong ty Sumiden
                lst = lst.Where(x => x.Name.StartsWith("B")).ToList();
            }
            else
            {
                lst = lst.Where(x => x.Id == iNodeType).ToList();

            }
            foreach(tblNodeType nodeType in lst)
            {
                NodeIds += nodeType.Id +";";
            }

            return lst;
        }

        [HttpGet]
        public FileContentResult ExportEffToExcel(string FromDate, string ToDate, string NodeType, string WorkShift = "")
        {
            //Kiểm tra điều kiện tìm kiếm
            int iWorkShift = 0;
            GetWorkShift(WorkShift, out iWorkShift);

            // load nodetype
            int iNodeType = 0;
            GetNodeType(NodeType, out iNodeType);

            DateTime startDate = DateTime.Today;

            DateTime endDate = DateTime.Today; //WebConstants.avMaxValue;

            if (!string.IsNullOrEmpty(FromDate))
            {
                try
                {
                    startDate = Convert.ToDateTime(FromDate.Trim());
                }
                catch { }
            }
            if (!string.IsNullOrEmpty(ToDate))
            {
                try
                {
                    endDate = Convert.ToDateTime(ToDate.Trim());
                }
                catch { }
            }
            int NumberOfDays = (int)(endDate - startDate).TotalDays;

            List<SummaryEventForm> model = GetEffReport(endDate.Year, endDate.Month, endDate.Day, NumberOfDays, iNodeType, iWorkShift);

            //Đầu tiên là phải nhân bản file templates

            string strFullpath = ConfigPath;
            string strTemplateFile = strFullpath + @"\Reports\Templates\Eff_Template.xlsx";
            string strFileName = strFullpath;

            //Mở file ra để sẵn sàng ghi

            XLWorkbook wb = new XLWorkbook(strTemplateFile);

            //Lấy thằng worksheet
            IXLWorksheet ws = wb.Worksheet("F2EFF");
            //Điền mấy thằng râu ria
            string nameWorkShift = "";
            if (iWorkShift > -1)
            {

                if (iWorkShift == 0)
                {
                    nameWorkShift = "Cả ngày";
                }
                else
                {
                    nameWorkShift = new WorkingShiftDao().GetNameById(iWorkShift);
                }

            }

            ws.Cell("B4").Value = "'" + nameWorkShift;
            ws.Cell("D4").Value = "'" + startDate.ToString("dd/MM/yyyy");
            ws.Cell("F4").Value = "'" + endDate.ToString("dd/MM/yyyy");

            int StartRow = 7, iCount = 1;
            double TotalRunning = 0, TotalStop = 0, TotalPlan = 0;
            foreach (var _row in model)
            {
                ws.Cell("A" + (StartRow + iCount)).Value = iCount;
                //ws.Cell("B" + (StartRow + iCount)).Value = _row.ReceiptCode;
                ws.Cell("B" + (StartRow + iCount)).Value = _row.NodeName;
                ws.Cell("C" + (StartRow + iCount)).Value = _row.NodeTypeName;
                //ws.Cell("D" + (StartRow + iCount)).Value = _row.NumberOfRunning;
                ws.Cell("D" + (StartRow + iCount)).Value = ConvertSecond2Hour(_row.RunningDuration);
                //ws.Cell("F" + (StartRow + iCount)).Value = _row.NumberOfStop;
                ws.Cell("E" + (StartRow + iCount)).Value = ConvertSecond2Hour(_row.StopDuration);

                ws.Cell("F" + (StartRow + iCount)).Value = ConvertMin2Hour(_row.PlanDuration);
                ws.Cell("G" + (StartRow + iCount)).Value = _row.WorkingPercent;
                TotalRunning += _row.RunningDuration;
                TotalStop += _row.StopDuration;
                TotalPlan += _row.PlanDuration;

                if (iCount < model.Count)
                {
                    ws.Row(StartRow + iCount).InsertRowsBelow(1);
                    iCount++;
                }
            }
            //Phần tổng
            ws.Cell("D" + (StartRow + iCount + 1)).Value = ConvertSecond2Hour(TotalRunning);
            ws.Cell("E" + (StartRow + iCount + 1)).Value = ConvertSecond2Hour(TotalStop);
            ws.Cell("F" + (StartRow + iCount + 1)).Value = ConvertMin2Hour(TotalPlan);
            double TotalPercent = 0;
            if (TotalPlan != 0)
            {
                TotalPercent = (double)TotalRunning / TotalPlan;
            }
            TotalPercent = Math.Round(100 * TotalPercent, 1);


            ws.Cell("G" + (StartRow + iCount + 1)).Value = TotalPercent;

            using (MemoryStream stream = new MemoryStream())
            {
                wb.SaveAs(stream);
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "EFF_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm") + ".xlsx");
            }

        }

        public ActionResult OperationReport(string ReportDate, string NodeType, string WorkShift)
        {
            //load workshift
            int iWorkShift = 0;
            GetWorkShift(WorkShift, out iWorkShift);
            //load nodetype
            int iNodeType = 0;
            GetNodeType(NodeType, out iNodeType);
            ViewBag.EventDef = new EventDefDao().listAll();

            DateTime reportDate = DateTime.Now.AddDays(0 - Day2Close);

            if (Session[GlobalConstants.ENDDATE_SESSION] != null)
            {
                reportDate = (DateTime)Session[GlobalConstants.ENDDATE_SESSION];
                Session.Remove(GlobalConstants.ENDDATE_SESSION);
            }

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
            ViewBag.ReportDate = reportDate.ToString("yyyy/MM/dd");
            //Lưu vào Session
            Session.Add(GlobalConstants.ENDDATE_SESSION, reportDate);

            return View("OperationReport");
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
            if (Session[GlobalConstants.ENDDATE_SESSION] != null)
            {
                reportDate = (DateTime)Session[GlobalConstants.ENDDATE_SESSION];
                Session.Remove(GlobalConstants.ENDDATE_SESSION);
            }

            if (!string.IsNullOrEmpty(ReportDate))
            {
                try
                {
                    reportDate = Convert.ToDateTime(ReportDate.Trim());
                }
                catch { }
            }
            ViewBag.ReportDate = reportDate.ToString("dd/MM/yyyy");

            //Lưu vào Session
            Session.Add(GlobalConstants.ENDDATE_SESSION, reportDate);


            List<OperationForm> model = new List<OperationForm>();

            DateTime _startTime = new DateTime(reportDate.Year, reportDate.Month, reportDate.Day, Hour2UpdateReportDaily, 0, 0);
            DateTime _finishTime = _startTime.AddDays(1);

            if (iWorkShift > 0)
            {
                tblWorkingShift shift = new WorkingShiftDao().ViewDetail(iWorkShift);
                _startTime = new DateTime(iYear, iMonth, iDay, shift.StartHour, shift.StartMinute, 0);
                _finishTime = new DateTime(iYear, iMonth, iDay, shift.FinishHour, shift.FinishMinute, 0);
                if (_startTime > _finishTime) _finishTime = _finishTime.AddDays(1);
            }

            string NodeTypeIds = "";
            List<tblNodeType> lstNodeType = GetNodeTypeLists(iNodeType, out NodeTypeIds);

            List<tblEvent> tblEvents = new EventDao().GetListByTimeWithFixMargin(_startTime, _finishTime, 0, 0);

            //tblEvents = tblEvents.OrderBy(r => r.NodeId).ThenBy(r => r.T3).ToList();

            foreach (tblEvent tblEvent in tblEvents)
            {
                OperationForm _form = new OperationForm().Cast(tblEvent);
                _form.strDuration = ConvertSecond2Time(_form.Duration);

                if (NodeTypeIds.Contains(_form.NodeTypeId + ";"))
                {
                    model.Add(_form);
                }
            }
            model = model.OrderBy(e => e.NodeTypeId).ThenBy(e => e.NodeId).ToList();

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public FileContentResult OperationToExcel(string ReportDate, string NodeType, string WorkShift)
        {
            int iWorkShift = 0;
            GetWorkShift(WorkShift, out iWorkShift);
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

            //Lấy dữ liệu

            List<OperationForm> model = new List<OperationForm>();

            string NodeTypeIds = "";
            List<tblNodeType> lstNodeType = GetNodeTypeLists(iNodeType, out NodeTypeIds);
            DateTime _startTime = new DateTime(reportDate.Year, reportDate.Month, reportDate.Day, Hour2UpdateReportDaily, 0, 0);
            DateTime _finishTime = _startTime.AddDays(1);

            if (iWorkShift > 0)
            {
                tblWorkingShift shift = new WorkingShiftDao().ViewDetail(iWorkShift);
                _startTime = new DateTime(iYear, iMonth, iDay, shift.StartHour, shift.StartMinute, 0);
                _finishTime = new DateTime(iYear, iMonth, iDay, shift.FinishHour, shift.FinishMinute, 0);
                if (_startTime > _finishTime) _finishTime = _finishTime.AddDays(1);
            }

            //List<tblEvent> tblEvents = new EventDao().ListAll(reportDate, reportDate, iWorkShift, 0, 0);
            List<tblEvent> tblEvents = new EventDao().GetListByTimeWithFixMargin(_startTime, _finishTime, 0, 0);

            foreach (tblEvent tblEvent in tblEvents)
            {
                OperationForm _form = new OperationForm().Cast(tblEvent);
                _form.strDuration = ConvertSecond2Time(_form.Duration);

                if (NodeTypeIds.Contains(_form.NodeTypeId + ";"))
                {
                    model.Add(_form);
                }
            }


            // huantn 2019-09-05 
            string strFullpath = ConfigPath;
            string strTemplateFile = strFullpath + @"\Reports\Templates\OperationReport_Template.xlsx";
            string fileName = "Running_Detail_";


            XLWorkbook wb = new XLWorkbook(strTemplateFile);

            //Lấy thằng worksheet
            IXLWorksheet ws = wb.Worksheet("F2EFF");
            //Điền mấy thằng râu ria
            string nameWorkShift = "";
            if (iWorkShift == 0)
            {
                nameWorkShift = Resources.Language.All;
            }
            else
            {
                nameWorkShift = new WorkingShiftDao().GetNameById(iWorkShift);
            }
            string nameNodeType = "";
            if (iNodeType == 0)
            {
                nameNodeType = Resources.Language.All;
            }
            else
            {
                nameNodeType = new WorkingShiftDao().GetNameById(iNodeType);
            }

            ws.Cell("D4").Value = "'" + nameWorkShift;
            ws.Cell("B4").Value = "'" + reportDate.ToString("dd/MM/yyyy");
            ws.Cell("F4").Value = "'" + nameNodeType;

            int StartRow = 6, iCount = 1;

            foreach (OperationForm _form in model)
            {

                ws.Cell("A" + (StartRow + iCount)).Value = iCount;
                ws.Cell("B" + (StartRow + iCount)).Value = _form.NodeName;
                ws.Cell("C" + (StartRow + iCount)).Value = _form.NodeTypeName;
                ws.Cell("D" + (StartRow + iCount)).Value = _form.EventDefName;
                ws.Cell("E" + (StartRow + iCount)).Value = "'" + _form.StartTime;
                ws.Cell("F" + (StartRow + iCount)).Value = "'" + _form.FinishTime;
                ws.Cell("G" + (StartRow + iCount)).Value = "'" + _form.strDuration;
                ws.Cell("H" + (StartRow + iCount)).Value = Math.Round(_form.Duration / 60, 2);

                if (iCount < model.Count)
                {
                    ws.Row(StartRow + iCount).InsertRowsBelow(1);
                }
                iCount++;

            }

            using (MemoryStream stream = new MemoryStream())
            {
                wb.SaveAs(stream);
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName + DateTime.Now.ToString("yyyy-MM-dd-HH-mm") + ".xlsx");
            }

        }


        [WebMethod]
        public JsonResult GetOperationReport_Old(string ReportDate, string NodeType, string WorkShift)
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
                if (reportDate.Date == DateTime.Now.Date || reportDate.Date == DateTime.Now.AddHours(0-Hour2UpdateReportDaily).Date)
                {
                    //Nếu là ngày hôm nay thì cho nó cập nhật lại phần sự kiện trước khi lấy.
                    //exec [CreateEventReport] @Year = 2019, @Month = 7, @Day = 23, @Hour2Update = 6
                    string query = "exec [CreateEventReport] @Year = " + reportDate.Year + ", @Month = " + reportDate.Month + ", @Day = " + reportDate.Day + ", @Hour2Update = " + Hour2UpdateReportDaily;

                    SqlCommand CreateCmd = new SqlCommand(query, con);
                    CreateCmd.ExecuteNonQuery();
                }
                // kiem tra la cong ty sumiden
                int iNodetypeClone = iNodeType;
                if (Is_Sumiden)
                {
                    iNodetypeClone = iNodeType == -1 ? 0 : iNodeType; // loai B se lay tat ca roi loai tru
                }

                string strQuery = "exec [ViewOperationEvent] @Year = " + reportDate.Year + ", @Month = " + reportDate.Month + ", @Day = " + reportDate.Day + ", @NodeType = " + iNodetypeClone;
                strQuery += ",@WorkShift=" + iWorkShift;
                DataTable dt = new DataTable();
                SqlCommand cmd = new SqlCommand(strQuery, con);
                cmd.CommandTimeout = 300;
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

            form.strDuration = ConvertSecond2Time(form.Duration);

            return form;
        }

        [HttpGet]
        public FileContentResult OperationToExcel_Old(string ReportDate, string NodeType, string WorkShift)
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
                if (reportDate.Date == DateTime.Now.Date || reportDate.Date == DateTime.Now.AddHours(0 - Hour2UpdateReportDaily).Date)
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

                // huantn 2019-09-05 
                string strFullpath = ConfigPath;
                string strTemplateFile = strFullpath + @"\Reports\Templates\OperationReport_Template.xlsx";
                string fileName = "OperationReport_";


                XLWorkbook wb = new XLWorkbook(strTemplateFile);

                //Lấy thằng worksheet
                IXLWorksheet ws = wb.Worksheet("F2EFF");
                //Điền mấy thằng râu ria
                string nameWorkShift = "";
                if (iWorkShift == 0)
                {
                    nameWorkShift = "Cả ngày";
                }
                else
                {
                    nameWorkShift = new WorkingShiftDao().GetNameById(iWorkShift);
                }
                ws.Cell("D4").Value = "'" + nameWorkShift;
                ws.Cell("F4").Value = "'" + reportDate.ToString("dd/MM/yyyy");

                int StartRow = 8, iTotal = 0;

                foreach (DataRow dr in dtSource.Rows)
                {
                    iTotal++;
                    OperationForm _form = new OperationForm();

                    _form = GetEachEvent(dr);
                    var is_add = false;
                    //huantn fix 2019-09-05
                    if (Is_Sumiden)
                    {
                        var session = (Model.DataModel.tblUser)Session[GlobalConstants.USER_SESSION];
                        if (
                            session.Role.Equals(GlobalConstants.AV_STAFF_PlASTIC_GROUP)
                            && _form.NodeTypeName.Equals("E")
                            )
                        {
                            is_add = true;
                        }
                        if (
                            session.Role.Equals(GlobalConstants.AV_STAFF_COPPER_GROUP)
                            && _form.NodeTypeName.Contains("B")
                            )
                        {
                            is_add = true;
                        }
                        if (
                       session.Role.Equals(GlobalConstants.AV_MANAGER_GROUP) ||
                       session.Role.Equals(GlobalConstants.AV_STAFF_GROUP) ||   //2019-09-05 add role staff
                       session.Role.Equals(GlobalConstants.AV_ADMIN_GROUP)
                        )
                        {
                            if ((iNodeType == -1 && _form.NodeTypeName.StartsWith("B")) || (iNodeType >= 0))
                            {
                                is_add = true;
                            }

                        }
                    }
                    else
                    {
                        is_add = true;
                    }
                    if (is_add)
                    {
                        ws.Cell("A" + (StartRow + iCount)).Value = (iCount+1);
                        ws.Cell("B" + (StartRow + iCount)).Value = _form.NodeName;
                        ws.Cell("C" + (StartRow + iCount)).Value = _form.NodeTypeName;
                        ws.Cell("D" + (StartRow + iCount)).Value = _form.EventDefName;
                        ws.Cell("E" + (StartRow + iCount)).Value = "'" + _form.StartTime;
                        ws.Cell("F" + (StartRow + iCount)).Value = "'" + _form.FinishTime;
                        ws.Cell("G" + (StartRow + iCount)).Value = Math.Round(_form.Duration / 60, 2);
                        ws.Cell("H" + (StartRow + iCount)).Value =  _form.strDuration;

                        if (dtSource.Rows.Count > iTotal)
                        {
                            ws.Row(StartRow + iCount).InsertRowsBelow(1);
                        }
                        iCount++;
                    }
                }

                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName + DateTime.Now.ToString("yyyy-MM-dd-HH-mm") + ".xlsx");
                }

            }
        }

        public JsonResult PrintOperationChart(string ReportDate, string NodeType, string WorkShift)
        {
            try
            {
                int iWorkShift = 0;
                GetWorkShift(WorkShift, out iWorkShift);

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

                int iNodeType = 0;
                if (!string.IsNullOrEmpty(NodeType))
                {
                    iNodeType = Convert.ToInt32(NodeType);
                }
                ViewBag.NodeType = iNodeType;

                List<OperationForm> model = new List<OperationForm>();

                using (SqlConnection con = new SqlConnection(strConStr))
                {

                    con.Open();
                    if (reportDate.Date == DateTime.Now.Date || reportDate.Date == DateTime.Now.AddHours(0 - Hour2UpdateReportDaily).Date)
                    {
                        //Nếu là ngày hôm nay thì cho nó cập nhật lại phần sự kiện trước khi lấy.
                        //exec [CreateEventReport] @Year = 2019, @Month = 7, @Day = 23, @Hour2Update = 6
                        string query = "exec [CreateEventReport] @Year = " + reportDate.Year + ", @Month = " + reportDate.Month + ", @Day = " + reportDate.Day + ", @Hour2Update = " + Hour2UpdateReportDaily;
                        SqlCommand CreateCmd = new SqlCommand(query, con);
                        CreateCmd.ExecuteNonQuery();
                    }

                    // kiem tra la cong ty sumiden
                    int iNodeTypeClone = iNodeType;
                    if (Is_Sumiden)
                    {
                        iNodeTypeClone = iNodeType == -1 ? 0 : iNodeType; // loai B se lay tat ca roi loai tru
                    }

                    string strQuery = "exec [ViewOperationEvent] @Year = " + reportDate.Year + ", @Month = " + reportDate.Month + ", @Day = " + reportDate.Day + ", @NodeType = " + iNodeTypeClone;
                    DataTable dt = new DataTable();
                    SqlCommand cmd = new SqlCommand(strQuery, con);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                    int iCount = 0;
                    foreach (DataRow dr in dt.Rows)
                    {
                        OperationForm _form = new OperationForm();

                        _form = GetEachEvent(dr);
                        if (Is_Sumiden)
                        {
                            var session = (Model.DataModel.tblUser)Session[GlobalConstants.USER_SESSION];
                            if (
                                session.Role.Equals(GlobalConstants.AV_STAFF_PlASTIC_GROUP)
                                && _form.NodeTypeName.Equals("E")
                                )
                            {
                                model.Add(_form);
                            }
                            if (
                                session.Role.Equals(GlobalConstants.AV_STAFF_COPPER_GROUP)
                                && _form.NodeTypeName.Contains("B")
                                )
                            {
                                model.Add(_form);
                            }
                            if (
                           session.Role.Equals(GlobalConstants.AV_MANAGER_GROUP) ||
                           session.Role.Equals(GlobalConstants.AV_STAFF_GROUP) ||   //2019-09-05 add role staff
                           session.Role.Equals(GlobalConstants.AV_ADMIN_GROUP)
                            )
                            {
                                if ((iNodeType == -1 && _form.NodeTypeName.StartsWith("B")) || (iNodeType >= 0))
                                {
                                    model.Add(_form);
                                }

                            }
                        }
                        else
                        {
                            model.Add(_form);
                        }

                        iCount++;
                    }
                }


                ViewData.Model = model;
                ViewBag.WebUrl = ConfigPath + @"\Report";//Common.WebConstants.WEB_RENDER;

                var sw = new StringWriter();
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, "OperationReport");
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);


                PdfPageSize pageSize = PdfPageSize.A4;
                HtmlToPdf converter = new HtmlToPdf();

                // set converter options
                converter.Options.PdfPageSize = pageSize;
                converter.Options.PdfPageOrientation = PdfPageOrientation.Landscape;
                converter.Options.MarginTop = 20;
                converter.Options.MarginBottom = 30;
                converter.Options.MarginLeft = 20;
                converter.Options.MarginRight = 20;

                //string PrintHtmlUrl = Common.WebConstants.WEB_RENDER + "/Print/Receipt/" + ReceiptId.ToString();
                //// create a new pdf document converting an url
                //PdfDocument doc = converter.ConvertUrl(PrintHtmlUrl);
                PdfDocument doc = converter.ConvertHtmlString(sw.GetStringBuilder().ToString());

                string strFullpath = ViewBag.WebUrl;
                string strFileName = "";

                strFileName += @"\" + iYear.ToString();
                if (!Directory.Exists(strFullpath + strFileName))
                {
                    Directory.CreateDirectory(strFullpath + strFileName);
                }
                strFileName += @"\" + (iMonth < 10 ? "0" : "") + iMonth.ToString();

                if (!Directory.Exists(strFullpath + strFileName))
                {
                    Directory.CreateDirectory(strFullpath + strFileName);
                }

                string strTime = DateTime.Now.Ticks.ToString();
                strFileName += @"\Operation_" + reportDate.ToString("yyyy-MM-dd") + "_" + strTime + ".pdf";

                // save pdf document
                doc.Save(strFullpath + strFileName);

                string url = "/Report" + strFileName.Replace(@"\", "/");

                return Json(url, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                string ret = "Error: " + e.Message;
                return Json(ret, JsonRequestBehavior.AllowGet);
            }

        }


        public ActionResult StopReport(string FromDate, string ToDate, string NodeType, string WorkShift)
        {
            //Kiểm tra điều kiện tìm kiếm
            ViewBag.FromDate = FromDate;
            ViewBag.ToDate = ToDate;
            // load workshift
            int iWorkShift = 0;
            GetWorkShift(WorkShift, out iWorkShift);

            // load nodetype
            int iNodeType = 0;
            GetNodeType(NodeType, out iNodeType);

            DateTime startDate = DateTime.Today.AddDays(0 - Day2Close); ;

            DateTime endDate = DateTime.Today.AddDays(0 - Day2Close); ; //WebConstants.avMaxValue;

            if (!string.IsNullOrEmpty(FromDate))
            {
                try
                {
                    startDate = Convert.ToDateTime(FromDate.Trim());
                }
                catch { }
            }

            ViewBag.FromDate = startDate.ToString("yyyy/MM/dd");

            if (!string.IsNullOrEmpty(ToDate))
            {
                try
                {
                    endDate = Convert.ToDateTime(ToDate.Trim());
                }
                catch { }
            }

            ViewBag.ToDate = endDate.ToString("yyyy/MM/dd");

            List<TotalStopForm> model = new List<TotalStopForm>();

            using (SqlConnection con = new SqlConnection(strConStr))
            {

                con.Open();
                if (endDate.Date == DateTime.Now.Date || endDate.Date == DateTime.Now.AddHours(0 - Hour2UpdateReportDaily).Date)
                {
                    //Nếu là ngày hôm nay thì cho nó cập nhật lại phần sự kiện trước khi lấy.
                    string query = "exec [CreateEventReport] @Year = " + endDate.Year + ", @Month = " + endDate.Month + ", @Day = " + endDate.Day + ", @Hour2Update = " + Hour2UpdateReportDaily;
                    SqlCommand CreateCmd = new SqlCommand(query, con);
                    CreateCmd.ExecuteNonQuery();
                }

                string strQuery = " exec [CreateStopReport] @FromYear = " + startDate.Year + ", @FromMonth = " + startDate.Month + ", @FromDay = " + startDate.Day + ", @ToYear = " + endDate.Year + ", @ToMonth = " + endDate.Month + ", @ToDay = " + endDate.Day;

                if (iNodeType != 0)
                {
                    strQuery += ", @NodeType = " + iNodeType;
                }
                strQuery += ", @Hour2Update = " + Hour2UpdateReportDaily;

                if (iWorkShift != 0)
                {
                    strQuery += ",@WorkShift = " + iWorkShift;
                }

                DataTable dt = new DataTable();
                SqlCommand cmd = new SqlCommand(strQuery, con);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                dt = ds.Tables[ds.Tables.Count - 1];
                int iCount = 0;
                foreach (DataRow dr in dt.Rows)
                {
                    TotalStopForm _form = new TotalStopForm();

                    _form = GetEachTotalStopEvent(dr);

                    model.Add(_form);

                    iCount++;
                }
            }

            return View("StopReport", model);
        }

        [HttpGet]
        public FileContentResult StopToExcel(string FromDate, string ToDate, string NodeType)
        {

            DateTime startDate = DateTime.Today.AddDays(0 - Day2Close); ;

            DateTime endDate = DateTime.Today.AddDays(0 - Day2Close); ; //WebConstants.avMaxValue;

            if (!string.IsNullOrEmpty(FromDate))
            {
                try
                {
                    startDate = Convert.ToDateTime(FromDate.Trim());
                }
                catch { }
            }

            ViewBag.FromDate = startDate.ToString("yyyy/MM/dd");

            if (!string.IsNullOrEmpty(ToDate))
            {
                try
                {
                    endDate = Convert.ToDateTime(ToDate.Trim());
                }
                catch { }
            }

            ViewBag.ToDate = endDate.ToString("yyyy/MM/dd");

            int iNodeType = 0;

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
            NodeTypes.Insert(0, new SelectListItem { Text = "-- "+Resources.Language.All +" --", Value = "" });

            ViewBag.NodeTypes = NodeTypes;

            string strNodeTypeName = "";
            List<TotalStopForm> model = new List<TotalStopForm>();

            using (SqlConnection con = new SqlConnection(strConStr))
            {

                con.Open();
                if (endDate.Date == DateTime.Now.Date || endDate.Date == DateTime.Now.AddHours(0 - Hour2UpdateReportDaily).Date)
                {
                    //Nếu là ngày hôm nay thì cho nó cập nhật lại phần sự kiện trước khi lấy.
                    string query = "exec [CreateEventReport] @Year = " + endDate.Year + ", @Month = " + endDate.Month + ", @Day = " + endDate.Day + ", @Hour2Update = " + Hour2UpdateReportDaily;
                    SqlCommand CreateCmd = new SqlCommand(query, con);
                    CreateCmd.ExecuteNonQuery();
                }

                string strQuery = " exec [CreateStopReport] @FromYear = " + startDate.Year + ", @FromMonth = " + startDate.Month + ", @FromDay = " + startDate.Day + ", @ToYear = " + endDate.Year + ", @ToMonth = " + endDate.Month + ", @ToDay = " + endDate.Day;

                if (iNodeType != 0)
                {
                    strQuery += ", @NodeType = " + iNodeType;
                }
                strQuery += ", @Hour2Update = " + Hour2UpdateReportDaily;

                DataTable dtSource = new DataTable();
                SqlCommand cmd = new SqlCommand(strQuery, con);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                dtSource = ds.Tables[ds.Tables.Count - 1];

                int iCount = 0;
                foreach (DataRow dr in dtSource.Rows)
                {
                    TotalStopForm _form = new TotalStopForm();

                    _form = GetEachTotalStopEvent(dr);
                    strNodeTypeName = _form.NodeTypeName;
                    model.Add(_form);

                    iCount++;
                }
            }

            string SheetName = "" + startDate.ToString("yyyy_MM_dd") + "-" + endDate.ToString("yyyy_MM_dd") + "";
            if (iNodeType != 0)
            {
                SheetName += "-" + strNodeTypeName;
            }
            DataTable dt = new DataTable(SheetName);

            dt.Columns.AddRange(new DataColumn[6] { new DataColumn(Resources.Language.Node),
                                            new DataColumn(Resources.Language.NodeType),
                                            new DataColumn(Resources.Language.NumberOfRunning),
                                            new DataColumn(Resources.Language.RunningDuration + "(" + Resources.Language.Minute + ")"),
                                            new DataColumn(Resources.Language.NumberOfStop),
                                            new DataColumn(Resources.Language.StopDuration + "(" + Resources.Language.Minute + ")")
                                            //new DataColumn(Resources.Language.StopWorkingPlan + "(" + Resources.Language.Minute + ")")
            });

            foreach (var _row in model)
            {
                dt.Rows.Add(_row.NodeName,
                    _row.NodeTypeName,
                    _row.NumberOfRunning,
                    _row.RunningDuration,
                    _row.NumberOfStop,
                    _row.StopDuration
                  //_row.PlanStopDuration
                  );
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "StopReport_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm") + ".xlsx");
                }
            }
        }
        public TotalStopForm GetEachTotalStopEvent(DataRow dr)
        {
            TotalStopForm form = new TotalStopForm();

            form.NodeId = int.Parse(dr["NodeId"].ToString());
            form.NodeName = dr["NodeName"].ToString();
            form.NodeTypeId = int.Parse(dr["NodeTypeId"].ToString());
            form.NodeTypeName = dr["NodeTypeName"].ToString();
            form.NumberOfRunning = int.Parse(dr["NumberOfRunning"] == DBNull.Value ? "0" : dr["NumberOfRunning"].ToString());
            form.RunningDuration = double.Parse(dr["RunningDuration"] == DBNull.Value ? "0" : dr["RunningDuration"].ToString());
            form.NumberOfStop = int.Parse(dr["NumberOfStop"] == DBNull.Value ? "0" : dr["NumberOfStop"].ToString());
            form.StopDuration = double.Parse(dr["StopDuration"] == DBNull.Value ? "0" : dr["StopDuration"].ToString());
            // form.PlanStopDuration = Math.Round(double.Parse(dr["PlanStopDuration"] == DBNull.Value ? "0" : dr["PlanStopDuration"].ToString()), 0);

            form.strRunningDuration = ConvertMin2Time(form.RunningDuration);
            form.strStopDuration = ConvertMin2Time(form.StopDuration);
            //  form.strPlanStopDuration = ConvertMin2Time(form.PlanStopDuration);

            return form;

        }

        public ActionResult StopReportByType(string FromDate, string ToDate, string WorkShift)
        {
            //Kiểm tra điều kiện tìm kiếm
            ViewBag.FromDate = FromDate;
            ViewBag.ToDate = ToDate;

            //load workshift
            int iWorkShift = 0;
            GetWorkShift(WorkShift, out iWorkShift);

            DateTime startDate = DateTime.Today.AddDays(0 - Day2Close); ;

            DateTime endDate = DateTime.Today.AddDays(0 - Day2Close); ; //WebConstants.avMaxValue;

            if (Session[GlobalConstants.STARTDATE_SESSION] != null)
            {
                startDate = (DateTime)Session[GlobalConstants.STARTDATE_SESSION];
                Session.Remove(GlobalConstants.STARTDATE_SESSION);
            }
            if (Session[GlobalConstants.ENDDATE_SESSION] != null)
            {
                endDate = (DateTime)Session[GlobalConstants.ENDDATE_SESSION];
                Session.Remove(GlobalConstants.ENDDATE_SESSION);
            }

            if (!string.IsNullOrEmpty(FromDate))
            {
                try
                {
                    startDate = Convert.ToDateTime(FromDate.Trim());
                }
                catch { }
            }

            ViewBag.FromDate = startDate.ToString("yyyy/MM/dd");

            if (!string.IsNullOrEmpty(ToDate))
            {
                try
                {
                    endDate = Convert.ToDateTime(ToDate.Trim());
                }
                catch { }
            }

            ViewBag.ToDate = endDate.ToString("yyyy/MM/dd");

            //Lưu vào Session
            Session.Add(GlobalConstants.STARTDATE_SESSION, startDate);
            Session.Add(GlobalConstants.ENDDATE_SESSION, endDate);

            int NumberOfDays = (int)(endDate - startDate).TotalDays;
            List<TotalStopTypeForm> model = GetTotalStopTypeForm(endDate.Year, endDate.Month, endDate.Day, NumberOfDays, iWorkShift);

            return View("StopTypeReport", model);
        }

        public List<TotalStopTypeForm> GetTotalStopTypeForm(int iYear, int iMonth, int iDay, int NumberOfDays, int iWorkShift)
        {
            List<TotalStopTypeForm> model = new List<TotalStopTypeForm>();
            List<tblSummaryEventReport> lstReport = new SummaryEventDao().GetListAll(iYear, iMonth, iDay, NumberOfDays, iWorkShift);

            string NodeIds = "";
            List<tblNodeType> lst = GetNodeTypeLists(0, out NodeIds);
            List<tblNodeOnline> lstNode = new NodeOnlineDao().listAll(true).OrderBy(x => x.NodeId).ToList();

            foreach (var nt in lst)
            {
                TotalStopTypeForm _form = new TotalStopTypeForm();

                _form.NodeTypeId = nt.Id;
                _form.NodeTypeName = nt.Name;
                _form.NumberOfNodes = lstNode.Where(x => x.NodeTypeId == nt.Id).Count();
                _form.WorkingPercent = 0;

                List<tblSummaryEventReport> subList = lstReport.Where(x => x.NodeTypeId == _form.NodeTypeId).ToList();
                if (subList.Count > 0)
                {
                    _form.PlanDuration = subList.Sum(x => (double)x.PlanDuration);
                    _form.RunningDuration = subList.Sum(x => (double)x.RunningDuration);
                    _form.StopDuration = subList.Sum(x => (double)x.StopDuration);

                }

                if (_form.PlanDuration != 0)
                {
                    _form.WorkingPercent = (float)_form.RunningDuration / _form.PlanDuration;
                }
                _form.WorkingPercent = Math.Round(100 * _form.WorkingPercent, 1);
                _form.PlanDurationInHour = ConvertMin2Hour(_form.PlanDuration);
                _form.RunningDurationInHour = ConvertSecond2Hour(_form.RunningDuration);
                _form.StopDurationInHour = ConvertSecond2Hour(_form.StopDuration);

                _form.strPlanDuration = ConvertSecond2Time(_form.PlanDuration);
                _form.strRunningDuration = ConvertSecond2Time(_form.RunningDuration);
                _form.strStopDuration = ConvertSecond2Time(_form.StopDuration);

                model.Add(_form);
            }

            //Lấy thêm thằng B đại diện
            TotalStopTypeForm _formB = new TotalStopTypeForm();

            _formB.NodeTypeId = -1;
            _formB.NodeTypeName = "B";
            _formB.NumberOfNodes = 0;
            _formB.WorkingPercent = 0;
            foreach (TotalStopTypeForm typeForm in model)
            {
                if (typeForm.NodeTypeName.StartsWith("B"))
                {
                    _formB.NumberOfNodes += typeForm.NumberOfNodes;
                    _formB.PlanDuration += typeForm.PlanDuration;
                    _formB.RunningDuration += typeForm.RunningDuration;
                    _formB.StopDuration += typeForm.StopDuration;
                }
            }
            if (_formB.PlanDuration != 0)
            {
                _formB.WorkingPercent = (float)_formB.RunningDuration / _formB.PlanDuration;
            }
            _formB.WorkingPercent = Math.Round(100 * _formB.WorkingPercent, 1);
            _formB.PlanDurationInHour = ConvertMin2Hour(_formB.PlanDuration);
            _formB.RunningDurationInHour = ConvertSecond2Hour(_formB.RunningDuration);
            _formB.StopDurationInHour = ConvertSecond2Hour(_formB.StopDuration);

            _formB.strPlanDuration = ConvertSecond2Time(_formB.PlanDuration);
            _formB.strRunningDuration = ConvertSecond2Time(_formB.RunningDuration);
            _formB.strStopDuration = ConvertSecond2Time(_formB.StopDuration);

            model.Add(_formB);

            model = model.OrderBy(x => x.NodeTypeId).ToList();

            return model;
        }

        [HttpGet]
        public FileContentResult StopTypeToExcel(string FromDate, string ToDate, string WorkShift = "")
        {

            //Kiểm tra điều kiện tìm kiếm
            ViewBag.FromDate = FromDate;
            ViewBag.ToDate = ToDate;

            //load workshift
            int iWorkShift = 0;
            GetWorkShift(WorkShift, out iWorkShift);

            DateTime startDate = DateTime.Today.AddDays(0 - Day2Close); ;

            DateTime endDate = DateTime.Today.AddDays(0 - Day2Close); ; //WebConstants.avMaxValue;

            if (!string.IsNullOrEmpty(FromDate))
            {
                try
                {
                    startDate = Convert.ToDateTime(FromDate.Trim());
                }
                catch { }
            }

            ViewBag.FromDate = startDate.ToString("yyyy/MM/dd");

            if (!string.IsNullOrEmpty(ToDate))
            {
                try
                {
                    endDate = Convert.ToDateTime(ToDate.Trim());
                }
                catch { }
            }

            ViewBag.ToDate = endDate.ToString("yyyy/MM/dd");

            int NumberOfDays = (int)(endDate - startDate).TotalDays;
            List<TotalStopTypeForm> model = GetTotalStopTypeForm(endDate.Year, endDate.Month, endDate.Day, NumberOfDays, iWorkShift);

            // ten cua file excel
            string fileName = "TypeRunning_";

            //Đầu tiên là phải nhân bản file templates

            string strFullpath = ConfigPath;
            string strTemplateFile = strFullpath + @"\Reports\Templates\TypeRunning_Template.xlsx";
            string strFileName = strFullpath;


            XLWorkbook wb = new XLWorkbook(strTemplateFile);

            //Lấy thằng worksheet
            IXLWorksheet ws = wb.Worksheet("F2EFF");
            //Điền mấy thằng râu ria
            string nameWorkShift = "";
            if (iWorkShift == 0)
            {
                nameWorkShift = "Cả ngày";
            }
            else
            {
                nameWorkShift = new WorkingShiftDao().GetNameById(iWorkShift);
            }
            ws.Cell("B4").Value = "'" + nameWorkShift;
            ws.Cell("D4").Value = "'" + startDate.ToString("dd/MM/yyyy");
            ws.Cell("F4").Value = "'" + endDate.ToString("dd/MM/yyyy");



            int StartRow = 7, iCount = 1, TotalNodes = 0;
            double TotalRunning = 0, TotalStop = 0, TotalPlan = 0, TotalWorkingPercent = 0;
            foreach (var _row in model)
            {
                ws.Cell("A" + (StartRow + iCount)).Value = iCount;
                ws.Cell("B" + (StartRow + iCount)).Value = _row.NodeTypeName;
                ws.Cell("C" + (StartRow + iCount)).Value = _row.NumberOfNodes;
                ws.Cell("D" + (StartRow + iCount)).Value = _row.RunningDurationInHour;
                ws.Cell("E" + (StartRow + iCount)).Value = _row.StopDurationInHour;
                ws.Cell("F" + (StartRow + iCount)).Value = _row.PlanDurationInHour;
                ws.Cell("G" + (StartRow + iCount)).Value = _row.WorkingPercent;

                if (_row.NodeTypeId != -1)
                {
                    TotalRunning += _row.RunningDurationInHour;
                    TotalStop += _row.StopDurationInHour;
                    TotalPlan += _row.PlanDurationInHour;
                    TotalWorkingPercent += _row.NumberOfNodes * _row.WorkingPercent;
                    TotalNodes += _row.NumberOfNodes;
                }
                if (iCount < model.Count)
                {
                    ws.Row(StartRow + iCount).InsertRowsBelow(1);
                    iCount++;
                }
            }
            //Phần tổng
            ws.Cell("C" + (StartRow + iCount + 1)).Value = TotalNodes;
            ws.Cell("D" + (StartRow + iCount + 1)).Value = TotalRunning;
            ws.Cell("E" + (StartRow + iCount + 1)).Value = TotalStop;
            ws.Cell("F" + (StartRow + iCount + 1)).Value = TotalPlan;
            ws.Cell("G" + (StartRow + iCount + 1)).Value = Math.Round((float)TotalWorkingPercent / TotalNodes, 1);

            using (MemoryStream stream = new MemoryStream())
            {
                wb.SaveAs(stream);
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName + DateTime.Now.ToString("yyyy-MM-dd-HH-mm") + "].xlsx");
            }
        }


        public ActionResult StopReportByType_Old(string FromDate, string ToDate, string WorkShift)
        {
            //Kiểm tra điều kiện tìm kiếm
            ViewBag.FromDate = FromDate;
            ViewBag.ToDate = ToDate;

            //load workshift
            int iWorkShift = 0;
            GetWorkShift(WorkShift, out iWorkShift);

            DateTime startDate = DateTime.Today.AddDays(0 - Day2Close); ;

            DateTime endDate = DateTime.Today.AddDays(0 - Day2Close); ; //WebConstants.avMaxValue;

            if (!string.IsNullOrEmpty(FromDate))
            {
                try
                {
                    startDate = Convert.ToDateTime(FromDate.Trim());
                }
                catch { }
            }

            ViewBag.FromDate = startDate.ToString("yyyy/MM/dd");

            if (!string.IsNullOrEmpty(ToDate))
            {
                try
                {
                    endDate = Convert.ToDateTime(ToDate.Trim());
                }
                catch { }
            }

            ViewBag.ToDate = endDate.ToString("yyyy/MM/dd");



            List<TotalStopTypeForm> model = new List<TotalStopTypeForm>();

            using (SqlConnection con = new SqlConnection(strConStr))
            {

                con.Open();
                if (endDate.Date >= DateTime.Now.Date || endDate.Date == DateTime.Now.AddHours(0 - Hour2UpdateReportDaily).Date)
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

        [HttpGet]
        public FileContentResult StopTypeToExcel_Old(string FromDate, string ToDate, string WorkShift = "")
        {

            //Kiểm tra điều kiện tìm kiếm
            ViewBag.FromDate = FromDate;
            ViewBag.ToDate = ToDate;

            //load workshift
            int iWorkShift = 0;
            GetWorkShift(WorkShift, out iWorkShift);

            DateTime startDate = DateTime.Today.AddDays(0 - Day2Close); ;

            DateTime endDate = DateTime.Today.AddDays(0 - Day2Close); ; //WebConstants.avMaxValue;

            if (!string.IsNullOrEmpty(FromDate))
            {
                try
                {
                    startDate = Convert.ToDateTime(FromDate.Trim());
                }
                catch { }
            }

            ViewBag.FromDate = startDate.ToString("yyyy/MM/dd");

            if (!string.IsNullOrEmpty(ToDate))
            {
                try
                {
                    endDate = Convert.ToDateTime(ToDate.Trim());
                }
                catch { }
            }

            ViewBag.ToDate = endDate.ToString("yyyy/MM/dd");



            List<TotalStopTypeForm> model = new List<TotalStopTypeForm>();

            using (SqlConnection con = new SqlConnection(strConStr))
            {

                con.Open();
                if (endDate.Date >= DateTime.Now.Date || endDate.Date == DateTime.Now.AddHours(0 - Hour2UpdateReportDaily).Date)
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
                foreach (DataRow dr in dt.Rows)
                {

                    TotalStopTypeForm _form = new TotalStopTypeForm();
                    _form = GetEachTotalStopTypeEvent(dr);

                    model.Add(_form);
                }


            }

            // ten cua file excel
            string fileName = "TypeRunning_";

            //Đầu tiên là phải nhân bản file templates

            string strFullpath = ConfigPath;
            string strTemplateFile = strFullpath + @"\Reports\Templates\TypeRunning_Template.xlsx";
            string strFileName = strFullpath;


            XLWorkbook wb = new XLWorkbook(strTemplateFile);

            //Lấy thằng worksheet
            IXLWorksheet ws = wb.Worksheet("F2EFF");
            //Điền mấy thằng râu ria
            string nameWorkShift = "";
            if (iWorkShift == 0)
            {
                nameWorkShift = "Cả ngày";
            }
            else
            {
                nameWorkShift = new WorkingShiftDao().GetNameById(iWorkShift);
            }
            ws.Cell("B4").Value = "'" + nameWorkShift;
            ws.Cell("D4").Value = "'" + startDate.ToString("dd/MM/yyyy");
            ws.Cell("F4").Value = "'" + endDate.ToString("dd/MM/yyyy");



            int StartRow = 7, iCount = 1, TotalNodes = 0;
            double TotalRunning = 0, TotalStop = 0, TotalPlan = 0, TotalWorkingPercent = 0;
            foreach (var _row in model)
            {
                ws.Cell("A" + (StartRow + iCount)).Value = iCount;
                ws.Cell("B" + (StartRow + iCount)).Value = _row.NodeTypeName;
                ws.Cell("C" + (StartRow + iCount)).Value = _row.NumberOfNodes;
                ws.Cell("D" + (StartRow + iCount)).Value = _row.RunningDurationInHour;
                ws.Cell("E" + (StartRow + iCount)).Value = _row.StopDurationInHour;
                ws.Cell("F" + (StartRow + iCount)).Value = _row.PlanDurationInHour;
                ws.Cell("G" + (StartRow + iCount)).Value = _row.WorkingPercent;

                if (_row.NodeTypeId != -1)
                {
                    TotalRunning += _row.RunningDurationInHour;
                    TotalStop += _row.StopDurationInHour;
                    TotalPlan += _row.PlanDurationInHour;
                    TotalWorkingPercent += _row.NumberOfNodes * _row.WorkingPercent;
                    TotalNodes += _row.NumberOfNodes;
                }
                if (iCount < model.Count)
                {
                    ws.Row(StartRow + iCount).InsertRowsBelow(1);
                    iCount++;
                }
            }
            //Phần tổng
            ws.Cell("C" + (StartRow + iCount + 1)).Value = TotalNodes;
            ws.Cell("D" + (StartRow + iCount + 1)).Value = TotalRunning;
            ws.Cell("E" + (StartRow + iCount + 1)).Value = TotalStop;
            ws.Cell("F" + (StartRow + iCount + 1)).Value = TotalPlan;
            ws.Cell("G" + (StartRow + iCount + 1)).Value = Math.Round((float)TotalWorkingPercent / TotalNodes, 1);

            using (MemoryStream stream = new MemoryStream())
            {
                wb.SaveAs(stream);
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName + DateTime.Now.ToString("yyyy-MM-dd-HH-mm") + "].xlsx");
            }
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

            form.RunningDurationInHour = ConvertSecond2Hour(form.RunningDuration);
            form.StopDurationInHour = ConvertSecond2Hour(form.StopDuration);
            form.PlanDurationInHour = ConvertMin2Hour(form.PlanDuration);

            form.strRunningDuration = ConvertSecond2Time(form.RunningDuration);
            form.strStopDuration = ConvertSecond2Time(form.StopDuration);
            form.strPlanDuration = ConvertMin2Time(form.PlanDuration);

            form.WorkingPercent = 0;
            if (form.PlanDuration != 0)
            {
                form.WorkingPercent = (float)form.RunningDuration / form.PlanDuration;
            }
            form.WorkingPercent = Math.Round(100 * form.WorkingPercent, 1);

            return form;

        }

        public string ConvertMin2Time(double TotalMinute) {
            string ret = "";
            long _hour = (long)TotalMinute / 60;
            long _min = (long)TotalMinute % 60;

            ret += (_hour < 10 ? "0" + _hour.ToString() : _hour.ToString());
            ret += ":" + (_min < 10 ? "0" + _min.ToString() : _min.ToString());

            return ret;
        }
        public string ConvertSecond2Time(double TotalSecond)
        {
            string ret = "";
            long _hour = (long)TotalSecond / 3600;
            long _min = (long)(TotalSecond / 60) % 60 ;
            long _second = (long)TotalSecond%60;

            ret += (_hour < 10 ? "0" + _hour.ToString() : _hour.ToString());
            ret += ":" + (_min < 10 ? "0" + _min.ToString() : _min.ToString());
            ret += ":" + (_second < 10 ? "0" + _second.ToString() : _second.ToString());

            return ret;
        }

        public double ConvertMin2Hour(double TotalMinute)
        {
            //double ret = "";
            //long _hour = (long)TotalMinute / 60;
            //long _min = (long)TotalMinute % 60;

            //ret += (_hour < 10 ? "0" + _hour.ToString() : _hour.ToString());
            //ret += ":" + (_min < 10 ? "0" + _min.ToString() : _min.ToString());
            return Math.Round((double)TotalMinute / 3600, 2);
        }
        public double ConvertSecond2Hour(double TotalSecond)
        {
            //double ret = "";
            //long _hour = (long)TotalMinute / 60;
            //long _min = (long)TotalMinute % 60;

            //ret += (_hour < 10 ? "0" + _hour.ToString() : _hour.ToString());
            //ret += ":" + (_min < 10 ? "0" + _min.ToString() : _min.ToString());
            return Math.Round((double)TotalSecond / 3600, 2);
        }
        
        public ActionResult StopDetailReport(string FromDate, string ToDate, string NodeId, string WorkShift)
        {
            //Kiểm tra điều kiện tìm kiếm
            ViewBag.FromDate = FromDate;
            ViewBag.ToDate = ToDate;
            //load workshift
            int iWorkShift = 0;
            GetWorkShift(WorkShift, out iWorkShift);
            //int iNode = 0;
            //GetNode(NodeId, out iNode);

            DateTime startDate = DateTime.Today.AddDays(0 - Day2Close); ;

            DateTime endDate = DateTime.Today.AddDays(0 - Day2Close); ; //WebConstants.avMaxValue;


            if (Session[GlobalConstants.STARTDATE_SESSION] != null)
            {
                startDate = (DateTime)Session[GlobalConstants.STARTDATE_SESSION];
                Session.Remove(GlobalConstants.STARTDATE_SESSION);
            }
            if (Session[GlobalConstants.ENDDATE_SESSION] != null)
            {
                endDate = (DateTime)Session[GlobalConstants.ENDDATE_SESSION];
                Session.Remove(GlobalConstants.ENDDATE_SESSION);
            }

            if (!string.IsNullOrEmpty(FromDate))
            {
                try
                {
                    startDate = Convert.ToDateTime(FromDate.Trim());
                }
                catch { }
            }

            ViewBag.FromDate = startDate.ToString("yyyy/MM/dd");



            if (!string.IsNullOrEmpty(ToDate))
            {
                try
                {
                    endDate = Convert.ToDateTime(ToDate.Trim());
                }
                catch { }
            }

            ViewBag.ToDate = endDate.ToString("yyyy/MM/dd");

            //Lưu vào Session
            Session.Add(GlobalConstants.STARTDATE_SESSION, startDate);
            Session.Add(GlobalConstants.ENDDATE_SESSION, endDate);

            int NumberOfDays = (int)(endDate - startDate).TotalDays;

            int iNodeId = 0;

            if (!string.IsNullOrEmpty(NodeId))
            {
                iNodeId = Convert.ToInt32(NodeId);
            }
            ViewBag.NodeId = iNodeId;
            List<SelectListItem> Nodes = new List<SelectListItem>();
            List<tblNodeOnline> lst = new NodeOnlineDao().listAll(true);
            foreach (var nt in lst)
            {
                Nodes.Add(new SelectListItem() { Value = nt.NodeId.ToString(), Text = nt.NodeName.ToString() });
            }
            Nodes.Insert(0, new SelectListItem { Text = "-- " + Resources.Language.All + " --", Value = "" });

            ViewBag.Nodes = Nodes;

            List<OperationForm> model = GetStopDetailReport(startDate.Year, startDate.Month, startDate.Day, NumberOfDays, iNodeId, iWorkShift);

            return View("StopDetailReport", model);
        }

        [HttpGet]
        public FileContentResult StopDetailToExcel(string FromDate, string ToDate, string NodeId, string WorkShift = "")
        {
            //Kiểm tra điều kiện tìm kiếm
            DateTime startDate = DateTime.Today.AddDays(0 - Day2Close); ;

            DateTime endDate = DateTime.Today.AddDays(0 - Day2Close); ; //WebConstants.avMaxValue;

            if (!string.IsNullOrEmpty(FromDate))
            {
                try
                {
                    startDate = Convert.ToDateTime(FromDate.Trim());
                }
                catch { }
            }

            if (!string.IsNullOrEmpty(ToDate))
            {
                try
                {
                    endDate = Convert.ToDateTime(ToDate.Trim());
                }
                catch { }
            }
            int iWorkShift = -1;

            if (!string.IsNullOrEmpty(WorkShift))
            {
                iWorkShift = Convert.ToInt32(WorkShift);
            }

            int iNodeId = 0;

            if (!string.IsNullOrEmpty(NodeId))
            {
                iNodeId = Convert.ToInt32(NodeId);
            }
            ViewBag.NodeId = iNodeId;
            List<SelectListItem> Nodes = new List<SelectListItem>();
            List<tblNodeOnline> lst = new NodeOnlineDao().listAll(true);

            foreach (var nt in lst)
            {
                Nodes.Add(new SelectListItem() { Value = nt.NodeId.ToString(), Text = nt.NodeName.ToString() });
            }
            Nodes.Insert(0, new SelectListItem { Text = "-- " + Resources.Language.All + " --", Value = "" });

            ViewBag.Nodes = Nodes;

            string strNodeName = "";

            int NumberOfDays = (int)(endDate - startDate).TotalDays;

            List<OperationForm> model = GetStopDetailReport(startDate.Year, startDate.Month, startDate.Day, NumberOfDays, iNodeId, iWorkShift);

            // file export excel
            string fileName = "DetailRunning_";
            //Đầu tiên là phải nhân bản file templates

            string strFullpath = ConfigPath;
            string strTemplateFile = strFullpath + @"\Reports\Templates\DetailRuning_Template.xlsx";
            string strFileName = strFullpath;

            //Mở file ra để sẵn sàng ghi

            XLWorkbook wb = new XLWorkbook(strTemplateFile);

            //Lấy thằng worksheet
            IXLWorksheet ws = wb.Worksheet("F2EFF");
            //Điền mấy thằng râu ria
            string nameWorkShift = "";
            if (iWorkShift > -1)
            {

                if (iWorkShift == 0)
                {
                    nameWorkShift = "Cả ngày";
                }
                else
                {
                    nameWorkShift = new WorkingShiftDao().GetNameById(iWorkShift);
                }

            }
            ws.Cell("B4").Value = "'" + nameWorkShift;
            ws.Cell("D4").Value = "'" + startDate.ToString("dd/MM/yyyy");
            ws.Cell("F4").Value = "'" + endDate.ToString("dd/MM/yyyy");



            int StartRow = 7, iCount = 1;
            foreach (var _row in model)
            {
                ws.Cell("A" + (StartRow + iCount)).Value = iCount;
                //ws.Cell("B" + (StartRow + iCount)).Value = _row.ReceiptCode;
                ws.Cell("B" + (StartRow + iCount)).Value = _row.NodeName;
                ws.Cell("C" + (StartRow + iCount)).Value = _row.NodeTypeName;
                ws.Cell("D" + (StartRow + iCount)).Value = _row.EventDefName;
                ws.Cell("E" + (StartRow + iCount)).Value = _row.StartTime;
                ws.Cell("F" + (StartRow + iCount)).Value = _row.FinishTime;
                ws.Cell("G" + (StartRow + iCount)).Value = Math.Round(_row.Duration / 60, 2);
                ws.Cell("H" + (StartRow + iCount)).Value = _row.strDuration;

                if (iCount < model.Count)
                {
                    ws.Row(StartRow + iCount).InsertRowsBelow(1);
                    iCount++;
                }
            }
            ////Phần tổng
            //ws.Cell("D" + (StartRow + iCount + 1)).Value = ConvertMin2Time(TotalRunning);
            //ws.Cell("E" + (StartRow + iCount + 1)).Value = ConvertMin2Time(TotalStop);
            //ws.Cell("F" + (StartRow + iCount + 1)).Value = ConvertMin2Time(TotalStopPlan);

            using (MemoryStream stream = new MemoryStream())
            {
                wb.SaveAs(stream);
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName + DateTime.Now.ToString("yyyy-MM-dd-HH-mm") + "].xlsx");
            }
        }

        public List<OperationForm> GetStopDetailReport(int iYear, int iMonth, int iDay, int NumberOfDays, int iNodeId, int iWorkShift)
        {

            List<OperationForm> model = new List<OperationForm>();

            DateTime _startTime = new DateTime(iYear, iMonth, iDay, Hour2UpdateReportDaily, 0, 0);
            DateTime _finishTime = _startTime.AddDays(1);
            DateTime _now = DateTime.Now;

            if (iWorkShift > 0)
            {
                tblWorkingShift shift = new WorkingShiftDao().ViewDetail(iWorkShift);

                for (int i = 0; i <= NumberOfDays; i++)
                {
                    DateTime _reportDate = _startTime.AddDays(i);
                    //Nếu chưa đến ngày ngày thì bỏ qua
                    if (_reportDate.Date > _now.Date)
                    {
                        break;
                    }

                    _startTime = new DateTime(_reportDate.Year, _reportDate.Month, _reportDate.Day, shift.StartHour, shift.StartMinute, 0);
                    _finishTime = new DateTime(_reportDate.Year, _reportDate.Month, _reportDate.Day, shift.FinishHour, shift.FinishMinute, 0);

                    if (_startTime > _finishTime) _finishTime = _finishTime.AddDays(1);

                    List<tblEvent> tblEvents = new EventDao().GetListByTimeWithFixMargin(_startTime, _finishTime, 0, iNodeId);
                    //tblEvents = tblEvents.OrderBy(r => r.NodeId).ThenBy(r => r.T3).ToList();
                    foreach (tblEvent tblEvent in tblEvents)
                    {
                        OperationForm _form = new OperationForm().Cast(tblEvent);
                        _form.strDuration = ConvertSecond2Time(_form.Duration);
                        model.Add(_form);
                    }

                }
            }
            else
            {
                _startTime = new DateTime(iYear, iMonth, iDay, Hour2UpdateReportDaily, 0, 0);
                _finishTime = _startTime.AddDays(NumberOfDays + 1);
                List<tblEvent> tblEvents = new EventDao().GetListByTimeWithFixMargin(_startTime, _finishTime, 0, iNodeId);
                //tblEvents = tblEvents.OrderBy(r => r.NodeId).ThenBy(r => r.T3).ToList();
                foreach (tblEvent tblEvent in tblEvents)
                {
                    OperationForm _form = new OperationForm().Cast(tblEvent);
                    _form.strDuration = ConvertSecond2Time(_form.Duration);
                    model.Add(_form);
                }

            }
            model = model.OrderBy(e => e.NodeTypeId).ThenBy(e => e.NodeId).ToList();
            return model;
        }

        public ActionResult StopDetailReport_Old(string FromDate, string ToDate, string NodeId, string WorkShift)
        {
            //Kiểm tra điều kiện tìm kiếm
            ViewBag.FromDate = FromDate;
            ViewBag.ToDate = ToDate;
            //load workshift
            int iWorkShift = 0;
            GetWorkShift(WorkShift, out iWorkShift);
            //int iNode = 0;
            //GetNode(NodeId, out iNode);

            DateTime startDate = DateTime.Today.AddDays(0 - Day2Close); ;

            DateTime endDate = DateTime.Today.AddDays(0 - Day2Close); ; //WebConstants.avMaxValue;


            if (Session[GlobalConstants.STARTDATE_SESSION] != null)
            {
                startDate = (DateTime)Session[GlobalConstants.STARTDATE_SESSION];
                Session.Remove(GlobalConstants.STARTDATE_SESSION);
            }
            if (Session[GlobalConstants.ENDDATE_SESSION] != null)
            {
                endDate = (DateTime)Session[GlobalConstants.ENDDATE_SESSION];
                Session.Remove(GlobalConstants.ENDDATE_SESSION);
            }

            if (!string.IsNullOrEmpty(FromDate))
            {
                try
                {
                    startDate = Convert.ToDateTime(FromDate.Trim());
                }
                catch { }
            }

            ViewBag.FromDate = startDate.ToString("yyyy/MM/dd");



            if (!string.IsNullOrEmpty(ToDate))
            {
                try
                {
                    endDate = Convert.ToDateTime(ToDate.Trim());
                }
                catch { }
            }

            ViewBag.ToDate = endDate.ToString("yyyy/MM/dd");

            //Lưu vào Session
            Session.Add(GlobalConstants.STARTDATE_SESSION, startDate);
            Session.Add(GlobalConstants.ENDDATE_SESSION, endDate);

            int NumberOfDays = (int)(endDate - startDate).TotalDays;

            int iNodeId = 0;

            if (!string.IsNullOrEmpty(NodeId))
            {
                iNodeId = Convert.ToInt32(NodeId);
            }
            ViewBag.NodeId = iNodeId;
            List<SelectListItem> Nodes = new List<SelectListItem>();
            List<tblNodeOnline> lst = new NodeOnlineDao().listAll(true);
            foreach (var nt in lst)
            {
                Nodes.Add(new SelectListItem() { Value = nt.NodeId.ToString(), Text = nt.NodeName.ToString() });
            }
            Nodes.Insert(0, new SelectListItem { Text = "-- " + Resources.Language.All + " --", Value = "" });

            ViewBag.Nodes = Nodes;

            List<OperationForm> model = new List<OperationForm>();

            using (SqlConnection con = new SqlConnection(strConStr))
            {

                con.Open();
                if (endDate.Date == DateTime.Now.Date || endDate.Date == DateTime.Now.AddHours(0 - Hour2UpdateReportDaily).Date)
                {
                    //Nếu là ngày hôm nay thì cho nó cập nhật lại phần sự kiện trước khi lấy.
                    string query = "exec [CreateEventReport] @Year = " + endDate.Year + ", @Month = " + endDate.Month + ", @Day = " + endDate.Day + ", @Hour2Update = " + Hour2UpdateReportDaily;
                    SqlCommand CreateCmd = new SqlCommand(query, con);
                    CreateCmd.ExecuteNonQuery();
                }


                string strQuery = " exec [CreateStopDetailReport] @FromYear = " + startDate.Year + ", @FromMonth = " + startDate.Month + ", @FromDay = " + startDate.Day + ", @ToYear = " + endDate.Year + ", @ToMonth = " + endDate.Month + ", @ToDay = " + endDate.Day;
                if (iNodeId != 0)
                {
                    strQuery += ", @NodeId = " + iNodeId;
                }
                strQuery += ", @Hour2Update = " + Hour2UpdateReportDaily;
                strQuery += ", @WorkShift = " + iWorkShift;

                DataTable dt = new DataTable();
                SqlCommand cmd = new SqlCommand(strQuery, con);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                int iCount = 0;
                foreach (DataRow dr in dt.Rows)
                {
                    OperationForm _form = new OperationForm();

                    _form = GetEachEvent(dr);
                    //_form.Duration = Math.Round(_form.Duration / 60.0, 1); //Tính ra phút
                    model.Add(_form);

                    iCount++;
                }
            }
            return View("StopDetailReport", model);
        }

        [HttpGet]
        public FileContentResult StopDetailToExcel_Old(string FromDate, string ToDate, string NodeId, string WorkShift = "")
        {
            //Kiểm tra điều kiện tìm kiếm
            DateTime startDate = DateTime.Today.AddDays(0 - Day2Close); ;

            DateTime endDate = DateTime.Today.AddDays(0 - Day2Close); ; //WebConstants.avMaxValue;

            if (!string.IsNullOrEmpty(FromDate))
            {
                try
                {
                    startDate = Convert.ToDateTime(FromDate.Trim());
                }
                catch { }
            }

            if (!string.IsNullOrEmpty(ToDate))
            {
                try
                {
                    endDate = Convert.ToDateTime(ToDate.Trim());
                }
                catch { }
            }
            int iWorkShift = -1;

            if (!string.IsNullOrEmpty(WorkShift))
            {
                iWorkShift = Convert.ToInt32(WorkShift);
            }

            int iNodeId = 0;

            if (!string.IsNullOrEmpty(NodeId))
            {
                iNodeId = Convert.ToInt32(NodeId);
            }
            ViewBag.NodeId = iNodeId;
            List<SelectListItem> Nodes = new List<SelectListItem>();
            List<tblNodeOnline> lst = new NodeOnlineDao().listAll(true);

            foreach (var nt in lst)
            {
                Nodes.Add(new SelectListItem() { Value = nt.NodeId.ToString(), Text = nt.NodeName.ToString() });
            }
            Nodes.Insert(0, new SelectListItem { Text = "-- " + Resources.Language.All + " --", Value = "" });

            ViewBag.Nodes = Nodes;

            string strNodeName = "";

            List<OperationForm> model = new List<OperationForm>();

            using (SqlConnection con = new SqlConnection(strConStr))
            {

                con.Open();
                if (endDate.Date == DateTime.Now.Date || endDate.Date == DateTime.Now.AddHours(0 - Hour2UpdateReportDaily).Date)
                {
                    //Nếu là ngày hôm nay thì cho nó cập nhật lại phần sự kiện trước khi lấy.
                    string query = "exec [CreateEventReport] @Year = " + endDate.Year + ", @Month = " + endDate.Month + ", @Day = " + endDate.Day + ", @Hour2Update = " + Hour2UpdateReportDaily;
                    SqlCommand CreateCmd = new SqlCommand(query, con);
                    CreateCmd.ExecuteNonQuery();
                }


                string strQuery = " exec [CreateStopDetailReport] @FromYear = " + startDate.Year + ", @FromMonth = " + startDate.Month + ", @FromDay = " + startDate.Day + ", @ToYear = " + endDate.Year + ", @ToMonth = " + endDate.Month + ", @ToDay = " + endDate.Day;
                if (iNodeId != 0)
                {
                    strQuery += ", @NodeId = " + iNodeId;
                }
                strQuery += ", @Hour2Update = " + Hour2UpdateReportDaily;
                strQuery += ", @WorkShift = " + iWorkShift;

                DataTable dtSource = new DataTable();
                SqlCommand cmd = new SqlCommand(strQuery, con);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dtSource);
                //int iCount = 0;
                foreach (DataRow dr in dtSource.Rows)
                {
                    OperationForm _form = new OperationForm();

                    _form = GetEachEvent(dr);
                    //_form.Duration = Math.Round(_form.Duration / 60.0, 1); //Tính ra phút
                    strNodeName = _form.NodeName;
                    model.Add(_form);

                    //  iCount++;
                }
            }
            // file export excel
            string fileName = "DetailRunning_";
            //Đầu tiên là phải nhân bản file templates

            string strFullpath = ConfigPath;
            string strTemplateFile = strFullpath + @"\Reports\Templates\DetailRuning_Template.xlsx";
            string strFileName = strFullpath;

            //Mở file ra để sẵn sàng ghi

            XLWorkbook wb = new XLWorkbook(strTemplateFile);

            //Lấy thằng worksheet
            IXLWorksheet ws = wb.Worksheet("F2EFF");
            //Điền mấy thằng râu ria
            string nameWorkShift = "";
            if (iWorkShift > -1) {

                if (iWorkShift == 0)
                {
                    nameWorkShift = "Cả ngày";
                }
                else
                {
                    nameWorkShift = new WorkingShiftDao().GetNameById(iWorkShift);
                }

            }
            ws.Cell("B4").Value = "'" + nameWorkShift;
            ws.Cell("D4").Value = "'" + startDate.ToString("dd/MM/yyyy");
            ws.Cell("F4").Value = "'" + endDate.ToString("dd/MM/yyyy");



            int StartRow = 7, iCount = 1;
            foreach (var _row in model)
            {
                ws.Cell("A" + (StartRow + iCount)).Value = iCount;
                //ws.Cell("B" + (StartRow + iCount)).Value = _row.ReceiptCode;
                ws.Cell("B" + (StartRow + iCount)).Value = _row.NodeName;
                ws.Cell("C" + (StartRow + iCount)).Value = _row.NodeTypeName;
                ws.Cell("D" + (StartRow + iCount)).Value = _row.EventDefName;
                ws.Cell("E" + (StartRow + iCount)).Value = _row.StartTime;
                ws.Cell("F" + (StartRow + iCount)).Value = _row.FinishTime;
                ws.Cell("G" + (StartRow + iCount)).Value = Math.Round(_row.Duration / 60, 2);
                ws.Cell("H" + (StartRow + iCount)).Value =  _row.strDuration;

                if (iCount < model.Count)
                {
                    ws.Row(StartRow + iCount).InsertRowsBelow(1);
                    iCount++;
                }
            }
            ////Phần tổng
            //ws.Cell("D" + (StartRow + iCount + 1)).Value = ConvertMin2Time(TotalRunning);
            //ws.Cell("E" + (StartRow + iCount + 1)).Value = ConvertMin2Time(TotalStop);
            //ws.Cell("F" + (StartRow + iCount + 1)).Value = ConvertMin2Time(TotalStopPlan);

            using (MemoryStream stream = new MemoryStream())
            {
                wb.SaveAs(stream);
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName + DateTime.Now.ToString("yyyy-MM-dd-HH-mm") + "].xlsx");
            }
        }
        #endregion

        // huantn 21-08-2019

        #region code new

        public ActionResult SetSession(string language)
        {
            Session[GlobalConstants.LANG_SESSION] = language;
            if (language != "")
            {
                return RedirectToAction("Index", "Home");
            }
            return View("SetSession");
        }
            public ActionResult StopWorkShiftReport(string SelectDate, string WorkShift, string isReport = "")
        {
            //Kiểm tra điều kiện tìm kiếm
            ViewBag.SelectDate = SelectDate;
            int iWorkShift = 0;
            GetWorkShift(WorkShift, out iWorkShift);
            // mac dinh la kieu 0 ~ dang bang , 1~bieu do
            int isViewReport = 0;
            if (!string.IsNullOrEmpty(isReport))
            {
                isViewReport = Convert.ToInt16(isReport);
            }
            ViewBag.isViewReport = isViewReport;

            List<SelectListItem> ReportTypes = new List<SelectListItem>();
            ReportTypes.Add(new SelectListItem() { Value = "0", Text = "Dạng bảng" });
            ReportTypes.Add(new SelectListItem() { Value = "1", Text = "Dạng biểu đồ" });
            ViewBag.ReportTypes = ReportTypes;

            DateTime ChooseDate = DateTime.Today;

            if (!string.IsNullOrEmpty(SelectDate))
            {
                try
                {
                    ChooseDate = Convert.ToDateTime(SelectDate.Trim());
                }
                catch { }
            }

            ViewBag.ChooseDate = ChooseDate.ToString("yyyy/MM/dd");


            List<TotalStopTypeForm> model = new List<TotalStopTypeForm>();

            using (SqlConnection con = new SqlConnection(strConStr))
            {

                con.Open();

                string strQuery = " exec [CreateStopTypeReport] @FromYear = " + ChooseDate.Year + ", @FromMonth = " + ChooseDate.Month + ", @FromDay = " + ChooseDate.Day + ", @ToYear = " + ChooseDate.Year + ", @ToMonth = " + ChooseDate.Month + ", @ToDay = " + ChooseDate.Day;

                strQuery += ", @Hour2Update = " + Hour2UpdateReportDaily + ", @WorkShift = " + iWorkShift;

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

            return View("StopWorkShiftReport", model);
        }

        public ActionResult StopReportByTypeAndWorkShift(string SelectDate, string NodeType, string WorkShift, string isReport = "")
        {
            //Kiểm tra điều kiện tìm kiếm
            ViewBag.SelectDate = SelectDate;

            // mac dinh la kieu 0 ~ dang bang , 1~bieu do
            int totalTimeWorkShift = 24 * 60;  // tong thoi gian cua ca lam viec 24h - ngay  hoac 8h - theo ca, don vi phut
            int isViewReport = 0;
            if (!string.IsNullOrEmpty(isReport))
            {
                isViewReport = Convert.ToInt16(isReport);
            }
            ViewBag.isViewReport = isViewReport;

            List<SelectListItem> ReportTypes = new List<SelectListItem>();
            ReportTypes.Add(new SelectListItem() { Value = "0", Text = "Dạng bảng" });
            ReportTypes.Add(new SelectListItem() { Value = "1", Text = "Dạng biểu đồ" });
            ViewBag.ReportTypes = ReportTypes;

            DateTime ChooseDate = DateTime.Today;

            if (!string.IsNullOrEmpty(SelectDate))
            {
                try
                {
                    ChooseDate = Convert.ToDateTime(SelectDate.Trim());
                }
                catch { }
            }

            ViewBag.ChooseDate = ChooseDate.ToString("yyyy/MM/dd");

            int iNodeType = 0;

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
            NodeTypes.Insert(0, new SelectListItem { Text = "-- " + Resources.Language.All + " --", Value = "" });

            ViewBag.NodeTypes = NodeTypes;

            // lay du lieu ca lam viec
            int iWorkShift = 0;

            if (!string.IsNullOrEmpty(WorkShift))
            {
                iWorkShift = Convert.ToInt32(WorkShift);
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
            WorkShifts.Insert(0, new SelectListItem { Text = "-- " + Resources.Language.All + " --", Value = "0" });
            ViewBag.WorkShifts = WorkShifts;

            ViewBag.totalTimeWorkShift = totalTimeWorkShift;

            List<TotalStopForm> model = new List<TotalStopForm>();

            using (SqlConnection con = new SqlConnection(strConStr))
            {

                con.Open();

                string strQuery = " exec [CreateStopReport] @FromYear = " + ChooseDate.Year + ", @FromMonth = " + ChooseDate.Month + ", @FromDay = " + ChooseDate.Day + ", @ToYear = " + ChooseDate.Year + ", @ToMonth = " + ChooseDate.Month + ", @ToDay = " + ChooseDate.Day;

                if (iNodeType != 0)
                {
                    strQuery += ", @NodeType = " + iNodeType;
                }
                strQuery += ", @Hour2Update = " + Hour2UpdateReportDaily;
                if (iWorkShift != 0)
                {
                    strQuery += ", @WorkShift = " + iWorkShift;
                }

                DataTable dt = new DataTable();
                SqlCommand cmd = new SqlCommand(strQuery, con);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                dt = ds.Tables[ds.Tables.Count - 1];
                int iCount = 0;
                foreach (DataRow dr in dt.Rows)
                {
                    TotalStopForm _form = new TotalStopForm();

                    _form = GetEachTotalStopEvent(dr);

                    model.Add(_form);

                    iCount++;
                }
            }

            return View("StopReportWorkShift", model);
        }

        public ActionResult StopDetailReportByWorkShif(string SelectDate, string NodeId, string WorkShift)
        {
            ViewBag.SelectDate = SelectDate;

            DateTime ChooseDate = DateTime.Today;

            if (!string.IsNullOrEmpty(SelectDate))
            {
                try
                {
                    ChooseDate = Convert.ToDateTime(SelectDate.Trim());
                }
                catch { }
            }

            ViewBag.ChooseDate = ChooseDate.ToString("yyyy/MM/dd");

            int iNodeId = 0;

            if (!string.IsNullOrEmpty(NodeId))
            {
                iNodeId = Convert.ToInt32(NodeId);
            }
            ViewBag.NodeId = iNodeId;
            List<SelectListItem> Nodes = new List<SelectListItem>();
            List<tblNodeOnline> lst = new NodeOnlineDao().listAll(true);
            foreach (var nt in lst)
            {
                Nodes.Add(new SelectListItem() { Value = nt.NodeId.ToString(), Text = nt.NodeName.ToString() });
            }
            Nodes.Insert(0, new SelectListItem { Text = "-- " + Resources.Language.All + " --", Value = "" });

            ViewBag.Nodes = Nodes;

            // lay du lieu ca lam viec
            int iWorkShift = 0;

            if (!string.IsNullOrEmpty(WorkShift))
            {
                iWorkShift = Convert.ToInt32(WorkShift);
            }
            ViewBag.WorkShift = iWorkShift;
            List<tblWorkingShift> lstWorkShift = new WorkingShiftDao().listAll().Where(x => x.Id != 1).ToList(); // 1 ~ id Ca HC
            List<SelectListItem> WorkShifts = new List<SelectListItem>();
            foreach (var _workShift in lstWorkShift)
            {
                WorkShifts.Add(new SelectListItem() { Value = _workShift.Id.ToString(), Text = _workShift.Name.ToString() });
            }
            WorkShifts.Insert(0, new SelectListItem { Text = "-- " + Resources.Language.All + " --", Value = "0" });
            ViewBag.WorkShifts = WorkShifts;

            List<OperationForm> model = new List<OperationForm>();

            using (SqlConnection con = new SqlConnection(strConStr))
            {

                con.Open();

                string strQuery = " exec [CreateStopDetailReport] @FromYear = " + ChooseDate.Year + ", @FromMonth = " + ChooseDate.Month + ", @FromDay = " + ChooseDate.Day + ", @ToYear = " + ChooseDate.Year + ", @ToMonth = " + ChooseDate.Month + ", @ToDay = " + ChooseDate.Day;
                if (iNodeId != 0)
                {
                    strQuery += ", @NodeId = " + iNodeId;
                }
                strQuery += ", @Hour2Update = " + Hour2UpdateReportDaily;

                strQuery += ", @WorkShift = " + iWorkShift;

                DataTable dt = new DataTable();
                SqlCommand cmd = new SqlCommand(strQuery, con);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                int iCount = 0;
                foreach (DataRow dr in dt.Rows)
                {
                    OperationForm _form = new OperationForm();

                    _form = GetEachEvent(dr);
                    //_form.Duration = Math.Round(_form.Duration / 60.0, 1); //Tính ra phút
                    model.Add(_form);

                    iCount++;
                }
            }
            return View("StopDetailReportByWorkShift", model);
        }

        public ActionResult ChartIncReport(string FromDate, string ToDate, string NodeType, string WorkShift)
        {
            //Kiểm tra điều kiện tìm kiếm
            ViewBag.FromDate = FromDate;
            ViewBag.ToDate = ToDate;
            int iWorkShift = 0;
            GetWorkShift(WorkShift, out iWorkShift);
            int iNodeType = 0;
            GetNodeType(NodeType, out iNodeType);
            DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            DateTime endDate = DateTime.Today.AddDays(0 - Day2Close); ; //WebConstants.avMaxValue;
            if (endDate < startDate)
            {
                endDate = startDate;
            }

            if (!string.IsNullOrEmpty(FromDate))
            {
                try
                {
                    startDate = Convert.ToDateTime(FromDate.Trim());
                }
                catch { }
            }

            ViewBag.FromDate = startDate.ToString("yyyy/MM/dd");

            if (!string.IsNullOrEmpty(ToDate))
            {
                try
                {
                    endDate = Convert.ToDateTime(ToDate.Trim());
                }
                catch { }
            }

            ViewBag.ToDate = endDate.ToString("yyyy/MM/dd");


            return View("ChartIncReport");
        }
        [WebMethod]

        public JsonResult GetChartIncReport(string FromDate, string ToDate, string NodeType, string WorkShift)
        {

            int iWorkShift = 0;
            GetWorkShift(WorkShift, out iWorkShift);

            DateTime startDate = DateTime.Today.AddDays(0 - Day2Close); ;

            DateTime endDate = DateTime.Today.AddDays(0 - Day2Close); ; //WebConstants.avMaxValue;

            if (!string.IsNullOrEmpty(FromDate))
            {
                try
                {
                    startDate = Convert.ToDateTime(FromDate.Trim());
                }
                catch { }
            }

            if (!string.IsNullOrEmpty(ToDate))
            {
                try
                {
                    endDate = Convert.ToDateTime(ToDate.Trim());
                }
                catch { }
            }

            int iNodeType = 0;

            if (!string.IsNullOrEmpty(NodeType))
            {
                iNodeType = Convert.ToInt32(NodeType);
            }
            ViewBag.NodeType = iNodeType;

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
                   // incPlanDuration += 24;
                    _formDr.PlanDuration = incPlanDuration = incPlanDuration + Math.Round(_formDr.PlanDuration / 3600, 0);
                    _formDr.ActualDuration = incActualDuration = incActualDuration + Math.Round(_formDr.ActualDuration / 3600, 0);
                    model.Add(_formDr);
                }
            }

            return Json(model, JsonRequestBehavior.AllowGet);
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
            WorkShifts.Insert(0, new SelectListItem { Text = "-- " + Resources.Language.All + " --", Value = "0" });
            ViewBag.WorkShifts = WorkShifts;
            ViewBag.totalTimeWorkShift = totalTimeWorkShift;
        }
        public void GetNodeType(string NodeType,out int iNodeType){
            iNodeType = 0;
            if (!string.IsNullOrEmpty(NodeType))
            {
                iNodeType = Convert.ToInt32(NodeType);
            }
            ViewBag.NodeType = iNodeType;

            List<SelectListItem> NodeTypes = new List<SelectListItem>();
            List<tblNodeType> lst = new NodeTypeDao().listAll(true);

            if (Is_Sumiden)
            {
                var session = (Model.DataModel.tblUser)Session[GlobalConstants.USER_SESSION];
                if (session.Role.Equals(GlobalConstants.AV_MANAGER_GROUP) ||
                    session.Role.Equals(GlobalConstants.AV_ADMIN_GROUP) ||
                    session.Role.Equals(GlobalConstants.AV_STAFF_GROUP) ||
                    session.Role.Equals(GlobalConstants.AV_STAFF_PlASTIC_GROUP)
                    )
                {
                    // cong ty E
                    tblNodeType CompE = lst.Where(x => x.Name.Equals("E")).First();
                    NodeTypes.Add(new SelectListItem() { Value = CompE.Id.ToString(), Text = CompE.Name.ToString() });
                }
                if (session.Role.Equals(GlobalConstants.AV_MANAGER_GROUP) ||
                    session.Role.Equals(GlobalConstants.AV_STAFF_GROUP) ||
                   session.Role.Equals(GlobalConstants.AV_ADMIN_GROUP)
                   )
                {
                    // cong ty khac
                    List<tblNodeType> lstCompOther = lst.Where(x => !x.Name.StartsWith("B") && !x.Name.Equals("E")).ToList();
                    foreach (var ntOth in lstCompOther)
                    {
                        NodeTypes.Add(new SelectListItem() { Value = ntOth.Id.ToString(), Text = ntOth.Name.ToString() });
                    }
                }
                if (session.Role.Equals(GlobalConstants.AV_MANAGER_GROUP) ||
                    session.Role.Equals(GlobalConstants.AV_ADMIN_GROUP) ||
                    session.Role.Equals(GlobalConstants.AV_STAFF_GROUP) ||
                    session.Role.Equals(GlobalConstants.AV_STAFF_COPPER_GROUP)
                   )
                {
                    // cong ty B
                    NodeTypes.Add(new SelectListItem() { Value = "-1", Text = "B" });
                    // cong ty con B
                    List<tblNodeType> lstCompB = lst.Where(x => x.Name.StartsWith("B")).ToList();
                    foreach (var ntB in lstCompB)
                    {
                        NodeTypes.Add(new SelectListItem() { Value = ntB.Id.ToString(), Text = "----" + ntB.Name.ToString() });
                    }
                }
            }
            else
            {
                foreach (var nt in lst)
                {
                    NodeTypes.Add(new SelectListItem() { Value = nt.Id.ToString(), Text = nt.Name.ToString() });
                }
            }
            NodeTypes.Insert(0, new SelectListItem { Text = "-- " + Resources.Language.All + " --", Value = "" });

            ViewBag.NodeTypes = NodeTypes;
        }
        public void GetNode(string Node, out int iNode)
        {
            iNode = 0;
            if (!string.IsNullOrEmpty(Node))
            {
                iNode = Convert.ToInt32(Node);
            }
            ViewBag.Node = iNode;

            List<SelectListItem> Nodes = new List<SelectListItem>();
            List<tblNodeOnline> lst = new NodeOnlineDao().listAll(true);

            foreach (var nt in lst)
            {
                Nodes.Add(new SelectListItem() { Value = nt.NodeId.ToString(), Text = nt.NodeName.ToString() });
            }
            Nodes.Insert(0, new SelectListItem { Text = "-- " + Resources.Language.All + " --", Value = "" });

            ViewBag.Nodes = Nodes;
        }

        #endregion
    }
}