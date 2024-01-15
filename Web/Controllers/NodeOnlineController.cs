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
using System.Data.SqlClient;
using System.Threading;
using System.Globalization;

namespace avSVAW.Controllers
{
    public class NodeOnlineController : Controller
    {
        string strConStr = ConfigurationManager.ConnectionStrings["ConStr"].ToString();
        public int Hour2UpdateReportDaily = int.Parse(ConfigurationManager.AppSettings["UpdateReportDaily"]);
        public int intRefresh = 30;      //30s
        // GET: EventDef
        /// <summary>  
        /// First Action method called when page loads  
        /// Fetch all the rows from DB and display it  
        /// </summary>  
        /// <returns>Home View</returns>  
        public ActionResult Index(string ViewType,string cRefresh)
        {
            /*
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
            */
            ViewBag.Factories = new FactoryDao().listAll();
            ViewBag.Lines = new LineDao().listAll();
            ViewBag.Zones = new ZoneDao().listAll();
            ViewBag.NodeTypes = new NodeTypeDao().listAll();
            ViewBag.ViewType = string.IsNullOrEmpty(ViewType) ? "0" : "1";

            ViewBag.ShowType = ConfigurationManager.AppSettings["DefaultShow"].ToString();
            ViewBag.isShowZone = (ConfigurationManager.AppSettings["ShowZone"].ToString() == "1");
            ViewBag.isShowLine = (ConfigurationManager.AppSettings["ShowLine"].ToString() == "1");


            ViewBag.LoadedTime = 0;
            ViewBag.TimeOut = 0;
            Models.ConfigForm config = new ConfigController().LoadConfig("TimeDelay0");
            int iTimeout = 0;
            if (config.ConfigValue != null && config.ConfigShow != null && Convert.ToBoolean(config.ConfigShow))
            {
                ViewBag.TimeOut = iTimeout = int.Parse(config.ConfigValue);
            }
            else
            {
                return RedirectToAction("Index", "SummaryEvent");
            }

            ViewBag.isRefresh = 0;
            ViewBag.cRefresh = 0;


            //List<tblNodeOnline> lst = new NodeOnlineDao().listAll();
            List<NodeOnlineForm> model = new List<NodeOnlineForm>();

            using (SqlConnection con = new SqlConnection(strConStr))
            {
                con.Open();

                //Cập nhật thằng kế hoạch phát
                DateTime d = DateTime.Now;
                //EXEC CheckPlannedStop @Year = 2019, @Month = 7, @Day = 29, @Hour = 3, @Minute = 0, @Hour2Update = 6
                string query = "exec [CheckPlannedStop] @Year = " + d.Year + ", @Month = " + d.Month + ", @Day = " + d.Day + ", @Hour =  " + d.Hour + ", @Minute = " + d.Minute + ", @Hour2Update = " + Hour2UpdateReportDaily;

                SqlCommand CreateCmd = new SqlCommand(query, con);
                CreateCmd.ExecuteNonQuery();
                //DateTime _now = DateTime.Now;

                //string strSQL = "EXEC CheckPlannedStop @Year = " + _now.Year + ", @Month = " + _now.Month + ", @Day = " + _now.Day + ", @Hour = " + _now.Hour + ", @Minute = " + _now.Minute + ", @Hour2Update = " + Hour2UpdateReportDaily;

                //DataTable dtSource = new DataTable();
                //SqlCommand cmd = new SqlCommand(strSQL, con);
                //SqlDataAdapter da = new SqlDataAdapter(cmd);
                //da.Fill(dtSource);
                //foreach (DataRow dr in dtSource.Rows)
                //{
                //    NodeOnlineForm form = new NodeOnlineForm();
                //    form.Cast(dr);
                //    model.Add(form);
                //}
                List<tblLine> lstLine = new LineDao().listAll();
                List<tblNodeOnline> lstNode = new NodeOnlineDao().listAll(false);
                foreach (tblLine _line in lstLine)
                {
                    foreach (var _node in lstNode.Where(x => x.LineId == _line.Id).ToList())
                    {
                        //if (lstLine.Where(x => x.Id == _node.LineId).Count() > 0)
                        //{
                            NodeOnlineForm form = new NodeOnlineForm();
                            form.Active = _node.Active;
                            form.DataToShow = _node.DataToShow;
                            form.FactoryId = _node.FactoryId;
                            form.FactoryName = _node.FactoryName;
                            form.LineId = _node.LineId;
                            form.NodeId = _node.NodeId;
                            form.NodeName = _node.NodeName;
                            form.NodeTypeId = _node.NodeTypeId;
                            form.NodeTypeName = _node.NodeTypeName;
                            form.nOrder = _node.nOrder;
                            form.OnlineTimeCount = _node.OnlineTimeCount;
                            form.Planned = _node.Planned;
                            form.Status = _node.Status;
                            form.TimeOut = _node.TimeOut;
                            form.UpdateTime = _node.UpdateTime;
                            form.WorkingTimeCount = _node.WorkingTimeCount;
                            form.ZoneId = _node.ZoneId;
                            form.ZoneName = _node.ZoneName;
                            model.Add(form);
                        //}
                    }
                }
            }
            return View("ViewLayout", model);
        }

        /// <summary>  
        /// Action method, called when the "Add New Record" link clicked  
        /// </summary>  
        /// <returns>Create View</returns>  
 

        [WebMethod]
        public JsonResult GetNodeOnlineJsonList()
        {
            List<tblNodeOnline> model = new NodeOnlineDao().listAll();
            return Json(model, JsonRequestBehavior.AllowGet);
        }
    }
}