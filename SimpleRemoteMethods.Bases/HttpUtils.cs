using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SimpleRemoteMethods.Bases
{
    public static class HttpUtils
    {
        public static async Task<HttpResponseMessage> SendRequest(HttpClient client, Uri uri, byte[] content, Action<HttpRequestMessage> requestPrepared = null)
        {
            try
            {
                var message = new HttpRequestMessage(HttpMethod.Post, uri);
                message.Content = new StreamContent(new MemoryStream(content));
                requestPrepared?.Invoke(message);
                var response = await client.SendAsync(message);
                return response;
            }
            catch (Exception e)
            {
                throw RemoteException.Get(RemoteExceptionData.ConnectionError, "/", e);
            }
        }

        public static async Task<Response> SendRequest(HttpClient client, Uri uri, Request request, string secretKey, Action<HttpRequestMessage> requestPrepared = null, Action<HttpResponseMessage> responseReceived = null)
        {
            var encrypted = new Encrypted<Request>(request, secretKey);
            using (var httpResponse = await SendRequest(client, uri, encrypted.Data, requestPrepared))
            {
                responseReceived?.Invoke(httpResponse);

                var content = await httpResponse.Content.ReadAsByteArrayAsync();
                if (content == null || content.Length == 0)
                    throw RemoteException.Get(RemoteExceptionData.UnknownData);

                if (Encrypted<Response>.IsClass(content))
                {
                    var encryptedResponse = new Encrypted<Response>(content);
                    var response = encryptedResponse.Decrypt(secretKey);
                    if (response == null)
                        throw RemoteException.Get(RemoteExceptionData.UnknownData);
                    return response;
                }
                else if (Encrypted<ErrorResponse>.IsClass(content))
                {
                    var encryptedErrorData = new Encrypted<ErrorResponse>(content);
                    var errorResponse = encryptedErrorData.Decrypt(secretKey);
                    if (errorResponse == null)
                        throw RemoteException.Get(RemoteExceptionData.UnknownData);
                    throw RemoteException.Get(errorResponse.ErrorData);
                }

                throw RemoteException.Get(RemoteExceptionData.UnknownData);
            }
        }

        public static async Task<UserTokenResponse> SendUserTokenRequest(HttpClient client, Uri uri, UserTokenRequest request, string secretKey, Action<HttpRequestMessage> requestPrepared = null, Action<HttpResponseMessage> responseReceived = null)
        {
            var encrypted = new Encrypted<UserTokenRequest>(request, secretKey);
            using (var httpResponse = await SendRequest(client, uri, encrypted.Data, requestPrepared))
            {
                responseReceived?.Invoke(httpResponse);

                var content = await httpResponse.Content.ReadAsByteArrayAsync();
                if (content == null)
                    throw RemoteException.Get(RemoteExceptionData.UnknownData);

                if (Encrypted<UserTokenResponse>.IsClass(content))
                {
                    var encryptedResponse = new Encrypted<UserTokenResponse>(content);
                    var response = encryptedResponse.Decrypt(secretKey);
                    if (response == null)
                        throw RemoteException.Get(RemoteExceptionData.UnknownData);
                    return response;
                }
                else if (Encrypted<ErrorResponse>.IsClass(content))
                {
                    var encryptedErrorData = new Encrypted<ErrorResponse>(content);
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
