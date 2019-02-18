using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleRemoteMethods.Bases
{
    public static class HttpUtils
    {
        public static async Task<HttpResponseMessage> SendRequest(SafeHttpClient client, byte[] content, Action<HttpRequestMessage> requestPrepared = null)
        {
            try
            {
                var message = new HttpRequestMessage(HttpMethod.Post, client.Uri);
                message.Content = new StreamContent(new MemoryStream(content));
                requestPrepared?.Invoke(message);
                var response = await client.SendAsync(message);
                return response;
            }
            catch (Exception e)
            {
                throw new RemoteException(ErrorCode.ConnectionError, "/", e);
            }
        }

        public static async Task<Response> SendRequest(SafeHttpClient client, Request request, string secretKey, Action<HttpRequestMessage> requestPrepared = null, Action<HttpResponseMessage> responseReceived = null)
        {
            var encrypted = new Encrypted<Request>(request, secretKey);
            using (var httpResponse = await SendRequest(client, encrypted.Data, requestPrepared))
            {
                responseReceived?.Invoke(httpResponse);

                if (httpResponse.StatusCode == HttpStatusCode.BadRequest)
                {
                    throw new RemoteException(ErrorCode.UnknownData);
                }

                if (httpResponse.StatusCode == HttpStatusCode.Unused)
                {
                    throw new RemoteException(ErrorCode.DecryptionErrorCode);
                }

                var content = await httpResponse.Content.ReadAsByteArrayAsync();
                if (content == null || content.Length == 0)
                {
                    throw new RemoteException(ErrorCode.UnknownData);
                }

                if (Encrypted<Response>.IsClass(content))
                {
                    var encryptedResponse = new Encrypted<Response>(content);
                    var response = encryptedResponse.Decrypt(secretKey);
                    if (response == null)
                    {
                        throw new RemoteException(ErrorCode.UnknownData);
                    }

                    return response;
                }
                else if (Encrypted<ErrorResponse>.IsClass(content))
                {
                    var encryptedErrorData = new Encrypted<ErrorResponse>(content);
                    var errorResponse = encryptedErrorData.Decrypt(secretKey);
                    if (errorResponse == null)
                    {
                        throw new RemoteException(ErrorCode.UnknownData);
                    }

                    throw new RemoteException(errorResponse.ErrorData);
                }

                throw new RemoteException(ErrorCode.UnknownData);
            }
        }

        public static async Task<UserTokenResponse> SendUserTokenRequest(SafeHttpClient client, UserTokenRequest request, string secretKey, Action<HttpRequestMessage> requestPrepared = null, Action<HttpResponseMessage> responseReceived = null)
        {
            var encrypted = new Encrypted<UserTokenRequest>(request, secretKey);
            using (var httpResponse = await SendRequest(client, encrypted.Data, requestPrepared))
            {
                responseReceived?.Invoke(httpResponse);

                if (httpResponse.StatusCode == HttpStatusCode.BadRequest)
                {
                    throw new RemoteException(ErrorCode.UnknownData);
                }

                if (httpResponse.StatusCode == HttpStatusCode.Unused)
                {
                    throw new RemoteException(ErrorCode.DecryptionErrorCode);
                }

                var content = await httpResponse.Content.ReadAsByteArrayAsync();
                if (content == null)
                {
                    throw new RemoteException(ErrorCode.UnknownData);
                }

                if (Encrypted<UserTokenResponse>.IsClass(content))
                {
                    var encryptedResponse = new Encrypted<UserTokenResponse>(content);
                    var response = encryptedResponse.Decrypt(secretKey);
                    if (response == null)
                    {
                        throw new RemoteException(ErrorCode.UnknownData);
                    }

                    return response;
                }
                else if (Encrypted<ErrorResponse>.IsClass(content))
                {
                    var encryptedErrorData = new Encrypted<ErrorResponse>(content);
                    var errorResponse = encryptedErrorData.Decrypt(secretKey);
                    if (errorResponse == null)
                    {
                        throw new RemoteException(ErrorCode.UnknownData);
                    }

                    throw new RemoteException(errorResponse.ErrorData);
                }

                throw new RemoteException(ErrorCode.UnknownData);
            }
        }
    }
}