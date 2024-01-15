using Model.Dao;
using Model.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace avSVAW.Models
{
    public class NodeForm : tblNode
    {
        public List<SelectListItem> Lines { get; set; }
        public string LineName { get; set; }
        public List<SelectListItem> Zones { get; set; }
        public string ZoneName { get; set; }
        public List<SelectListItem> NodeTypes { get; set; }
        public string NodeTypeName { get; set; }
        public long Create()
        {
            NodeDao nodeDao = new NodeDao();
            return nodeDao.Insert(Cast());
        }

        public void Update()
        {
            NodeDao nodeDao = new NodeDao();
            nodeDao.Update(Cast());
        }

        public void Cast(tblNode node)
        {
            this.Id = node.Id;
            this.Name = node.Name;
            this.Description = node.Description;
            this.Active = node.Active;
            this.LineId = node.LineId;
            this.LineName = new LineDao().GettblLineNameById(node.LineId);
            this.ZoneId = node.ZoneId;
            this.ZoneName = new ZoneDao().GettblZoneNameById(node.ZoneId);
            this.NodeTypeId = node.NodeTypeId;
            this.NodeTypeName = new NodeTypeDao().GettblNodeTypeNameById(node.NodeTypeId);
            this.NodeTypeId = node.nOrder;

        }
        public tblNode Cast()
        {
            return new tblNode()
            {
                Id = this.Id,
                Name = this.Name,
                Description = this.Description,
                Active = this.Active,
                LineId = this.LineId,
                ZoneId= this.ZoneId,
                NodeTypeId = this.NodeTypeId,
                nOrder = this.nOrder
            };
        }
    }
}