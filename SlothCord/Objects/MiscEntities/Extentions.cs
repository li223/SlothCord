using SlothCord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SlothCord.Objects
{
    public static class Extensions
    {
        private static List<RateLimitObject> RateLimits = new List<RateLimitObject>()
        {
            new RateLimitObject()
            {
                CurrentLimit = 5,
                InitalLimit = 5,
                Time = 5000,
                RequestType = RequestType.PostMessage,
                LastRequestStamp = DateTimeOffset.Now,
                Type = "channel"
            },
            new RateLimitObject()
            {
                CurrentLimit = 5,
                InitalLimit = 5,
                Time = 1000,
                RequestType = RequestType.DeleteMessage,
                LastRequestStamp = DateTimeOffset.Now,
                Type = "channel"
            },
            new RateLimitObject()
            {
                CurrentLimit = 1,
                InitalLimit = 1,
                Time = 250,
                RequestType = RequestType.DeletePutReaction,
                LastRequestStamp = DateTimeOffset.Now,
                Type = "channel"
            },
            new RateLimitObject()
            {
                CurrentLimit = 10,
                InitalLimit = 10,
                Time = 10000,
                RequestType = RequestType.PatchMember,
                LastRequestStamp = DateTimeOffset.Now,
                Type = "guild"
            },
            new RateLimitObject()
            {
                CurrentLimit = 1,
                InitalLimit = 1,
                Time = 1000,
                RequestType = RequestType.PatchMemberNick,
                LastRequestStamp = DateTimeOffset.Now,
                Type = "guild"
            },
            new RateLimitObject()
            {
                CurrentLimit = 2,
                InitalLimit = 2,
                Time = 3600000,
                RequestType = RequestType.PatchUsername,
                LastRequestStamp = DateTimeOffset.Now,
                Type = "account"
            },
            new RateLimitObject()
            {
                CurrentLimit = 50,
                InitalLimit = 50,
                Time = 1000,
                RequestType = RequestType.Other,
                LastRequestStamp = DateTimeOffset.Now,
                Type = "account"
            }
        };

        public static async Task<HttpResponseMessage> SendRequestAsync(this HttpClient http, HttpRequestMessage msg, RequestType type, CancellationTokenSource cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                HttpResponseMessage httpResponse = null;
                var obj = RateLimits.FirstOrDefault(x => x.RequestType == type);
                RateLimits.Remove(obj);
                var timediff = DateTimeOffset.Now - obj.LastRequestStamp;
                if (timediff.Milliseconds > obj.Time)
                {
                    obj.LastRequestStamp = DateTime.Now;
                    obj.CurrentLimit = obj.InitalLimit;
                }
                if (obj.CurrentLimit <= -1 && timediff.Milliseconds <= obj.Time)
                    httpResponse = await InternalWaitAsync(http, obj.Time, msg).ConfigureAwait(false);
                else
                {
                    obj.CurrentLimit -= 1;
                    httpResponse = await http.SendAsync(msg).ConfigureAwait(false);
                }
                RateLimits.Add(obj);
                return httpResponse;
            }
            else return null;
        }

        private static async Task<HttpResponseMessage> InternalWaitAsync(HttpClient client, int retry_in, HttpRequestMessage msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] ->  Internal Ratelimit Reached, waiting {retry_in}ms");
            Console.ForegroundColor = ConsoleColor.White;
            await Task.Delay(retry_in).ConfigureAwait(false);
            var response = await client.SendAsync(msg).ConfigureAwait(false);
            return response;
        }
    }

    internal struct RateLimitObject
    {
        public int InitalLimit { get; set; }
        public int CurrentLimit { get; set; }
        public int Time { get; set; }
        public string Type { get; set; }
        public RequestType RequestType { get; set; }
        public DateTimeOffset LastRequestStamp { get; set; }
    }

    public enum RequestType
    {
        PostMessage = 0, 
        DeleteMessage = 1,
        DeletePutReaction = 2,
        PatchMember = 3, 
        PatchMemberNick = 4,
        PatchUsername = 5,
        Other = 6,
    }
}
