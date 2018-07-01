﻿using System;
using System.Reflection;
using System.Threading.Tasks;

namespace SlothCord.Commands
{
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

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class RemainingStringAttribute : Attribute
    { }
}
