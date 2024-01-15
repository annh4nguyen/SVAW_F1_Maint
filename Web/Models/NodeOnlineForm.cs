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
    public class NodeOnlineForm : tblNodeOnline
    {
        public string Planned { get; set; }

        public void Cast(tblNodeOnline node)
        {

         this.NodeId = node.NodeId;
            this.NodeName = node.NodeName;
            this.LineId = node.LineId;
            this.LineName = node.LineName;
            this.ZoneId = node.ZoneId;
            this.ZoneName = node.ZoneName;
            this.FactoryId = node.FactoryId;
            this.FactoryName = node.FactoryName;
            this.NodeTypeId = node.NodeTypeId;
            this.NodeTypeName = node.NodeTypeName;
            this.Active = (bool)node.Active;
            this.nOrder = (int)node.nOrder;
            this.Status = node.Status;
            this.UpdateTime = (DateTime)node.UpdateTime;
            this.DataToShow = node.DataToShow;
            this.OnlineTimeCount = node.OnlineTimeCount;
            this.WorkingTimeCount = node.WorkingTimeCount;
            this.TimeOut = node.TimeOut;
        }

        public void Cast(DataRow dr)
        {

            this.NodeId = (dr["NodeId"] == DBNull.Value ? 0 : (int)dr["NodeId"]);
            this.NodeName = (dr["NodeName"] == DBNull.Value ? "" : dr["NodeName"].ToString());
            this.LineId = (dr["LineId"] == DBNull.Value ? 0 : (int)dr["LineId"]);
            this.LineName = (dr["NodeName"] == DBNull.Value ? "" : dr["NodeName"].ToString());
            this.ZoneId = (dr["ZoneId"] == DBNull.Value ? 0 : (int)dr["ZoneId"]);
            this.ZoneName = (dr["NodeName"] == DBNull.Value ? "" : dr["NodeName"].ToString());
            this.FactoryId = (dr["FactoryId"] == DBNull.Value ? 0 : (int)dr["FactoryId"]);
            this.FactoryName = (dr["NodeName"] == DBNull.Value ? "" : dr["NodeName"].ToString());
            this.NodeTypeId = (dr["NodeTypeId"] == DBNull.Value ? 0 : (int)dr["NodeTypeId"]);
            this.NodeTypeName = (dr["NodeName"] == DBNull.Value ? "" : dr["NodeName"].ToString());
            this.Active = (dr["Active"] == DBNull.Value ? false : (bool)dr["Active"]);
            this.nOrder = (dr["nOrder"] == DBNull.Value ? 0 : (int)dr["nOrder"]);
            this.Status = (dr["Status"] == DBNull.Value ? "" : dr["Status"].ToString());
            this.UpdateTime = (dr["UpdateTime"] == DBNull.Value) ? DateTime.MinValue : (DateTime)dr["UpdateTime"];
            this.DataToShow = (dr["DataToShow"] == DBNull.Value ? "" : dr["DataToShow"].ToString());
            this.OnlineTimeCount = (dr["OnlineTimeCount"] == DBNull.Value ? 0 : (long)dr["OnlineTimeCount"]);
            this.WorkingTimeCount = (dr["WorkingTimeCount"] == DBNull.Value ? 0 : (long)dr["WorkingTimeCount"]);
            this.TimeOut = (dr["TimeOut"] == DBNull.Value ? 0 : (int)dr["TimeOut"]);
            this.Planned = (dr["Planned"] == DBNull.Value ? "" : dr["Planned"].ToString());

        }
    }
}