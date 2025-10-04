using System.Globalization;
using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Buttons.Neutral;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Neutral;

public sealed class ChefRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public DoomableType DoomHintType => DoomableType.Death;
    [HideFromIl2Cpp] public List<KeyValuePair<int, PlatterType>> StoredBodies { get; set; } = [];
    public string LocaleKey => "Chef";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");
    private static string _tabCounter = TouLocale.GetParsed("TouRoleChefTabCounter");
    public bool TargetsServed { get; set; }
    public int BodiesServed { get; set; }

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Cook", "Cook"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}CookWikiDescription"),
                    TouNeutAssets.IgniteButtonSprite),
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Serve", "Serve"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}ServeWikiDescription"),
                    TouNeutAssets.ChefServeSprites.AsEnumerable().Random()!),
            };
        }
    }

    public Color RoleColor => TownOfUsColors.Chef;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralOutlier;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IntroSound = TouAudio.ToppatIntroSound,
        Icon = TouRoleIcons.Chef,
        MaxRoleCount = 1,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>()
    };

    public bool MetWinCon => TargetsServed;

    public bool WinConditionMet()
    {
        return false;
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);
        stringB.AppendLine(CultureInfo.InvariantCulture, $"<b>{_tabCounter.Replace("<bodiesFed>", $"{BodiesServed}")}</b>");

        return stringB;
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        _tabCounter = TouLocale.GetParsed("TouRoleChefTabCounter").Replace("<bodiesTotal>", $"{(int)OptionGroupSingleton<ChefOptions>.Instance.ServingsNeeded}");

        var serveMods = ModifierUtils.GetActiveModifiers<ChefServedModifier>().ToList();
        BodiesServed = serveMods.Count;
        if (BodiesServed >= OptionGroupSingleton<ChefOptions>.Instance.ServingsNeeded)
        {
            TargetsServed = true;
        }

        if (Player.AmOwner)
        {
            CustomButtonSingleton<ChefServeButton>.Instance.UpdateServingType();
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        if (!Player.HasModifier<BasicGhostModifier>() && TargetsServed)
        {
            Player.AddModifier<BasicGhostModifier>();
        }
    }

    public override bool CanUse(IUsable usable)
    {
        if (!GameManager.Instance.LogicUsables.CanUse(usable, Player))
        {
            return false;
        }

        var console = usable.TryCast<Console>()!;
        return console == null || console.AllowImpostor;
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return TargetsServed;
    }

    [MethodRpc((uint)TownOfUsRpc.CookBody)]
    public static void RpcCookBody(PlayerControl chef, DeadBody body)
    {
        if (chef.Data.Role is not ChefRole role)
        {
            Logger<TownOfUsPlugin>.Error("RpcCookBody - Invalid chef");
            return;
        }

        var target = MiscUtils.PlayerById(body.ParentId);
        var platter = PlatterType.Salmon;
        if (target != null)
        {
            if (target.HasModifier<MiniModifier>())
            {
                platter = PlatterType.Cake;
            }
            else if (target.HasModifier<GiantModifier>())
            {
                platter = PlatterType.Turkey;
            }
            else if (target.HasModifier<FlashModifier>())
            {
                platter = PlatterType.Burger;
            }
        }
        role.StoredBodies.Add(new KeyValuePair<int, PlatterType>(body.ParentId, platter));

        if (body != null)
        {
            /*var touAbilityEvent = new TouAbilityEvent(AbilityType.JanitorClean, player, body);
            MiraEventManager.InvokeEvent(touAbilityEvent);*/

            Coroutines.Start(body.CoClean());
            //Coroutines.Start(CrimeSceneComponent.CoClean(body));
        }
    }
    [MethodRpc((uint)TownOfUsRpc.ServeBody)]
    public static void RpcServeBody(PlayerControl chef, PlayerControl target)
    {
        if (chef.Data.Role is not ChefRole role)
        {
            Logger<TownOfUsPlugin>.Error("RpcServeBody - Invalid chef");
            return;
        }

        if (role.StoredBodies.Count == 0)
        {
            Logger<TownOfUsPlugin>.Error("RpcServeBody - No Bodies found!");
            return;
        }

        var platter = role.StoredBodies[0];
        ++role.BodiesServed;
        if (role.BodiesServed >= OptionGroupSingleton<ChefOptions>.Instance.ServingsNeeded)
        {
            role.TargetsServed = true;
        }

        target.AddModifier<ChefServedModifier>(chef, (int)platter.Value, platter.Key);
        
        role.StoredBodies.RemoveAt(0);
    }
}

public enum PlatterType
{
    Empty,
    Salmon,
    Cake,
    Burger,
    Turkey
}