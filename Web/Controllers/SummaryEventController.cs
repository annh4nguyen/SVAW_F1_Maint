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
using System.Threading;
using System.Globalization;

namespace avSVAW.Controllers
{
    public class SummaryEventController : Controller
    {
        int Day2Close = int.Parse(ConfigurationManager.AppSettings["Day2Close"]);
        string strConStr = ConfigurationManager.ConnectionStrings["ConStr"].ToString();
        // GET: EventDef
        /// <summary>  
        /// First Action method called when page loads  
        /// Fetch all the rows from DB and display it  
        /// </summary>  
        /// <returns>Home View</returns>  
        public ActionResult Index()
        {
            var sessionLang = Session[GlobalConstants.LANG_SESSION];
            string culture = "vi-VN";
            if (sessionLang != null)
            {
                string lang = Convert.ToString(sessionLang);
                culture = string.Empty;
                if (lang.ToLower().CompareTo("vi") == 0 || string.IsNullOrEmpty(culture))
                {
                    culture = "vi-VN";
                }
                if (lang.ToLower().CompareTo("en") == 0 || string.IsNullOrEmpty(culture))
                {
                    culture = "en-US";
                }

            }
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(culture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
            ViewBag.LoadedTime = 0;
            ViewBag.TimeOut = 0;
            Models.ConfigForm config = new ConfigController().LoadConfig("TimeDelay1");
            Models.ConfigForm NodeType = new ConfigController().LoadNextConfigByNodeType("TimeDelay2", "0");
            if (config !=null && config.ConfigValue != null && config.ConfigShow != null && Convert.ToBoolean(config.ConfigShow))
            {
                ViewBag.TimeOut = int.Parse(config.ConfigValue);
                if (NodeType != null)
                {
                    ViewBag.NodeTypeId = NodeType.ConfigText;
                }
            }
            else
            {
                if(NodeType == null)
                {
                    return RedirectToAction("Index", "NodeOnline");
                }
                else
                {
                    return RedirectToAction("Detail", "SummaryEvent", new { @Id = NodeType.ConfigText });
                }
                
            }

            DateTime Yesterday = GetLastestWorkingDay();
            int iYear = Yesterday.Year;
            int iMonth = Yesterday.Month;
            int iDay = Yesterday.Day;
            ViewBag.ReportDate = Yesterday.ToString("dd/MM/yyyy");

            List<SummaryEventTypeForm> lstForm = new List<SummaryEventTypeForm>();
            List<tblNodeType> lstType = new NodeTypeDao().listAll(true);

            //List<tblSummaryEventReport> lstReport = new SummaryEventDao().ListAll(iYear, iMonth, iDay, 0);
            List<tblSummaryEventReport> lstReport = new SummaryEventDao().GetListAll(iYear, iMonth, iDay, 0);

            double TotalPlan = 0, TotalActual = 0;
            int iCount = 0, iTotalNode = 0;
            foreach (var _type in lstType)
            {
                if (!_type.Name.Contains("NC"))
                {
                    SummaryEventTypeForm _form = new SummaryEventTypeForm();
                    _form.NodeTypeId = _type.Id;
                    _form.Year = iYear;
                    _form.Month = iMonth;
                    _form.Day = iDay;
                    int iNodeCount = 0;
                    foreach (var _item in lstReport)
                    {
                        if (_item.NodeTypeId == _type.Id)
                        {
                            _form.PlanDuration += (double)_item.PlanDuration;
                            _form.ActualDuration += (double)_item.RunningDuration;
                            iNodeCount++;
                        }
                    }
                    string strINodeCount = iNodeCount == 0 ? "" : iNodeCount.ToString();
                    _form.NodeTypeName = _type.Name + "\n(" + strINodeCount + " máy)";
                    _form.NumberOfNodes = iNodeCount;

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
                    iTotalNode += iNodeCount;
                }
            }
            double _totalPercent = 0;
            if (TotalPlan != 0)
            {
                _totalPercent =100*(float)TotalActual / TotalPlan;
            }
            ViewBag.TotalPercent = Math.Round(_totalPercent,1);
            ViewBag.NumberOfNodes = iTotalNode;
            ViewBag.NumberOfTypes = iCount;

            return View("Index", lstForm);
        }
        public ActionResult Detail(int Id)
        {
            var sessionLang = Session[GlobalConstants.LANG_SESSION];
            string culture = "vi-VN";
            if (sessionLang != null)
            {
                string lang = Convert.ToString(sessionLang);
                culture = string.Empty;
                if (lang.ToLower().CompareTo("vi") == 0 || string.IsNullOrEmpty(culture))
                {
                    culture = "vi-VN";
                }
                if (lang.ToLower().CompareTo("en") == 0 || string.IsNullOrEmpty(culture))
                {
                    culture = "en-US";
                }

            }
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(culture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
            //ViewBag.TimeOut  = 1000 * int.Parse(ConfigurationManager.AppSettings["TimeOut"]);

            //ViewBag.LoadedTime = 0;
            //ViewBag.TimeOut = int.Parse(new ConfigController().LoadConfig("TimeOut").ConfigValue);

            //Models.ConfigForm config = new ConfigController().LoadConfig("LoadedTime");

            //if (config.ConfigText != "Detail")
            //{
            //    //Nếu nó khác thì load lần đầu --> Ghi lại giá trị
            //    ViewBag.LoadedTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            //    new ConfigController().Save(config.ConfigKey, ViewBag.LoadedTime, "Detail");
            //}
            //else
            //{
            //    ViewBag.LoadedTime = config.ConfigValue;
            //}
            ViewBag.NodeTypeId = "0";
            ViewBag.LoadedTime = 0;
            ViewBag.TimeOut = 0;
            Models.ConfigForm config = new ConfigController().LoadConfigByNodeType("TimeDelay2",Id.ToString());
            Models.ConfigForm NodeType = new ConfigController().LoadNextConfigByNodeType("TimeDelay2", Id.ToString());
            if (config !=null && config.ConfigValue != null && config.ConfigShow != null && Convert.ToBoolean(config.ConfigShow))
            {
                ViewBag.TimeOut = int.Parse(config.ConfigValue);
                if (NodeType != null)
                {
                    ViewBag.NodeTypeId = NodeType.ConfigText;
                }
            }
            else
            {
               
                if (NodeType == null)
                {
                    return RedirectToAction("Index", "NodeOnline");
                }
                else
                {
                    return RedirectToAction("Detail", "SummaryEvent", new { @Id = NodeType });
                }

            }

            DateTime Yesterday = GetLastestWorkingDay();
            int iYear = Yesterday.Year;
            int iMonth = Yesterday.Month;
            int iDay = Yesterday.Day;
            ViewBag.ReportDate = Yesterday.ToString("dd/MM/yyyy");

            List<SummaryEventForm> lstForm = new List<SummaryEventForm>();

            //List<tblSummaryEventReport> lstReport = new SummaryEventDao().ListAll(iYear, iMonth, iDay, 0);
            List<tblSummaryEventReport> lstReport = new SummaryEventDao().GetListAll(iYear, iMonth, iDay, 0);

            double TotalPlan = 0, TotalActual = 0;
            int iCount = 0;
            string strNodeTypeName = "";
            foreach (var _item in lstReport)
            {
                if (_item.NodeTypeId == Id)
                {
                    SummaryEventForm _form = new SummaryEventForm();
                    _form.NodeId = _item.NodeId;
                    _form.NodeName = _item.NodeName;
                            
                    _form.NodeTypeId = Id;
                    _form.NodeTypeName = _item.NodeTypeName;

                    _form.PlanDuration += (double)_item.PlanDuration;
                    _form.RunningDuration += (double)_item.RunningDuration;
                    _form.StopDuration += (double)_item.StopDuration;

                    if (_form.PlanDuration != 0)
                    {
                        _form.WorkingPercent = (float)_form.RunningDuration / _form.PlanDuration;
                    }
                    _form.WorkingPercent = Math.Round(100 * _form.WorkingPercent, 1);
                    lstForm.Add(_form);

                    strNodeTypeName = _form.NodeTypeName;
                    TotalPlan += _form.PlanDuration;
                    TotalActual += _form.RunningDuration;

                    iCount++;

                }
            }

            double _totalPercent = 0;
            if (TotalPlan != 0)
            {
                _totalPercent = 100 * (float)TotalActual / TotalPlan;
            }
            ViewBag.TotalPercent = Math.Round(_totalPercent, 1);
            ViewBag.NumberOfNodes = iCount;
            ViewBag.NodeTypeName = strNodeTypeName;

            return View("Detail", lstForm);
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

                    List<tblWorkingPlan> workingPlans = new WorkingPlanDao().ListAll(Yesterday.Year, Yesterday.Month, Yesterday.Day, 0);
                    iCount = workingPlans.Count;

                    /*
                    string strQuery = "SELECT * FROM tblWorkingPlan WHERE [Year] = " + Yesterday.Year + " AND [Month] = " + Yesterday.Month + " AND [Day] = " + Yesterday.Day;

                    DataTable dt = new DataTable();
                    SqlCommand cmd = new SqlCommand(strQuery, con);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    dt = ds.Tables[ds.Tables.Count - 1];
                    iCount = dt.Rows.Count;
                    */
                }
                while (iCount <= 0);

            }
   
            return Yesterday;
        }

    }
}