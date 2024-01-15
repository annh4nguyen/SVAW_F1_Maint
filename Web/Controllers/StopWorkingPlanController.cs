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
using System.Data.SqlClient;
using System.Configuration;

namespace avSVAW.Controllers
{
    public class StopWorkingPlanController : BaseController
    {
        string strConStr = ConfigurationManager.ConnectionStrings["ConStr"].ToString();
        public int Hour2UpdateReportDaily = int.Parse(ConfigurationManager.AppSettings["UpdateReportDaily"]);

        // GET: EventDef
        /// <summary>  
        /// First Action method called when page loads  
        /// Fetch all the rows from DB and display it  
        /// </summary>  
        /// <returns>Home View</returns>  
        public ActionResult Index(string Year, string Month, string Shift, string NodeType, string Node)
        {
            int iYear = DateTime.Now.Year, iMonth = DateTime.Now.Month;
            int iNodeType = 0, iShift = 0;
            string strNode = "";

            if (!string.IsNullOrEmpty(Year))
            {
                iYear = Convert.ToInt32(Year);
            }
            if (!string.IsNullOrEmpty(Month))
            {
                iMonth = Convert.ToInt32(Month);
            }

            if (!string.IsNullOrEmpty(NodeType))
            {
                iNodeType = Convert.ToInt32(NodeType);
            }

            if (!string.IsNullOrEmpty(Shift))
            {
                iShift = Convert.ToInt32(Shift);
            }

            if (!string.IsNullOrEmpty(Node))
            {
                strNode = Node;
            }

            ViewBag.Year = iYear;
            ViewBag.Month = iMonth;

            ViewBag.Shift = iShift;
            ViewBag.NodeType = iNodeType;
            ViewBag.Node = strNode;


            List<SelectListItem> Years = new List<SelectListItem>();
            for (int i = ViewBag.Year; i <= ViewBag.Year + 1; i++)
            {
                Years.Add(new SelectListItem() { Value = i.ToString(), Text = i.ToString() });
            }
            ViewBag.Years = Years;

            List<SelectListItem> Months = new List<SelectListItem>();
            for (int i = 0; i <= 12; i++)
            {
                Months.Add(new SelectListItem() { Value = i.ToString(), Text = i.ToString() });
            }
            ViewBag.Months = Months;

            List<SelectListItem> NodeTypes = new List<SelectListItem>();
            List<tblNodeType> lst = new NodeTypeDao().listAll(true);
            foreach (var nt in lst)
            {
                NodeTypes.Add(new SelectListItem() { Value = nt.Id.ToString(), Text = nt.Name.ToString() });
            }
            NodeTypes.Insert(0, new SelectListItem { Text = "-- " + Resources.Language.All + " --", Value = "" });

            ViewBag.NodeTypes = NodeTypes;

            List<SelectListItem> Shifts = new List<SelectListItem>();
            List<tblWorkingShift> shifts = new WorkingShiftDao().listAll();
            foreach (var s in shifts)
            {
                Shifts.Add(new SelectListItem() { Value = s.Id.ToString(), Text = s.Name + " [" + s.StartHour + ":" + s.StartMinute + "-" + s.FinishHour + ":" + s.FinishMinute + "]" });
            }
            Shifts.Insert(0, new SelectListItem { Text = "-- " + Resources.Language.All + " --", Value = "" });

            ViewBag.Shifts = Shifts;

            List<StopWorkingPlanForm> model = new List<StopWorkingPlanForm>();
            List<tblStopWorkingPlan> plans = new StopWorkingPlanDao().ListAll(iYear, iMonth,0,0);
            int iWorkingId = 0;
            foreach (tblStopWorkingPlan p in plans)
            {
                if (p.WorkingId != iWorkingId)
                {
                    iWorkingId = p.WorkingId;

                    StopWorkingPlanForm w = new StopWorkingPlanForm();
                    w.Cast(iYear, iMonth, iWorkingId);
                    if (w.isShow(iShift, iNodeType, strNode))
                    {
                        model.Add(w);
                    }
                }
            }

            return View("Index", model);
        }

        /// <summary>  
        /// Action method, called when the "Add New Record" link clicked  
        /// </summary>  
        /// <returns>Create View</returns>  
        public ActionResult Create()
        {
            ViewBag.Id = 0;
            ViewBag.Year = DateTime.Now.Year;
            ViewBag.Month = DateTime.Now.Month;

            List<SelectListItem> Years = new List<SelectListItem>();
            for (int i = ViewBag.Year; i <= ViewBag.Year + 1; i++)
            {
                Years.Add(new SelectListItem() { Value = i.ToString(), Text = i.ToString() });
            }
            ViewBag.Years = Years;

            List<SelectListItem> Months = new List<SelectListItem>();
            for (int i = 0; i <= 12; i++)
            {
                Months.Add(new SelectListItem() { Value = i.ToString(), Text = i.ToString() });
            }
            ViewBag.Months = Months;

            ViewBag.Days = DateTime.DaysInMonth(ViewBag.Year, ViewBag.Month);
            ViewBag.Nodes = new NodeDao().listAll(true);
            ViewBag.Shifts = new WorkingShiftDao().listAll();
            ViewBag.NodeTypes = new NodeTypeDao().listAll(true);

            tblStopWorkingPlan model = new tblStopWorkingPlan();
            model.Year = ViewBag.Year;
            model.Month = ViewBag.Month;

            return View();
        }

