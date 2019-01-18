﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SimpleRemoteMethods.Bases
{
    public static class HttpUtils
    {
        public static async Task<HttpResponseMessage> SendRequest(HttpClient client, Uri uri, string content)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, uri);
                request.Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(content)));
                request.Method = HttpMethod.Post;
                var response = await client.SendAsync(request);
                return response;
            }
            catch (Exception e)
            {
                throw RemoteException.Get(RemoteExceptionData.ConnectionError, string.Empty, e);
            }
        }

        public static async Task<Response> SendRequest(HttpClient client, Uri uri, Request request, string secretKey)
        {
            var encrypted = new Encrypted<Request>(request, secretKey);
            using (var httpResponse = await SendRequest(client, uri, encrypted.ToString()))
            {
                var content = await (httpResponse.Content as StreamContent)?.ReadAsStringAsync();
                if (content == null)
                    throw RemoteException.Get(RemoteExceptionData.UnknownData);

                if (Encrypted<Response>.IsClass(content))
                {
                    var encryptedResponse = Encrypted<Response>.FromString(content);
                    var response = encryptedResponse.Decrypt(secretKey);
                    if (response == null)
                        throw RemoteException.Get(RemoteExceptionData.UnknownData);
                    return response;
                }
                else if (Encrypted<ErrorResponse>.IsClass(content))
                {
                    var encryptedErrorData = Encrypted<ErrorResponse>.FromString(content);
                    var errorResponse = encryptedErrorData.Decrypt(secretKey);
                    if (errorResponse == null)
                        throw RemoteException.Get(RemoteExceptionData.UnknownData);
                    throw RemoteException.Get(errorResponse.ErrorData);
                }

                throw RemoteException.Get(RemoteExceptionData.UnknownData);
            }
        }

        public static async Task<UserTokenResponse> SendUserTokenRequest(HttpClient client, Uri uri, UserTokenRequest request, string secretKey)
        {
            var encrypted = new Encrypted<UserTokenRequest>(request, secretKey);
            using (var httpResponse = await SendRequest(client, uri, encrypted.ToString()))
            {
                var content = await (httpResponse.Content as StreamContent)?.ReadAsStringAsync();
                if (content == null)
                    throw RemoteException.Get(RemoteExceptionData.UnknownData);

                if (Encrypted<UserTokenResponse>.IsClass(content))
                {
                    var encryptedResponse = Encrypted<UserTokenResponse>.FromString(content);
                    var response = encryptedResponse.Decrypt(secretKey);
                    if (response == null)
                        throw RemoteException.Get(RemoteExceptionData.UnknownData);
                    return response;
                }
                else if (Encrypted<ErrorResponse>.IsClass(content))
                {
                    var encryptedErrorData = Encrypted<ErrorResponse>.FromString(content);
                    var errorResponse = encryptedErrorData.Decrypt(secretKey);
                    if (errorResponse == null)
                        throw RemoteException.Get(RemoteExceptionData.UnknownData);
                    throw RemoteException.Get(errorResponse.ErrorData);
                }

                throw RemoteException.Get(RemoteExceptionData.UnknownData);
            }
        }
    }
}
