using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace WebApplication1 {
    public class HttpHelper
    {

		public string WebPost(string url, string postData, string cotentType = "application/json", string token = "", string timestamp = "", bool Extend = false)
		{
			//LogHelper.Info(url);
			WebRequest request = WebRequest.Create(url);
			request.Method = "POST";
			//string postData = JsonConvert.SerializeObject(data); 

			byte[] byteArray = Encoding.UTF8.GetBytes(postData);
			request.ContentType = cotentType;
			request.ContentLength = byteArray.Length;
			request.Timeout = 6000;

			try
			{
				Stream dataStream = request.GetRequestStream();
				dataStream.Write(byteArray, 0, byteArray.Length);
				dataStream.Close();
				WebResponse response = request.GetResponse();
				////LogHelper.Info(((HttpWebResponse)response).StatusDescription);
				dataStream = response.GetResponseStream();
				StreamReader reader = new StreamReader(dataStream, Encoding.UTF8);
				string responseFromServer = reader.ReadToEnd();
				reader.Close();
				dataStream.Close();
				response.Close();
				return responseFromServer;
			}
			catch (Exception e)
			{
				//LogHelper.Info(e.Message);
				return "";
			}
		}


        public string WebGet(string url) {
            //using (var client = new HttpClient()) {
            //	//请求结果
            //	string result = client.GetAsync(url).Result.Content.ReadAsStringAsync().Result;

            //	//LogHelper.Info(result);
            //	return result;

            //}
            ////LogHelper.Info(url);
            WebRequest request = WebRequest.Create(url);
            request.Timeout = 6000;
            request.Method = "GET";

            try {
                WebResponse response = request.GetResponse();
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();

                reader.Close();
                dataStream.Close();
                response.Close();
                //LogHelper.Info(responseFromServer);
                return responseFromServer;
            }
            catch (Exception e) {
                return "";
            }
        }

		public string Get(string url, string contentType = "application/x-www-form-urlencoded") {
			WebRequest request = WebRequest.Create(url);
			request.Method = "Get";
			request.ContentType = contentType;
			StreamReader reader = null;
			Stream stream = null;
			WebResponse rsp = null;
			try {

				rsp = request.GetResponse();
				stream = rsp.GetResponseStream();
				reader = new StreamReader(stream);
				return reader.ReadToEnd();
			}
			catch {
				return "";
			}
			finally {
				// 释放资源
				if (reader != null) reader.Close();
				if (stream != null) stream.Close();
				if (rsp != null) rsp.Close();
			}

		}

		public string Post(string url, string postData, string contentType = "application/json", string sessionId = "") 
		{
            Console.WriteLine(url);
			WebRequest request = WebRequest.Create(url);
			request.Method = "POST";
			byte[] byteArray = Encoding.UTF8.GetBytes(postData);
			request.ContentType = contentType;
			request.ContentLength = byteArray.Length;
			if (sessionId != "") {
				request.Headers.Set("ASP.NET_SessionId", sessionId);
			}
			StreamReader reader = null;
			Stream stream = null;
			WebResponse rsp = null;
			try {
				stream = request.GetRequestStream();
				stream.Write(byteArray, 0, byteArray.Length);
				stream.Close();
				rsp = request.GetResponse();
				stream = rsp.GetResponseStream();
				reader = new StreamReader(stream);
				return reader.ReadToEnd();
			}
			catch {
				return "";
			}
			finally {
				// 释放资源
				if (reader != null) reader.Close();
				if (stream != null) stream.Close();
				if (rsp != null) rsp.Close();
			}

		}


		public string Post(string url, Dictionary<string, string> dic) {
			var param = dic.Select(a => { return string.Format("{0}={1}", a.Key, a.Value); }).ToList();
			return Post(url, string.Join("&", param), "application/x-www-form-urlencoded");

		}

		public string PostWithCookie(string url, string sessinId) {
			return PostWithCookie(url, "", "", sessinId);
		}
		public string PostWithCookie(string url, string postData, string contentType = "application/json", string sessionId = "") {
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			request.Method = "POST";
			byte[] byteArray = Encoding.UTF8.GetBytes(postData);
			request.ContentType = contentType;
			request.ContentLength = byteArray.Length;
			if (sessionId != "") {
				request.CookieContainer = new CookieContainer();
				request.CookieContainer.SetCookies(new Uri("http://" + request.RequestUri.Authority), "ASP.NET_SessionId=" + sessionId);
			}
			StreamReader reader = null;
			Stream stream = null;
			WebResponse rsp = null;
			try {
				stream = request.GetRequestStream();
				stream.Write(byteArray, 0, byteArray.Length);
				stream.Close();
				rsp = request.GetResponse();
				stream = rsp.GetResponseStream();
				reader = new StreamReader(stream);
				return reader.ReadToEnd();
			}
			catch {
				return "";
			}
			finally {
				// 释放资源
				if (reader != null) reader.Close();
				if (stream != null) stream.Close();
				if (rsp != null) rsp.Close();
			}

		}


	}
}
