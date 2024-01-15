using Model.Dao;
using Model.DataModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace avSVAW.Models
{
    public class ConfigForm1
    {
        public string ConfigKey { get; set; }
        public string ConfigText { get; set; }
        public string ConfigValue { get; set; }
        public string NodeTypeId { get; set; }
        public string ConfigShow { get; set; }
    }
        public class ConfigForm
    {
        public string ConfigKey { get; set; }
        public string ConfigText { get; set; }
        public string ConfigValue { get; set; }
        public string NodeTypeId { get; set; }
        public string ConfigShow { get; set; }
        public ConfigForm(DataRow dr)
        {
            ConfigKey = (dr["Key"] == DBNull.Value) ? "" : dr["Key"].ToString();
            ConfigText = (dr["Text"] == DBNull.Value) ? "" : dr["Text"].ToString();
            ConfigValue = (dr["Value"] == DBNull.Value) ? "" : dr["Value"].ToString();
            ConfigShow = (dr["Enable"] == DBNull.Value) ? "" : dr["Enable"].ToString();
            NodeTypeId = "0"; // khong co cau hinh ve nodetype
        }
        public ConfigForm(string _ConfigKey, string _ConfigText, string _ConfigValue , string _NodeTypeId, string _ConfigShow)
        {
            ConfigKey = _ConfigKey;
            ConfigText = _ConfigText;
            ConfigValue = _ConfigValue;
            NodeTypeId = _NodeTypeId;
            ConfigShow = _ConfigShow;

        }
    }
}