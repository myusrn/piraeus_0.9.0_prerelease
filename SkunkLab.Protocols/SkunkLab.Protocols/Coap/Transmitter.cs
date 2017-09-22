using System;
using System.Collections.Generic;
using System.Timers;
using System.Linq;

namespace SkunkLab.Protocols.Coap
{
    public class Transmitter
    {
        public Transmitter(double lifetimeMilliseconds, double retryMilliseconds, int maxRetryAttempts)
        {
            this.lifetimeMilliseconds = lifetimeMilliseconds;
            this.retryMilliseconds = retryMilliseconds;
            maxAttempts = maxRetryAttempts;
            timer = new Timer(1000);
            timer.Elapsed += Timer_Elapsed;
            retryContainer = new Dictionary<ushort, Tuple<DateTime, int, CoapMessage>>();
            container = new Dictionary<ushort, Tuple<string, DateTime, Action<CodeType, string, byte[]>>>();
        }        

        private double lifetimeMilliseconds;
        private double retryMilliseconds;
        private int maxAttempts;
        private ushort currentId;
        private Timer timer;
        private Dictionary<ushort, Tuple<string, DateTime, Action<CodeType, string, byte[]>>> container;
        private Dictionary<ushort, Tuple<DateTime, int, CoapMessage>> retryContainer;
        public event EventHandler<CoapMessageEventArgs> OnRetry;

        public ushort NewId(byte[] token, Action<CodeType, string, byte[]> action = null)
        {
            currentId++;
            currentId = currentId == ushort.MaxValue ? (ushort)1 : currentId;

            while(container.ContainsKey(currentId))
            {
                currentId++;
                currentId = currentId == ushort.MaxValue ? (ushort)1 : currentId;
            }

            Tuple<string, DateTime, Action<CodeType, string, byte[]>> tuple = new Tuple<string, DateTime, Action<CodeType, string, byte[]>>(Convert.ToBase64String(token), DateTime.UtcNow.AddMilliseconds(lifetimeMilliseconds), action);
            container.Add(currentId, tuple);

            timer.Enabled = true;

            return currentId;
        }

        public void AddMessage(CoapMessage message)
        {
            if(message.MessageType == CoapMessageType.Confirmable)
            {
                if (!retryContainer.ContainsKey(message.MessageId))
                {
                    retryContainer.Add(message.MessageId, new Tuple<DateTime, int, CoapMessage>(DateTime.UtcNow.AddMilliseconds(retryMilliseconds), 0, message));
                }
            }
        }

        public void DispatchResponse(CoapMessage message)
        {
            var query = container.Where((c) => c.Value.Item1 == Convert.ToBase64String(message.Token));

            if(query != null && query.Count() == 1)
            {
                query.First().Value.Item3(message.Code, MediaTypeConverter.ConvertFromMediaType(message.ContentType), message.Payload);
                container.Remove(query.First().Key);
            }

            timer.Enabled = container.Count() > 0;
        }

        public void Remove(ushort id)
        {
            container.Remove(id);
            retryContainer.Remove(id);

            timer.Enabled = container.Count > 0;
        }

        public void Clear()
        {
            container.Clear();
            retryContainer.Clear();
            timer.Enabled = false;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var query = container.Where((c) => c.Value.Item2 < DateTime.UtcNow);

            List<ushort> list = new List<ushort>();
            if (query != null && query.Count() > 0)
            {
                foreach(var item in query)
                {
                    list.Add(item.Key);
                }
            }

            foreach(var item in list)
            {
                container.Remove(item);
                if(retryContainer.ContainsKey(item))
                {
                    retryContainer.Remove(item);
                }
            }

            ManageRetries();

            timer.Enabled = container.Count() > 0;
        }

        private void ManageRetries()
        {
            var retryQuery = retryContainer.Where((c) => c.Value.Item1 < DateTime.UtcNow);

            if (retryQuery != null)
            {
                List<ushort> retryUpdateList = new List<ushort>();
                foreach (var item in retryQuery)
                {
                    OnRetry(this, new CoapMessageEventArgs(item.Value.Item3));
                    retryUpdateList.Add(item.Key);
                }

                List<ushort> retryRemoveList = new List<ushort>();
                List<KeyValuePair<ushort, Tuple<DateTime, int, CoapMessage>>> kvpList = new List<KeyValuePair<ushort, Tuple<DateTime, int, CoapMessage>>>();
                foreach (var item in retryUpdateList)
                {
                    Tuple<DateTime, int, CoapMessage> tuple = retryContainer[item];
                    if (tuple.Item2 + 1 == maxAttempts - 1)
                    {
                        retryRemoveList.Add(item);
                    }
                    else
                    {
                        Tuple<DateTime, int, CoapMessage> t = new Tuple<DateTime, int, CoapMessage>(tuple.Item1.AddMilliseconds(retryMilliseconds), tuple.Item2 + 1, tuple.Item3);
                        kvpList.Add(new KeyValuePair<ushort, Tuple<DateTime, int, CoapMessage>>(item, t));
                    }
                }

                foreach (var item in retryRemoveList)
                {
                    retryContainer.Remove(item);
                }

                foreach (var item in kvpList)
                {
                    retryContainer[item.Key] = item.Value;
                }
            }
        }


    }
}
