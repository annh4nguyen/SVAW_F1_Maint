using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace avSVAW
{
    public class EventMonitoring
    {
        public int NodeId;
        public int LastEventDef;
        public int LastEventStatus;
        public DateTime LastEventTime;
        public string DataToShow;
        public EventMonitoring(int _nodeid, int _eventid, int _eventstatus, DateTime _eventtime, string _datatoshow)
        {
            NodeId = _nodeid;
            LastEventDef = _eventid;
            LastEventStatus = _eventstatus;
            LastEventTime = _eventtime;
            DataToShow = _datatoshow;
        }
    }
}