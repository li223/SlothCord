using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SlothCord.Objects
{
    public static class Extensions
    {
        private static int _ratelimit { get; set; }
        private static DispatchType _prevType { get; set; }
        private static DateTimeOffset _lastRequestStamp { get; set; }

        public static async Task<HttpResponseMessage> SendRequestAsync(this HttpClient http, HttpRequestMessage msg, DispatchType type)
        {
            HttpResponseMessage httpResponse = null;
            switch (type)
            {
                case DispatchType.MessageCreate:
                    {
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
                            httpResponse = await InternalWaitAsync(http, 5000, msg).ConfigureAwait(false);
                        else httpResponse = await http.SendAsync(msg).ConfigureAwait(false);
                        break;
                    }

                case DispatchType.MessageDelete:
                    {
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
                            httpResponse = await InternalWaitAsync(http, 1000, msg).ConfigureAwait(false);
                        else httpResponse = await http.SendAsync(msg).ConfigureAwait(false);
                        break;
                    }
            }
            return httpResponse;
        }

        static async Task<HttpResponseMessage> InternalWaitAsync(HttpClient client, int retry_in, HttpRequestMessage msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] ->  Internal Ratelimit Reached, waiting {retry_in}ms");
            Console.ForegroundColor = ConsoleColor.White;
            await Task.Delay(retry_in).ConfigureAwait(false);
            var response = await client.SendAsync(msg).ConfigureAwait(false);
            return response;
        }
    }
}
