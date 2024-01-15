using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class GlobalConstants
    {
        public static string AV_MANAGER_GROUP = "MANAGER";
        public static string AV_ADMIN_GROUP = "ADMIN";
        public static string AV_STAFF_GROUP = "STAFF";
        public static string AV_CONFIG_GROUP = "CONFIG";
        public static string AV_STAFF_PlASTIC_GROUP = "STAFF_PLASTIC";
        public static string AV_STAFF_COPPER_GROUP = "STAFF_COPPER";
        public static string CUSTOMER_SESSION = "CUSTOMER_SESSION";
        public static string USER_SESSION = "USER_SESSION";
        public static string LANG_SESSION = "LANG_SESSION";
        public static string SESSION_CREDENTIALS = "SESSION_CREDENTIALS";
        public static string CartSession = "CartSession";

        public static string STARTDATE_SESSION = "STARTDATE";
        public static string ENDDATE_SESSION = "ENDDATE";

    }

    public class UserFunction
    {

        public static bool DELETE_PERMISSION(string strRole)
        {
            if (strRole == Common.GlobalConstants.AV_ADMIN_GROUP)
                return true;
            if (strRole == Common.GlobalConstants.AV_MANAGER_GROUP)
                return true;
            if (strRole == Common.GlobalConstants.AV_CONFIG_GROUP)
                return true;

            return false;
        }
        public static bool USER_MANAGE_PERMISSION(string strRole)
        {
            if (strRole == Common.GlobalConstants.AV_ADMIN_GROUP)
                return true;
            if (strRole == Common.GlobalConstants.AV_MANAGER_GROUP)
                return true;
            if (strRole == Common.GlobalConstants.AV_CONFIG_GROUP)
                return true;

            return false;
        }
        public static bool USER_CONFIG_PERMISSION(string strRole)
        {
            if (strRole == Common.GlobalConstants.AV_CONFIG_GROUP)
                return true;
            return false;
        }

        public static bool VIEW_COST_PERMISSION(string strRole)
        {
            //if (strRole == Common.GlobalConstants.AV_ACCOUNTANT_GROUP)
            //    return true;
            //if (strRole == Common.GlobalConstants.AV_MANAGER_GROUP)
            //    return true;

            return false;
        }

    }
}
