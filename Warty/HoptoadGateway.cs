using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

namespace Warty
{
    public class HoptoadGateway
    {
        private const string VERSION = "2.0";
        private const string API_KEY = "key goes here";
        private const string NOTIFIER_NAME = "Warty";
        private const string NOTIFIER_VERSION = "0.0.1";
        private const string NOTIFIER_URL = "https://github.com/dtrietsch/warty";
        private const string ENVIRONMENT_NAME = "development";
        private const string NOTIFIER_API_URL = @"http://hoptoadapp.com/notifier_api/v2/notices";

        public string Notify(Exception e)
        {
            return SendXmlToHoptoad(ConvertExceptionIntoNotificationXml(e));
        }

        private XmlDocument ConvertExceptionIntoNotificationXml(Exception e)
        {
            System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace(e, true);

            XmlDocument hoptoadXml = new XmlDocument();

            XmlNode notice;
            XmlAttribute version;
            notice = hoptoadXml.CreateElement("notice");
            version = hoptoadXml.CreateAttribute("version");
            version.Value = VERSION;
            notice.Attributes.Append(version);
            hoptoadXml.AppendChild(notice);

            XmlNode apiKey;
            apiKey = hoptoadXml.CreateElement("api-key");
            apiKey.InnerText = API_KEY;
            notice.AppendChild(apiKey);

            XmlNode notifier, notifierName, notifierVersion, notifierUrl;
            notifier = hoptoadXml.CreateElement("notifier");
            notice.AppendChild(notifier);
            notifierName = hoptoadXml.CreateElement("name");
            notifierName.InnerText = NOTIFIER_NAME;
            notifier.AppendChild(notifierName);
            notifierVersion = hoptoadXml.CreateElement("version");
            notifierVersion.InnerText = NOTIFIER_VERSION;
            notifier.AppendChild(notifierVersion);
            notifierUrl = hoptoadXml.CreateElement("url");
            notifierUrl.InnerText = NOTIFIER_URL;
            notifier.AppendChild(notifierUrl);

            XmlNode error, errorClass, errorMessage;
            error = hoptoadXml.CreateElement("error");
            notice.AppendChild(error);
            errorClass = hoptoadXml.CreateElement("class");
            errorClass.InnerText = e.GetType().ToString();
            error.AppendChild(errorClass);
            errorMessage = hoptoadXml.CreateElement("message");
            errorMessage.InnerText = e.Message;
            error.AppendChild(errorMessage);

            XmlNode backtrace, line;
            XmlAttribute method, file, number;
            backtrace = hoptoadXml.CreateElement("backtrace");
            error.AppendChild(backtrace);
            for (int j = 0; j < trace.FrameCount; ++j)
            {
                line = hoptoadXml.CreateElement("line");
                backtrace.AppendChild(line);
                file = hoptoadXml.CreateAttribute("file");
                file.Value = trace.GetFrame(j).GetFileName();
                line.Attributes.Append(file);
                number = hoptoadXml.CreateAttribute("number");
                number.Value = trace.GetFrame(j).GetFileLineNumber().ToString();
                line.Attributes.Append(number);
                method = hoptoadXml.CreateAttribute("method");
                method.Value = trace.GetFrame(j).GetMethod().Name;
                line.Attributes.Append(method);
            }

            XmlNode serverEnvironment, environmentName;
            serverEnvironment = hoptoadXml.CreateElement("server-environment");
            notice.AppendChild(serverEnvironment);
            environmentName = hoptoadXml.CreateElement("environment-name");
            environmentName.InnerText = ENVIRONMENT_NAME;
            serverEnvironment.AppendChild(environmentName);

            return hoptoadXml;
        }

        private string SendXmlToHoptoad(XmlDocument hoptoadXml)
        {
            string notifierApiUrl = NOTIFIER_API_URL;

            WebRequest notifierRequest = WebRequest.Create(notifierApiUrl);
            notifierRequest.Method = "Post";
            notifierRequest.ContentType = "text/xml";
            UTF8Encoding encoder = new UTF8Encoding();
            byte[] data = encoder.GetBytes(hoptoadXml.InnerXml.ToString());
            notifierRequest.ContentLength = data.Length;
            Stream requestStream = notifierRequest.GetRequestStream();
            requestStream.Write(data, 0, data.Length);
            requestStream.Close();

            WebResponse response = notifierRequest.GetResponse();
            StreamReader streamReader = new StreamReader(response.GetResponseStream());
            return streamReader.ReadToEnd();
        }
    }
}
