﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SimpleRemoteMethods.Bases
{
    public static class HttpUtils
    {
        public static async Task<HttpResponseMessage> SendRequest(Uri uri, string content)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Content = new StringContent(content);
            var response = await client.SendAsync(request);
            return response;
        }

        public static async Task<Response> SendRequest(Uri uri, Request request, string secretKey)
        {
            var encrypted = new Encrypted<Request>(request, secretKey);
            var httpResponse = await SendRequest(uri, encrypted.ToString());
            var content = await (httpResponse.Content as StringContent)?.ReadAsStringAsync();
            if (content == null)
                throw RemoteException.Get(RemoteExceptionData.UnknownData);
            var encryptedResponse = Encrypted<Response>.FromString(content);
            var response = encryptedResponse.Decrypt(secretKey);
            if (response == null)
                throw RemoteException.Get(RemoteExceptionData.UnknownData);
            if (response.RemoteException != null)
                throw RemoteException.Get(response.RemoteException);
            return response;
        }

        public static async Task<UserTokenResponse> SendUserTokenRequest(Uri uri, UserTokenRequest request, string secretKey)
        {
            var encrypted = new Encrypted<UserTokenRequest>(request, secretKey);
            var httpResponse = await SendRequest(uri, encrypted.ToString());
            var content = await (httpResponse.Content as StringContent)?.ReadAsStringAsync();
            if (content == null)
                throw RemoteException.Get(RemoteExceptionData.UnknownData);
            var encryptedResponse = Encrypted<UserTokenResponse>.FromString(content);
            var response = encryptedResponse.Decrypt(secretKey);
            if (response == null)
                throw RemoteException.Get(RemoteExceptionData.UnknownData);
            if (response.RemoteException != null)
                throw RemoteException.Get(response.RemoteException);
            return response;
        }
    }
}