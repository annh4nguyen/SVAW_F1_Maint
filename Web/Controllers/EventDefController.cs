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
    public class EventDefController : BaseController
    {
        
        // GET: EventDef
        /// <summary>  
        /// First Action method called when page loads  
        /// Fetch all the rows from DB and display it  
        /// </summary>  
        /// <returns>Home View</returns>  
        public ActionResult Index()
        {
            List<tblEventDef> lst = new EventDefDao().listAll();
            return View("Index", lst);
        }
        public ActionResult _MonitoringStatus()
        {
            List<tblEventDef> lst = new EventDefDao().listAll();
            return  View("_MonitoringStatus", lst);
            //PartialView("_Status", lst);
        }

        /// <summary>  
        /// Action method, called when the "Add New Record" link clicked  
        /// </summary>  
        /// <returns>Create View</returns>  
        public ActionResult Create()
        {
            tblEventDef eventdef = new tblEventDef();
            return View(eventdef);
        }

        [HttpPost]
        public ActionResult Create(tblEventDef model)
        {
            if (ModelState.IsValid)
            {
                long id = new EventDefDao().Insert(model);
            }
            return RedirectToAction("Index");
        }

        ///// <summary>  
        ///// Action method called when the user click "View" Link  
        ///// </summary>  
        ///// <param name="EventDefId">Student ID</param>  
        ///// <returns>Edit View</returns>  
        //public ActionResult View(int EventDefId)
        //{
        //    tblEventDef model = new tblEventDef();
        //    return View("View", model.GetEventDefByID(EventDefId));
        //}

        ///// <summary>  
        ///// Action method called when the user click "Edit" Link  
        ///// </summary>  
        ///// <param name="EventDefId">Student ID</param>  
        ///// <returns>Edit View</returns>  
        public ActionResult Edit(int Id)
        {
            tblEventDef model = new EventDefDao().ViewDetail(Id);
            return View(model);
        }


        [HttpPost]
        public ActionResult Edit(tblEventDef model)
        {
            var dao = new EventDefDao();
            dao.Update(model);
            return RedirectToAction("Index");
        }

        /// <summary>  
        /// Action method called when the "Delete" link clicked  
        /// </summary>  
        /// <param name="EventDefId">Stutend ID to edit</param>  
        /// <returns>Home view</returns>  
        public ActionResult Delete(int Id)
        {
            new EventDefDao().Delete(Id);
            return RedirectToAction("Index");
        }


        [WebMethod]
        public JsonResult GetEventDefJsonList()
        {
            List<tblEventDef> lst = new EventDefDao().listAll();
            return Json(lst);
        }
    }
}