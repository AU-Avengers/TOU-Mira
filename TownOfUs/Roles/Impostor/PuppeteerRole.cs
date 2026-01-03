using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities.Extensions;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modules.ControlSystem;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Patches.ControlSystem;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Roles.Impostor;

public sealed class PuppeteerRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ITransportTrigger
{
    [HideFromIl2Cpp]
    public MonoBehaviour? OnTransport()
    {
        return Controlled;
    }
    [HideFromIl2Cpp] public PlayerControl? Controlled { get; set; }
    public float ControlTimer { get; set; }

    private LobbyNotificationMessage? controllerNotification;

    public DoomableType DoomHintType => DoomableType.Trickster;
    public string LocaleKey => "Puppeteer";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") + MiscUtils.AppendOptionsText(GetType());
    }

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorPower;

    public CustomRoleConfiguration Configuration => new(this)
    {
        UseVanillaKillButton = false,
        Icon = TouRoleIcons.Puppeteer,
        MaxRoleCount = 1,
        CanUseVent = OptionGroupSingleton<PuppeteerOptions>.Instance.CanVent
    };

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities =>
    [
        new(TouLocale.GetParsed($"TouRole{LocaleKey}Control", "Control"),
            TouLocale.GetParsed($"TouRole{LocaleKey}ControlWikiDescription"),
            TouImpAssets.ControlSprite),
    ];

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        ClearControlLocal();
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        ClearControlLocal();
    }

    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);

        if (Player.AmOwner && Controlled != null)
        {
            RpcPuppeteerEndControl(PlayerControl.LocalPlayer, Controlled);
        }
    }

    public override void OnDeath(DeathReason reason)
    {
        RoleBehaviourStubs.OnDeath(this, reason);

        if (!Player.AmOwner || Controlled == null)
        {
            ClearControlLocal();
            return;
        }

        RpcPuppeteerEndControl(PlayerControl.LocalPlayer, Controlled);
    }

    public void FixedUpdate()
    {
        if (Player == null || Player.Data == null || Player.HasDied() || !Player.AmOwner)
        {
            return;
        }

        if (Controlled == null)
        {
            return;
        }

        if (Controlled.Data == null || Controlled.HasDied() || Controlled.Data.Disconnected || Player.HasDied())
        {
            RpcPuppeteerEndControl(PlayerControl.LocalPlayer, Controlled);
            return;
        }

        var duration = OptionGroupSingleton<PuppeteerOptions>.Instance.ControlDuration.Value;
        if (duration > 0f)
        {
            if (ControlTimer > duration)
            {
                ControlTimer = duration;
            }

            // Don't tick cooldown during grace period
            if (!PuppeteerControlState.IsInInitialGrace(Controlled.PlayerId))
            {
                ControlTimer -= Time.fixedDeltaTime;
            }

            if (ControlTimer <= 0f && Controlled != null)
            {
                RpcPuppeteerEndControl(PlayerControl.LocalPlayer, Controlled);
            }
        }
    }

    public void ClearControlLocal()
    {
        Controlled = null;
        ControlTimer = 0f;
        ClearNotifications();
    }

    private void CreateNotification()
    {
        if (Controlled == null || PlayerControl.LocalPlayer == null || !Player.AmOwner)
        {
            return;
        }

        if (controllerNotification == null)
        {
            var controllerText = TouLocale.GetParsed("TouRolePuppeteerControlNotif", $"You are controlling {Controlled.Data.PlayerName}!");
            controllerNotification = Helpers.CreateAndShowNotification(
                $"<b>{TownOfUsColors.Impostor.ToTextColor()}{controllerText.Replace("<player>", Controlled.Data.PlayerName)}</color></b>",
                Color.white, new Vector3(0f, 2f, -20f), spr: TouRoleIcons.Puppeteer.LoadAsset());
            controllerNotification?.AdjustNotification();
        }
    }

    private void ClearNotifications()
    {
        if (controllerNotification != null && controllerNotification.gameObject != null)
        {
            controllerNotification.gameObject.Destroy();
            controllerNotification = null;
        }
    }

    [MethodRpc((uint)TownOfUsRpc.PuppeteerControl)]
    public static void RpcPuppeteerControl(PlayerControl puppeteer, PlayerControl target)
    {
        if (puppeteer.Data.Role is not PuppeteerRole role)
        {
            Error("RpcPuppeteerControl - Invalid puppeteer");
            return;
        }

        if (target == null || target.Data == null || target.HasDied())
        {
            return;
        }

        if (target.IsInTargetingAnimState())
        {
            return;
        }

        role.Controlled = target;
        role.ControlTimer = OptionGroupSingleton<PuppeteerOptions>.Instance.ControlDuration.Value;

        PuppeteerControlState.SetControl(target.PlayerId, puppeteer.PlayerId);
        if (!target.HasModifier<PuppeteerControlModifier>())
        {
            target.AddModifier<PuppeteerControlModifier>(puppeteer);
        }

        if (target.inVent)
        {
            target.MyPhysics.ExitAllVents();
        }

        if (puppeteer.AmOwner)
        {
            CustomButtonSingleton<TownOfUs.Buttons.Impostor.PuppeteerControlButton>.Instance.SetActive(true, role);
            role.CreateNotification();
        }
        else if (target.AmOwner && OptionGroupSingleton<PuppeteerOptions>.Instance.VictimSeesControlDirection.Value > 0)
        {
            puppeteer.AddModifier<PuppeteerHintArrowModifier>(PlayerControl.LocalPlayer);
        }
    }

    [MethodRpc((uint)TownOfUsRpc.PuppeteerEndControl)]
    public static void RpcPuppeteerEndControl(PlayerControl puppeteer, PlayerControl target)
    {
        if (puppeteer.Data.Role is not PuppeteerRole role)
        {
            return;
        }

        if (target != null)
        {
            PuppeteerControlState.ClearControl(target.PlayerId);
            if (target.TryGetModifier<PuppeteerControlModifier>(out var mod))
            {
                target.RemoveModifier(mod);
            }
        }

        role.ClearControlLocal();

        if (puppeteer.AmOwner)
        {
            var btn = CustomButtonSingleton<TownOfUs.Buttons.Impostor.PuppeteerControlButton>.Instance;
            btn.ResetCooldownAndOrEffect();
        }

        role.ClearNotifications();
    }

    [MethodRpc((uint)TownOfUsRpc.PuppeteerTriggerInteraction)]
    public static void RpcPuppeteerTriggerInteraction(PlayerControl puppeteer, PlayerControl controlled, Vector2 interactablePosition)
    {
        if (puppeteer.Data.Role is not PuppeteerRole role)
        {
            Error("RpcPuppeteerTriggerInteraction - Invalid puppeteer");
            return;
        }

        if (controlled == null || controlled.Data == null || controlled.HasDied())
        {
            return;
        }

        if (role.Controlled != controlled || !PuppeteerControlState.IsControlled(controlled.PlayerId, out _))
        {
            return;
        }

        var interactable = FindInteractableAtPosition(interactablePosition, controlled);
        if (interactable == null)
        {
            return;
        }

        TriggerInteractionAsPlayer(controlled, interactable);
    }

    private static IUsable? FindInteractableAtPosition(Vector2 position, PlayerControl player)
    {
        if (player == null)
        {
            return null;
        }

        var closestDistance = float.MaxValue;
        IUsable? closestInteractable = null;

        var cached = ControlledPlayerInteractionPatches.GetCachedInteractables();
        var interactablesToCheck = cached != null && cached.Count > 0 
            ? cached 
            : GetInteractablesList();

        const float maxCheckDistance = 5f;

        foreach (var usable in interactablesToCheck)
        {
            if (usable == null)
            {
                continue;
            }

            var obj = usable.TryCast<MonoBehaviour>();
            if (obj == null)
            {
                continue;
            }

            var objPos = (Vector2)obj.transform.position;
            var distance = Vector2.Distance(position, objPos);
            if (distance > maxCheckDistance || distance > usable.UsableDistance)
            {
                continue;
            }

            bool canUse;
            usable.CanUse(player.Data, out canUse, out _);
            if (!canUse)
            {
                continue;
            }

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestInteractable = usable;
            }
        }

        return closestInteractable;
    }

    private static List<IUsable> GetInteractablesList()
    {
        var result = new List<IUsable>();
        var allUsables = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();
        foreach (var obj in allUsables)
        {
            if (obj.TryCast<IUsable>() is { } usable && usable.TryCast<Vent>() == null)
            {
                result.Add(usable);
            }
        }
        return result;
    }

    private static void TriggerInteractionAsPlayer(PlayerControl player, IUsable interactable)
    {
        if (player == null || interactable == null)
        {
            return;
        }

        if (interactable.TryCast<Ladder>() is { } ladder)
        {
            if (!player.AmOwner)
            {
                return;
            }
            player.MyPhysics.RpcClimbLadder(ladder);
            ladder.CoolDown = ladder.MaxCoolDown;
        }
        else if (interactable.TryCast<ZiplineConsole>() is { } ziplineConsole)
        {
            if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
            {
                return;
            }
            if (ziplineConsole.zipline != null)
            {
                player.CheckUseZipline(player, ziplineConsole.zipline, ziplineConsole.atTop);
            }
        }
        else if (interactable.TryCast<OpenDoorConsole>() is { } openDoorConsole)
        {
            if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
            {
                return;
            }
            openDoorConsole.myDoor.SetDoorway(true);
        }
        else if (interactable.TryCast<DoorConsole>() is { } doorConsole)
        {
            if (player.AmOwner)
            {
                player.NetTransform.Halt();
                var minigame = Object.Instantiate(doorConsole.MinigamePrefab, Camera.main.transform);
                minigame.transform.localPosition = new Vector3(0f, 0f, -50f);

                try
                {
                    minigame.Cast<IDoorMinigame>().SetDoor(doorConsole.MyDoor);
                }
                catch (InvalidCastException)
                {
                    /* ignored */
                }

                minigame.Begin(null);
            }
        }
        else if (interactable.TryCast<PlatformConsole>() is { } platformConsole)
        {
            if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
            {
                return;
            }
            var platform = platformConsole.Platform;
            if (platform != null)
            {
                var vector = platform.transform.position - player.transform.position;
                if (!platform.Target && vector.magnitude <= 3f)
                {
                    platform.IsDirty = true;
                    platform.StartCoroutine(platform.UsePlatform(player));
                }
            }
        }
        else if (interactable.TryCast<DeconControl>() is { } deconControl)
        {
            if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
            {
                return;
            }
            deconControl.cooldown = 6f;
            if (Constants.ShouldPlaySfx())
            {
                SoundManager.Instance.PlaySound(deconControl.UseSound, false);
            }
            deconControl.OnUse.Invoke();
        }
    }

    public void LobbyStart()
    {
        PuppeteerControlState.ClearAll();

        foreach (var puppetMod in ModifierUtils.GetActiveModifiers<PuppeteerControlModifier>())
        {
            puppetMod.Player.RemoveModifier(puppetMod);
        }
    }
}