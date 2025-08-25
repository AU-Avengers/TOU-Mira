using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using UnityEngine;

namespace TownOfUs.Assets;

public static class TouAssets
{
    private const string ShortPath = "TownOfUs.Resources";
    private const string CounterPath = "TownOfUs.Resources.AbilityCounters";

    public static readonly AssetBundle MainBundle = AssetBundleManager.Load("tou-assets");

    public static LoadableAsset<Sprite> Banner { get; } = new LoadableResourceAsset($"{ShortPath}.Banner.png");
    
    public static LoadableAsset<Sprite> FoolsMenuSprite(int value)
    {
            var sprite = FoolsNormal;
            switch (value)
            {
                case 1:
                    sprite = FoolsHorse;
                    break;
                case 2:
                    sprite = FoolsLong;
                    break;
                case 3:
                    sprite = FoolsLongHorse;
                    break;
            }
            return sprite;
    }
    public static LoadableAsset<Sprite> FoolsNormal { get; } =
        new LoadableResourceAsset($"{ShortPath}.AprilFools.Normal.png");
    public static LoadableAsset<Sprite> FoolsHorse { get; } =
        new LoadableResourceAsset($"{ShortPath}.AprilFools.Horse.png");
    public static LoadableAsset<Sprite> FoolsLong { get; } =
        new LoadableResourceAsset($"{ShortPath}.AprilFools.Long.png");
    public static LoadableAsset<Sprite> FoolsLongHorse { get; } =
        new LoadableResourceAsset($"{ShortPath}.AprilFools.LongHorse.png");

    public static LoadableAsset<Sprite> BlankSprite { get; } =
        new LoadableResourceAsset($"{ShortPath}.BlankSprite.png");
    public static LoadableAsset<Sprite> RangeSprite { get; } = new LoadableResourceAsset($"{ShortPath}.Range.png");
    public static LoadableAsset<Sprite> CircleSprite { get; } =
        new LoadableResourceAsset($"{ShortPath}.Circle.png", 512);

    public static LoadableAsset<Sprite> AuAvengersSprite { get; } =
        new LoadableResourceAsset($"{ShortPath}.AuAvengers.png", 290);
    public static LoadableAsset<Sprite> AbilityCounterPlayerSprite { get; } =
        new LoadableResourceAsset($"{CounterPath}.Player.png");

    public static LoadableAsset<Sprite> AbilityCounterVentSprite { get; } =
        new LoadableResourceAsset($"{CounterPath}.Vent.png");

    public static LoadableAsset<Sprite> AbilityCounterBodySprite { get; } =
        new LoadableResourceAsset($"{CounterPath}.Body.png");

    public static LoadableAsset<Sprite> AbilityCounterBasicSprite { get; } =
        new LoadableResourceAsset($"{CounterPath}.Basic.png");
    
    public static readonly LoadableAsset<GameObject> RoleSelectionGame =
        new LoadableBundleAsset<GameObject>("SelectRoleGame", MainBundle);
    
    public static readonly LoadableAsset<GameObject> AltRoleSelectionGame =
        new LoadableBundleAsset<GameObject>("AmbassadorRoleGame", MainBundle);
    
    public static readonly LoadableAsset<GameObject> ConfirmMinigame =
        new LoadableBundleAsset<GameObject>("AmbassadorConfirmGame", MainBundle);

    public static LoadableAsset<GameObject> WikiPrefab { get; } =
        new LoadableBundleAsset<GameObject>("IngameWiki", MainBundle);

    public static LoadableAsset<GameObject> FirstRoundShield { get; } =
        new LoadableBundleAsset<GameObject>("FirstRoundShieldBubble", MainBundle);

    public static LoadableAsset<GameObject> ClericBarrier { get; } =
        new LoadableBundleAsset<GameObject>("ClericBarrier", MainBundle);

    public static LoadableAsset<GameObject> MedicShield { get; } =
        new LoadableBundleAsset<GameObject>("MedicShield", MainBundle);
    
    public static LoadableAsset<GameObject> WardenFort { get; } =
        new LoadableBundleAsset<GameObject>("WardenFort", MainBundle);

    public static LoadableAsset<GameObject> EclipsedPrefab { get; } =
        new LoadableBundleAsset<GameObject>("Eclipsed", MainBundle);
    
