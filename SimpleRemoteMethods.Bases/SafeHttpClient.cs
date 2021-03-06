﻿using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleRemoteMethods.Bases
{
    public class SafeHttpClient
    {
        private readonly DefferedDisposeTracker _disposeTracker = new DefferedDisposeTracker();
        private readonly object _getClientLocker = new object();
        private readonly object _recreateClientLocker = new object();
        private HttpClient _client;
        private DateTime _clientCreateDateTime;
        private bool _clientRecreatingNow = false;

        public Uri Uri { get; }
        public TimeSpan ConnectionTimeout { get; }
        public TimeSpan LeaseTimeout { get; }

        public SafeHttpClient(Uri uri, TimeSpan connectionTimeout, TimeSpan leaseTimeout)
        {
            if (leaseTimeout < TimeSpan.FromMinutes(1))
            {
                throw new ArgumentException("LeaseTimeout cannot be less than 1 minute");
            }

            Uri = uri;
            ConnectionTimeout = connectionTimeout;
            LeaseTimeout = leaseTimeout;
        }

        public async Task<HttpResponseMessage> SendAsync(byte[] content, Action<HttpRequestMessage> requestPrepared = null)
        {
            try
            {
                return await SendAsyncInternal(content, requestPrepared);
            }
            catch (HttpRequestException e)
                when (e.InnerException is WebException we &&
                        (we.Status == WebExceptionStatus.NameResolutionFailure ||
                         we.Status == WebExceptionStatus.Timeout ||
                         we.Status == WebExceptionStatus.ConnectFailure))
            {
                RecreateClient();
                return await SendAsyncInternal(content, requestPrepared);
            }
        }

        private async Task<HttpResponseMessage> SendAsyncInternal(byte[] content, Action<HttpRequestMessage> requestPrepared = null)
        {
            var client = GetClient();
            try
            {
                _disposeTracker.BeginUse(client);
                using (var message = new HttpRequestMessage(HttpMethod.Post, Uri))
                using (var ms = new MemoryStream(content))
                using (var httpContent = new StreamContent(ms))
                {
                    requestPrepared?.Invoke(message);
                    message.Content = httpContent;
                    return await client.SendAsync(message);
                }
            }
            finally
            {
                _disposeTracker.EndUse(client);
            }
        }

        private HttpClient GetClient()
        {
            lock (_getClientLocker)
            {
                if (_client == null || DateTime.Now - _clientCreateDateTime > LeaseTimeout)
                {
                    RecreateClient();
                }

                return _client;
            }
        }

        private void RecreateClient()
        {
            if (_clientRecreatingNow)
            {
                lock (_recreateClientLocker)
                {
                    return;
                }
            }

            lock (_recreateClientLocker)
            {
                _clientRecreatingNow = true;

                if (_client != null)
                {
                    _disposeTracker.DisposeWhenUseIsComplete(_client);
                }

                _client = new HttpClient();
                _client.Timeout = ConnectionTimeout;
                _clientCreateDateTime = DateTime.Now;

                _clientRecreatingNow = false;
            }
        }
    }
}