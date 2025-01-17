namespace Site12.API.Features.Scombat;

using System;
using System.Linq;
using CommandSystem;
using Extensions;
using SCombatCmds;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class SCombatCommand : ParentCommand
{
    public SCombatCommand() => LoadGeneratedCommands();

    public override string Command => "scombat";

    public override string[] Aliases => ["scb", "scombatcommand"];

    public override string Description => "Arguments : scombat {logs/give} {args2} {args3}";

    public sealed override void LoadGeneratedCommands()
    {
        RegisterCommand(new GiveCommand());
        RegisterCommand(new ListCommand());
    }

    protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        response = AllCommands.Where(command => sender.CheckRemoteAdmin(out _)).Aggregate("\nPlease enter a valid subcommand:", (current, command) => current + $"\n\n<color=yellow><b>- {command.Command} ({string.Join(", ", command.Aliases)})\nscombat {command.Command} {"{" + string.Join("} {", ((IUsageProvider)command).Usage) + "}"}</b></color>\n<color=white>{command.Description}</color>");
        return false;
    }
}