    public static LoadableAsset<GameObject> AmbushPrefab { get; } =
        new LoadableBundleAsset<GameObject>("Ambush", MainBundle);

    public static LoadableAsset<GameObject> EscapistMarkPrefab { get; } =
        new LoadableBundleAsset<GameObject>("EscapistMark", MainBundle);

    public static LoadableAsset<GameObject> MeetingDeathPrefab { get; } =
        new LoadableBundleAsset<GameObject>("DeathAnimation", MainBundle);

    public static LoadableAsset<GameObject> MayorRevealPrefab { get; set; } =
        new LoadableBundleAsset<GameObject>("MayorReveal", MainBundle);
    
    public static LoadableAsset<GameObject> MayorPostRevealPrefab { get; set; } =
        new LoadableBundleAsset<GameObject>("MayorPostReveal", MainBundle);

    public static LoadableAsset<AnimationClip> MeetingDeathAnim1 { get; } =
        new LoadableBundleAsset<AnimationClip>("DeathMeetingShotFront", MainBundle);

    public static LoadableAsset<AnimationClip> MeetingDeathBloodAnim1 { get; } =
        new LoadableBundleAsset<AnimationClip>("DeathMeetingShotFrontBlood", MainBundle);

    public static LoadableAsset<AnimationClip> MeetingDeathAnim2 { get; } =
        new LoadableBundleAsset<AnimationClip>("DeathMeetingShotRight", MainBundle);

    public static LoadableAsset<AnimationClip> MeetingDeathBloodAnim2 { get; } =
        new LoadableBundleAsset<AnimationClip>("DeathMeetingShotRightBlood", MainBundle);

    public static LoadableAsset<AnimationClip> MeetingDeathAnim3 { get; } =
        new LoadableBundleAsset<AnimationClip>("DeathMeetingBody", MainBundle);

    public static LoadableAsset<AnimationClip> MeetingDeathBloodAnim3 { get; } =
        new LoadableBundleAsset<AnimationClip>("DeathMeetingBodyBlood", MainBundle);

    public static LoadableAsset<GameObject> ScatterUI { get; } =
        new LoadableBundleAsset<GameObject>("ScatterUI", MainBundle);

    public static LoadableAsset<Sprite> MenuOption { get; } =
        new LoadableBundleAsset<Sprite>("MenuOption.png", MainBundle);
        
    public static LoadableAsset<Sprite> MenuOptionActive { get; } =
        new LoadableBundleAsset<Sprite>("MenuOptionActive.png", MainBundle);
    
    public static LoadableAsset<Sprite> WikiButton { get; } = new LoadableBundleAsset<Sprite>("WikiButton.png", MainBundle);

    public static LoadableAsset<Sprite> WikiButtonActive { get; } =
        new LoadableBundleAsset<Sprite>("WikiButtonActive.png", MainBundle);

    public static LoadableAsset<Sprite> ZoomPlus { get; } = new LoadableBundleAsset<Sprite>("Plus.png", MainBundle);
    public static LoadableAsset<Sprite> ZoomMinus { get; } = new LoadableBundleAsset<Sprite>("Minus.png", MainBundle);

    public static LoadableAsset<Sprite> ZoomPlusActive { get; } =
        new LoadableBundleAsset<Sprite>("PlusActive", MainBundle);

    public static LoadableAsset<Sprite> ZoomMinusActive { get; } =
        new LoadableBundleAsset<Sprite>("MinusActive", MainBundle);

    public static LoadableAsset<Sprite> TeamChatInactive { get; } =
        new LoadableBundleAsset<Sprite>("TeamChatInactive", MainBundle);

    public static LoadableAsset<Sprite> TeamChatActive { get; } =
        new LoadableBundleAsset<Sprite>("TeamChatActive", MainBundle);

    public static LoadableAsset<Sprite> TeamChatSelected { get; } =
        new LoadableBundleAsset<Sprite>("TeamChatSelected", MainBundle);

    public static LoadableAsset<Sprite> BarryButtonSprite { get; } =
        new LoadableBundleAsset<Sprite>("BarryButton", MainBundle);

    public static LoadableAsset<Sprite> BroadcastSprite { get; } =
        new LoadableBundleAsset<Sprite>("BroadcastButton", MainBundle);

