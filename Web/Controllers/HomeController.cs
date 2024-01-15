using Common;
using Model.Dao;
using Model.DataModel;
using avSVAW.App_Start;
using avSVAW.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;


namespace avSVAW.Controllers
{
    [SessionTimeout]
    public class HomeController : BaseController
    {
        string strConStr = ConfigurationManager.ConnectionStrings["ConStr"].ToString();

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

    }
}
