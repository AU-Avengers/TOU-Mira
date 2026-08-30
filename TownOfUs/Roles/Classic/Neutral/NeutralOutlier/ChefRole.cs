using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using System.Text;
using TownOfUs.Buttons.Neutral;
using TownOfUs.Events.Neutral;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules.RainbowMod;
using TownOfUs.Modules.TimeLord;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Neutral;

public sealed class ChefRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant, IContinuesGame, IUnlovable, IProgressTally
{
    public void InitialSetup()
    {
        TmpSpriteUtils.CreateSpriteAsset(TouAssets.ChefProgressNone.LoadAsset(),
            "TouMira.Role.Neutral.Chef.Ui.None", 1.45f);
        TmpSpriteUtils.CreateSpriteAsset(TouAssets.ChefProgressBodyNormal.LoadAsset(),
            "TouMira.Role.Neutral.Chef.Ui.BodyNormal", 1.45f);
        TmpSpriteUtils.CreateSpriteAsset(TouAssets.ChefProgressBodyMini.LoadAsset(),
            "TouMira.Role.Neutral.Chef.Ui.BodyMini", 1.45f);
        TmpSpriteUtils.CreateSpriteAsset(TouAssets.ChefProgressBodyFlash.LoadAsset(),
            "TouMira.Role.Neutral.Chef.Ui.BodyFlash", 1.45f);
        TmpSpriteUtils.CreateSpriteAsset(TouAssets.ChefProgressBodyGiant.LoadAsset(),
            "TouMira.Role.Neutral.Chef.Ui.BodyGiant", 1.45f);
        TmpSpriteUtils.CreateSpriteAsset(TouAssets.ChefProgressFedUncolored.LoadAsset(),
            "TouMira.Role.Neutral.Chef.Ui.PlayerUncolored", 1.45f);
        TmpSpriteUtils.CreateSpriteAsset(TouAssets.ChefProgressFedRainbow.LoadAsset(),
            "TouMira.Role.Neutral.Chef.Ui.PlayerRainbow", 1.45f);
    }
    private static string GetIcon(string name)
    {
        return $"<sprite name=\"{name}\">";
    }
    private static string GetIconColored(string name, string color)
    {
        return $"<sprite name=\"{name}\" color=#{color}>";
    }
    public string GetBodyTally()
    {
        var count = OptionGroupSingleton<ChefOptions>.Instance.ServingsNeeded;
        var tally = new StringBuilder();
        var fedPlayers = ModifierUtils.GetPlayersWithModifier<ChefServedModifier>().ToList();
        foreach (var plr in fedPlayers)
        {
            count--;
            if (RainbowUtils.IsRainbow(plr.Data.DefaultOutfit.ColorId))
            {
                tally.Append(GetIcon(_uxIcons[6]));
            }
            else
            {
                tally.Append(GetIconColored(_uxIcons[5], Palette.TextColors[plr.Data.DefaultOutfit.ColorId].ToHtmlStringRGBA()));
            }
        }
        foreach (var body in StoredBodies)
        {
            count--;
            tally.Append(GetIcon(_uxIcons[(int)body.Value]));
        }

        while (count > 0)
        {
            count--;
            tally.Append(GetIcon(_uxIcons[0]));
        }

        return $"({tally})";
    }
    public bool ProgressOnName(bool localDead, bool inMeeting, bool amOwner, out string progress)
    {
        if (amOwner || localDead)
        {
            progress = GetBodyTally();
            return true;
        }

        progress = string.Empty;
        return false;
    }

    public string ProgressOnSummaryNormal => string.Empty;

    public string ProgressOnSummaryDetailed =>
        string.Empty;

