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
    public class FactoryController : BaseController
    {
        
        // GET: EventDef
        /// <summary>  
        /// First Action method called when page loads  
        /// Fetch all the rows from DB and display it  
        /// </summary>  
        /// <returns>Home View</returns>  
        public ActionResult Index()
        {
            List<tblFactory> lst = new FactoryDao().listAll();
            return View("Index", lst);
        }

        /// <summary>  
        /// Action method, called when the "Add New Record" link clicked  
        /// </summary>  
        /// <returns>Create View</returns>  
        public ActionResult Create()
        {
            tblFactory factory = new tblFactory();
            return View(factory);
        }

        [HttpPost]
        public ActionResult Create(tblFactory model)
        {
            if (ModelState.IsValid)
            {
                long id = new FactoryDao().Insert(model);
            }
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int Id)
        {
            tblFactory model = new FactoryDao().ViewDetail(Id);
            return View(model);
        }


        [HttpPost]
        public ActionResult Edit(tblFactory model)
        {
            var dao = new FactoryDao();
            dao.Update(model);
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int Id)
        {
            new FactoryDao().Delete(Id);
            return RedirectToAction("Index");
        }


        [WebMethod]
        public JsonResult GetEventDefJsonList()
        {
            List<tblFactory> lst = new FactoryDao().listAll();
            return Json(lst);
        }
    }
}