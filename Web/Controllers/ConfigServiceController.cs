//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Data;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;
//using System.Web.Services;
//using Common;
//using Model.Dao;
//using Model.DataModel;
//using avSVAW.Common;
//using avSVAW.Models;
//using avSVAW.App_Start;
//using System.Web.Configuration;

//namespace avSVAW.Controllers
//{
//    public class ConfigServiceController : BaseController
//    {
//        public static string ConfigPath = WebConfigurationManager.AppSettings["ConfigPath"];

//        // GET: EventDef
//        /// <summary>  
//        /// First Action method called when page loads  
//        /// Fetch all the rows from DB and display it  
//        /// </summary>  
//        /// <returns>Home View</returns>  
//        public ActionResult Index()
//        {
//            List<tblWorkingShift> lstWorkShift = new WorkingShiftDao().listAll().Where(x => x.Id != 1).ToList(); // 1 ~ id Ca HC
//            ViewBag.WorkShifts = lstWorkShift;
//            tblConfigService ConfigService = new ConfigServiceDao().listAll().First();
//            ViewBag.ConfigService = ConfigService;
//            return View("Index");
//        }
   

//        [HttpPost]
//        public ActionResult Save(string ExportWorkShift, string ExportWeek, string ExportMonth,string ExportQuarter,string ExportYear,string DateInWeek,string PathFolder)
//        {
//            var dao = new ConfigServiceDao();
//            tblConfigService ConfigService = new ConfigServiceDao().listAll().First();
//            bool _ExportWorkShift = false, _ExportWeek = false, _ExportMonth = false, _ExportQuarter = false, _ExportYear = false;
//            if (!string.IsNullOrEmpty(ExportWorkShift))
//            {
//                _ExportWorkShift = Convert.ToBoolean(ExportWorkShift);

//            }
//            if (!string.IsNullOrEmpty(ExportWeek))
//            {
//                _ExportWeek = Convert.ToBoolean(ExportWeek);

//            }
//            if (!string.IsNullOrEmpty(ExportMonth))
//            {
//                _ExportMonth = Convert.ToBoolean(ExportMonth);

//            }
//            if (!string.IsNullOrEmpty(ExportQuarter))
//            {
//                _ExportQuarter = Convert.ToBoolean(ExportQuarter);

//            }
//            if (!string.IsNullOrEmpty(ExportYear))
//            {
//                _ExportYear = Convert.ToBoolean(ExportYear);

//            }
//            int iDateInWeek = 0;
//            if (!string.IsNullOrEmpty(DateInWeek))
//            {
//                iDateInWeek = Convert.ToInt32(DateInWeek);
//            }
//            string sPathFolder = "";
//            if (!string.IsNullOrEmpty(PathFolder)){
//                sPathFolder = PathFolder;
//            }
//            ConfigService.ExportWeek = _ExportWeek;
//            ConfigService.ExportWorkShift = _ExportWorkShift;
//            ConfigService.ExportMonth = _ExportMonth;
//            ConfigService.ExportQuarter = _ExportQuarter;
//            ConfigService.ExportYear = _ExportYear;
//            ConfigService.DateInWeek = iDateInWeek;
//            ConfigService.PathFolder = sPathFolder;
//            dao.Update(ConfigService);
//            return RedirectToAction("Index");
//        }
//    }
//}