    public TallyLocation TallyPlacement(bool inMeeting) => inMeeting ? TallyLocation.Auto : TallyLocation.AboveName;
    private static string[] _uxIcons =
    [
        "TouMira.Role.Neutral.Chef.Ui.None",
        "TouMira.Role.Neutral.Chef.Ui.BodyNormal",
        "TouMira.Role.Neutral.Chef.Ui.BodyMini",
        "TouMira.Role.Neutral.Chef.Ui.BodyFlash",
        "TouMira.Role.Neutral.Chef.Ui.BodyGiant",
        "TouMira.Role.Neutral.Chef.Ui.PlayerUncolored",
        "TouMira.Role.Neutral.Chef.Ui.PlayerRainbow",
    ];
    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        if (!playerControl.AmOwner)
        {
            return;
        }
        ImportantTextTask orCreateTask = PlayerTask.GetOrCreateTask<ImportantTextTask>(playerControl, 0);
        orCreateTask.Text = $"{TownOfUsColors.Neutral.ToTextColor()}{MiraLocaleManager.Get("NeutralOutlierTaskHeader")}</color>";
        orCreateTask.name = "NeutralRoleText";
    }

    public bool IsUnlovable => true;
    public bool ContinuesGame => !Player.HasDied() && StoredBodies.Count != 0 && Helpers.GetAlivePlayers().Any(x => !x.HasModifier<ChefServedModifier>() && x != Player);
    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<ForensicRole>());
    public DoomableType DoomHintType => DoomableType.Death;
    [HideFromIl2Cpp] public List<KeyValuePair<int, PlatterType>> StoredBodies { get; set; } = [];
    public string IdPart => "Chef";
    public string RoleName => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}");
    public string RoleDescription => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.TabDescription");
    private static string _tabCounter = MiraLocaleManager.Get("TownOfUsMira.Role.ChefTabCounter");
    public bool TargetsServed { get; set; }
    public int BodiesServed { get; set; }

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return
            [
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Cook", "Cook"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Cook.WikiDescription"),
                    TouNeutAssets.ChefCookSprite),
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Serve", "Serve"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Serve.WikiDescription"),
                    TouNeutAssets.ChefServeSprites.AsEnumerable().Random()!),
            ];
        }
    }

    public Color RoleColor => TownOfUsColors.Chef;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralOutlier;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Chef.LoadAsset(), "TouMira.Role.Neutral.Chef", 1.45f),
        IntroSound = TouAudio.ChefSound,
        Icon = TouRoleIcons.Chef,
        OptionsScreenshot = TouBanners.NeutralRoleBanner,
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
        stringB.AppendLine(TownOfUsPlugin.Culture, $"<b>{_tabCounter.Replace("<bodiesFed>", $"{BodiesServed}")}</b>");

        return stringB;
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        _tabCounter = MiraLocaleManager.Get("TownOfUsMira.Role.ChefTabCounter").Replace("<bodiesTotal>",
            $"{(int)OptionGroupSingleton<ChefOptions>.Instance.ServingsNeeded}");

        var serveMods = ModifierUtils.GetActiveModifiers<ChefServedModifier>().ToList();
        BodiesServed = serveMods.Count;
        if (BodiesServed >= OptionGroupSingleton<ChefOptions>.Instance.ServingsNeeded)
        {
            TargetsServed = true;
        }

        if (Player.AmOwner)
        {
            CustomButtonSingleton<ChefServeButton>.Instance.UpdateServingType();

            if (!OptionGroupSingleton<ChefOptions>.Instance.ChefArrows)
            {
                return;
            }

            var deadBodies = UnityEngine.Object.FindObjectsOfType<DeadBody>().ToArray();
            foreach (var deadPlayer in PlayerControl.AllPlayerControls.ToArray().Where(x => x.HasDied()))
            {
                if (deadBodies.Select(x => x.ParentId).Contains(deadPlayer.PlayerId))
                {
                    Coroutines.Start(ChefEvents.CoCreateChefArrow(deadPlayer));
                }
            }
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        TouRoleUtils.ClearTaskHeader(Player);

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
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(chef);
            return;
        }
        if (chef.Data.Role is not ChefRole role)
        {
            Error("RpcCookBody - Invalid chef");
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
            // Record Chef cook event for Time Lord rewind system
            var player = MiscUtils.PlayerById(body.ParentId);
            if (player != null)
            {
                TownOfUs.Events.Crewmate.TimeLordEventHandlers.RecordChefCook(chef, body, platter);
            }
            var destroyBody = (BodyVitalsMode)OptionGroupSingleton<GameMechanicOptions>.Instance.CleanedBodiesAppearance.Value;

            if (OptionGroupSingleton<TimeLordOptions>.Instance.UncleanBodiesOnRewind)
            {
                var bodyPlayer = MiscUtils.PlayerById(body.ParentId);
                if (bodyPlayer != null)
                {
                    TownOfUs.Events.Crewmate.TimeLordEventHandlers.RecordBodyCleaned(chef, body, body.transform.position, 
                        TimeLordBodyManager.CleanedBodySource.Janitor);
                }
                Coroutines.Start(TimeLordBodyManager.CoHideBodyForTimeLord(body, destroyBody));
            }
            else
            {
                Coroutines.Start(body.CoCleanCustom(destroyBody));
            }
            // Coroutines.Start(CrimeSceneComponent.CoClean(body));
        }
    }
    [MethodRpc((uint)TownOfUsRpc.ServeBody)]
    public static void RpcServeBody(PlayerControl chef, PlayerControl target)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(chef);
            return;
        }
        if (chef.Data.Role is not ChefRole role)
        {
            Error("RpcServeBody - Invalid chef");
            return;
        }

        if (role.StoredBodies.Count == 0)
        {
            Error("RpcServeBody - No Bodies found!");
            return;
        }

        var platter = role.StoredBodies[0];
        ++role.BodiesServed;
        if (role.BodiesServed >= OptionGroupSingleton<ChefOptions>.Instance.ServingsNeeded)
        {
            role.TargetsServed = true;
        }

        target.AddModifier<ChefServedModifier>(chef, (int)platter.Value, platter.Key);

        TownOfUs.Events.Crewmate.TimeLordEventHandlers.RecordChefServe(chef, target, (byte)platter.Key, platter.Value);

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