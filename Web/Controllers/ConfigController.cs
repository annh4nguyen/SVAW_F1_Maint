using System;
using System.Collections.Generic;
using System.IO;
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
using System.Web.Configuration;

namespace avSVAW.Controllers
{
    public class ConfigController : BaseController
    {
        public static string ConfigPath = WebConfigurationManager.AppSettings["ConfigPath"];

        // GET: EventDef
        /// <summary>  
        /// First Action method called when page loads  
        /// Fetch all the rows from DB and display it  
        /// </summary>  
        /// <returns>Home View</returns>  
        public ActionResult Index()
        {
            List<ConfigForm1> model = new List<ConfigForm1>();
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();

            string Path = ConfigPath + @"\svaw.config";

            if (System.IO.File.Exists(Path))
            {
                //load NodeType
                List<tblNodeType> lstNodeType = new NodeTypeDao().listAll(true);
                bool isTimeDelay1 = false;
                System.IO.StreamReader reader = new System.IO.StreamReader(Path);
                ds.ReadXml(Path);
                reader.Close();
                dt = ds.Tables["CONFIG"];
                foreach (DataRow dr in dt.Rows)
                {
                    string _ConfigKey = (dr["Key"] == DBNull.Value) ? "" : dr["Key"].ToString();
                    if (_ConfigKey != "LoadedTime")
                    {
                        ConfigForm configForm = new ConfigForm(dr);
                        if (_ConfigKey == "TimeDelay0"|| _ConfigKey == "TimeDelay1")
                        {
                            isTimeDelay1 = true;
                        }
                        if (_ConfigKey == "TimeDelay2")
                        {
                            int idNodeType  = 0;
                            try
                            {
                                idNodeType = Convert.ToInt32(configForm.ConfigText);
                            }
                            catch { }
                            var NodeType = lstNodeType.Where(p => p.Id == idNodeType).ToList();
                            if (NodeType.Count() > 0)
                            {
                                configForm.NodeTypeId = NodeType[0].Id.ToString();
                                configForm.ConfigText = "Màn hình hiệu suất máy " + NodeType[0].Name;
                                lstNodeType.RemoveAt(lstNodeType.FindIndex(p=>p.Id==NodeType[0].Id));
                            }
                        }
                        ConfigForm1 config1 = castConfig(configForm);
                        model.Add(config1);
                    }
                }
                if(isTimeDelay1 == false)
                {
                    ConfigForm configForm = new ConfigForm("TimeDelay0", "Màn hình Giám sát trạng thái vận hành máy", "120","0", "true");
                    ConfigForm1 config1 = castConfig(configForm);
                    model.Add(config1);
                    ConfigForm configForm1 = new ConfigForm("TimeDelay1", "Màn hình hiệu suất tổng thể các loại máy", "60", "0", "true");
                    ConfigForm1 config2 = castConfig(configForm1);
                    model.Add(config2);
                }
                if (lstNodeType.Count > 0)
                {
                  foreach(var _nodeType in lstNodeType)
                    {
                        ConfigForm configForm = new ConfigForm("TimeDelay2", "Màn hình hiệu suất máy "+_nodeType.Name, "60",_nodeType.Id.ToString(), "true");
                        ConfigForm1 config = castConfig(configForm);
                        model.Add(config);
                    }
                    
                }
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Save(string ConfigKey, string ConfigValue, string ConfigText)
        {
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();

            string Path = ConfigPath + @"\svaw.config";
            //ConfigKey = "TimeOut";
            if (System.IO.File.Exists(Path))
            {
                System.IO.StreamReader reader = new System.IO.StreamReader(Path);
                ds.ReadXml(Path);
                reader.Close();
                dt = ds.Tables["CONFIG"];
                foreach (DataRow dr in dt.Rows)
                {
                    string _ConfigKey = (dr["Key"] == DBNull.Value) ? "" : dr["Key"].ToString();
                    if (_ConfigKey == ConfigKey)
                    {
                        dr["Value"] = ConfigValue;
                        dr["Text"] = ConfigText;
                    }
                }
            }
            if (System.IO.File.Exists(Path))
            {
                System.IO.File.Delete(Path);
            }
            if (ds.Tables.Contains("CONFIG"))
            {
                ds.Tables.Remove("CONFIG");
            }
            ds.Tables.Add(dt);

            ds.WriteXml(Path);

            return RedirectToAction("Index");
        }
        public ConfigForm1 castConfig(ConfigForm conf)
        {
            ConfigForm1 conf1 = new ConfigForm1();
            conf1.ConfigKey = conf.ConfigKey;
            conf1.ConfigText = conf.ConfigText;
            conf1.ConfigValue = conf.ConfigValue;
            conf1.NodeTypeId = conf.NodeTypeId;
            conf1.ConfigShow = conf.ConfigShow;
            return conf1;
        }
        [HttpPost]
        public ActionResult SaveList(List<ConfigForm1> lstConfigForm)
        {
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();

            string Path = ConfigPath + @"\svaw.config";
            if (System.IO.File.Exists(Path))
            {
                System.IO.StreamReader reader = new System.IO.StreamReader(Path);
                ds.ReadXml(Path);
                reader.Close();
                dt = ds.Tables["CONFIG"];
                foreach (var configForm in lstConfigForm)
                {
                    DataRow dr = null;
                    int iNodeTypeId = 0;
                    if (!string.IsNullOrEmpty(configForm.NodeTypeId))
                    {
                        try
                        {
                            iNodeTypeId = Convert.ToInt32(configForm.NodeTypeId);
                        }
                        catch { }
                    }
                    var isInsertOrUpdate = false;
                    foreach(DataRow _dr in dt.Rows)
                    {
                        string _ConfigKey = (_dr["Key"] == DBNull.Value) ? "" : _dr["Key"].ToString();
                        string _ConfigText = (_dr["Text"] == DBNull.Value) ? "" : _dr["Text"].ToString();
                        if(_ConfigKey == configForm.ConfigKey)
                        {
                            if(iNodeTypeId > 0)
                            {
                                if(_ConfigText == configForm.NodeTypeId)
                                {
                                    isInsertOrUpdate = true;
                                    dr = _dr;
                                }
                            }
                            else
                            {
                                if (_ConfigText == configForm.ConfigText)
                                {
                                    isInsertOrUpdate = true;
                                    dr = _dr;
                                }
                            }
                        }
                    }

                   if(dr == null)
                    {
                        dr = dt.NewRow();
                    }

                    //   string _ConfigKey = (dr["Key"] == DBNull.Value) ? "" : dr["Key"].ToString();
                        dr["Key"] = configForm.ConfigKey;
                        dr["Value"] = configForm.ConfigValue;
                        dr["Text"] = configForm.ConfigText;
                        dr["Enable"] = configForm.ConfigShow;
                   
                    if (iNodeTypeId > 0)
                    {
                        dr["Text"] = configForm.NodeTypeId;
                    }
                    if (!isInsertOrUpdate)
                    {
                        dt.Rows.Add(dr);
                    }
                }
            }
            if (System.IO.File.Exists(Path))
            {
                System.IO.File.Delete(Path);
            }
            if (ds.Tables.Contains("CONFIG"))
            {
                ds.Tables.Remove("CONFIG");
            }
            ds.Tables.Add(dt);

            ds.WriteXml(Path);

            return RedirectToAction("Index");
        }
        //public List<ConfigForm> GetListOfConfigForms()
        //{
        //    List<ConfigForm> model = new List<ConfigForm>();
        //    DataTable dt = new DataTable();
        //    DataSet ds = new DataSet();

        //    string Path = Common.WebConstants.ConfigPath + @"\svaw.config";

        //    if (System.IO.File.Exists(Path))
        //    {

        //        System.IO.StreamReader reader = new System.IO.StreamReader(Path);
        //        ds.ReadXml(Path);
        //        reader.Close();
        //        dt = ds.Tables["CONFIG"];
        //        foreach (DataRow dr in dt.Rows)
        //        {
        //            ConfigForm configForm = new ConfigForm(dr);
        //            model.Add(configForm);
        //        }
        //    }

        //    return model;
        //}

        public ConfigForm LoadConfig(string ConfigKey)
        {
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();

            string Path = ConfigPath + @"\svaw.config";

            if (System.IO.File.Exists(Path))
            {

                System.IO.StreamReader reader = new System.IO.StreamReader(Path);
                ds.ReadXml(Path);
                reader.Close();
                dt = ds.Tables["CONFIG"];
                foreach (DataRow dr in dt.Rows)
                {
                    string _ConfigKey = (dr["Key"] == DBNull.Value) ? "" : dr["Key"].ToString();
                    if (_ConfigKey == ConfigKey)
                    {
                        ConfigForm configForm = new ConfigForm(dr);
                        return configForm;
                    }
                }
            }

            return null;
        }
        public ConfigForm LoadConfigByNodeType(string ConfigKey, string NodeType = "")
        {
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();

            string Path = ConfigPath + @"\svaw.config";

            if (System.IO.File.Exists(Path))
            {

                System.IO.StreamReader reader = new System.IO.StreamReader(Path);
                ds.ReadXml(Path);
                reader.Close();
                dt = ds.Tables["CONFIG"];
                foreach (DataRow dr in dt.Rows)
                {
                    string _ConfigKey = (dr["Key"] == DBNull.Value) ? "" : dr["Key"].ToString();
                    string _ConfigText = (dr["Text"] == DBNull.Value) ? "" : dr["Text"].ToString();
                    string _Enable = (dr["Enable"] == DBNull.Value) ? "" : dr["Enable"].ToString();
                    if (_ConfigKey == ConfigKey && NodeType== _ConfigText && Convert.ToBoolean(_Enable))
                    {
                        ConfigForm configForm = new ConfigForm(dr);
                        return configForm;
                    }
                }
            }
            return null;
        }

        public ConfigForm LoadNextConfigByNodeType(string ConfigKey,string NodeType = "")
        {
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();

            string Path = ConfigPath + @"\svaw.config";

            if (System.IO.File.Exists(Path))
            {

                System.IO.StreamReader reader = new System.IO.StreamReader(Path);
                ds.ReadXml(Path);
                reader.Close();
                dt = ds.Tables["CONFIG"];
                bool isNextNodeType = false;
                foreach (DataRow dr in dt.Rows)
                {
                    string _ConfigKey = (dr["Key"] == DBNull.Value) ? "" : dr["Key"].ToString();
                    string _ConfigText = (dr["Text"] == DBNull.Value) ? "" : dr["Text"].ToString();
                    string _Enable = (dr["Enable"] == DBNull.Value) ? "" : dr["Enable"].ToString();
                    if (_ConfigKey == ConfigKey && Convert.ToBoolean(_Enable))
                    {
                        ConfigForm configForm = new ConfigForm(dr);
                        if (isNextNodeType)
                        {
                            // kiem tra xem co node type ko?
                            try
                            {
                                string nodeTypeName = new NodeTypeDao().GettblNodeTypeNameById(Convert.ToInt32(_ConfigText));
                                if (nodeTypeName != "")
                                {
                                    return configForm;
                                }
                            }catch(Exception Ex)
                            {
                                return null;
                            }
                        }
                        if (NodeType == _ConfigText)
                        {
                            isNextNodeType = true;
                        }
                        if (NodeType == "0")
                        {
                            return configForm;
                        }
                    }
                }
            }

            return null;
        }

        [WebMethod]
        public JsonResult GetEventDefJsonList()
        {
            List<tblEventDef> lst = new EventDefDao().listAll();
            return Json(lst);
        }
    }
}