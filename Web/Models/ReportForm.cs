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
    public class EventReport
    {

        public int ReportYear { get; set; }
        public int ReportMonth { get; set; }
        public int ReportDay { get; set; }
        public int NodeId { get; set; }
        public string NodeName { get; set; }
        public int NodeTypeId { get; set; }
        public string NodeTypeName { get; set; }
        public int LineId { get; set; }
        public string LineName { get; set; }
        public int ZoneId { get; set; }
        public string ZoneName { get; set; }
        public int FactoryId { get; set; }
        public int EventDefId { get; set; }
        public string EventDefName { get; set; }
        public UInt64 TotalDuration { get; set; }

        public EventReport GetEachEventReport(DataRow dr, int iYear, int iMonth, int iDay)
        {
            EventReport Report = new EventReport();
            Report.ReportYear = iYear;
            Report.ReportMonth = iMonth;
            Report.ReportDay = iDay;

            Report.NodeId = int.Parse(dr["NodeId"].ToString());
            Report.NodeName = dr["NodeName"].ToString();
            Report.LineId = int.Parse(dr["LineId"].ToString());
            Report.LineName = dr["LineName"].ToString();
            Report.ZoneId = int.Parse(dr["ZoneId"].ToString());
            Report.ZoneName = dr["ZoneName"].ToString();
            Report.FactoryId = int.Parse(dr["FactoryId"].ToString());
            Report.EventDefId = int.Parse(dr["EventDefId"].ToString());
            Report.EventDefName = dr["EventDefName"].ToString();
            Report.TotalDuration = UInt64.Parse(dr["TotalDuration"] == DBNull.Value ? "0" : dr["TotalDuration"].ToString());

            return Report;
        }


    }

    public class EventDetailReport
    {

        public long NodeId { get; set; }
        public int EventDefId { get; set; }
        public DateTime T3 { get; set; }
        public DateTime T2 { get; set; }
        public DateTime T1 { get; set; }
        public UInt64 WaitDuration { get; set; }
        public UInt64 ProcessDuration { get; set; }
        public UInt64 TotalDuration { get; set; }

        public EventDetailReport GetEachEventReportDetail(DataRow dr, int iCount)
        {
            EventDetailReport Report = new EventDetailReport();

            Report.NodeId = int.Parse(dr["NodeId"].ToString());
            Report.EventDefId = int.Parse(dr["EventDefId"].ToString());

            Report.T3 = (DateTime)(dr["T3"] == DBNull.Value ? DateTime.MinValue : dr["T3"]);
            Report.T2 = (DateTime)(dr["T2"] == DBNull.Value ? DateTime.MinValue : dr["T2"]);
            Report.T1 = (DateTime)(dr["T1"] == DBNull.Value ? DateTime.MinValue : dr["T1"]);

            Report.WaitDuration = UInt64.Parse(dr["WaitDuration"] == DBNull.Value ? "0" : dr["WaitDuration"].ToString());
            Report.ProcessDuration = UInt64.Parse(dr["ProcessDuration"] == DBNull.Value ? "0" : dr["ProcessDuration"].ToString());
            Report.TotalDuration = UInt64.Parse(dr["TotalDuration"] == DBNull.Value ? "0" : dr["TotalDuration"].ToString());

            return Report;
        }


    }
}