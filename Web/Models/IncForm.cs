using Model.Dao;
using Model.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace avSVAW.Models
{
    public class IncForm
    {
        public int stt { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
 
        public double ActualDuration { get; set; }
        public double PlanDuration { get; set; }
    }
}