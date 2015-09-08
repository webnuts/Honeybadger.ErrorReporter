using System;
using System.Web;

namespace Honeybadger.ErrorReporter
{
    public class HoneybadgerErrorRegistrationModule : IHttpModule
    {
        public void Init(HttpApplication httpApplication)
        {
            httpApplication.AuthenticateRequest += AuthenticateRequest;
            httpApplication.Error += ContextError;
        }

        static void AuthenticateRequest(object sender, EventArgs e)
        {
            var app = (HttpApplication)sender;

            var honeybadgerErrorTestMessage = app.Request.QueryString["HoneybadgerErrorTest"];

            if (string.IsNullOrEmpty(honeybadgerErrorTestMessage) == false)
            {
                var honeybadgerService = new HoneybadgerService();
                string honeybadgerResponse;
                honeybadgerService.ReportException(new Exception(honeybadgerErrorTestMessage), out honeybadgerResponse);
            }
        }

        static void ContextError(object sender, EventArgs e)
        {
            var httpApplication = sender as HttpApplication;
            if (httpApplication != null)
            {
                var context = httpApplication.Context;
                if (context != null)
                {
                    var exception = context.Server.GetLastError();
                    string honeybadgerResponse;

                    var honeybadgerService = new HoneybadgerService();
                    honeybadgerService.ReportException(exception, out honeybadgerResponse);
                }
            }
        }

        public void Dispose()
        {
            
        }
    }
}
