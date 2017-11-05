﻿using SkunkLab.Channels;
using SkunkLab.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SkunkLab.Core.Adapters;

namespace SkunkLab.Servers
{
    public class TcpServerListener
    {
        public TcpServerListener(IPEndPoint localEP, CancellationToken token)
        {
            listener = new TcpListener(localEP);
            this.token = token;
            dict = new Dictionary<string, ProtocolAdapter>();
        }

        public TcpServerListener(IPAddress address, int port, CancellationToken token)
        {
            listener = new TcpListener(address, port);
            this.token = token;
            dict = new Dictionary<string, ProtocolAdapter>();
        }

        private TcpListener listener;
        private CancellationToken token;
        private Dictionary<string, ProtocolAdapter> dict;

        public async Task StartAsync()
        {
            listener.ExclusiveAddressUse = false;
            listener.Start();

            while (!token.IsCancellationRequested)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                Task task = Task.Factory.StartNew(async () =>
                {
                    await ManageConnection(client);
                });

                await Task.WhenAll(task);
            }
        }

        public async Task StopAsync()
        {
            listener.Stop();
            await TaskDone.Done;            
        }

        private async Task ManageConnection(TcpClient client)
        {
            ProtocolAdapter adapter = ProtocolAdapterFactory.Create(client, token);
            dict.Add(adapter.Channel.Id, adapter);
            adapter.OnError += Adapter_OnError;
            adapter.OnClose += Adapter_OnClose;
            await adapter.Channel.OpenAsync();
            await adapter.Channel.ReceiveAsync();
        }

        private void Adapter_OnClose(object sender, ProtocolAdapterCloseEventArgs args)
        {
            Trace.TraceWarning("Protocol adapter on channel {0} closing.", args.ChannelId);
            if (dict.ContainsKey(args.ChannelId))
            {
                ProtocolAdapter adapter = dict[args.ChannelId];
                Task task = adapter.Channel.CloseAsync();
                Task.WaitAll(task);
                adapter.Dispose();
                dict.Remove(args.ChannelId);
            }
        }

        private void Adapter_OnError(object sender, ProtocolAdapterErrorEventArgs args)
        {
            Trace.TraceError("Protocol Adapter on channel {0} threw error {1}", args.ChannelId, args.Error.Message);

            if (dict.ContainsKey(args.ChannelId))
            {
                ProtocolAdapter adapter = dict[args.ChannelId];
                Task task =  adapter.Channel.CloseAsync();
                Task.WaitAll(task);
                adapter.Dispose();
                dict.Remove(args.ChannelId);
            }


        }
    }
}
