using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                return;
            }

            string port = "8080";
            HttpListener httpListener = new HttpListener();
            httpListener.Prefixes.Add(string.Format("http://192.168.1.175/GetVersion:{0}/", 8080));
            httpListener.Start();
            httpListener.BeginGetContext(new AsyncCallback(GetContext), httpListener);  //开始异步接收request请求
            Console.WriteLine("监听端口:" + port);
            Console.Read();
        }

        static void GetContext(IAsyncResult ar)
        {
            Console.WriteLine("收到请求:" + ar);
            HttpListener httpListener = ar.AsyncState as HttpListener;
            HttpListenerContext context = httpListener.EndGetContext(ar);  //接收到的请求context（一个环境封装体）

            httpListener.BeginGetContext(new AsyncCallback(GetContext), httpListener);  //开始 第二次 异步接收request请求

            HttpListenerRequest request = context.Request;  //接收的request数据
            HttpListenerResponse response = context.Response;  //用来向客户端发送回复

            response.ContentType = "html";
            response.ContentEncoding = Encoding.UTF8;

            using (Stream output = response.OutputStream)  //发送回复
            {
                string FilePath = "D:/LocalServer/GetVersion/version";
                string content = File.ReadAllText(FilePath);
                byte[] buffer = Encoding.UTF8.GetBytes(content);
                output.Write(buffer, 0, buffer.Length);
            }
        }
    }
}
