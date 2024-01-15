using Model.DataModel;

namespace Model.Dao
{
    public class BaseDao
    {
        public AvaniDataContext db = null;
        public BaseDao()
        {
            string con = System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString;
            if (!string.IsNullOrEmpty(con))
            {
                db = new AvaniDataContext(con);
            }
            else
            {
                db = new AvaniDataContext();
            }
        }
    }
}
