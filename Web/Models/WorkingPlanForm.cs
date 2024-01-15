using Model.Dao;
using Model.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace avSVAW.Models
{
    public class WorkingPlanForm 
    {
        public int WorkingId { get; set; }
        public string ShiftIds { get; set; }
        public string ShiftNames { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string Days { get; set; }
        public string NodeIds { get; set; }
        public string NodeNames { get; set; }
        public string NodeTypeIds { get; set; }

        public void Cast(int iYear, int iMonth, int iWorkingId)
        {
            int Year = iYear, Month = iMonth;
            List<tblWorkingPlan> plans = new WorkingPlanDao().ListAll(iWorkingId);
            string strDays = "; ", strNodeIds = "; ", strNodeNames = "; ", strShiftIds = "; ", strShiftNames = "; ", strNodeTypes = "; ";
            foreach (tblWorkingPlan p in plans)
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

                if (!strShiftIds.Contains("; " + p.ShiftId + ";"))
                {
                    strShiftIds += p.ShiftId + "; ";
                    strShiftNames += new WorkingShiftDao().GetNameById(p.ShiftId) + "; ";
                }
                Year = p.Year;
                Month = p.Month;
            }
            this.WorkingId = iWorkingId;
            this.Year = Year;
            this.Month = Month;

            if (strShiftIds.Length > 2)
            {
                strShiftIds = strShiftIds.Substring(2);
            }
            if (strShiftNames.Length > 2)
            {
                strShiftNames = strShiftNames.Substring(2);
            }
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

            this.ShiftIds = strShiftIds;
            this.ShiftNames = strShiftNames;
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
                    ret = ("; " + this.ShiftIds).Contains("; " + Shift + ";");
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