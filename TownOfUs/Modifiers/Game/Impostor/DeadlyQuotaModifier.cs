using System.Collections;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Impostor;

public sealed class DeadlyQuotaModifier : TouGameModifier, IWikiDiscoverable
{
    public int KillCount { get; set; }
    public int KillQuota { get; private set; }

    public bool IgnoreQuota =>
        OptionGroupSingleton<DeadlyQuotaOptions>.Instance.RemoveQuotaUponDeath && Player.HasDied();
    public override string LocaleKey => "DeadlyQuota";
    public override string ModifierName => TouLocale.Get($"TouModifier{LocaleKey}");
    public override string IntroInfo => KillQuota == 1 ? TouLocale.GetParsed($"TouModifier{LocaleKey}IntroBlurb") : TouLocale.GetParsed($"TouModifier{LocaleKey}IntroBlurbPlural").Replace("<amount>", KillQuota.ToString(TownOfUsPlugin.Culture));

    public override string GetDescription()
    {
        return TouLocale.GetParsed($"TouModifier{LocaleKey}TabDescription").Replace("<amount>", KillCount.ToString(TownOfUsPlugin.Culture)).Replace("<total>", KillQuota.ToString(TownOfUsPlugin.Culture));
    }

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouModifier{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }
    public override Color FreeplayFileColor => new Color32(255, 25, 25, 255);

    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.DeadlyQuota;
    public override ModifierFaction FactionType => ModifierFaction.ImpostorPassive;

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<ImpostorModifierOptions>.Instance.DeadlyQuotaChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<ImpostorModifierOptions>.Instance.DeadlyQuotaAmount;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsImpostor();
    }
    public override void OnActivate()
    {
        if (Player.AmOwner)
        {
            var newQuota = OptionGroupSingleton<DeadlyQuotaOptions>.Instance.GenerateKillQuota();
            KillQuota = newQuota;
            Coroutines.Start(SetUpQuota(newQuota));
        }
    }

    private IEnumerator SetUpQuota(int newQuota)
    {
        yield return new WaitForSeconds(0.01f);
        RpcSetDeadlyQuota(Player, newQuota);
    }
    public override bool? DidWin(GameOverReason reason)
    {
        if (!IgnoreQuota && KillCount < KillQuota)
        {
            return false;
        }
        return null;
    }

    public override void OnMeetingStart()
    {
        if (!Player.HasDied() && Player.AmOwner && KillCount == 0)
        {
            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>{TouLocale.GetParsed("TouModifierDeadlyQuotaWarningNotif").Replace("<amount>", KillQuota.ToString(TownOfUsPlugin.Culture))}</b>",
                Color.white,
                new Vector3(0f, 1f, -20f),
                spr: TouModifierIcons.DeadlyQuota.LoadAsset());


            notif1.Text.SetOutlineThickness(0.4f);
        }
    }

    [MethodRpc((uint)TownOfUsRpc.SetDeadlyQuota)]
    public static void RpcSetDeadlyQuota(PlayerControl player, int quota)
    {
        if (player.TryGetModifier<DeadlyQuotaModifier>(out var quotaMod))
        {
            quotaMod.KillQuota = quota;
        }
    }


}