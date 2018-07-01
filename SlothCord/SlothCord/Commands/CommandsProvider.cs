using SlothCord.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SlothCord.Commands
{
    public class CommandsProvider
    {
        public string Prefix { get; set; }
        public event CommandErroredEvent CommandErrored;

        public List<Command?> CommandsList { get; private set;} = new List<Command?>();
        public List<CommandGroup?> GroupCommandsList { get; private set; } = new List<CommandGroup?>();

        public Task AddCommandAsync<T>() where T : new()
        {
            var methods = (((typeof(T)).GetMethods()).Where(x => x.IsPublic && (x.GetCustomAttribute<CommandAttribute>() != null)) as IReadOnlyList<MethodInfo>);

            var groups = (((typeof(T)).GetNestedTypes()).Where(x => x.IsPublic && (x.GetCustomAttribute<GroupAttribute>() != null)) as IReadOnlyList<Type>);

            for (var count = 0; count < methods.Count(); count++)
            {
                CommandsList.Add(new Command()
                {
                    ClassInstance = new T(),
                    Method = methods[count],
                    CommandName = methods[count].GetCustomAttribute<CommandAttribute>().CommandName,
                    MethodParams = methods[count].GetParameters()
                });
            }

            for (var count = 0; count < groups.Count(); count++)
            {
                var submths = groups[count].GetMethods().Where(x => x.IsPublic && (x.GetCustomAttribute<CommandAttribute>() != null)) as IReadOnlyList<MethodInfo>;
                var subcmds = new List<Command>();
                for (var i = 0; i < submths.Count(); i++)
                {
                    subcmds.Add(new Command()
                    {
                        ClassInstance = groups[count],
                        Method = methods[i],
                        CommandName = methods[i].GetCustomAttribute<CommandAttribute>().CommandName,
                        ParentCommandName = groups[count].GetCustomAttribute<GroupAttribute>().GroupName
                    });
                }

                GroupCommandsList.Add(new CommandGroup()
                {
                    GroupName = methods[count].GetCustomAttribute<GroupAttribute>().GroupName,
                    SubCommands = subcmds
                });
            }

            return Task.CompletedTask;
        }

        public Task<List<string>> ParseArguementsAsync(DiscordMessage message)
        {
            var msg = message.Content.Replace(Prefix, "");
            var arr = msg.ToCharArray() as IReadOnlyList<char>;
            var arguements = new List<string>();
            var sb = new StringBuilder();
            var addon = false;
            for(var i = 0; i < arr.Count; i++)
            {
                switch(arr[i])
                {
                    case '"':
                        {
                            if (addon) addon = false;
                            else addon = true;
                            break;
                        }
                    case ' ':
                        {
                            if (!addon)
                            {
                                arguements.Add(sb.ToString());
                                sb = new StringBuilder();
                            }
                            else sb.Append(' ');
                            break;
                        }
                    default:
                        {
                            sb.Append(arr[i]);
                            break;
                        }
                }
            }
            return Task.FromResult(arguements);
        }

        public async Task ExecuteCommandAsync(List<string> Args, Command Target, bool IsSubCommand, DiscordClient client, DiscordMessage msg)
        {
            try
            {
                var passingargs = new List<object>();
                if (IsSubCommand) Args.RemoveRange(0, 2);
                else Args.RemoveAt(0);
                for(var i = 0; i < Args.Count; i++)
                {
                    object arg = null;
                    if (Regex.IsMatch(Args[i], @"^(<@!?[\d]+>)$"))
                    {
                        var id = ulong.Parse(Args[i].Remove(0, 2).Replace("!", "").Replace(">", ""));
                        if (Target.MethodParams[i].ParameterType == typeof(DiscordGuildMember))
                        {
                            var guild = client.Guilds.FirstOrDefault(x => x.Channels.Any(a => a.Id == msg.ChannelId));
                            var member = guild?.Members.FirstOrDefault(x => x.UserData.Id == msg.UserAuthor.Id);
                            arg = member;
                        }
                        else if (Target.MethodParams[i].ParameterType == typeof(DiscordUser)) arg = await client.GetUserAsync(id).ConfigureAwait(false);
                        else arg = Convert.ChangeType(Args[i], Target.MethodParams[i].ParameterType);
                    }
                    else if(ulong.TryParse(Args[i], out ulong res))
                    {
                        if (Target.MethodParams[i].ParameterType == typeof(DiscordGuildMember))
                        {
                            var guild = client.Guilds.FirstOrDefault(x => x.Channels.Any(a => a.Id == msg.ChannelId));
                            var member = guild?.Members.FirstOrDefault(x => x.UserData.Id == msg.UserAuthor.Id);
                            arg = member;
                        }
                        else if (Target.MethodParams[i].ParameterType == typeof(DiscordUser)) arg = await client.GetUserAsync(res).ConfigureAwait(false);
                        else arg = Convert.ChangeType(Args[i], Target.MethodParams[i].ParameterType);
                    }
                    else arg = Convert.ChangeType(Args[i], Target.MethodParams[i].ParameterType);
                    passingargs.Add(arg);
                }
                Target.Method.Invoke(Target.ClassInstance, passingargs.ToArray());
            }
            catch(Exception ex)
            {
                this.CommandErrored?.Invoke(ex).ConfigureAwait(false);
            }
        }
    }

    public struct Command
    {
        public string ParentCommandName { get; internal set; }
        public string CommandName { get; internal set; }
        public MethodInfo Method { get; internal set; }
        public IReadOnlyList<ParameterInfo> MethodParams { get; internal set; }
        public object ClassInstance { get; internal set; }
    }

    public struct CommandGroup
    {
        public string GroupName { get; internal set; }
        public List<Command> SubCommands { get; internal set; }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class CommandAttribute : Attribute
    {
        internal string CommandName { get; set; }
        public CommandAttribute(string Name)
        {
            this.CommandName = Name;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class GroupAttribute : Attribute
    {
        internal string GroupName { get; set; }
        internal bool RequireSubCommand { get; set; }

        public GroupAttribute(string Name, bool RequiresSubCommand)
        {
            this.GroupName = Name;
            this.RequireSubCommand = RequiresSubCommand;
        }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class RemainingStringAttribute : Attribute
    { }
}
