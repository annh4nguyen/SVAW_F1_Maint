using Model.Dao;
using Model.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace avSVAW.Models
{
    public class StopWorkingPlanForm 
    {
        public int WorkingId { get; set; }
        public int FromHour { get; set; }
        public int FromMinute { get; set; }
        public int ToHour { get; set; }
        public int ToMinute { get; set; }
        public int TotalMinute { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string Days { get; set; }
        public string NodeIds { get; set; }
        public string NodeNames { get; set; }
        public string NodeTypeIds { get; set; }

        public void Cast(int iYear, int iMonth, int iWorkingId)
        {
            int Year = iYear, Month = iMonth;
            int iFromHour = 0, iFromMinute = 0, iToHour = 0, iToMinute = 0, iTotalMinute = 0;
            List<tblStopWorkingPlan> plans = new StopWorkingPlanDao().ListAll(iWorkingId);
            string strDays = "; ", strNodeIds = "; ", strNodeNames = "; ", strNodeTypes = "; ";
            foreach (tblStopWorkingPlan p in plans)
            {
                if (!strDays.Contains("; " + p.Day + ";"))
                {
                    strDays += p.Day + "; ";
                }
                if (!strNodeIds.Contains("; " + p.NodeId + ";"))
                {
                    strNodeIds += p.NodeId + "; ";
                    strNodeNames += new NodeDao().GettblNodeNameById(p.NodeId) + "; ";
                    strNodeTypes += new NodeDao().GetNodeTypeById(p.NodeId) + "; ";
                }

                Year = p.Year;
                Month = p.Month;
                iFromHour = p.FromHour;
                iFromMinute = p.FromMinute;
                iToHour = p.ToHour;
                iToMinute = p.ToMinute;
                iTotalMinute = p.TotalMinute;
            }
            this.WorkingId = iWorkingId;
            this.Year = Year;
            this.Month = Month;
            this.FromHour = iFromHour;
            this.FromMinute = iFromMinute;
            this.ToHour = iToHour;
            this.ToMinute = iToMinute;
            this.TotalMinute = iTotalMinute;

            if (strDays.Length > 2)
            {
                strDays = strDays.Substring(2);
            }
            if (strNodeIds.Length > 2)
            {
                strNodeIds = strNodeIds.Substring(2);
            }
            if (strNodeNames.Length > 2)
            {
                strNodeNames = strNodeNames.Substring(2);
            }
            if (strNodeTypes.Length > 2)
            {
                strNodeTypes = strNodeTypes.Substring(2);
            }

            this.Days = strDays;
            this.NodeIds = strNodeIds;
            this.NodeNames = strNodeNames;
            this.NodeTypeIds = strNodeTypes;

        }

        public bool isShow(int Shift, int NodeType, string Node)
        {
            bool ret = true;
            if (NodeType != 0)
            {
                ret = ("; " + this.NodeTypeIds).Contains("; " + NodeType + ";");
            }

            if (ret)
            {
                if (Shift != 0)
                {
                    tblWorkingShift workingShift = new WorkingShiftDao().ViewDetail(Shift);
                    DateTime _shiftFrom = new DateTime(2019, 1, 1, workingShift.StartHour, workingShift.StartMinute, 0);
                    DateTime _shiftTo = new DateTime(2019, 1, 1, workingShift.FinishHour, workingShift.FinishHour, 0);

                    DateTime _workingFrom = new DateTime(2019, 1, 1, this.FromHour, this.FromMinute, 0);
                    DateTime _workingTo = new DateTime(2019, 1, 1, this.ToHour, this.ToMinute, 0);

                    ret = (_workingFrom >= _shiftFrom && _workingTo <= _shiftTo);
                }
            }

            if (ret)
            {
                if (Node.Trim() != "")
                {
                    ret = this.NodeNames.Contains(Node.ToUpper());
                }
            }
            return ret;

        }

    }
}