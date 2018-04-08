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
        internal List<SlothUserCommandGroup> UserDefinedGroups = new List<SlothUserCommandGroup>();

        public void RegisterCommand<T>() where T : new()
        {
            var type = typeof(T);
            var groups = type.GetNestedTypes().Where(x => x.GetCustomAttribute<GroupAttribute>() != null);
            foreach (var group in groups)
            {
                var groupatt = group.GetCustomAttribute<GroupAttribute>();
                var methods = group.GetMethods();
                var exemeth = methods.FirstOrDefault(x => x.Name == "ExecuteAsync");
                List<SlothUserCommand> cmds = new List<SlothUserCommand>();
                for (var count = 0; count < group.GetMethods().Count(); count++)
                    if (methods[count].HasAttribute<CommandAttribute>())
                        cmds.Add(new SlothUserCommand()
                        {
                            ClassInstance = Activator.CreateInstance(group),
                            MethodName = methods[count].Name,
                            Method = methods[count],
                            CommandName = methods[count].GetCustomAttribute<CommandAttribute>().CommandName,
                            Parameters = methods[count].GetParameters()
                        });
                UserDefinedGroups.Add(new SlothUserCommandGroup()
                {
                    InvokeWithoutSubCommand = groupatt.InvokeWithoutSubCommand,
                    GroupName = groupatt.GroupName,
                    Commands = cmds,
                    GroupExecuteCommand = (groupatt.InvokeWithoutSubCommand) ? new SlothUserCommand()
                    {
                        ClassInstance = Activator.CreateInstance(group),
                        MethodName = exemeth?.Name,
                        Method = exemeth,
                        Parameters = exemeth?.GetParameters()
                    } : null
                });
            }
            UserDefinedCommands.AddRange(type.GetMethods().Where(x => x.GetCustomAttribute<CommandAttribute>() != null && x.IsPublic).Select(x => new SlothUserCommand()
            {
                ClassInstance = new T(),
                MethodName = x.Name,
                Method = x,
                Parameters = x.GetParameters(),
                CommandName = x.GetCustomAttribute<CommandAttribute>().CommandName
            }));
        }
    }
            
    public class SlothCommandContext : ApiBase
    {
        public DiscordGuild Guild { get; internal set; }
        public DiscordChannel Channel { get; internal set; }
        public DiscordUser User { get; internal set; }
        public DiscordClient Client { get; internal set; }
        public async Task RespondAsync(string text = null, bool is_tts = false, DiscordEmbed embed = null) => await Channel.SendMessageAsync(text, is_tts, embed);
        public async Task<DiscordUser> GetUserAsync(ulong user_id)
        {
            var response = await _httpClient.GetAsync(new Uri($"{_baseAddress}/users/{user_id}"));
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<DiscordUser>(content);
            else return null;
        }
    }

    public class SlothUserCommand
    { 
        public string CommandName { get; internal set; }
        public object ClassInstance { get; internal set; }
        public string MethodName { get; internal set; }
        public ParameterInfo[] Parameters { get; internal set; }
        public MethodInfo Method { get; internal set; }
    }

    public class SlothUserCommandGroup
    {
        public bool InvokeWithoutSubCommand { get; internal set; }
        public SlothUserCommand GroupExecuteCommand { get; internal set; }
        public string GroupName { get; internal set; }
        public IReadOnlyList<SlothUserCommand> Commands { get; internal set; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        public CommandAttribute(string name)
        {
            this.CommandName = name;
        }
        public string CommandName { get; private set; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class GroupAttribute : Attribute
    {
        public GroupAttribute(string name, bool CanInvokeWithoutSubCommand = false)
        {
            this.GroupName = name;
            this.InvokeWithoutSubCommand = CanInvokeWithoutSubCommand;
        }
        public string GroupName { get; private set; }
        public bool InvokeWithoutSubCommand { get; private set; }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class RemainingStringAttribute : Attribute { }
    public static class Extensions
    {
        public static bool HasAttribute<T>(this MethodInfo method)
        {
            return method.CustomAttributes.Any(x => x.AttributeType == typeof(T));
        }
    }
}
