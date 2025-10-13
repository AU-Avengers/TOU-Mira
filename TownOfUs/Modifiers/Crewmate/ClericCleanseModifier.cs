using System.Text;
using MiraAPI.Events;
using MiraAPI.Modifiers;
using Reactor.Utilities.Extensions;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Utilities;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class ClericCleanseModifier(PlayerControl cleric) : BaseModifier
{
    public enum EffectType : byte
    {
        Douse,
        Hack,
        Infect,
        Blackmail,
        Blind,
        Flash,
        Hypnosis,
        Hex
    }

    public override string ModifierName => "Cleric Cleanse";
    public override bool HideOnUi => true;
    public PlayerControl Cleric { get; } = cleric;

    public List<EffectType> Effects { get; set; } = [];
    public bool Cleansed { get; set; }

    public override void OnActivate()
    {
        base.OnActivate();
        var touAbilityEvent = new TouAbilityEvent(AbilityType.ClericCleanse, Cleric, Player);
        MiraEventManager.InvokeEvent(touAbilityEvent);

        Effects = FindNegativeEffects(Player);

        // Logger<TownOfUsPlugin>.Error($"ClericCleanseModifier.OnActivate");
    }

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent?.RemoveModifier(this);
    }

    public override void OnMeetingStart()
    {
        // Logger<TownOfUsPlugin>.Error($"ClericCleanseModifier.OnMeetingStart");
        if (Cleric.AmOwner)
        {
            var text = new StringBuilder($"Cleansed effects on {Player.Data.PlayerName}:");

            foreach (var effect in Effects)
            {
                text.Append(TownOfUsPlugin.Culture, $" {effect.ToString()},");
            }

            text = text.Remove(text.Length - 1, 1);

            if (Effects.Count == 0)
            {
                text = new StringBuilder($"No negative effects were found on {Player.Data.PlayerName}.");
            }

            var title = $"<color=#{TownOfUsColors.Cleric.ToHtmlStringRGBA()}>Cleric Feedback</color>";
            MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, title, text.ToString(), false, true);
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (!Cleansed)
        {
            CleansePlayer();
            Cleansed = true;
        }
    }

    private void CleansePlayer()
    {
        // Logger<TownOfUsPlugin>.Error($"ClericCleanseModifier.CleansePlayer");
        if (Effects.Contains(EffectType.Douse))
        {
            Player.RemoveModifier<ArsonistDousedModifier>();
        }

        if (Effects.Contains(EffectType.Hack))
        {
            Player.RemoveModifier<GlitchHackedModifier>();
        }

        if (Effects.Contains(EffectType.Infect))
        {
            Player.RemoveModifier<PlaguebearerInfectedModifier>();
        }

        if (Effects.Contains(EffectType.Blackmail))
        {
            Player.RemoveModifier<BlackmailedModifier>();
        }

        if (Effects.Contains(EffectType.Blind))
        {
            Player.RpcRemoveModifier<EclipsalBlindModifier>();
        }

        if (Effects.Contains(EffectType.Flash))
        {
            Player.RemoveModifier<GrenadierFlashModifier>();
        }

        if (Effects.Contains(EffectType.Hypnosis))
        {
            Player.RemoveModifier<HypnotisedModifier>();
        }

        if (Effects.Contains(EffectType.Hex))
        {
            Player.RemoveModifier<SpellslingerHexedModifier>();
        }
    }

    public static List<EffectType> FindNegativeEffects(PlayerControl player)
    {
        var effects = new List<EffectType>();

        if (player.HasModifier<ArsonistDousedModifier>())
        {
            effects.Add(EffectType.Douse);
        }

        if (player.HasModifier<GlitchHackedModifier>())
        {
            effects.Add(EffectType.Hack);
        }

        if (player.HasModifier<PlaguebearerInfectedModifier>())
        {
            effects.Add(EffectType.Infect);
        }

        if (player.HasModifier<BlackmailedModifier>())
        {
            effects.Add(EffectType.Blackmail);
        }

        if (player.HasModifier<EclipsalBlindModifier>())
        {
            effects.Add(EffectType.Blind);
        }

        if (player.HasModifier<GrenadierFlashModifier>())
        {
            effects.Add(EffectType.Flash);
        }

        if (player.HasModifier<HypnotisedModifier>())
        {
            effects.Add(EffectType.Hypnosis);
        }

        if (player.HasModifier<SpellslingerHexedModifier>())
        {
            effects.Add(EffectType.Hex);
        }

        return effects;
    }
}