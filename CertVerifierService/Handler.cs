using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using CertVerifierService.Commands;

namespace CertVerifierService
{
    public class Handler
    {

    
        public void Handle(Task task)
        {
            HttpListenerContext httpContext = task.TaskData as HttpListenerContext;
            if (httpContext == null) return;
            long taskInPoolTime = task.Timer.ElapsedMilliseconds;
            Log.Write("Incoming request: " + httpContext.Request.Url + ". Taken from task pool after " + taskInPoolTime + " ms of waiting.");
            try
            {
                byte[] readBuffer = new byte[httpContext.Request.ContentLength64];
                if (httpContext.Request.InputStream.Read(readBuffer, 0, readBuffer.Length)== 0)
                    throw new Exception("Can't read request body.");
                var p = new Parameters(readBuffer, httpContext.Request.QueryString);
                ICommand command = CommandFactory.GetCommand(p);
                byte[] buffer = Encoding.UTF8.GetBytes(command.Execute());
                WriteResponse(httpContext, buffer, HttpStatusCode.OK, "OK", "text/plain");
            }
            catch (Exception ex)
            {
                HandleError(httpContext, ex);
            }
            finally
            {
               FinshHandling(httpContext);
               long taskProcessingTime = task.Timer.ElapsedMilliseconds - taskInPoolTime;
               Log.Write("Request processed in " + taskProcessingTime + " ms. Total: " + (taskInPoolTime+taskProcessingTime) + " ms.");
            }
        }

        
        private void HandleError(HttpListenerContext httpContext, Exception exception)
        {
            Log.Write("ERROR: " + exception);
            try
            {
                WriteResponse(httpContext, Encoding.UTF8.GetBytes(exception.ToString()), HttpStatusCode.InternalServerError,
                              "Internal Error", "text/plain");
            }
            catch (Exception exception2)
            {
            }
        }

        private void FinshHandling(HttpListenerContext httpContext)
        {
            try
            {
                httpContext.Response.Close();
            }
            catch (Exception exception)
            {
                httpContext.Response.OutputStream.Dispose();
                httpContext.Response.Close();
            }
        }

        protected void WriteResponse(HttpListenerContext httpContext, byte[] content, HttpStatusCode statusCode, string statusDescription, string contentType)
        {
            HttpListenerResponse response = httpContext.Response;
            response.ContentType = contentType;
            response.StatusCode = (int)statusCode;
            response.StatusDescription = statusDescription;
            response.ContentLength64 = content.Length;
            response.ContentEncoding = Encoding.UTF8;
            response.OutputStream.Write(content, 0, content.Length);
        }
    }
}