        [HttpPost]
        public ActionResult Create(tblStopWorkingPlan model)
        {
            if (ModelState.IsValid)
            {
                new StopWorkingPlanDao().Insert(model);
            }
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int Id)
        {
            StopWorkingPlanForm w = new StopWorkingPlanForm();
            w.Cast(0, 0, Id);

            ViewBag.Year = w.Year;
            ViewBag.Month = w.Month;
            ViewBag.WorkingId = w.WorkingId;
            ViewBag.FromHour = w.FromHour;
            ViewBag.FromMinute = w.FromMinute;
            ViewBag.ToHour = w.ToHour;
            ViewBag.ToMinute = w.ToMinute;

            ViewBag.WorkingPlanDays = "; " + w.Days;
            ViewBag.WorkingPlanNodes = "; " + w.NodeIds;

            List<SelectListItem> Years = new List<SelectListItem>();
            for (int i = ViewBag.Year; i <= ViewBag.Year + 1; i++)
            {
                Years.Add(new SelectListItem() { Value = i.ToString(), Text = i.ToString() });
            }
            ViewBag.Years = Years;

            List<SelectListItem> Months = new List<SelectListItem>();
            for (int i = 0; i <= 12; i++)
            {
                Months.Add(new SelectListItem() { Value = i.ToString(), Text = i.ToString() });
            }
            ViewBag.Months = Months;

            ViewBag.Days = DateTime.DaysInMonth(ViewBag.Year, ViewBag.Month);
            ViewBag.Nodes = new NodeDao().listAll(true);
            ViewBag.Shifts = new WorkingShiftDao().listAll();
            ViewBag.NodeTypes = new NodeTypeDao().listAll(true);

            return View();
        }


        //[HttpPost]
        //public ActionResult Edit(tblWorkingShift model)
        //{
        //    new WorkingShiftDao().Update(model);
        //    return RedirectToAction("Index");
        //}

        public ActionResult Delete(int Id)
        {
            if (Id != 0) //Đây là update rồi
            {
                //Xóa bỏ các loại trong bảng đi
                new StopWorkingPlanDao().DeleteWorking(Id);
            }
            return RedirectToAction("Index");
        }


        [WebMethod]
        public JsonResult InserOrUpdateWorkingPlan(string Id, string Year, string Month, string FromHour, string FromMinute, string ToHour, string ToMinute, string Days, string Nodes)
        {
            int _Id = 0;
            int _Year = int.Parse(Year);
            int _Month = int.Parse(Month);

            if (!string.IsNullOrEmpty(Id))
            {
                _Id = int.Parse(Id); ;
            }

            if (_Id != 0) //Đây là update rồi
            {
                //Xóa bỏ các loại trong bảng đi
                new StopWorkingPlanDao().DeleteWorking(_Id);
            }
            //Chèn thêm mới vào
            string[] arrDay = Days.Split(';');
            string[] arrNode = Nodes.Split(';');


            int WorkingId = new StopWorkingPlanDao().MaxWorkingId() + 1;

            foreach (string _day in arrDay)
            {
                if (_day != "")
                {
                    foreach (string _node in arrNode)
                    {
                        if (_node != "")
                        {
                            tblStopWorkingPlan p = new tblStopWorkingPlan();
                            p.WorkingId = WorkingId;
                            p.Month = _Month;
                            p.Year = _Year;
                            p.Day = int.Parse(_day);
                            p.FromHour = int.Parse(FromHour);
                            p.FromMinute = int.Parse(FromMinute);
                            p.ToHour = int.Parse(ToHour);
                            p.ToMinute = int.Parse(ToMinute);
                            p.NodeId = int.Parse(_node);
                            bool Check = new StopWorkingPlanDao().CheckDuplicate(p);
                            if (!Check) {
                                new StopWorkingPlanDao().Insert(p);
                            }
                        }
                    }
                }

            }

            //Chạy lại phần báo cáo

            using (SqlConnection con = new SqlConnection(strConStr))
            {

                con.Open();
                //Nếu là ngày hôm nay thì cho nó cập nhật lại phần sự kiện trước khi lấy.
                //exec [CreateEventReport] @Year = 2019, @Month = 7, @Day = 22, @Hour2Update = 6

                int days = DateTime.DaysInMonth(_Year, _Month);

                for (int iDay = 1; iDay <= days; iDay++) {
                    string query = "exec [CreateEventReport] @Year = " + _Year + ", @Month = " + _Month + ", @Day = " + iDay + ", @Hour2Update = " + Hour2UpdateReportDaily;
                    SqlCommand CreateCmd = new SqlCommand(query, con);
                    CreateCmd.ExecuteNonQuery();
                }
            }

            return Json("OK", JsonRequestBehavior.AllowGet);
        }
    }
}