    public static LoadableAsset<Sprite> DisperseSprite { get; } =
        new LoadableBundleAsset<Sprite>("DisperseButton", MainBundle);

    public static LoadableAsset<Sprite> VitalsSprite { get; } =
        new LoadableBundleAsset<Sprite>("VitalsButton", MainBundle);

    public static LoadableAsset<Sprite> CameraSprite { get; } = new LoadableBundleAsset<Sprite>("CamButton", MainBundle);

    public static LoadableAsset<Sprite> AdminSprite { get; } =
        new LoadableBundleAsset<Sprite>("AdminButton", MainBundle);

    public static LoadableAsset<Sprite> KillSprite { get; } = new LoadableBundleAsset<Sprite>("KillButton", MainBundle);
    public static LoadableAsset<Sprite> VentSprite { get; } = new LoadableBundleAsset<Sprite>("VentButton", MainBundle);

    public static LoadableAsset<Sprite> HysteriaSprite { get; } =
        new LoadableBundleAsset<Sprite>("Hysteria.png", MainBundle);

    public static LoadableAsset<Sprite> HysteriaCleanSprite { get; } =
        new LoadableBundleAsset<Sprite>("HysteriaClean.png", MainBundle);

    public static LoadableAsset<Sprite> ShootMeetingSprite { get; } =
        new LoadableBundleAsset<Sprite>("Shoot.png", MainBundle);

    public static LoadableAsset<Sprite> BlackmailLetterSprite { get; } =
        new LoadableBundleAsset<Sprite>("BlackmailLetter", MainBundle);

    public static LoadableAsset<Sprite> BlackmailOverlaySprite { get; } =
        new LoadableBundleAsset<Sprite>("BlackmailOverlay", MainBundle);

    public static LoadableAsset<Sprite> FootprintSprite { get; } =
        new LoadableBundleAsset<Sprite>("Footprint", MainBundle);

    public static LoadableAsset<Sprite> SwapActive { get; } =
        new LoadableBundleAsset<Sprite>("SwapActive.png", MainBundle);

    public static LoadableAsset<Sprite> SwapInactive { get; } =
        new LoadableBundleAsset<Sprite>("SwapDisabled.png", MainBundle);

    public static LoadableAsset<Sprite> RevealButtonSprite { get; } =
        new LoadableBundleAsset<Sprite>("Reveal.png", MainBundle);

    public static LoadableAsset<Sprite> RevealCleanSprite { get; } =
        new LoadableBundleAsset<Sprite>("RevealClean.png", MainBundle);

    public static LoadableAsset<Sprite> Guess { get; } = new LoadableBundleAsset<Sprite>("Guess.png", MainBundle);
    public static LoadableAsset<Sprite> InJailSprite { get; } = new LoadableBundleAsset<Sprite>("InJail", MainBundle);

    public static LoadableAsset<Sprite> JailCellSprite { get; } =
        new LoadableBundleAsset<Sprite>("JailCell", MainBundle);

    public static LoadableAsset<Sprite> ImitateSelectSprite { get; } =
        new LoadableBundleAsset<Sprite>("ImitateSelect.png", MainBundle);

    public static LoadableAsset<Sprite> ImitateDeselectSprite { get; } =
        new LoadableBundleAsset<Sprite>("ImitateDeselect.png", MainBundle);

    public static LoadableAsset<Sprite> ExecuteSprite { get; } =
        new LoadableBundleAsset<Sprite>("Execute.png", MainBundle);

    public static LoadableAsset<Sprite> ExecuteCleanSprite { get; } =
        new LoadableBundleAsset<Sprite>("ExecuteClean.png", MainBundle);

    public static LoadableAsset<Sprite> RetrainSprite { get; } =
        new LoadableBundleAsset<Sprite>("Retrain.png", MainBundle);
    
    public static LoadableAsset<Sprite> RetrainCleanSprite { get; } =
        new LoadableBundleAsset<Sprite>("RetrainClean.png", MainBundle);
    
    public static LoadableAsset<Sprite> Hacked { get; } = new LoadableBundleAsset<Sprite>("Hacked", MainBundle);

