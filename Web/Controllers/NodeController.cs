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
    public class NodeController : BaseController
    {
        
        // GET: EventDef
        /// <summary>  
        /// First Action method called when page loads  
        /// Fetch all the rows from DB and display it  
        /// </summary>  
        /// <returns>Home View</returns>  
        public ActionResult Index()
        {
            ViewBag.Factories = new FactoryDao().listAll();
            ViewBag.Lines = new LineDao().listAll();
            ViewBag.Zones = new ZoneDao().listAll();
            ViewBag.NodeType = new NodeTypeDao().listAll();

            List<tblNode> lst = new NodeDao().listAll();
            List<NodeForm> model = new List<NodeForm>();
            foreach(tblNode l in lst)
            {
                NodeForm lf = new NodeForm();
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
            NodeForm model = new NodeForm();
            model.Lines = new LineDao().listAll().Select(n =>
                new SelectListItem
                {
                    Value = n.Id.ToString(),
                    Text = n.Code
                }).ToList();

            model.Zones = new ZoneDao().listAll().Select(n =>
                new SelectListItem
                {
                    Value = n.Id.ToString(),
                    Text = n.Name
                }).ToList();

            model.NodeTypes = new NodeTypeDao().listAll().Select(n =>
                new SelectListItem
                {
                    Value = n.Id.ToString(),
                    Text = n.Code
                }).ToList();

            return View(model);
        }

        [HttpPost]
        public ActionResult Create(NodeForm model)
        {
            if (ModelState.IsValid)
            {
                tblNode l = model.Cast();
                
                new NodeDao().Insert(l);
            }
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int Id)
        {
            tblNode l = new NodeDao().ViewDetail(Id);
            NodeForm model = new NodeForm();
            model.Cast(l);
            model.Lines = new LineDao().listAll().Select(n =>
                   new SelectListItem
                   {
                       Value = n.Id.ToString(),
                       Text = n.Code
                   }).ToList();

            model.Zones = new ZoneDao().listAll().Select(n =>
                new SelectListItem
                {
                    Value = n.Id.ToString(),
                    Text = n.Name
                }).ToList();

            model.NodeTypes = new NodeTypeDao().listAll().Select(n =>
                new SelectListItem
                {
                    Value = n.Id.ToString(),
                    Text = n.Code
                }).ToList();

            return View(model);
        }


        [HttpPost]
        public ActionResult Edit(NodeForm model)
        {
            tblNode l = model.Cast();
            new NodeDao().Update(l);
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int Id)
        {
            new NodeDao().Delete(Id);
            return RedirectToAction("Index");
        }


        [WebMethod]
        public JsonResult GetNodeJsonList()
        {
            List<tblNode> lst = new NodeDao().listAll();
            List<NodeForm> model = new List<NodeForm>();
            foreach (tblNode l in lst)
            {
                NodeForm lf = new NodeForm();
                lf.Cast(l);
                model.Add(lf);
            }

            return Json(model, JsonRequestBehavior.AllowGet);
        }
    }
}