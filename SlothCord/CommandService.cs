using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SlothCord.Commands
{
    public class CommandService
    {
        internal List<SlothUserCommand> UserDefinedCommands = new List<SlothUserCommand>();
        public void RegisterCommand<T>() where T : new()
        {
            var type = typeof(T);
            var methods = type.GetMethods().Where(x => x.GetCustomAttribute(typeof(CommandAttribute)) != null && x.IsPublic);
            foreach (var method in methods)
            {
                UserDefinedCommands.Add(new SlothUserCommand()
                {
                    ClassInstance = new T(),
                    MethodName = method.Name,
                    Method = method,
                    Parameters = method.GetParameters(),
                    CommandName = method.GetCustomAttribute<CommandAttribute>().CommandName
                });
            }
        }
    }
    public class SlothCommandContext
    {
        public DiscordGuild Guild { get; set; }
        public DiscordChannel Channel { get; set; }
        public DiscordUser User { get; set; }
        public async Task RespondAsync(string text = null, bool is_tts = false, DiscordEmbed embed = null) => await Channel.SendMessageAsync(text, is_tts, embed);
    }
    public class SlothUserCommand
    { 
        public string CommandName { get; set; }
        public object ClassInstance { get; set; }
        public string MethodName { get; set; }
        public ParameterInfo[] Parameters { get; set; }
        public MethodInfo Method { get; set; }
    }
    public class CommandAttribute : Attribute
    {
        public CommandAttribute(string name)
        {
            this.CommandName = name;
        }
        public string CommandName { get; private set; }
    }
    public class RemainingStringAttribute : Attribute { }
}
