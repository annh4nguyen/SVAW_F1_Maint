//using Model.DataModel;
//using PagedList;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Model.Dao
//{
//    public class ConfigServiceDao
//    {
//        AvaniDataContext db = null;
//        public ConfigServiceDao()
//        {
//            string con = System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString;
//            if (!string.IsNullOrEmpty(con))
//            {
//                db = new AvaniDataContext(con);
//            }else
//            {
//                db = new AvaniDataContext();
//            }
//        }

//        public long Insert(tblConfigService entity)
//        {
//            try
//            {
//                db.tblConfigServices.InsertOnSubmit(entity);
//                db.SubmitChanges();
//            }
//            catch (Exception e) { }
//            return entity.Id;
//        }
//        public bool Update(tblConfigService entity)
//        {
//            try
//            {
//                var tblConfigService = db.tblConfigServices.SingleOrDefault(x => x.Id == entity.Id);
//                tblConfigService.ExportWorkShift = entity.ExportWorkShift;
//                tblConfigService.ExportWeek = entity.ExportWeek;
//                tblConfigService.ExportMonth = entity.ExportMonth;
//                tblConfigService.ExportQuarter = entity.ExportQuarter;
//                tblConfigService.ExportYear = entity.ExportYear;
//                tblConfigService.PathFolder = entity.PathFolder;
//                tblConfigService.DateInWeek = entity.DateInWeek;
             
//                db.SubmitChanges();
//                return true;
//            }
//            catch (Exception ex)
//            {
//                //logging
//                return false;
//            }

//        }

//        public List<tblConfigService> listAll()
//        {
//            return db.tblConfigServices.ToList();
//        }
     
//        public tblConfigService ViewDetail(int id)
//        {
//            try
//            {
//                return db.tblConfigServices.SingleOrDefault(x => x.Id == id);
//            }
//            catch
//            {
//                return null;
//            }
//        }

        

//        public bool Delete(int id)
//        {
//            try
//            {
//                var line = db.tblConfigServices.SingleOrDefault(x => x.Id == id);
//                db.tblConfigServices.DeleteOnSubmit(line);
//                db.SubmitChanges();
//                return true;
//            }
//            catch (Exception)
//            {
//                return false;
//            }

//        }
//    }
//}
