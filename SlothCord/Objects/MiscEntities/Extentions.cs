using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SlothCord.Objects
{
    public static class Extentions
    {
        private static int _ratelimit { get; set; }
        private static RequestType _prevType { get; set; }
        private static DateTimeOffset _lastRequestStamp { get; set; }
        public static async Task<HttpResponseMessage> SendRequestAsync(this HttpClient http, ApiBase apibase, HttpRequestMessage msg, RequestType type)
        {
            switch(type)
            {
                case RequestType.CREATE_MESSAGE:
                    {
                        if (_prevType != RequestType.CREATE_MESSAGE)
                        {
                            _prevType = RequestType.CREATE_MESSAGE;
                            _ratelimit = 5;
                        }
                        else _ratelimit.Key -= 1;
                        var timediff = DateTimeOffset.Now - _lastRequestStamp;
                        if (timediff.Seconds > 5)
                        {
                            _lastRequestStamp = DateTime.Now;
                            _ratelimit = 5;
                        }
                        if (_ratelimit.Key == -1 && timediff.Seconds < 5) return await apibase.RetryAsync(5000, msg).ConfigureAwait(false);
                        else return await http.SendAsync(msg);
                    }
            }
            var response = await http.SendAsync(msg).ConfigureAwait(false);
            return response;
        }
    }
    public enum RequestType
    {
        CREATE_MESSAGE = 0
    }
}
