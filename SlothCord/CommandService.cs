﻿using Newtonsoft.Json;
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
        public event OnCommandError CommandErrored;
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
        internal async Task ConvertArgumentsAsync(string prefix, DiscordClient client, DiscordMessage msg)
        {
            List<object> Args = new List<object>();
            Args.AddRange(msg.Content.Replace(prefix, "").Split(' ').ToList());
            var group = UserDefinedGroups.FirstOrDefault(x => x.GroupName == (string)Args[0]);
            if (group == null)
            {
                var cmd = UserDefinedCommands.FirstOrDefault(x => x.CommandName == (Args[0] as string));
                if (cmd == null)
                {
                    CommandErrored?.Invoke(this, "Command does not exist");
                    return;
                }
                Args.Remove(Args[0]);
                await ExecuteCommandAsync(client, msg, Args, cmd);
            }
            else
            {
                Args.Remove(Args[0]);
                var cmd = group.Commands?.FirstOrDefault(x => x.CommandName == (Args[0] as string));
                if (cmd == null)
                {
                    if (!group.InvokeWithoutSubCommand)
                    {
                        CommandErrored?.Invoke(this, "Command does not exist");
                        return;
                    }
                    else
                    {
                        if (group.GroupExecuteCommand == null || group.GroupExecuteCommand.Method == null)
                        {
                            CommandErrored?.Invoke(this, "Command does not exist and no ExecuteAsync method exists");
                            return;
                        }
                        else cmd = group.GroupExecuteCommand;
                    }
                }
                if (cmd != group.GroupExecuteCommand) Args.Remove(Args[0]);

                await ExecuteCommandAsync(client, msg, Args, cmd);
            }
        }
        public Task ExecuteCommandAsync(DiscordClient client, DiscordMessage msg, List<object> Args, SlothUserCommand cmd)
        {
            var guild = client.Guilds.FirstOrDefault(x => x.Channels.Any(a => a.Id == msg.ChannelId));
            var channel = guild.Channels.First(x => x.Id == msg.ChannelId);
            var passargs = new List<object>();
            int checkammount = 0;
            var countval = cmd.Parameters.Count();
            if (cmd.Parameters.FirstOrDefault()?.ParameterType == typeof(SlothCommandContext))
            {
                passargs.Add(new SlothCommandContext()
                {
                    Channel = channel,
                    Guild = guild,
                    User = msg.Author,
                    Client = client
                });
                countval--;
            }
            for (var i = 0; i < countval; i++)
            {
                checkammount = (passargs.Count - 1);
                object currentarg = (Args.Count > checkammount) ? Args[i] : null;
                if (currentarg != null)
                {
                    if (new Regex(@"(<@(?:!)\d+>)").IsMatch(currentarg as string))
                    {
                        var strid = new Regex(@"((<@)(?:!))").Replace(Args[i] as string, "").Replace(">", "");
                        var id = ulong.Parse(strid);
                        var member = guild.Members.FirstOrDefault(x => x.UserData.Id == id);
                        var cachedUser = client.InternalUserCache?.FirstOrDefault(x => x.Id == id);
                        if (member != null && currentarg.GetType() == typeof(DiscordGuildMember)) currentarg = member;
                        else if (cachedUser != null) currentarg = cachedUser;
                    }
                    else
                    {
                        var type = cmd.Parameters[i].ParameterType;
                        if (type != typeof(SlothCommandContext))
                            currentarg = Convert.ChangeType(Args[i], type);
                    }
                }
                var check = cmd.Parameters[i].CustomAttributes.Any(y => y.AttributeType == typeof(RemainingStringAttribute));
                if ((bool)check)
                {
                    var sb = new StringBuilder();
                    for (var o = 0; o < Args.Count; o++)
                        if (Args.IndexOf(Args[o]) >= Args.IndexOf(Args[i]))
                            sb.Append($" {Args[o]}");
                    passargs.Add(sb.ToString());
                    break;
                }
                passargs.Add(currentarg);
            }
            try
            {
                cmd.Method.Invoke(cmd.ClassInstance, passargs.ToArray());
            }
            catch (TargetParameterCountException)
            {
                CommandErrored?.Invoke(this, "Required parameter does not have a value");
            }
            catch(Exception ex)
            {
                CommandErrored?.Invoke(this, ex.Message);
            }
            return Task.CompletedTask;
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
