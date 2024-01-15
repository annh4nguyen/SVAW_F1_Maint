using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace avSVAW.Common
{
    public class WebConstants
    {
        //public static string CUSTOMER_SESSION = "CUSTOMER_SESSION";
        //public static string USER_SESSION = "USER_SESSION";
        //public static string SESSION_CREDENTIALS = "SESSION_CREDENTIALS";
        //public static string CartSession = "CartSession";
        //public static string AVATAR_LOCATION = WebConfigurationManager.AppSettings["AvatarLocation"];
        //public static string ORDER_RECEIVE_LOCATION = WebConfigurationManager.AppSettings["OrderReceiveLocation"];
        //public static string ORDER_RETURN_LOCATION = WebConfigurationManager.AppSettings["OrderReturnLocation"];
        //public static string REPORT_DEFINITION = WebConfigurationManager.AppSettings["ReportDefinition"];
        //public static string REPORT_RENDER = WebConfigurationManager.AppSettings["ReportRender"];

        public static int TypeArea2 = 1;
        public static int TypeArea2Nextway = 2;

        public static DateTime avMinValue = new DateTime(DateTime.Now.Year, 1, 1);
        public static DateTime avMaxValue = new DateTime(DateTime.Now.Year, 12, 31);

        public static DateTime avStartOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        public static DateTime avEndOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));

        public static DateTime avStartOfLastMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-1);

        public static DateTime _endoflastmonth = avEndOfMonth.AddMonths(-1);
        public static DateTime avEndOfLastMonth = new DateTime(_endoflastmonth.Year, _endoflastmonth.Month, DateTime.DaysInMonth(_endoflastmonth.Year, _endoflastmonth.Month));

        public static int Days2Finish = int.Parse(WebConfigurationManager.AppSettings["Days2Finish"]);
        public static int TT_BangKe = int.Parse(WebConfigurationManager.AppSettings["TT_BangKe"]);
        public static string MT_ProviderCode = WebConfigurationManager.AppSettings["MT_ProviderCode"];
        public static float VAT = float.Parse(WebConfigurationManager.AppSettings["VAT"]);

        public static string CurrentCulture { set; get; }

        public static SelectListItem itemHeaderEmpty {
            get {
                return new SelectListItem { Text = "", Value = "" };
            }
        }

        public static SelectListItem itemHeaderSelectAll
        {
            get
            {
                return new SelectListItem { Text = "-- " + Resources.Language.All + " --", Value = "" };
            }
        }

        public static SelectListItem itemHeaderSelectAllWithText(string strText)
        {
            return new SelectListItem { Text = strText, Value = "" };
        }
        public static string RemoveUnicode(string text)
        {
            string[] arr1 = new string[] { "á", "à", "ả", "ã", "ạ", "â", "ấ", "ầ", "ẩ", "ẫ", "ậ", "ă", "ắ", "ằ", "ẳ", "ẵ", "ặ",
            "đ",
            "é","è","ẻ","ẽ","ẹ","ê","ế","ề","ể","ễ","ệ",
            "í","ì","ỉ","ĩ","ị",
            "ó","ò","ỏ","õ","ọ","ô","ố","ồ","ổ","ỗ","ộ","ơ","ớ","ờ","ở","ỡ","ợ",
            "ú","ù","ủ","ũ","ụ","ư","ứ","ừ","ử","ữ","ự",
            "ý","ỳ","ỷ","ỹ","ỵ",};
                    string[] arr2 = new string[] { "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a",
            "d",
            "e","e","e","e","e","e","e","e","e","e","e",
            "i","i","i","i","i",
            "o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o",
            "u","u","u","u","u","u","u","u","u","u","u",
            "y","y","y","y","y",};
            for (int i = 0; i < arr1.Length; i++)
            {
                text = text.Replace(arr1[i], arr2[i]);
                text = text.Replace(arr1[i].ToUpper(), arr2[i].ToUpper());
            }
            return text.Replace(" ","_");
        }

        public static string ReadNumber(double totalNeed2Pay)
        {
            try
            {
                string rs = "";
                //total = Math.Round(total, 0);
                string[] ch = { "không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };
                string[] rch = { "lẻ", "mốt", "", "", "", "lăm" };
                string[] u = { "", "mươi", "trăm", "nghìn", "", "", "triệu", "", "", "tỷ", "", "", "nghìn", "", "", "triệu" };
                string nstr = totalNeed2Pay.ToString();

                int[] n = new int[nstr.Length];
                int len = n.Length;
                for (int i = 0; i < len; i++)
                {
                    n[len - 1 - i] = Convert.ToInt32(nstr.Substring(i, 1));
                }

                for (int i = len - 1; i >= 0; i--)
                {
                    if (i % 3 == 2)// số 0 ở hàng trăm
                    {
                        if (n[i] == 0 && n[i - 1] == 0 && n[i - 2] == 0) continue;//nếu cả 3 số là 0 thì bỏ qua không đọc
                    }
                    else if (i % 3 == 1) // số ở hàng chục
                    {
                        if (n[i] == 0)
                        {
                            if (n[i - 1] == 0) { continue; }// nếu hàng chục và hàng đơn vị đều là 0 thì bỏ qua.
                            else
                            {
                                rs += " " + rch[n[i]]; continue;// hàng chục là 0 thì bỏ qua, đọc số hàng đơn vị
                            }
                        }
                        if (n[i] == 1)//nếu số hàng chục là 1 thì đọc là mười
                        {
                            rs += " mười"; continue;
                        }
                    }
                    else if (i != len - 1)// số ở hàng đơn vị (không phải là số đầu tiên)
                    {
                        if (n[i] == 0)// số hàng đơn vị là 0 thì chỉ đọc đơn vị
                        {
                            if (i + 2 <= len - 1 && n[i + 2] == 0 && n[i + 1] == 0) continue;
                            rs += " " + (i % 3 == 0 ? u[i] : u[i % 3]);
                            continue;
                        }
                        if (n[i] == 1)// nếu là 1 thì tùy vào số hàng chục mà đọc: 0,1: một / còn lại: mốt
                        {
                            rs += " " + ((n[i + 1] == 1 || n[i + 1] == 0) ? ch[n[i]] : rch[n[i]]);
                            rs += " " + (i % 3 == 0 ? u[i] : u[i % 3]);
                            continue;
                        }
                        if (n[i] == 5) // cách đọc số 5
                        {
                            if (n[i + 1] != 0) //nếu số hàng chục khác 0 thì đọc số 5 là lăm
                            {
                                rs += " " + rch[n[i]];// đọc số 
                                rs += " " + (i % 3 == 0 ? u[i] : u[i % 3]);// đọc đơn vị
                                continue;
                            }
                        }
                    }

                    rs += (rs == "" ? " " : ", ") + ch[n[i]];// đọc số
                    rs += " " + (i % 3 == 0 ? u[i] : u[i % 3]);// đọc đơn vị
                }
                if (rs[rs.Length - 1] != ' ')
                    rs += " đồng";
                else
                    rs += "đồng";

                if (rs.Length > 2)
                {
                    string rs1 = rs.Substring(0, 2);
                    rs1 = rs1.ToUpper();
                    rs = rs.Substring(2);
                    rs = rs1 + rs;
                }
                return rs.Trim().Replace("lẻ,", "lẻ").Replace("mươi,", "mươi").Replace("trăm,", "trăm").Replace("mười,", "mười");
            }
            catch
            {
                return "";
            }
        }
    }

    public enum NOTI_MSG_TYPE
    {
        JOB = 1,
        ORDER_RECEIVE = 2,
        ORDER_RETURN = 3
    }
}