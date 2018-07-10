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
        public IServiceProvider Services { get; set; }
        public bool AllowDms { get; set; } = false;

        public event CommandErroredEvent CommandErrored;

        public List<Command?> CommandsList { get; private set; } = new List<Command?>();
        public List<CommandGroup?> GroupCommandsList { get; private set; } = new List<CommandGroup?>();

        public Task AddCommandModuleAsync<T>() where T : new()
        {
            var type = typeof(T);

            CommandsList.AddRange(type.GetMethods().Where(x => x.HasAttribute<CommandAttribute>() && x.IsPublic).Select(x => new Command()
            {
                ClassInstance = new T(),
                Method = x,
                MethodParams = x.GetParameters(),
                CommandName = x.GetCustomAttribute<CommandAttribute>().CommandName,
                Aliases = x.GetCustomAttribute<AliasesAttribute>()?.Aliases,
                ParentCommandName = null
            } as Command?));

            var groups = typeof(T).GetNestedTypes().Where(x => x.IsPublic && (x.HasAttribute<GroupAttribute>())) as IReadOnlyList<Type>;

            for (var count = 0; count < groups?.Count(); count++)
            {
                var submths = groups[count].GetMethods().Where(x => x.IsPublic && (x.GetCustomAttribute<CommandAttribute>() != null)) as IReadOnlyList<MethodInfo>;
                var subcmds = new List<Command>();
                for (var i = 0; i < submths.Count(); i++)
                {
                    subcmds.Add(new Command()
                    {
                        ClassInstance = groups[count],
                        Method = CommandsList[i].Value.Method,
                        Aliases = CommandsList[i].Value.Method.GetCustomAttribute<AliasesAttribute>()?.Aliases,
                        CommandName = CommandsList[i].Value.Method.GetCustomAttribute<CommandAttribute>().CommandName,
                        ParentCommandName = groups[count].GetCustomAttribute<GroupAttribute>().GroupName
                    });
                }

                GroupCommandsList.Add(new CommandGroup()
                {
                    GroupName = groups[count].GetCustomAttribute<GroupAttribute>().GroupName,
                    Aliases = groups[count].GetCustomAttribute<AliasesAttribute>().Aliases,
                    SubCommands = subcmds,
                    ExecuteMethod = subcmds.FirstOrDefault(x => x.Method.Name == "ExecuteGroupAsync").Method
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
            for (var i = 0; i < arr.Count; i++)
            {
                switch (arr[i])
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
            if (!string.IsNullOrWhiteSpace(sb.ToString()))
                arguements.Add(sb.ToString());
            return Task.FromResult(arguements);
        }

        public async Task ExecuteCommandAsync(List<string> Args, Command Target, bool IsSubCommand, DiscordClient client, DiscordMessage msg)
        {
            try
            {
                var passingargs = new List<object>();
                if (IsSubCommand) Args.RemoveRange(0, 2);
                else Args.RemoveAt(0);

                for (var i = 0; i < Target.MethodParams.Count; i++)
                {
                    var att = Target.MethodParams[i].CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(RemainingStringAttribute));

                    if (att != null)
                    {
                        var sb = new StringBuilder();
                        for (var a = 0; a < Args.Count; a++)
                            sb.Append($" {Args[a]}");
                        passingargs.Add(sb.ToString().Remove(0, 1));
                        break;
                    }

                    object arg = null;
                    if (Target.MethodParams[i].ParameterType == typeof(SlothContext))
                    {
                        var guild = client.Guilds.FirstOrDefault(x => x.Channels.Any(a => a.Id == msg.ChannelId));
                        var channel = guild?.Channels.FirstOrDefault(x => x.Id == msg.ChannelId);
                        arg = new SlothContext()
                        {
                            GuildChannel = channel,
                            Channel = client.PrivateChannels.FirstOrDefault(x => x.Id == msg.ChannelId),
                            InvokingMember = guild.Members.FirstOrDefault(x => x.UserData.Id == msg.UserAuthor.Id),
                            Guild = guild,
                            InvokingUser = msg.UserAuthor,
                            Message = msg,
                            Provider = this,
                            Services = this.Services
                        };
                    }

                    else if (Regex.IsMatch(Args[i], @"^(<@!?[\d]+>)$"))
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

                    else if (ulong.TryParse(Args[i], out ulong res))
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
                    if(Args.IndexOf(arg.ToString()) > -1)
                        Args.RemoveAt(Args.IndexOf(arg.ToString()));
                }

                if (passingargs.Count == 0) Target.Method.Invoke(Target.ClassInstance, passingargs.ToArray());

                else Target.Method.Invoke(Target.ClassInstance, passingargs.ToArray());
            }
            catch (Exception ex)
            {
                this.CommandErrored?.Invoke(ex).ConfigureAwait(false);
            }
        }
    }

    public struct Command
    {
        public string ParentCommandName { get; internal set; }
        public string CommandName { get; internal set; }
        public string[] Aliases { get; internal set; }
        public MethodInfo Method { get; internal set; }
        public IReadOnlyList<ParameterInfo> MethodParams { get; internal set; }
        public object ClassInstance { get; internal set; }
    }

    public struct CommandGroup
    {
        public string GroupName { get; internal set; }
        public string[] Aliases { get; internal set; }
        public MethodInfo ExecuteMethod { get; internal set; }
        public List<Command> SubCommands { get; internal set; }
    }

    public class SlothContext : ChannelMethods
    {
        public DiscordUser InvokingUser { get; internal set; }
        public DiscordGuildMember InvokingMember { get; internal set; }
        public DiscordGuild Guild { get; internal set; }
        public DiscordMessage Message { get; internal set; }
        public DiscordGuildChannel GuildChannel { get; internal set; }
        public DiscordChannel Channel { get; internal set; }
        public CommandsProvider Provider { get; internal set; }
        public IServiceProvider Services { get; internal set; }

        public async Task<DiscordMessage> ReplyAsync(string message = null, bool istts = false, DiscordEmbed embed = null)
        {
            var id = (GuildChannel != null) ? GuildChannel.Id : Channel.Id;
            return await base.CreateMessageAsync(id, message, istts, embed).ConfigureAwait(false);
        }
    }
    public static class Extensions
    {
        public static bool HasAttribute<T>(this MethodInfo method)
            => method.CustomAttributes.Any(x => x.AttributeType == typeof(T));

        public static bool HasAttribute<T>(this Type type)
            => type.CustomAttributes.Any(x => x.AttributeType == typeof(T));
    }
}