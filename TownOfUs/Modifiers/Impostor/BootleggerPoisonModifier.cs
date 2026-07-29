using MiraAPI.Modifiers;
using Reactor.Utilities.Extensions;

namespace TownOfUs.Modifiers.Impostor;

public sealed class BootleggerPoisonModifier(PlayerControl bootlegger) : BaseModifier
{
    public override string ModifierName => "Poison";
    public override bool HideOnUi => true;
    public PoisonProgress Poison = PoisonProgress.Begun;
    public bool HasReceivedSickMsg;
    public PlayerControl Bootlegger = bootlegger;

    public override void OnMeetingStart()
    {
        if (Player.HasDied())
        {
            return;
        }

        if (Poison == PoisonProgress.Sick && !HasReceivedSickMsg)
        {
            HasReceivedSickMsg = true;
            var title = $"<color=#{TownOfUsColors.ImpSoft.ToHtmlStringRGBA()}>Bootlegger Feedback</color>";
            if (Player.AmOwner)
            {
                var msg =
                    "You have a sudden feeling that a drink is making you feel sick! Next time the Bootlegger gives you a drink, you will die of poison!";
                MiscUtils.AddFakeChat(Player.Data, title, msg, false, true);
            }
            else if (Bootlegger && Bootlegger.AmOwner)
            {
                var msg =
                    $"Your victim, {Player.Data.PlayerName}, has noticed the effects of your drink! Next time you roleblock them, they will die of poison.";
                MiscUtils.AddFakeChat(Player.Data, title, msg, false, true);
            }
        }
    }
}

public enum PoisonProgress
{
    Begun,
    Sick,
    Poison
}