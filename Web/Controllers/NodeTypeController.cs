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
    public class NodeTypeController : BaseController
    {
        
        // GET: NodeType
        /// <summary>  
        /// First Action method called when page loads  
        /// Fetch all the rows from DB and display it  
        /// </summary>  
        /// <returns>Home View</returns>  
        public ActionResult Index()
        {
            List<tblNodeType> lst = new NodeTypeDao().listAll();
            return View("Index", lst);
        }
        public ActionResult _Status()
        {
            List<tblNodeType> lst = new NodeTypeDao().listAll();
            return PartialView("_Status", lst);
        }

        /// <summary>  
        /// Action method, called when the "Add New Record" link clicked  
        /// </summary>  
        /// <returns>Create View</returns>  
        public ActionResult Create()
        {
            tblNodeType eventdef = new tblNodeType();
            return View(eventdef);
        }

        [HttpPost]
        public ActionResult Create(tblNodeType model)
        {
            if (ModelState.IsValid)
            {
                long id = new NodeTypeDao().Insert(model);
            }
            return RedirectToAction("Index");
        }

        ///// <summary>  
        ///// Action method called when the user click "View" Link  
        ///// </summary>  
        ///// <param name="NodeTypeId">Student ID</param>  
        ///// <returns>Edit View</returns>  
        //public ActionResult View(int NodeTypeId)
        //{
        //    tblNodeType model = new tblNodeType();
        //    return View("View", model.GetNodeTypeByID(NodeTypeId));
        //}

        ///// <summary>  
        ///// Action method called when the user click "Edit" Link  
        ///// </summary>  
        ///// <param name="NodeTypeId">Student ID</param>  
        ///// <returns>Edit View</returns>  
        public ActionResult Edit(int Id)
        {
            tblNodeType model = new NodeTypeDao().ViewDetail(Id);
            return View(model);
        }


        [HttpPost]
        public ActionResult Edit(tblNodeType model)
        {
            var dao = new NodeTypeDao();
            dao.Update(model);
            return RedirectToAction("Index");
        }

        /// <summary>  
        /// Action method called when the "Delete" link clicked  
        /// </summary>  
        /// <param name="NodeTypeId">Stutend ID to edit</param>  
        /// <returns>Home view</returns>  
        public ActionResult Delete(int Id)
        {
            new NodeTypeDao().Delete(Id);
            return RedirectToAction("Index");
        }


        [WebMethod]
        public JsonResult GetNodeTypeJsonList()
        {
            List<tblNodeType> lst = new NodeTypeDao().listAll();
            return Json(lst);
        }
    }
}