using Model.Dao;
using Model.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace avSVAW.Models
{
    public class LineForm : tblLine
    {
        public List<SelectListItem> Factories { get; set; }
        public string FactoryName { get; set; }
        public long Create()
        {
            LineDao nodeDao = new LineDao();
            return nodeDao.Insert(Cast());
        }

        public void Update()
        {
            LineDao nodeDao = new LineDao();
            nodeDao.Update(Cast());
        }

        public void Cast(tblLine node)
        {
            this.Id = node.Id;
            this.Name = node.Name;
            this.Description = node.Description;
            this.Code = node.Code;
            this.FactoryId = node.FactoryId;
            this.nOrder = node.nOrder;
            this.FactoryName = new FactoryDao().GettblFactoryNameById(node.FactoryId);
        }
        public tblLine Cast()
        {
            return new tblLine()
            {
                Id = this.Id,
                Name = this.Name,
                Description = this.Description,
                Code = this.Code,
                FactoryId = this.FactoryId,
                nOrder = this.nOrder
            };
        }
    }
}