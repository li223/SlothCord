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

        public static async Task<string> SendRequestAsync(this HttpClient http, ApiBase apibase, HttpRequestMessage msg, DispatchType type)
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
                        if (_ratelimit <= -1 && timediff.Seconds <= 5)
                            content = await apibase.RetryAsync(5000, msg).ConfigureAwait(false);
                        else
                        {
                            var response = await http.SendAsync(msg).ConfigureAwait(false);
                            content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            return content;
                        }
                        return content;
                    }
                case DispatchType.MessageDelete:
                    {
                        string content;
                        if (_prevType != DispatchType.MessageDelete)
                        {
                            _prevType = DispatchType.MessageDelete;
                            _ratelimit = 5;
                        }
                        else _ratelimit -= 1;
                        var timediff = DateTimeOffset.Now - _lastRequestStamp;
                        if (timediff.Seconds > 1)
                        {
                            _lastRequestStamp = DateTime.Now;
                            _ratelimit = 5;
                        }
                        if (_ratelimit <= -1 && timediff.Seconds <= 1)
                            content = await apibase.RetryAsync(1000, msg).ConfigureAwait(false);
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
}