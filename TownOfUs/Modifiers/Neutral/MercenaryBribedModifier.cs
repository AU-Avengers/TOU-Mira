using MiraAPI.Events;
using MiraAPI.Modifiers;
using Reactor.Utilities.Extensions;
using TownOfUs.Events.TouEvents;
using TownOfUs.Utilities;

namespace TownOfUs.Modifiers.Neutral;

public sealed class MercenaryBribedModifier(PlayerControl mercenary) : BaseModifier
{
    public bool alerted;
    public override string ModifierName => "Mercenary Bribed";
    public override bool HideOnUi => true;

    public PlayerControl Mercenary { get; } = mercenary;

    // Multiple mercs can bribe the same player in this case
    public override bool Unique => false;

    public override void OnActivate()
    {
        base.OnActivate();

        var touAbilityEvent = new TouAbilityEvent(AbilityType.MercenaryBribe, Mercenary, Player);
        MiraEventManager.InvokeEvent(touAbilityEvent);
    }

    public override void OnMeetingStart()
    {
        if (!Player.AmOwner)
        {
            return;
        }

        if (alerted)
        {
            return;
        }

        var title = $"<color=#{TownOfUsColors.Mercenary.ToHtmlStringRGBA()}>Mercenary Feedback</color>";
        MiscUtils.AddFakeChat(Player.Data, title, "You have been bribed by a Mercenary!", false, true);

        alerted = true;
    }
}