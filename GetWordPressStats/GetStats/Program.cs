using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace Examples.System.Ham
{

public class WebRequestGetExample
    {
        public static void Main()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://public-api.wordpress.com/rest/v1.1/sites/56752040/stats/summary/?fields=views&period=year&num=5");
            request.Headers["Authorization"] = "Bearer YourGuidHere";
            string foxGreet =@"
              /^._
,___,--~~~~--' /'~  FoxDeploy Traffic Checking App!
`~--~\ )___,)/ '
    (/\\_(/\\_      Let's check the stats!
                ";

            Console.WriteLine(foxGreet);
            request.Credentials = CredentialCache.DefaultCredentials;
            // Get the response.  
            WebResponse response = request.GetResponse();
            // Display the status.  
            Console.WriteLine("HTTP Status Code:"+((HttpWebResponse)response).StatusDescription);
            // Get the stream containing content returned by the server.  
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.  
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.  
            string responseFromServer = reader.ReadToEnd();
            // Display the content.  
            //Console.WriteLine(responseFromServer);
            // Clean up the streams and the response.  
            var split = responseFromServer.Split(',');

            string Hits = split[2].Split(':')[1];

            Console.WriteLine(String.Format("{0:#,###,###}", Hits));
            
            //RootObject r = responseFromServer;

            //Console.Write(r);

            reader.Close();
            response.Close();


            

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}