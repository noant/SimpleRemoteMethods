using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace SimpleRemoteMethods.ServerSide
{
    /// <summary>
    /// Class for tracking and checking request id
    /// </summary>
    public class RequestIdChecker
    {
        private List<string> _requestIds = new List<string>();
        private string _requestIdsPath;

        public RequestIdChecker()
        {
            var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _requestIdsPath = Path.Combine(assemblyFolder, "requestIds");
            if (File.Exists(_requestIdsPath))
                _requestIds.AddRange(File.ReadLines(_requestIdsPath));
        }

        /// <summary>
        /// Checke where is requeest was executed
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns></returns>
        public virtual bool IsNewRequest(string requestId)
        {
            if (!_requestIds.Contains(requestId))
            {
                AppendRequestId(requestId);
                return true;
            }
            return false;
        }

        protected virtual void AppendRequestId(string requestId)
        {
            _requestIds.Add(requestId);
            lock (_requestIds)
            {
                if (_requestIds.Count > 10000)
                {
                    _requestIds.RemoveRange(0, 5000);
                    File.WriteAllText(_requestIdsPath, requestId);
                }
                else
                    File.AppendAllLines(_requestIdsPath, new[] { requestId });
            }
        }
    }
}
