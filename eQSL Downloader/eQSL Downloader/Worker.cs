using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace eQSL_Downloader
{
    public class Worker
    {
        // Volatile is used as hint to the compiler that this data 
        // member will be accessed by multiple threads. 
        private volatile bool _shouldStop;

        public delegate void ErrorDelegate(string error);
        public ErrorDelegate Error = null;

        public delegate void DoneDelegate(string result);
        public DoneDelegate Done = null;

        public delegate void StatusDelegate(string status);
        public StatusDelegate Status = null;

        public string CallSign { get; set; }
        public string PassWord { get; set; }
        public bool Archive { get; set; }
        public string StoragePath { get; set; }

        public void DoWork()
        {
            var client = new CookieAwareWebClient();
            client.Encoding = Encoding.UTF8;

            // Post values
            var values = new NameValueCollection();
            values.Add("Callsign", CallSign);
            values.Add("EnteredPassword", PassWord);
            values.Add("Login", "Go");   //The button

            // Logging in
            client.UploadValues("http://www.eqsl.cc/qslcard/LoginFinish.cfm", values); // You may verify the result. It works with https :)

            var html = "";

            // Download some secret page
            if (Archive)
            {
                html = client.DownloadString("http://www.eqsl.cc/qslcard/InBox.cfm?Archive=1&Reject=0");
            }
            else
            {
                html = client.DownloadString("http://www.eqsl.cc/qslcard/InBox.cfm?Archive=0&Reject=0");
            }

            if (html.IndexOf(@"eQSLs more than can be displayed on this screen") > -1)
            {
                System.Windows.Forms.MessageBox.Show(@"You have too many cards.  You will need to wait for an updated version!");
                return;
            }
 
            currentIndex = 0;
            string mylist = parseDisplay(html);
            string[] items = mylist.Split('|');
            int countOfCards = items.Count();
            if (Status != null)
                Status("Downloading " + countOfCards + " eQSL cards");
            foreach (string s in items)
            {
                if (s.Trim().Length == 0)
                {
                    continue;
                }
                string h = null;
                string tmp = null;
                string callsign = null;
                int deadcounter = 5;
                while (h == null && deadcounter > 0)
                {
                    try
                    {
                        h = client.DownloadString("http://www.eqsl.cc/qslcard/" + s);
                        string s1 = s.Substring(s.IndexOf("Callsign=") + 9);
                        callsign = s1.Substring(0, s1.IndexOf("&"));
                        tmp = h;
                        h = h.Substring(h.IndexOf("img src="));
                        h = h.Substring(h.IndexOf("\"") + 1);
                        h = h.Substring(0, h.IndexOf("\""));
                    }
                    catch (Exception ex)
                    {
                        //
                        // < !--Application In Root eQSL Folder -->
                        // ERROR - Too many queries overloading the system.Slow down!
                        //
                        if (tmp.Contains("Slow down"))
                        {
                            deadcounter--;
                            if (Status != null)
                                Status("We have to slow down. Wait 10 seconds.");
                            System.Threading.Thread.Sleep(10000);
                        }
                    }
                }

                if (!(System.IO.Directory.Exists(StoragePath)))
                {
                    System.IO.Directory.CreateDirectory(StoragePath);
                }

                string filename = System.IO.Path.Combine(StoragePath, callsign + "-" + DateTime.Now.ToString("yyMMddmmssff") + ".png");

                client.DownloadFile("http://www.eqsl.cc" + h, filename);
                currentIndex = currentIndex + 1;
                //<CENTER>
                //<img src="/CFFileServlet/_cf_image/_cfimg-632732702018634097.PNG" alt="" />
                //get call sign 
                // download page
                // parse image name 
                // download image 
                // save image as call sign...  if exists.. _1,_2 etc... 
                if (Status != null)
                    Status("Downloading card : " + currentIndex.ToString() + " of " + countOfCards.ToString());
                // slowdown a little bit.
                System.Threading.Thread.Sleep(500); 
            }
            if (Done != null)
                Done("Download of " + currentIndex.ToString() + " QSL cards Complete");

        }

        private int currentIndex = 0;

       

        private string parseDisplay(string h)
        {
            string newString = "";

            int i = h.IndexOf("DisplayeQSL.cfm");


            while (i > -1)
            {
                h = h.Substring(i);
                string urlString = h.Substring(0, h.IndexOf("'"));
                h = h.Substring(15); // cut out current displayeQSL
                i = h.IndexOf("DisplayeQSL.cfm");
                newString = newString + urlString + "|";
            }

            //foreach (string s in h.Split(new string[] {@"DisplayeQSL.cfm?Callsign"}, StringSplitOptions.RemoveEmptyEntries))
            //{
            //    newString = newString + "||";
            //}

            return newString;

        }


        public class CookieAwareWebClient : WebClient
        {
            private CookieContainer cookieContainer = new CookieContainer();

            protected override WebRequest GetWebRequest(Uri address)
            {
                WebRequest request = base.GetWebRequest(address);
                if (request is HttpWebRequest)
                {
                    (request as HttpWebRequest).CookieContainer = cookieContainer;
                }
                return request;
            }
        }

    }
}
