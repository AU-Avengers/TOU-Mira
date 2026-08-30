using System.Text.RegularExpressions;
using Il2CppSystem.Text;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using TownOfUs.Options.Modifiers;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Crewmate;

public sealed class TaskmasterModifier : TouGameModifier, IWikiDiscoverable
{
    public override ModifierUiConfiguration Configuration => new(
        TownOfUsColors.Taskmaster,
        TmpSpriteUtils.CreateSpriteAsset(TouModifierIcons.Taskmaster.LoadAsset(),
            "TouMira.Modifier.Crewmate.Taskmaster", 1.45f));
    public override string IdPart => "Taskmaster";
    public override string ModifierName => MiraLocaleManager.Get($"TouModifier{IdPart}");
    public override string IntroInfo => MiraLocaleManager.Get($"TouModifier{IdPart}.IntroBlurb");

    public override string GetDescription()
    {
        return MiraLocaleManager.Get($"TouModifier{IdPart}.TabDescription");
    }

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TouModifier{IdPart}.WikiDescription")
               + MiscUtils.AppendOptionsText(GetType());
    }

    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Taskmaster;
    public override Color FreeplayFileColor => new Color32(140, 255, 255, 255);

    public override ModifierFaction FactionType => ModifierFaction.CrewmatePassive;

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.TaskmasterChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.TaskmasterAmount;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsCrewmate() && role is not SnitchRole &&
               !(GameOptionsManager.Instance.currentGameOptions.MapId is 4 or 6);
    }

    public void OnRoundStart()
    {
        if (Player.AmOwner && Player.myTasks.Count > 0 && !Player.HasDied() && Player.IsCrewmate())
        {
            var tasks = Player.myTasks.ToArray().Where(x => x.TryCast<NormalPlayerTask>() != null && !x.IsComplete)
                .ToList();

            if (tasks.Count > 0)
            {
                tasks.Shuffle();

                var randomTask = tasks[0];

                HudManager.Instance.ShowTaskComplete();
                Player.RpcCompleteTask(randomTask.Id);

                var sb = new StringBuilder();
                randomTask.AppendTaskText(sb);

                var pattern = @" \(.*?\)";
                var query = sb.ToString();
                var taskText = Regex.Replace(query, pattern, string.Empty);
                taskText = taskText.Replace(Environment.NewLine, "");

                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>{TownOfUsColors.Taskmaster.ToTextColor()}{MiraLocaleManager.Get("TouModifierTaskmasterTaskNotif").Replace("<taskName>", taskText)}</b></color>",
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouModifierIcons.Taskmaster.LoadAsset());
                notif1.AdjustNotification();
            }
        }
    }
}