using Model.Dao;
using Model.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace avSVAW.Models
{
    public class SummaryEventForm
    {
        public long NodeId { get; set; }
        public string NodeName { get; set; }
        public int NodeTypeId { get; set; }
        public string NodeTypeName { get; set; }
        public double PlanDuration { get; set; }
        public double PlanDurationInHour { get; set; }

        public int NumberOfRunning { get; set; }
        public double RunningDuration { get; set; }
        public double RunningDurationInHour { get; set; }

        public int NumberOfStop { get; set; }
        public double StopDuration { get; set; }
        public double StopDurationInHour { get; set; }

        public double WorkingPercent { get; set; }

    }
    public class SummaryEventTypeForm
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int NodeTypeId { get; set; }
        public string NodeTypeName { get; set; }
        public int NumberOfNodes { get; set; }
        public double PlanDuration { get; set; }
        public double ActualDuration { get; set; }
        public double WorkingPercent { get; set; }

        public SummaryEventTypeForm()
        {
            this.Year = this.Month = this.Day = this.NodeTypeId = 0;
            this.NodeTypeName = "";
            this.NumberOfNodes = 0;
            this.PlanDuration = this.ActualDuration = 0;
            this.WorkingPercent = 0;
        }
    }

    public class StopForm
    {
        public int NodeId { get; set; }
        public string NodeName { get; set; }
        public int NodeTypeId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime FinishTime { get; set; }
        public double Duration { get; set; }
        public string strDuration { get; set; }
    }
    public class TotalStopForm
    {
        public int NodeId { get; set; }
        public string NodeName { get; set; }
        public int NodeTypeId { get; set; }
        public string NodeTypeName { get; set; }
        public int NumberOfRunning { get; set; }
        public double RunningDuration { get; set; }
        public string strRunningDuration { get; set; }
        public int NumberOfStop { get; set; }
        public double StopDuration { get; set; }
        public string strStopDuration { get; set; }
        public double PlanStopDuration { get; set; }
        public string strPlanStopDuration { get; set; }
    }

    public class TotalStopTypeForm
    {
        public int NodeTypeId { get; set; }
        public string NodeTypeName { get; set; }
        public int NumberOfNodes { get; set; }

        public double RunningDuration { get; set; }
        public double RunningDurationInHour { get; set; }
        public string strRunningDuration { get; set; }

        public double StopDuration { get; set; }
        public string strStopDuration { get; set; }
        public double StopDurationInHour { get; set; }

        public double PlanDuration { get; set; }
        public double PlanDurationInHour { get; set; }
        public string strPlanDuration { get; set; }

        public double WorkingPercent { get; set; }


    }


}