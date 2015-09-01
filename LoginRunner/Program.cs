using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace LoginRunner
{
    public class Program
    {
        public static void Main(string[] args)
        {
#region VARIABLES
            //URL that shows up when you click login for the first time
            string initialLoginPage = @"https://loginPage";
            //URL that shows up after for confirming TOS and clicking submit
            string confirmationPage = @"https://confirmationPage";
            //TODO: Change
            string confimationPacketData =
                string.Format(
                    "guestUser.acceptUsePolicy=true&guestUser.name={0}&guestUser.password={1}&redirect=&err_flag=",
                    args[0], args[1]);
            string cookieHeader;
#endregion VARIABLES
            Console.WriteLine("Logging In");
            /*
            The following code trys to go to a web address and watches for a redirect. I used the ip adress 1.1.1.1
            because on the network I was testing that would trigger a redirect or (if I was already connected) it
            would trigger an exception.
            */
#region TestForConnection
            string pageSource;
            string testURL = "http://1.1.1.1";
            HttpWebRequest requestConnectionTest = (HttpWebRequest)WebRequest.Create(testURL);
            requestConnectionTest.Proxy = null;
            requestConnectionTest.AllowAutoRedirect = true;
            requestConnectionTest.CookieContainer = new CookieContainer();
            requestConnectionTest.Timeout = Int32.MaxValue;
            WebResponse responseConnectionTest = null;
            try
            {
                responseConnectionTest = requestConnectionTest.GetResponse();
            }
            catch (Exception)
            {
                Console.WriteLine("You should be connected to the internet or your network adapter is unplugged.");
                return;
            }
            //store the html from response
            using (StreamReader sr = new StreamReader(responseConnectionTest.GetResponseStream()))
            {
                pageSource = sr.ReadToEnd();
            }

            //Look for a link in the HTML. This is specific to my uni and may need change
            Regex reg = new Regex("https://[^\"]*");
            string redirectPage = reg.Match(pageSource).Value;
            #endregion TestForConnection

#region Login
            //Loads the page which was redirected to.
            HttpWebRequest getRequest = (HttpWebRequest)WebRequest.Create(redirectPage);
            getRequest.Proxy = null;
            getRequest.AllowAutoRedirect = true;
            getRequest.CookieContainer = new CookieContainer();
            WebResponse getResponse = getRequest.GetResponse();
            using (StreamReader sr = new StreamReader(getResponse.GetResponseStream()))
            {
                pageSource = sr.ReadToEnd();
            }
            //Saves the cookies given by the website (we will need these later)
            CookieCollection col = getRequest.CookieContainer.GetCookies(new Uri(redirectPage));
            //This is declared here because we needed the session ID stored in cookies
            //TODO: Change
            string initialPacketData =
                string.Format(
                    "guestUser.name={0}&guestUser.password={1}&redirect=&switch_url=&err_flag=&byodSessionId={2}&byodAction=",
                    args[0], args[1], col[0].Value);

            //Submit login data
            HttpWebRequest req1 = (HttpWebRequest)WebRequest.Create(initialLoginPage);
            req1.Proxy = null;
            //Set headers
            req1.ContentType = "application/x-www-form-urlencoded";
            req1.KeepAlive = true;
            req1.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            //TODO: Change
            req1.Referer = "https://referer";
            req1.Headers.Add("Cache-Control", "max-age=0");
            //TODO: Change
            req1.Headers.Add("Origin", "https://origin");
            req1.Headers.Add("Upgrade-Insecure-Requests", "1");
            req1.Headers.Add("Accept-Encoding", "gzip, deflate");
            req1.Headers.Add("Accept-Language", "en-US,en;q=0.8");
            //Return the same cookies that the server gave earlier
            req1.CookieContainer = getRequest.CookieContainer;
            req1.Method = "POST";
            //Add generated packet data
            byte[] bytes1 = Encoding.ASCII.GetBytes(initialPacketData);
            req1.ContentLength = bytes1.Length;
            using (Stream os = req1.GetRequestStream())
            {
                os.Write(bytes1, 0, bytes1.Length);
            }
            //Get response from server
            WebResponse resp1 = req1.GetResponse();
            cookieHeader = resp1.Headers["Set-cookie"];

            //Submit seond form using identical tactics
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(confirmationPage);
            req.Proxy = null;
            req.ContentType = "application/x-www-form-urlencoded";
            req.KeepAlive = true;
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            //TODO: Change
            req.Referer = "https://referer";
            req.Headers.Add("Cache-Control", "max-age=0");
            //TODO: Change
            req.Headers.Add("Origin", "https://origin");
            req.Headers.Add("Upgrade-Insecure-Requests", "1");
            req.Headers.Add("Accept-Encoding", "gzip, deflate");
            req.Headers.Add("Accept-Language", "en-US,en;q=0.8");
            req.CookieContainer = getRequest.CookieContainer;
            req.Method = "POST";
            byte[] bytes = Encoding.ASCII.GetBytes(confimationPacketData);
            req.ContentLength = bytes.Length;
            using (Stream os = req.GetRequestStream())
            {
                os.Write(bytes, 0, bytes.Length);
            }
            WebResponse resp = req.GetResponse();
            cookieHeader = resp.Headers["Set-cookie"];
#endregion Login
        }
    }
}
