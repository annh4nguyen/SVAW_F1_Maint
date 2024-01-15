using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;


namespace avSVAW
{
    public class Monitoring
    {
        string Message2Monitoring = "";
        DateTime CheckDateTime = DateTime.Now;
        //Here we will add a function for register monitoring (will add sql dependency)
        public void RegisterMonitoring(DateTime currentTime)
        {
            string conStr = System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString;
            // huantn add signalr
            string sqlCommandTrigger = "SELECT [NodeId] FROM [dbo].[tblTriggerNodeOnline]";



            string sqlCommand = " SELECT [NodeId], [NodeName], [Status],[UpdateTime],[DataToShow],[Planned] " +
                                " FROM [dbo].[tblNodeOnline] " +
                                " WHERE  NodeName != 'NC' AND [NodeId] In (SELECT NodeId FROM [dbo].[tblTriggerNodeOnline]) ";
            //you can notice here I have added table name like this [dbo].[Contacts] with [dbo], its mendatory when you use Sql Dependency
            using (SqlConnection con = new SqlConnection(conStr))
            {

                SqlCommand cmdTrigger = new SqlCommand(sqlCommandTrigger, con);

                SqlCommand cmd = new SqlCommand(sqlCommand, con);
              
                if (con.State != System.Data.ConnectionState.Open)
                {
                    con.Open();
                }
                cmdTrigger.Notification = null;
                cmd.Notification = null;

                SqlDependency sqlDep = new SqlDependency(cmdTrigger);

                //sqlDep.OnChange -= new OnChangeEventHandler(sqlDep_OnChange);
                sqlDep.OnChange += new OnChangeEventHandler(sqlDep_OnChange);
                ////we must have to execute the command here
                //cmd.ExecuteReader();
                cmdTrigger.ExecuteNonQuery();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            int _nodeid = reader.GetInt32(0);
                            string _nodename = reader.GetString(1);
                            string _status = reader.GetString(2);
                            DateTime _time = reader.GetDateTime(3);
                            string _eventtime = _time.ToString("yyyy/MM/dd HH:mm:ss");
                            string _data = ""; // reader.GetString(4);
                            string _planned = reader.GetString(5); 
                            Message2Monitoring += _nodeid.ToString() + "|" + _nodename.ToString() + "|" + _status.ToString() + "|" + _eventtime + "|" + _data + "|" + _planned + ";";

                            //break;
                            //Thread.Sleep(30);
                        }
                    }
                }

                //delete table trigger
                string sqlDelete = "DELETE FROM [tblTriggerNodeOnline]";
                SqlCommand cmdDelete = new SqlCommand(sqlDelete, con);
                cmdDelete.ExecuteNonQuery();

            }
        }

    

        void sqlDep_OnChange(object sender, SqlNotificationEventArgs e)
        {
            //or you can also check => if (e.Info == SqlNotificationInfo.Insert) , if you want notification only for inserted record
            if (e.Type == SqlNotificationType.Change)
            {
                SqlDependency sqlDep = sender as SqlDependency;
                sqlDep.OnChange -= sqlDep_OnChange;

                DateTime checktime = CheckDateTime;

                RegisterMonitoring(checktime);


                //from here we will send notification message to client
                var monitoringHub = GlobalHost.ConnectionManager.GetHubContext<MonitoringHub>();

                monitoringHub.Clients.All.notify(Message2Monitoring);
                //monitoringHub.Clients.All.addNotification(Message2Monitoring);
                
                //Reset
                //Clear Message
                Message2Monitoring = "";
                //Update Checked Time
                CheckDateTime = DateTime.Now;

            }
        }

        //public List<tblEmployee> GetData(DateTime afterDate)
        //{
        //    using (SignalRDBEntities dc = new SignalRDBEntities())
        //    {
        //        return dc.tblEmployees.Where(a => a.AddedOn > afterDate).OrderByDescending(a => a.AddedOn).ToList();
        //    }
        //}
    }
}