using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SlothCord.Objects
{
    public static class Extensions
    {
        private static int _ratelimit { get; set; }
        private static DispatchType _prevType { get; set; }
        private static DateTimeOffset _lastRequestStamp { get; set; }

        public static async Task<string> SendRequestAsync(this HttpClient http, ApiBase apibase, HttpRequestMessage msg, DispatchType type, object postedto = null)
        {

            switch (type)
            {
                case DispatchType.MessageCreate:
                    {
                        string content;
                        if (_prevType != DispatchType.MessageCreate)
                        {
                            _prevType = DispatchType.MessageCreate;
                            _ratelimit = 5;
                        }
                        else _ratelimit -= 1;
                        var timediff = DateTimeOffset.Now - _lastRequestStamp;
                        if (timediff.Seconds > 5)
                        {
                            _lastRequestStamp = DateTime.Now;
                            _ratelimit = 5;
                        }
                        if (_ratelimit == -1 && timediff.Seconds < 5)
                            content = await apibase.RetryAsync(5000, msg).ConfigureAwait(false);
                        else
                        {
                            var response = await http.SendAsync(msg).ConfigureAwait(false);
                            content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            return content;
                        }
                        return content;
                    }
                default: return "";
            }
        }
    }

    internal struct RateLimits
    {
        const int MessageCreate = 5;
    }

    internal struct Ratelimit
    {
        public string PerType { get; set; }
        public HttpMethod MethodType { get; set; }
        public string EndpointName { get; set; }
        public KeyValuePair<int, int> Rate { get; set; }
    }
}