    public static LoadableAsset<Sprite> BarricadeVentSprite { get; } =
        new LoadableBundleAsset<Sprite>("BarricadeVent1.png", MainBundle);
    public static LoadableAsset<Sprite> BarricadeVentSprite2 { get; } =
        new LoadableBundleAsset<Sprite>("BarricadeVent2.png", MainBundle);
    public static LoadableAsset<Sprite> BarricadeVentSprite3 { get; } =
        new LoadableBundleAsset<Sprite>("BarricadeVent3.png", MainBundle);

    public static LoadableAsset<Sprite> BarricadeFungleSprite { get; } =
        new LoadableBundleAsset<Sprite>("BarricadePlant.png", MainBundle);

    public static LoadableAsset<Sprite> LighterSprite { get; } = new LoadableBundleAsset<Sprite>("Lighter", MainBundle);
    public static LoadableAsset<Sprite> DarkerSprite { get; } = new LoadableBundleAsset<Sprite>("Darker", MainBundle);

    public static LoadableAsset<Sprite> ArrowSprite
    {
        get
        {
            var sprite = ArrowBasicSprite;
            switch (TownOfUsPlugin.ArrowStyle.Value)
            {
                case 1:
                    sprite = ArrowDarkOutSprite;
                    break;
                case 2:
                    sprite = ArrowLightOutSprite;
                    break;
                case 3:
                    sprite = ArrowLegacySprite;
                    break;
            }
            return sprite;
        }
    }
    public static string ArrowSpriteName
    {
        get
        {
            var name = TouLocale.Get(TouNames.ArrowDefault);
            switch (TownOfUsPlugin.ArrowStyle.Value)
            {
                case 1:
                    name = TouLocale.Get(TouNames.ArrowDarkGlow);
                    break;
                case 2:
                    name = TouLocale.Get(TouNames.ArrowColorGlow);
                    break;
                case 3:
                    name = TouLocale.Get(TouNames.ArrowLegacy);
                    break;
            }
            return name;
        }
    }

    public static LoadableAsset<Sprite> ArrowBasicSprite { get; } = new LoadableBundleAsset<Sprite>("Arrow.png", MainBundle);
    public static LoadableAsset<Sprite> ArrowDarkOutSprite { get; } = new LoadableBundleAsset<Sprite>("ArrowDarkOut.png", MainBundle);
    public static LoadableAsset<Sprite> ArrowLightOutSprite { get; } = new LoadableBundleAsset<Sprite>("ArrowLightOut.png", MainBundle);
    
    public static LoadableAsset<Sprite> ArrowLegacySprite { get; } =
        new LoadableBundleAsset<Sprite>("ArrowLegacy", MainBundle);

    public static LoadableAsset<Sprite> CrimeSceneSprite { get; } =
        new LoadableBundleAsset<Sprite>("CrimeScene", MainBundle);

    public static LoadableAsset<Sprite> ScreenFlash { get; } =
        new LoadableBundleAsset<Sprite>("ScreenFlash", MainBundle);

    public static LoadableAsset<Sprite> KillBG { get; } = new LoadableBundleAsset<Sprite>("KillBackground", MainBundle);

    public static LoadableAsset<Sprite> RetributionBG { get; } =
        new LoadableBundleAsset<Sprite>("RetributionBackground", MainBundle);

    public static LoadableAsset<Sprite> GameSummarySprite { get; } =
        new LoadableBundleAsset<Sprite>("GameSummaryButton", MainBundle);

    public static LoadableAsset<Sprite> MapVentSprite { get; } =
        new LoadableBundleAsset<Sprite>("MapVent.png", MainBundle);

    public static LoadableAsset<Sprite> MapBodySprite { get; } =
        new LoadableBundleAsset<Sprite>("MapBody.png", MainBundle);

    public static LoadableAsset<Sprite> WikiBgSprite { get; } = new LoadableBundleAsset<Sprite>("WikiBg", MainBundle);

    public static LoadableAsset<Sprite> TimerDrawSprite { get; } =
        new LoadableBundleAsset<Sprite>("TimerDraw.png", MainBundle);

    public static LoadableAsset<Sprite> TimerImpSprite { get; } =
        new LoadableBundleAsset<Sprite>("TimerImp.png", MainBundle);

    public static void Initialize()
    {
        AuAvengersAnims.Initialize();
    }
}