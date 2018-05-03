﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SlothCord.Objects;

namespace SlothCord.Commands
{
    public class CommandService
    {
        /// <summary>
        /// Sets if commands can be ran via direct messages
        /// </summary>
        public bool AllowDmCommands { get; set; } = true;

        /// <summary>
        /// Service collection
        /// </summary>
        public IServiceProvider Services { get; set; }

        /// <summary>
        /// Prefix for commands
        /// </summary>
        public string StringPrefix { get; set; }

        public event OnCommandError CommandErrored;

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
                {
                    if (methods[count].HasAttribute<CommandAttribute>())
                        cmds.Add(new SlothUserCommand()
                        {
                            ClassInstance = Activator.CreateInstance(group),
                            MethodName = methods[count].Name,
                            Method = methods[count],
                            CommandName = methods[count].GetCustomAttribute<CommandAttribute>().CommandName,
                            Parameters = methods[count].GetParameters(),
                            CommandNameAliases = methods[count].GetCustomAttribute<AliasesAttribute>()?.Aliases ?? null
                        });
                }

                UserDefinedGroups.Add(new SlothUserCommandGroup()
                {
                    InvokeWithoutSubCommand = groupatt.InvokeWithoutSubCommand,
                    GroupName = groupatt.GroupName,
                    Commands = cmds,
                    GroupNameAliases = group.GetCustomAttribute<AliasesAttribute>()?.Aliases ?? null,
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

        public void RegisterCommand(object obj)
        {
            var type = obj as Type;
            var groups = type.GetNestedTypes().Where(x => x.GetCustomAttribute<GroupAttribute>() != null);
            foreach (var group in groups)
            {
                var groupatt = group.GetCustomAttribute<GroupAttribute>();
                var methods = group.GetMethods();
                var exemeth = methods.FirstOrDefault(x => x.Name == "ExecuteAsync");

                List<SlothUserCommand> cmds = new List<SlothUserCommand>();
                for (var count = 0; count < group.GetMethods().Count(); count++)
                {
                    if (methods[count].HasAttribute<CommandAttribute>())
                        cmds.Add(new SlothUserCommand()
                        {
                            ClassInstance = Activator.CreateInstance(group),
                            MethodName = methods[count].Name,
                            Method = methods[count],
                            CommandName = methods[count].GetCustomAttribute<CommandAttribute>().CommandName,
                            Parameters = methods[count].GetParameters(),
                            CommandNameAliases = methods[count].GetCustomAttribute<AliasesAttribute>()?.Aliases ?? null
                        });
                }

                UserDefinedGroups.Add(new SlothUserCommandGroup()
                {
                    InvokeWithoutSubCommand = groupatt.InvokeWithoutSubCommand,
                    GroupName = groupatt.GroupName,
                    Commands = cmds,
                    GroupNameAliases = group.GetCustomAttribute<AliasesAttribute>()?.Aliases ?? null,
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
                ClassInstance = Activator.CreateInstance(type),
                MethodName = x.Name,
                Method = x,
                Parameters = x.GetParameters(),
                CommandName = x.GetCustomAttribute<CommandAttribute>().CommandName
            }));

        }

        internal List<SlothUserCommand> UserDefinedCommands = new List<SlothUserCommand>();

        internal List<SlothUserCommandGroup> UserDefinedGroups = new List<SlothUserCommandGroup>();

        internal async Task ConvertArgumentsAsync(DiscordClient client, DiscordMessage msg)
        {
            var guild = client.Guilds.FirstOrDefault(x => x.Channels.Any(a => a.Id == msg.ChannelId));
            var channel = guild.Channels.FirstOrDefault(x => x.Id == msg.ChannelId);
            var member = guild.Members.FirstOrDefault(x => x.UserData.Id == msg.Author.Id);
            var roles = new List<DiscordRole>();
            foreach (var id in member.RoleIds)
                roles.Add(guild.Roles.FirstOrDefault(x => x.Id == id));
            member.Roles = roles;
            var context = new SlothCommandContext()
            {
                Message = msg,
                Channel = channel,
                Guild = guild,
                User = msg.Author,
                Client = client,
                Member = member,
                Services = this.Services
            };
            List<object> Args = new List<object>();
            Args.AddRange(msg.Content.Replace(this.StringPrefix, "").Split(' ').ToList());
            var group = UserDefinedGroups.FirstOrDefault(x => x.GroupName == (string)Args[0]);
            if (group == null) group = UserDefinedGroups.FirstOrDefault(x => x.GroupNameAliases?.Any(a => a == (string)Args[0]) ?? false);
            if (group == null)
            {
                var cmd = UserDefinedCommands.FirstOrDefault(x => x.CommandName == (string)Args[0]);
                if(cmd == null) cmd = UserDefinedCommands.FirstOrDefault(x => x.CommandNameAliases?.Any(a => a == (string)Args[0]) ?? false);
                if (cmd == null)
                {
                    CommandErrored?.Invoke(this, new CommandErroredArgs()
                    {
                        Message = "Command does not exist",
                        Channel = context.Channel,
                        Exception = null,
                        Guild = context.Guild
                    });
                    return;
                }
                Args.Remove(Args[0]);
                await ExecuteCommandAsync(client, msg, Args, cmd, context);
            }
            else
            {
                Args.Remove(Args[0]);
                SlothUserCommand cmd = null;
                if (Args.Count > 0)
                {
                    cmd = group.Commands?.FirstOrDefault(x => x.CommandName == (string)Args[0]);
                    if (cmd == null) cmd = UserDefinedCommands.FirstOrDefault(x => x.CommandNameAliases?.Any(a => a == (string)Args[0]) ?? false);
                }
                if (cmd == null)
                {
                    if (!group.InvokeWithoutSubCommand)
                    {
                        CommandErrored?.Invoke(this, new CommandErroredArgs()
                        {
                            Message = "Command does not exist",
                            Channel = context.Channel,
                            Exception = null,
                            Guild = context.Guild
                        });
                        return;
                    }
                    else
                    {
                        if (group.GroupExecuteCommand == null || group.GroupExecuteCommand.Method == null)
                        {
                            CommandErrored?.Invoke(this, new CommandErroredArgs()
                            {
                                Message = "Command does not exist and no ExecuteAsync method exists",
                                Channel = context.Channel,
                                Exception = null,
                                Guild = context.Guild
                            });
                            return;
                        }
                        else cmd = group.GroupExecuteCommand;
                    }
                }
                if (cmd != group.GroupExecuteCommand) Args.Remove(Args[0]);

                await ExecuteCommandAsync(client, msg, Args, cmd, context);
            }
        }
        
        internal Task ExecuteCommandAsync(DiscordClient client, DiscordMessage msg, List<object> Args, SlothUserCommand cmd, SlothCommandContext context)
        {
            if (cmd.Method.HasAttribute<RequireOwnerAttribute>() && (msg.Author.Id != client.CurrentUser.Id)) return Task.CompletedTask;
            if(!AllowDmCommands && !client.Guilds.Any(x => x.Channels.Any(a => a.Id == msg.ChannelId))) return Task.CompletedTask;

            var passargs = new List<object>();
            int pos = 0;
            var countval = cmd.Parameters.Count();
            if (cmd.Parameters.FirstOrDefault()?.ParameterType == typeof(SlothCommandContext))
            {
                passargs.Add(context);
                countval--;
                pos++;
            }
            for (var i = 0; i < countval; i++)
            {
                object currentarg = null;
                if(Args.Count > 0)
                    currentarg = Args[i];
                var check = cmd.Parameters[pos].CustomAttributes.Any(y => y.AttributeType == typeof(RemainingStringAttribute));
                if (check)
                {
                    var sb = new StringBuilder();
                    for (var o = 0; o < Args.Count; o++)
                            sb.Append($" {Args[o]}");
                    passargs.Add(sb.ToString());
                    break;
                }

                if (currentarg != null)
                {
                    if (new Regex(@"(<@(?:!)\d+>)").IsMatch(currentarg as string))
                    {
                        var strid = new Regex(@"((<@)(?:!))").Replace(Args[i] as string, "").Replace(">", "");
                        var id = ulong.Parse(strid);
                        var cachedUser = client.InternalUserCache?.FirstOrDefault(x => x.Id == id);
                        if (context.Member != null && currentarg.GetType() == typeof(DiscordGuildMember)) currentarg = context.Member;
                        else if (cachedUser != null) currentarg = cachedUser;
                    }
                    else
                    {
                        var type = cmd.Parameters[pos].ParameterType;
                        currentarg = Convert.ChangeType(Args[i], type);
                    }
                }
                passargs.Add(currentarg);
            }
            try
            {
                var result = cmd.Method.Invoke(cmd.ClassInstance, passargs.ToArray());
            }
            catch (TargetParameterCountException)
            {
                CommandErrored?.Invoke(this, new CommandErroredArgs()
                {
                    Message = "Required parameter does not have a value",
                    Channel = context.Channel,
                    Exception = null,
                    Guild = context.Guild
                });
            }
            catch(Exception ex)
            {
                CommandErrored?.Invoke(this, new CommandErroredArgs()
                {
                    Message = ex.Message,
                    Channel = context.Channel,
                    Exception = ex,
                    Guild = context.Guild
                });
            }
            return Task.CompletedTask;
        }
    }

    public class SlothCommandContext : ApiBase
    {
        public IServiceProvider Services { get; internal set; }
        public DiscordMessage Message { get; internal set; }
        public DiscordGuild Guild { get; internal set; }
        public DiscordChannel Channel { get; internal set; }
        public DiscordUser User { get; internal set; }
        public DiscordGuildMember Member { get; internal set; }
        public DiscordClient Client { get; internal set; }

        public async Task<DiscordMessage> RespondAsync(string text = null, bool is_tts = false, DiscordEmbed embed = null)
        {
            return await Channel.SendMessageAsync(text, is_tts, embed);
        }
    }

    public class SlothUserCommand
    { 
        public string CommandName { get; internal set; }
        public object ClassInstance { get; internal set; }
        public string MethodName { get; internal set; }
        public ParameterInfo[] Parameters { get; internal set; }
        public MethodInfo Method { get; internal set; }
        public string[] CommandNameAliases { get; internal set; }
    }

    public class SlothUserCommandGroup
    {
        public bool InvokeWithoutSubCommand { get; internal set; }
        public SlothUserCommand GroupExecuteCommand { get; internal set; }
        public string GroupName { get; internal set; }
        public IReadOnlyList<SlothUserCommand> Commands { get; internal set; }
        public string[] GroupNameAliases { get; internal set; }
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

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class AliasesAttribute : Attribute
    {
        public AliasesAttribute(string[] aliases)
        {
            this.Aliases = aliases;
        }
        public string[] Aliases { get; private set; }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class RemainingStringAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class RequireOwnerAttribute : Attribute { }

    public static class Extensions
    {
        public static bool HasAttribute<T>(this MethodInfo method)
        {
            return method.CustomAttributes.Any(x => x.AttributeType == typeof(T));
        }
    }
}
