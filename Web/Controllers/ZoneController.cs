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
    public class ZoneController : BaseController
    {
        
        // GET: EventDef
        /// <summary>  
        /// First Action method called when page loads  
        /// Fetch all the rows from DB and display it  
        /// </summary>  
        /// <returns>Home View</returns>  
        public ActionResult Index()
        {
            List<tblZone> lst = new ZoneDao().listAll();
            List<ZoneForm> model = new List<ZoneForm>();
            foreach(tblZone l in lst)
            {
                ZoneForm lf = new ZoneForm();
                lf.Cast(l);
                model.Add(lf);
            }
            return View("Index", model);
        }

        /// <summary>  
        /// Action method, called when the "Add New Record" link clicked  
        /// </summary>  
        /// <returns>Create View</returns>  
        public ActionResult Create()
        {
            ZoneForm model = new ZoneForm();
            model.Factories = new FactoryDao().listAll().Select(n =>
                new SelectListItem
                {
                    Value = n.Id.ToString(),
                    Text = n.Name
                }).ToList();

            return View(model);
        }

        [HttpPost]
        public ActionResult Create(ZoneForm model)
        {
            if (ModelState.IsValid)
            {
                tblZone l = model.Cast();
                
                new ZoneDao().Insert(l);
            }
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int Id)
        {
            tblZone l = new ZoneDao().ViewDetail(Id);
            ZoneForm model = new ZoneForm();
            model.Cast(l);
            model.Factories = new FactoryDao().listAll().Select(n =>
                new SelectListItem
                {
                    Value = n.Id.ToString(),
                    Text = n.Name
                }).ToList();
            return View(model);
        }


        [HttpPost]
        public ActionResult Edit(ZoneForm model)
        {
            tblZone l = model.Cast();
            new ZoneDao().Update(l);
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int Id)
        {
            new ZoneDao().Delete(Id);
            return RedirectToAction("Index");
        }


        [WebMethod]
        public JsonResult GetEventDefJsonList()
        {
            List<tblZone> lst = new ZoneDao().listAll();
            return Json(lst);
        }
    }
}