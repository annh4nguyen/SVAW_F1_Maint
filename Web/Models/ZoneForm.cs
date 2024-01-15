using Model.Dao;
using Model.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace avSVAW.Models
{
    public class ZoneForm : tblZone
    {
        public List<SelectListItem> Factories { get; set; }
        public string FactoryName { get; set; }
        public long Create()
        {
            ZoneDao nodeDao = new ZoneDao();
            return nodeDao.Insert(Cast());
        }

        public void Update()
        {
            ZoneDao nodeDao = new ZoneDao();
            nodeDao.Update(Cast());
        }

        public void Cast(tblZone node)
        {
            this.Id = node.Id;
            this.Name = node.Name;
            this.Description = node.Description;
            this.Color = node.Color;
            this.FactoryId = node.FactoryId;
            this.FactoryName = new FactoryDao().GettblFactoryNameById(node.FactoryId);
        }
        public tblZone Cast()
        {
            return new tblZone()
            {
                Id = this.Id,
                Name = this.Name,
                Description = this.Description,
                Color = this.Color,
                FactoryId = this.FactoryId
            };
        }
    }
}