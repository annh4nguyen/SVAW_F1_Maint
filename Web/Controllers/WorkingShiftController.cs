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

namespace avSVAW.Controllers
{
    public class WorkingShiftController : BaseController
    {
        
        // GET: EventDef
        /// <summary>  
        /// First Action method called when page loads  
        /// Fetch all the rows from DB and display it  
        /// </summary>  
        /// <returns>Home View</returns>  
        public ActionResult Index()
        {
            List<tblWorkingShift> model = new WorkingShiftDao().listAll();
            return View("Index", model);
        }

        /// <summary>  
        /// Action method, called when the "Add New Record" link clicked  
        /// </summary>  
        /// <returns>Create View</returns>  
        public ActionResult Create()
        {
            tblWorkingShift model = new tblWorkingShift();
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(tblWorkingShift model)
        {
            if (ModelState.IsValid)
            {
                new WorkingShiftDao().Insert(model);
            }
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int Id)
        {
            tblWorkingShift model = new WorkingShiftDao().ViewDetail(Id);
            return View(model);
        }


        [HttpPost]
        public ActionResult Edit(tblWorkingShift model)
        {
            new WorkingShiftDao().Update(model);
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int Id)
        {
            new WorkingShiftDao().Delete(Id);
            return RedirectToAction("Index");
        }


        [WebMethod]
        public JsonResult GetEventDefJsonList()
        {
            List<tblWorkingShift> lst = new WorkingShiftDao().listAll();
            return Json(lst);
        }
    }
}