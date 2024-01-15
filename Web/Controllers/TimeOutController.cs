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
    public class TimeOutController : BaseController
    {
        
        /// <returns>Home View</returns>  
        public ActionResult Index()
        {
            List<tblNodeType> lst = new NodeTypeDao().listAll(true);
            return View("Index", lst);
        }
        public ActionResult _Status()
        {
            List<tblNodeType> lst = new NodeTypeDao().listAll();
            return PartialView("_Status", lst);
        }

        [WebMethod]
        public JsonResult UpdateTimeOut(string NodeTypes, string TimeOuts)
        {
     
            //Chèn thêm mới vào
            string[] arrNodeType = NodeTypes.Split(';');
            string[] arrTimeOut = TimeOuts.Split(';');


            for(int i = 0; i < arrNodeType.Length; i++)
            {
                string _nodetype = arrNodeType[i];

                if (_nodetype != "")
                {
                    int NodeTypeId = int.Parse(_nodetype);
                    int iTimeOut = 0;
                    if (arrTimeOut[i] != "") {
                        iTimeOut = int.Parse(arrTimeOut[i]);
                    }
                    tblNodeType entity = new NodeTypeDao().ViewDetail(NodeTypeId);
                    entity.MaxStopTime = iTimeOut;
                    new NodeTypeDao().Update(entity);

                    new NodeOnlineDao().UpdateTimeOut(NodeTypeId, iTimeOut);
                }

            }

            //Update NodeOnline


            return Json("OK", JsonRequestBehavior.AllowGet);
        }

    }
}