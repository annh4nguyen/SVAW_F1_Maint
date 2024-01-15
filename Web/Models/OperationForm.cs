using Model.Dao;
using Model.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace avSVAW.Models
{
    public class OperationForm
    {
        public long EventId { get; set; }
        public int NodeId { get; set; }
        public string NodeName { get; set; }
        public int NodeTypeId { get; set; }
        public string NodeTypeName { get; set; }
        public int EventDefId { get; set; }
        public string EventDefName { get; set; }
        public string StartTime { get; set; }
        public string FinishTime { get; set; }
        public double Duration { get; set; }
        public double PlanDuration { get; set; }
        public string strDuration { get; set; }
        public long RunningDuration { get; set; }
        public long StopDuration { get; set; }
        public long AlarmDuration { get; set; }
        public long OtherDuration { get; set; }
        public long TotalDuration { get; set; }
        public long total()
        {
            return AlarmDuration + RunningDuration + StopDuration + OtherDuration;
        }

        public OperationForm Cast(tblEvent item)
        {
            OperationForm ret = new OperationForm();
            ret.EventId = (long)item.Id;
            ret.NodeId = (int)item.NodeId;
            tblNodeOnline nodeOnline = new NodeOnlineDao().ViewDetail(ret.NodeId);

            ret.NodeName = nodeOnline.NodeName;
            ret.NodeTypeId = nodeOnline.NodeTypeId;
            ret.NodeTypeName = nodeOnline.NodeTypeName;
            ret.EventDefId = (int)item.EventDefId;
            ret.EventDefName = new EventDefDao().GettblEventDefNameById(ret.EventDefId);
            DateTime _start = (DateTime)item.T3;

            DateTime _finish = (DateTime)item.T1;

            ret.StartTime = _start.ToString("yyyy-MM-dd HH:mm:ss");
            ret.FinishTime = _finish.ToString("yyyy-MM-dd HH:mm:ss");

            ret.Duration = (_finish - _start).TotalSeconds;

            return ret;
        }
    }
}