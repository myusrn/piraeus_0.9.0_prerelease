using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebGatewayTest.Formatters
{
    public class BinaryMediaTypeFormatter : MediaTypeFormatter
    {
        private const string supportedMediaType = "application/octet-stream";


        public BinaryMediaTypeFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue(supportedMediaType));
            SupportedEncodings.Add(Encoding.GetEncoding("utf-8"));
            SupportedEncodings.Add(Encoding.GetEncoding("utf-16"));
            SupportedEncodings.Add(Encoding.ASCII);
        }

        public override bool CanReadType(Type type)
        {
            return type == typeof(byte[]);
        }

        public override bool CanWriteType(Type type)
        {
            return type == typeof(byte[]);
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            //TaskCompletionSource<object> source = new TaskCompletionSource<object>();

            //using (MemoryStream stream = new MemoryStream())
            //{
            //    readStream.CopyTo(stream);
            //    byte[] buffer = stream.ToArray();
            //    source.SetResult(Encoding.UTF8.GetString(buffer));
            //}

            //return source.Task;

            var taskCompletionSource = new TaskCompletionSource<object>();
            try
            {
                var memoryStream = new MemoryStream();
                readStream.CopyTo(memoryStream);
                taskCompletionSource.SetResult(memoryStream.ToArray());
            }
            catch (Exception e)
            {
                taskCompletionSource.SetException(e);
            }
            return taskCompletionSource.Task;

        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger, CancellationToken cancellationToken)
        {
            //TaskCompletionSource<object> source = new TaskCompletionSource<object>();

            //using (MemoryStream stream = new MemoryStream())
            //{
            //    readStream.CopyTo(stream);
            //    byte[] buffer = stream.ToArray();
            //    source.SetResult(Encoding.UTF8.GetString(buffer));
            //}

            //return source.Task;
            var taskCompletionSource = new TaskCompletionSource<object>();
            try
            {
                var memoryStream = new MemoryStream();
                readStream.CopyTo(memoryStream);
                taskCompletionSource.SetResult(memoryStream.ToArray());
            }
            catch (Exception e)
            {
                taskCompletionSource.SetException(e);
            }
            return taskCompletionSource.Task;

        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            var buff = value as byte[];
            return writeStream.WriteAsync(buff, 0, buff.Length);


            //TaskCompletionSource<object> source = new TaskCompletionSource<object>();
            //byte[] buffer = value == null ? new byte[0] : Encoding.UTF8.GetBytes(value as string);

            //using (MemoryStream stream = new MemoryStream(buffer))
            //{
            //    stream.CopyTo(writeStream);
            //}

            //source.SetResult(null);

            //return source.Task;
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext, CancellationToken cancellationToken)
        {
            var buff = value as byte[];
            return writeStream.WriteAsync(buff, 0, buff.Length, cancellationToken);

            //TaskCompletionSource<object> source = new TaskCompletionSource<object>();
            //byte[] buffer = value == null ? new byte[0] : Encoding.UTF8.GetBytes(value as string);

            //using (MemoryStream stream = new MemoryStream(buffer))
            //{
            //    stream.CopyTo(writeStream);
            //}

            //source.SetResult(null);

            //return source.Task;
        }
    }
}