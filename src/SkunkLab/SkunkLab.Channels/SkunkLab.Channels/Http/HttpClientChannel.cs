﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using SkunkLab.Diagnostics.Logging;

namespace SkunkLab.Channels.Http
{
    public class HttpClientChannel : HttpChannel
    {
        public HttpClientChannel(string endpoint, string resourceUriString, string contentType, string securityToken, List<KeyValuePair<string, string>> indexes = null)
        {
            Id = "http-" + Guid.NewGuid().ToString();
            requestUri = new Uri(endpoint);
            this.contentType = contentType;
            this.securityToken = securityToken;
            this.resourceUriString = resourceUriString;
            this.indexes = indexes;
        }

        public HttpClientChannel(string endpoint, string resourceUriString, string contentType, X509Certificate2 certificate, List<KeyValuePair<string, string>> indexes = null)
        {
            Id = "http-" + Guid.NewGuid().ToString();
            requestUri = new Uri(endpoint);
            this.contentType = contentType;
            this.certificate = certificate;
            this.resourceUriString = resourceUriString;
            this.indexes = indexes;
        }

        public HttpClientChannel(string endpoint, string securityToken, IEnumerable<Observer> observers, CancellationToken token = default(CancellationToken))
        {
            Id = "http-" + Guid.NewGuid().ToString();
            requestUri = new Uri(endpoint);
            this.securityToken = securityToken;
            this.observers = observers;
            this.token = token;
            this.tokenSource = new CancellationTokenSource();
            this.internalToken = tokenSource.Token;
            this.token.Register(() => this.tokenSource.Cancel());
        }

        public HttpClientChannel(string endpoint, X509Certificate2 certificate, IEnumerable<Observer> observers, CancellationToken token = default(CancellationToken))
        {
            Id = "http-" + Guid.NewGuid().ToString();
            requestUri = new Uri(endpoint);
            this.certificate = certificate;
            this.observers = observers;
            this.token = token;
            this.tokenSource = new CancellationTokenSource();
            this.internalToken = tokenSource.Token;
            this.token.Register(() => this.tokenSource.Cancel());
        }
        

        private IEnumerable<KeyValuePair<string, string>> indexes;
        private CancellationTokenSource tokenSource;
        private IEnumerable<Observer> observers;
        private X509Certificate2 certificate;
        private string securityToken;
        private string contentType;
        private Uri requestUri;
        private string resourceUriString;
        private CancellationToken internalToken;
        private CancellationToken token;
        private bool disposed;
        private ChannelState _state;

        public override bool IsAuthenticated { get; internal set; }

        public override bool IsEncrypted { get; internal set; }
        public override int Port { get; internal set; }

        public override bool IsConnected
        {
            get { return State == ChannelState.Open; }
        }

        public override string Id { get; internal set; }
        public override ChannelState State
        {
            get { return _state; }
            internal set
            {
                if(value != _state)
                {
                    OnStateChange?.Invoke(this, new ChannelStateEventArgs(Id, value));
                }

                _state = value;
            }
        }

        public override event EventHandler<ChannelReceivedEventArgs> OnReceive;
        public override event EventHandler<ChannelCloseEventArgs> OnClose;
        public override event EventHandler<ChannelOpenEventArgs> OnOpen;
        public override event EventHandler<ChannelErrorEventArgs> OnError;
        public override event EventHandler<ChannelStateEventArgs> OnStateChange;
        public override event EventHandler<ChannelRetryEventArgs> OnRetry;
        public override event EventHandler<ChannelSentEventArgs> OnSent;
        public override event EventHandler<ChannelObserverEventArgs> OnObserve;

     

        public override async Task AddMessageAsync(byte[] message)
        {            
            await Log.LogInfoAsync("Http client channel cannot add messages to the channel without calling SendAsync method.");
            await TaskDone.Done;
        }

