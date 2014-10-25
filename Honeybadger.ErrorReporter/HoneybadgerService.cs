using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Security;

namespace Honeybadger.ErrorReporter
{
    public class HoneybadgerService
    {
        public bool ReportException(Exception exception, out string honeybadgerResponse)
        {
            while (exception.InnerException != null)
            {
                exception = exception.InnerException;
            }
            var reportingContent = GetReportingContent(exception);

            var reportingJson = new JavaScriptSerializer().Serialize(reportingContent);

            return SendErrorToHoneybadger(reportingJson, out honeybadgerResponse);
        }

        private static dynamic GetReportingContent(Exception exception)
        {
            var context = HttpContext.Current;
            var server = context.Server;
            return new
            {
                notifier = GetNotifier(),
                error = GetErrorInformation(exception),
                request = GetRequestInformation(context.Request),
                server = new
                {
                    project_root = new
                    {
                        path = server.MapPath("~/")
                    },
                    environment_name = ConfigurationManager.AppSettings["honeybadger-environment-name"],
                    hostname = context.Request.ServerVariables["SERVER_NAME"]
                }
            };
        }

        private static dynamic GetRequestInformation(HttpRequest request)
        {
            return new
            {
                url = request.Url.ToString(),
                form = request.Form,
                context = GetContext()
            };
        }

        private static dynamic GetContext()
        {
            MembershipUser membershipUser = null;
            try
            {
                membershipUser = Membership.GetUser();
            }
            catch { }
            dynamic context = null;
            if (membershipUser != null)
            {
                context = new
                {
                    username = membershipUser.UserName,
                    user_id = membershipUser.ProviderUserKey
                };
            }
            return context;
        }

        private static dynamic GetErrorInformation(Exception exception)
        {
            return new
            {
                @class = exception.GetType().Name,
                message = exception.Message,
                backtrace = GetBacktraceFromException(exception)
            };
        }

        private static dynamic GetNotifier()
        {
            return new
            {
                name = "Honeybadger .NET Notifier",
                url = "https://github.com/webnuts/honeybadger-error-reporter",
                version = "1.0"
            };
        }

        private static bool SendErrorToHoneybadger(string reportingJson, out string honeybadgerResponse)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.honeybadger.io/v1/notices");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Accept = "application/json";
            httpWebRequest.Method = "POST";

            httpWebRequest.Headers.Add("X-API-Key", ConfigurationManager.AppSettings["honeybadger-api-key"]);

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(reportingJson);
                streamWriter.Flush();
                streamWriter.Close();

                var webResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var responseStream = webResponse.GetResponseStream())
                {
                    honeybadgerResponse = responseStream != null ? new StreamReader(responseStream).ReadToEnd() : "";
                }

                if (webResponse.StatusCode != HttpStatusCode.Created)
                {
                    return false;
                }
            }
            return true;
        }

        private static List<dynamic> GetBacktraceFromException(Exception exception)
        {
            var backtrace = new List<dynamic>();
            var stackTrace = new StackTrace(exception, true);
            for (var i = 0; i < stackTrace.FrameCount; i++)
            {
                var frame = stackTrace.GetFrame(i);
                backtrace.Add(new
                {
                    number = frame.GetFileLineNumber(),
                    file = frame.GetFileName(),
                    method = frame.GetMethod().Name
                });
            }
            return backtrace;
        }
    }
}