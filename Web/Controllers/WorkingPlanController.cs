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
    public class WorkingPlanController : BaseController
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

            List<WorkingPlanForm> model = new List<WorkingPlanForm>();
            List<tblWorkingPlan> plans = new WorkingPlanDao().ListAll(iYear, iMonth,0,0);
            int iWorkingId = 0;
            foreach (tblWorkingPlan p in plans)
            {
                if (p.WorkingId != iWorkingId)
                {
                    iWorkingId = p.WorkingId;

                    WorkingPlanForm w = new WorkingPlanForm();
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
        public ActionResult Create(string Year = "", string Month = "")
        {
            ViewBag.Id = 0;
            int iYear = DateTime.Now.Year;
            if (!string.IsNullOrEmpty(Year))
            {
                iYear = Convert.ToInt32(Year);
            }
            int iMonth = DateTime.Now.Month;
            if (!string.IsNullOrEmpty(Month))
            {
                iMonth = Convert.ToInt32(Month);
            }
            ViewBag.Year = iYear;
            ViewBag.Month = iMonth;

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

            tblWorkingPlan model = new tblWorkingPlan();
            model.Year = ViewBag.Year;

            return View();
        }

        [HttpPost]
        public ActionResult Create(tblWorkingPlan model)
        {
            if (ModelState.IsValid)
            {
                new WorkingPlanDao().Insert(model);
            }
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int Id, string Year = "", string Month = "")
        {
            WorkingPlanForm w = new WorkingPlanForm();
            int iYear = 0;
            if (!string.IsNullOrEmpty(Year))
            {
                iYear = Convert.ToInt32(Year);
            }
            int iMonth =0;
            if (!string.IsNullOrEmpty(Month))
            {
                iMonth = Convert.ToInt32(Month);
            }
            w.Cast(iYear, iMonth, Id);

            ViewBag.Year = iYear>0?iYear:w.Year;
            ViewBag.Month = iMonth > 0 ? iMonth : w.Month;
            ViewBag.WorkingId = w.WorkingId;
            ViewBag.WorkingPlanDays = "; " + w.Days;
            ViewBag.WorkingPlanShifs = "; " + w.ShiftIds;
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


        [HttpPost]
        public ActionResult Edit(tblWorkingShift model)
        {
            new WorkingShiftDao().Update(model);
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int Id)
        {
            if (Id != 0) //Đây là update rồi
            {
                //Xóa bỏ các loại trong bảng đi
                new WorkingPlanDao().DeleteWorking(Id);
            }
            return RedirectToAction("Index");
        }


        [WebMethod]
        public JsonResult InserOrUpdateWorkingPlan(string Id, string Year, string Month, string Shifts, string Days, string Nodes)
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
                //new WorkingPlanDao().DeleteWorking(_Id);
                //Xóa bằng lệnh cho nhanh
                using (SqlConnection con = new SqlConnection(strConStr))
                {
                    con.Open();
                    string query = "DELETE FROM [tblWorkingPlan] WHERE WorkingId = " + _Id;
                    SqlCommand CreateCmd = new SqlCommand(query, con);
                    CreateCmd.ExecuteNonQuery();
                }
            }
            //Chèn thêm mới vào
            string[] arrShift = Shifts.Split(';');
            string[] arrDay = Days.Split(';');
            string[] arrNode = Nodes.Split(';');

            int WorkingId = new WorkingPlanDao().MaxWorkingId() + 1;
            using (SqlConnection con = new SqlConnection(strConStr))
            {
                con.Open();

                foreach (string _shift in arrShift)
                {
                    if (_shift != "")
                    {

                        foreach (string _day in arrDay)
                        {
                            if (_day != "")
                            {
                                foreach (string _node in arrNode)
                                {
                                    if (_node != "")
                                    {
                                        //tblWorkingPlan p = new tblWorkingPlan();
                                        //p.WorkingId = WorkingId;
                                        //p.Month = _Month;
                                        //p.Year = _Year;
                                        //p.Day = int.Parse(_day);
                                        //p.ShiftId = int.Parse(_shift);
                                        //p.NodeId = int.Parse(_node);
                                        //bool Check = new WorkingPlanDao().CheckDuplicate(p);
                                        //if (!Check) {
                                        //    new WorkingPlanDao().Insert(p);
                                        //}

                                        string query = "INSERT INTO [tblWorkingPlan] ([WorkingId],[Year],[Month],[Day],[ShiftId],[NodeId]) ";
                                        query += " VALUES(" + WorkingId + "," + _Year + "," + _Month + "," + _day + "," + _shift + "," + _node + ")";
                                        SqlCommand CreateCmd = new SqlCommand(query, con);
                                        CreateCmd.ExecuteNonQuery();
                                    }
                                }
                            }

                        }
                    }

                }
            //}

            ////Chạy lại phần báo cáo

            //using (SqlConnection con = new SqlConnection(strConStr))
            //{

                con.Open();
                int days = DateTime.DaysInMonth(_Year, _Month);

                for (int iDay = 1; iDay <= days; iDay++)
                {
                    string query = "exec [CreateEventReport] @Year = " + _Year + ", @Month = " + _Month + ", @Day = " + iDay + ", @Hour2Update = " + Hour2UpdateReportDaily;
                    SqlCommand CreateCmd = new SqlCommand(query, con);
                    CreateCmd.ExecuteNonQuery();
                }
            }

            return Json("OK", JsonRequestBehavior.AllowGet);
        }
    }
}