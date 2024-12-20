using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;

namespace ET
{
    public class WService: AService
    {
        private long idGenerate = 200000000;

        private HttpListener httpListener;

        private readonly Dictionary<long, WChannel> channels = new();

        public ThreadSynchronizationContext ThreadSynchronizationContext;

        public WService(string prefix)
        {
            this.ThreadSynchronizationContext = new ThreadSynchronizationContext();
            this.httpListener = new HttpListener();
            StartAccept(prefix).NoContext();
        }

        public WService()
        {
            this.ServiceType = ServiceType.Outer;
            this.ThreadSynchronizationContext = new ThreadSynchronizationContext();
        }

        private long GetId
        {
            get
            {
                return ++this.idGenerate;
            }
        }

        public override void Create(long id, IPEndPoint ipEndpoint)
        {
            ClientWebSocket webSocket = new();
            WChannel channel = new(id, webSocket, ipEndpoint, this);
            this.channels[channel.Id] = channel;
        }

        public override void Update()
        {
            this.ThreadSynchronizationContext.Update();
        }

        public override void Remove(long id, int error = 0)
        {
            if (!this.channels.TryGetValue(id, out WChannel channel))
            {
                return;
            }

            channel.Error = error;
            this.channels.Remove(id);
            channel.Dispose();
        }

        public override bool IsDisposed()
        {
            return this.ThreadSynchronizationContext == null;
        }

        protected void Get(long id, IPEndPoint ipEndPoint)
        {
            if (!this.channels.TryGetValue(id, out _))
            {
                this.Create(id, ipEndPoint);
            }
        }

        public WChannel Get(long id)
        {
            this.channels.TryGetValue(id, out WChannel channel);
            return channel;
        }

        public override void Dispose()
        {
            base.Dispose();

            this.ThreadSynchronizationContext = null;
            this.httpListener?.Close();
            this.httpListener = null;
        }

        private async ETTask StartAccept(string prefix)
        {
            try
            {
                this.httpListener.Prefixes.Add(prefix);
                httpListener.Start();
                while (true)
                {
                    try
                    {
                        HttpListenerContext httpListenerContext = await this.httpListener.GetContextAsync();
                        HttpListenerWebSocketContext webSocketContext = await httpListenerContext.AcceptWebSocketAsync(null);
                        WChannel channel = new(this.GetId, webSocketContext, this);
                        channel.RemoteAddress = httpListenerContext.Request.RemoteEndPoint;
                        this.channels[channel.Id] = channel;

                        this.AcceptCallback(channel.Id, channel.RemoteAddress);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                }
            }
            catch (HttpListenerException e)
            {
                if (e.ErrorCode == 5)
                {
                    throw new Exception($"请先在cmd中运行: netsh http add urlacl url=http://*:你的address中的端口/ user=Everyone, address: {prefix}", e);
                }

                Log.Error($"{prefix} {e}");
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public override void Send(long channelId, MemoryBuffer memoryBuffer)
        {
            this.channels.TryGetValue(channelId, out WChannel channel);
            if (channel == null)
            {
                return;
            }

            channel.Send(memoryBuffer);
        }
    }
}