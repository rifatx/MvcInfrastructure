using MvcService.Data;
using MvcService.Data.Helpers;
using MvcService.Messaging;
using MvcService.Messaging.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MvcServiceClient
{
    class Program
    {
        static void Main(string[] args)
        {
            //HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create("http://localhost/MvcService/Main/Method1");

            //req.Method = "POST";
            //req.ContentType = "application/json";

            //using (Stream reqs = req.GetRequestStream())
            //{
            //    byte[] reqb = Encoding.ASCII.GetBytes("{\"id\":1,\"name\":\"osman\"}");
            //    reqs.Write(reqb, 0, reqb.Length);
            //    reqs.Flush();


            //    HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            //    using (Stream resps = resp.GetResponseStream())
            //    {
            //        StreamReader rdr = new StreamReader(resps);

            //        Console.WriteLine(rdr.ReadToEnd());
            //    }
            //}


            Console.WriteLine(MessagingService.Current["http://localhost/MvcService/Main/", "Method1"].GetResponse<TestData, TestData>(new TestData() { id = 1, name = "osman" }));

            MessagingService.Current["http://localhost/MvcService/Main/", "Method2", 1000].GetResponseAsync<TestData, TestData>(new TestData() { id = 3, name = "osman3" },
                (req, res) =>
                {
                    Console.WriteLine(res);
                },
                (req) =>
                {
                    Console.WriteLine("error: " + req);
                });

            Console.WriteLine(MessagingService.Current["Method1"].GetResponse<TestData, TestData>(new TestData() { id = 2, name = "osman2" }));

            Console.WriteLine("bitti");

            Console.ReadLine();
        }
    }
}
