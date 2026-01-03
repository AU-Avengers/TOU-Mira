using MiraAPI.GameOptions;
using Reactor.Utilities;
using TownOfUs.Interfaces;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Utilities;
using TownOfUs.Utilities.ControlSystem;

namespace TownOfUs.Modifiers.Impostor;

/// <summary>
/// Applied to the victim while they are controlled by a Puppeteer.
/// - Disables their buttons/actions (via DisabledModifier base checks).
/// - Forces their appearance to match the Puppeteer (visible to others).
/// Movement/input suppression is handled by Harmony patches while this modifier is present.
/// </summary>
public sealed class PuppeteerControlModifier(PlayerControl controller) : DisabledModifier, IUncontrollable
{
    public override string ModifierName => "Puppeteer Controlled";
    public override bool HideOnUi => true;
    public override bool AutoStart => true;

    public override bool CanUseAbilities => false;
    public override bool CanReport => false;

    public override float Duration => OptionGroupSingleton<PuppeteerOptions>.Instance.ControlDuration.Value;
    public PlayerControl Controller { get; } = controller;

    private LobbyNotificationMessage? _controlledNotification;

    public override void OnActivate()
    {
        if (Player.AmOwner)
        {
            TouAudio.PlaySound(TouAudio.HackedSound);
            CreateNotification();
            Coroutines.Start(MiscUtils.CoFlash(Palette.ImpostorRed, Duration));

            if (Minigame.Instance)
                Minigame.Instance.Close();

            if (MapBehaviour.Instance)
                MapBehaviour.Instance.Close();
            if (Player.inVent)
            {
                Player.MyPhysics.RpcExitVent(Vent.currentVent.Id);
                Player.MyPhysics.ExitAllVents();
            }
        }
        else if (Controller.AmOwner)
        {
            try { Controller.NetTransform.Halt(); } catch { /* ignored */ }
            if (HudManager.InstanceExists && HudManager.Instance != null)
            {
                HudManager.Instance.PlayerCam.SetTarget(Player);
            }
            try
            {
                if (Controller.lightSource != null && Player != null)
                {
                    Controller.lightSource.transform.SetParent(Player.transform);
                    Controller.lightSource.Initialize(Player.Collider.offset / 2f);
                }
            }
            catch { /* ignored */ }
        }
    }

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent!.RemoveModifier(this);
    }

    public override void OnDeactivate()
    {
        ClearNotification();
        if (Controller.AmOwner)
        {
            Controller.moveable = true;
            try { Controller.NetTransform.Halt(); } catch { /* ignored */ }
            if (HudManager.InstanceExists && HudManager.Instance != null)
            {
                HudManager.Instance.PlayerCam.SetTarget(Controller);
            }
            try
            {
                if (Controller.lightSource != null)
                {
                    Controller.lightSource.transform.SetParent(Controller.transform);
                    Controller.lightSource.Initialize(Controller.Collider.offset / 2f);
                }
            }
            catch { /* ignored */ }
        }
    }

    private void CreateNotification()
    {
        if (_controlledNotification == null)
        {
            var controllerName = Controller?.Data?.Role is Roles.ITownOfUsRole touRole ? touRole.RoleName : "Puppeteer";
            _controlledNotification = ControlledFeedbackUtilities.ShowControlledByNotification(
                controllerName,
                TownOfUsColors.Impostor,
                TouRoleIcons.Puppeteer.LoadAsset());
            _controlledNotification?.AdjustNotification();
        }
    }

    public void ClearNotification()
    {
        ControlledFeedbackUtilities.ClearNotification(ref _controlledNotification);
    }
}