        public override async Task CloseAsync()
        {
            await Log.LogInfoAsync("Channel '{0}' is closing.", Id);

            State = ChannelState.ClosedReceived;

           if(!internalToken.IsCancellationRequested)
            {
                tokenSource.Cancel();
            }

            OnClose?.Invoke(this, new ChannelCloseEventArgs(Id));

            State = ChannelState.Closed;
            await TaskDone.Done;
        }

        

        public override async Task OpenAsync()
        {
            await Log.LogInfoAsync("Channel {0} is opening", Id);
            OnOpen?.Invoke(this, new ChannelOpenEventArgs(Id, null));
            State = ChannelState.None;

        }

        public override async Task ReceiveAsync()
        {
            while(!internalToken.IsCancellationRequested)
            {
                await Log.LogAsync("Channel '{0}' is starting receive loop.", Id);
                State = ChannelState.Connecting;
                HttpWebRequest request = GetRequest(HttpMethod.Get);
                Port = request.RequestUri.Port;
                IsEncrypted = request.RequestUri.Scheme == "https";

                try
                {
                    State = ChannelState.Open;
                    using (HttpWebResponse response = await request.GetResponseAsync().WithCancellation<WebResponse>(internalToken) as HttpWebResponse)                   
                    {
                        if (response.StatusCode == HttpStatusCode.OK ||
                            response.StatusCode == HttpStatusCode.Accepted)
                        {
                            using (Stream stream = response.GetResponseStream())
                            {
                                byte[] buffer = new byte[response.ContentLength];
                                await stream.ReadAsync(buffer, 0, buffer.Length);

                                string resourceHeader = response.Headers.Get(HttpChannelConstants.RESOURCE_HEADER);
                                string resourceUriString = new Uri(resourceHeader).ToLower();

                                foreach (Observer observer in observers)
                                {
                                    await Log.LogAsync("Http client channel calling observers from receive loop.");
                                    if(observer.ResourceUri.ToLower() == resourceUriString)
                                    {
                                        
                                        observer.Update(observer.ResourceUri, response.ContentType, buffer);
                                        await Log.LogInfoAsync("Http client channel observer updated.");
                                    }
                                }

                                List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
                                list.Add(new KeyValuePair<string, string>("Resource", resourceUriString));
                                list.Add(new KeyValuePair<string, string>("Content-Type", response.ContentType));                               
                                OnReceive?.Invoke(this, new ChannelReceivedEventArgs(Id, buffer, list));
                            }
                        }
                        else
                        {
                            //unexpected status code
                            OnError?.Invoke(this, new ChannelErrorEventArgs(Id, new WebException(String.Format("Unexpected status code {0}", response.StatusCode))));
                        }

                        State = ChannelState.Closed;
                    }
                }
                catch (OperationCanceledException)
                {
                    //not an error
                    await Log.LogInfoAsync("Http client channel '{0}' receive cancelled.", Id);
                    State = ChannelState.Aborted;
                }
                catch (AggregateException ae)
                {
                    await Log.LogErrorAsync("Http client channel '{0}' receive error '{1}'", Id, ae.Flatten().InnerException.Message);
                    State = ChannelState.Closed;
                    OnError?.Invoke(this, new ChannelErrorEventArgs(Id, ae.Flatten()));
                }     
                catch(Exception ex)
                {
                    await Log.LogErrorAsync("Http client channel '{0}' receive error '{1}'", Id, ex.Message);
                    State = ChannelState.Closed;
                    OnError?.Invoke(this, new ChannelErrorEventArgs(Id, ex));
                }
            }
        }

