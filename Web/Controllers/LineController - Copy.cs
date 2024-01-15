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
    public class LineController : BaseController
    {
        
        // GET: EventDef
        /// <summary>  
        /// First Action method called when page loads  
        /// Fetch all the rows from DB and display it  
        /// </summary>  
        /// <returns>Home View</returns>  
        public ActionResult Index()
        {
            List<tblLine> lst = new LineDao().listAll();
            List<LineForm> model = new List<LineForm>();
            foreach(tblLine l in lst)
            {
                LineForm lf = new LineForm();
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
            LineForm model = new LineForm();
            model.Factories = new FactoryDao().listAll().Select(n =>
                new SelectListItem
                {
                    Value = n.Id.ToString(),
                    Text = n.Name
                }).ToList();

            return View(model);
        }

        [HttpPost]
        public ActionResult Create(LineForm model)
        {
            if (ModelState.IsValid)
            {
                tblLine l = model.Cast();
                
                new LineDao().Insert(l);
            }
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int Id)
        {
            tblLine l = new LineDao().ViewDetail(Id);
            LineForm model = new LineForm();
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
        public ActionResult Edit(LineForm model)
        {
            tblLine l = model.Cast();
            new LineDao().Update(l);
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int Id)
        {
            new LineDao().Delete(Id);
            return RedirectToAction("Index");
        }


        [WebMethod]
        public JsonResult GetEventDefJsonList()
        {
            List<tblLine> lst = new LineDao().listAll();
            return Json(lst);
        }
    }
}