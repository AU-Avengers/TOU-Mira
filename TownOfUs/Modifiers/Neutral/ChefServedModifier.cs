using MiraAPI.GameOptions;
using MiraAPI.Modifiers.Types;
using Reactor.Utilities.Extensions;
using TownOfUs.Options.Modifiers.Universal;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;

namespace TownOfUs.Modifiers.Neutral;

public sealed class ChefServedModifier(PlayerControl chef, int servingType, int bodyId) : TimedModifier
{
    public bool Alerted;
    public override float Duration => OptionGroupSingleton<ChefOptions>.Instance.SideEffectDuration.Value;
    public float HalfDuration { get; set; }
    public override bool AutoStart => false;
    public override bool RemoveOnComplete => false;
    public PlatterType FoodType => (PlatterType)servingType;
    public int BodyId => bodyId;
    public override string ModifierName => "Chef Served";
    public override bool HideOnUi => true;

    public PlayerControl Chef { get; } = chef;
    public float SpeedFactor { get; set; } = 1f;
    public bool HasFinished { get; set; }

    public override void OnActivate()
    {
        base.OnActivate();
        SpeedFactor = 1f;
        HalfDuration = Duration / 2f;

        if (Duration > 0f)
        {
            switch (FoodType)
            {
                case PlatterType.Turkey:
                    SpeedFactor = OptionGroupSingleton<GiantOptions>.Instance.GiantSpeed;
                    break;
                case PlatterType.Cake:
                    SpeedFactor = OptionGroupSingleton<MiniOptions>.Instance.MiniSpeed;
                    break;
                case PlatterType.Burger:
                    SpeedFactor = OptionGroupSingleton<FlashOptions>.Instance.FlashSpeed;
                    break;
            }
        }
        /*var touAbilityEvent = new TouAbilityEvent(AbilityType.MercenaryBribe, Mercenary, Player);
        MiraEventManager.InvokeEvent(touAbilityEvent);*/
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (HasFinished)
        {
            return;
        }
        
        if (TimerActive)
        {
            if (TimeRemaining <= 0.5f)
            {
                SpeedFactor = 1f;
                HasFinished = true;
            }
            else if (FoodType is PlatterType.Burger && TimeRemaining <= HalfDuration)
            {
                SpeedFactor = 0.5f;
            }
        }
    }

    public override void OnTimerComplete()
    {
        base.OnTimerComplete();
        HasFinished = true;
        SpeedFactor = 1f;
    }

    public override void OnMeetingStart()
    {
        if (Chef.HasDied())
        {
            ModifierComponent?.RemoveModifier(this);
            return;
        }

        if (Alerted)
        {
            if (TimerActive)
            {
                PauseTimer();
            }
            return;
        }

        Alerted = true;
        if (Duration <= 1f)
        {
            HasFinished = true;
        }

        if (!Player.AmOwner)
        {
            return;
        }

        var title = $"<color=#{TownOfUsColors.Chef.ToHtmlStringRGBA()}>{TouLocale.Get("TouRoleChefMessageTitle")}</color>";

        MiscUtils.AddFakeChat(Player.Data, title, TouLocale.Get("TouRoleChefCustomerMessage"), false, true);
    }
}