        public override async Task SendAsync(byte[] message)
        {
            try
            {
                HttpWebRequest request = GetRequest(HttpMethod.Post);
                request.ContentLength = message.Length;
                Port = request.RequestUri.Port;
                IsEncrypted = request.RequestUri.Scheme == "https";               

                using (Stream stream = await request.GetRequestStreamAsync().WithCancellation(internalToken))
                {
                    await stream.WriteAsync(message, 0, message.Length);
                }

                using (HttpWebResponse response = await request.GetResponseAsync().WithCancellation(internalToken) as HttpWebResponse)
                {
                    if (response.StatusCode == HttpStatusCode.OK ||
                        response.StatusCode == HttpStatusCode.Accepted ||
                        response.StatusCode == HttpStatusCode.NoContent)
                    {
                        IsAuthenticated = true;
                        State = ChannelState.CloseSent;
                    }
                    else
                    {
                        State = ChannelState.Aborted;
                        OnError?.Invoke(this, new ChannelErrorEventArgs(Id, new WebException(
                            String.Format("Invalid HTTP response status code {0}", response.StatusCode))));
                    }
                }

                OnSent?.Invoke(this, new ChannelSentEventArgs(Id, null));
            }
            catch(OperationCanceledException oce)
            {
                await Log.LogWarningAsync("Channel '{0}' cancelled with '{1}'", Id, oce.Message);
                State = ChannelState.Aborted;
            }
            catch(AggregateException ae)
            {
                State = ChannelState.Aborted;
                OnError?.Invoke(this, new ChannelErrorEventArgs(Id, ae.Flatten()));
            }
            catch(WebException we)
            {
                await Log.LogErrorAsync("Channel '{0}' error with '{1}'", Id, we.Message);
                State = ChannelState.Aborted;
                OnError?.Invoke(this, new ChannelErrorEventArgs(Id, we.InnerException));
            }
            catch(Exception ex)
            {
                await Log.LogErrorAsync("Channel '{0}' error with '{1}'", Id, ex.Message);
                State = ChannelState.Aborted;
                OnError?.Invoke(this, new ChannelErrorEventArgs(Id, ex));
            }
        }
        protected void Disposing(bool dispose)
        {
            if (dispose & !disposed)
            {
                if(internalToken.CanBeCanceled)
                {
                    this.tokenSource.Cancel();
                }

                IsAuthenticated = false;
                IsEncrypted = false;
                State = ChannelState.Closed;
                Port = 0;

                certificate = null;
                securityToken = null;
                observers = null;
                disposed = true;
            }
        }

        public override void Dispose()
        {
            Disposing(true);
            GC.SuppressFinalize(this);
        }

        private HttpWebRequest GetRequest(HttpMethod method)
        {
            HttpWebRequest request = HttpWebRequest.Create(requestUri) as HttpWebRequest;
            //request.Accept = contentType;

            if (method == HttpMethod.Get)
            {
                //if(string.IsNullOrEmpty(contentType))
                //{
                //    contentType = "application/json";
                //}

                request.ContentLength = 0;
                request.Method = "GET";
                //request.ContentType = contentType;
                request.KeepAlive = true;
                request.Timeout = Int32.MaxValue;
                if (observers != null)
                {
                    foreach (Observer observer in observers)
                    {
                        request.Headers.Add(HttpChannelConstants.SUBSCRIBE_HEADER, observer.ResourceUri.ToLower());
                    }
                }
            }
            else if(method == HttpMethod.Post)
            {
                request.Method = "POST";
                request.ContentType = contentType;

                Uri resourceUri = new Uri(resourceUriString.ToLower(CultureInfo.InvariantCulture));

                request.Headers.Add(HttpChannelConstants.RESOURCE_HEADER, resourceUri.ToString());

                if (indexes != null)
                {
                    foreach(KeyValuePair<string,string> index in indexes)
                    {
                        request.Headers.Add(HttpChannelConstants.INDEX_HEADER, index.Key + ";" + index.Value);
                    }
                }
            }
            else
            {
                throw new HttpRequestException(String.Format("Invalid request verb {0}", method.ToString()));
            }

            if (!string.IsNullOrEmpty(securityToken))
            {
                request.Headers.Add("Authorization", String.Format("Bearer {0}", securityToken));
            }

            if (certificate != null)
            {
                request.ClientCertificates.Add(certificate);
            }            

            return request;
        }
    }
}
