using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;


namespace Telegram
{
    static class Sender
    {
        public static string Send(string url, string message = "", string contentType = "application/x-www-form-urlencoded")
        {
            url = UrlClearer.Clear(url);

            // Create a request using a URL that can receive a post. 
            WebRequest request = WebRequest.Create(url);
            // Set the Method property of the request to POST.
            request.Method = "POST";
            // Create POST data and convert it to a byte array.
            //string postData = "{\"value1\":\"" + message + "\"}";
            string postData = message;
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            // Set the contentType property of the WebRequest.
            request.ContentType = contentType;
            // Set the ContentLength property of the WebRequest.
            //  request.ContentLength = byteArray.Length;
            // Get the request stream.
            Stream dataStream = request.GetRequestStream();
            // Write the data to the request stream. 
            dataStream.Write(byteArray, 0, byteArray.Length);
            // Close the Stream object.
            dataStream.Close();
            // Get the response.
            WebResponse response = request.GetResponse();
            // Display the status.
            //Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            // Get the stream containing content returned by the server.
            dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();
            // Display the content.
            // Clean up the streams.
            reader.Close();
            dataStream.Close();
            response.Close();

            return responseFromServer;
        }
    }
}
