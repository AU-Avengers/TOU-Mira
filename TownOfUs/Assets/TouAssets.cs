using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using UnityEngine;

namespace TownOfUs.Assets;

public static class TouAssets
{
    private const string ShortPath = "TownOfUs.Resources";
    private const string CounterPath = "TownOfUs.Resources.AbilityCounters";

    public static readonly AssetBundle MainBundle = AssetBundleManager.Load("tou-assets");

    public static readonly LoadableAsset<GameObject> RoleSelectionGame =
        new LoadableBundleAsset<GameObject>("SelectRoleGame", MainBundle);
    
    public static readonly LoadableAsset<GameObject> AltRoleSelectionGame =
        new LoadableBundleAsset<GameObject>("AmbassadorRoleGame", MainBundle);

    public static LoadableAsset<GameObject> WikiPrefab { get; } =
        new LoadableBundleAsset<GameObject>("IngameWiki", MainBundle);

    public static LoadableAsset<GameObject> FirstRoundShield { get; } =
        new LoadableBundleAsset<GameObject>("FirstRoundShield", MainBundle);

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

    public static LoadableAsset<Sprite> Banner { get; } = new LoadableResourceAsset($"{ShortPath}.Banner.png");

    public static LoadableAsset<Sprite> BlankSprite { get; } =
        new LoadableResourceAsset($"{ShortPath}.BlankSprite.png");

    public static LoadableAsset<Sprite> WikiButton { get; } = new LoadableResourceAsset($"{ShortPath}.WikiButton.png");

    public static LoadableAsset<Sprite> WikiButtonActive { get; } =
        new LoadableResourceAsset($"{ShortPath}.WikiButtonActive.png");

    public static LoadableAsset<Sprite> ZoomPlus { get; } = new LoadableResourceAsset($"{ShortPath}.Plus.png");
    public static LoadableAsset<Sprite> ZoomMinus { get; } = new LoadableResourceAsset($"{ShortPath}.Minus.png");

    public static LoadableAsset<Sprite> ZoomPlusActive { get; } =
        new LoadableResourceAsset($"{ShortPath}.PlusActive.png");

    public static LoadableAsset<Sprite> ZoomMinusActive { get; } =
        new LoadableResourceAsset($"{ShortPath}.MinusActive.png");

    public static LoadableAsset<Sprite> TeamChatInactive { get; } =
        new LoadableResourceAsset($"{ShortPath}.TeamChatInactive.png");

    public static LoadableAsset<Sprite> TeamChatActive { get; } =
        new LoadableResourceAsset($"{ShortPath}.TeamChatActive.png");

    public static LoadableAsset<Sprite> TeamChatSelected { get; } =
        new LoadableResourceAsset($"{ShortPath}.TeamChatSelected.png");

    public static LoadableAsset<Sprite> BarryButtonSprite { get; } =
        new LoadableResourceAsset($"{ShortPath}.BarryButton.png");

    public static LoadableAsset<Sprite> BroadcastSprite { get; } =
        new LoadableResourceAsset($"{ShortPath}.BroadcastButton.png");

    public static LoadableAsset<Sprite> DisperseSprite { get; } =
        new LoadableResourceAsset($"{ShortPath}.DisperseButton.png");

    public static LoadableAsset<Sprite> VitalsSprite { get; } =
        new LoadableResourceAsset($"{ShortPath}.VitalsButton.png");

    public static LoadableAsset<Sprite> CameraSprite { get; } = new LoadableResourceAsset($"{ShortPath}.CamButton.png");

    public static LoadableAsset<Sprite> AdminSprite { get; } =
        new LoadableResourceAsset($"{ShortPath}.AdminButton.png");

    public static LoadableAsset<Sprite> KillSprite { get; } = new LoadableResourceAsset($"{ShortPath}.KillButton.png");
    public static LoadableAsset<Sprite> VentSprite { get; } = new LoadableResourceAsset($"{ShortPath}.VentButton.png");
    public static LoadableAsset<Sprite> RangeSprite { get; } = new LoadableResourceAsset($"{ShortPath}.Range.png");

    public static LoadableAsset<Sprite> HysteriaSprite { get; } =
        new LoadableResourceAsset($"{ShortPath}.Hysteria.png", 300);

    public static LoadableAsset<Sprite> HysteriaCleanSprite { get; } =
        new LoadableResourceAsset($"{ShortPath}.HysteriaClean.png", 300);

    public static LoadableAsset<Sprite> ShootMeetingSprite { get; } =
        new LoadableResourceAsset($"{ShortPath}.Shoot.png", 300);

    public static LoadableAsset<Sprite> BlackmailLetterSprite { get; } =
        new LoadableResourceAsset($"{ShortPath}.BlackmailLetter.png");

    public static LoadableAsset<Sprite> BlackmailOverlaySprite { get; } =
        new LoadableResourceAsset($"{ShortPath}.BlackmailOverlay.png");

    public static LoadableAsset<Sprite> FootprintSprite { get; } =
        new LoadableResourceAsset($"{ShortPath}.Footprint.png");

    public static LoadableAsset<Sprite> CircleSprite { get; } =
        new LoadableResourceAsset($"{ShortPath}.Circle.png", 512);

    public static LoadableAsset<Sprite> SwapActive { get; } =
        new LoadableResourceAsset($"{ShortPath}.SwapActive.png", 300);

    public static LoadableAsset<Sprite> SwapInactive { get; } =
        new LoadableResourceAsset($"{ShortPath}.SwapDisabled.png", 300);

    public static LoadableAsset<Sprite> RevealButtonSprite { get; } =
        new LoadableResourceAsset($"{ShortPath}.Reveal.png", 300);

    public static LoadableAsset<Sprite> RevealCleanSprite { get; } =
        new LoadableResourceAsset($"{ShortPath}.RevealClean.png", 300);

    public static LoadableAsset<Sprite> Guess { get; } = new LoadableResourceAsset($"{ShortPath}.Guess.png", 300);
    public static LoadableAsset<Sprite> InJailSprite { get; } = new LoadableResourceAsset($"{ShortPath}.InJail.png");

    public static LoadableAsset<Sprite> JailCellSprite { get; } =
        new LoadableResourceAsset($"{ShortPath}.JailCell.png");

    public static LoadableAsset<Sprite> ImitateSelectSprite { get; } =
        new LoadableResourceAsset($"{ShortPath}.ImitateSelect.png", 300);

    public static LoadableAsset<Sprite> ImitateDeselectSprite { get; } =
        new LoadableResourceAsset($"{ShortPath}.ImitateDeselect.png", 300);

    public static LoadableAsset<Sprite> ExecuteSprite { get; } =
        new LoadableResourceAsset($"{ShortPath}.Execute.png", 254);

    public static LoadableAsset<Sprite> ExecuteCleanSprite { get; } =
        new LoadableResourceAsset($"{ShortPath}.ExecuteClean.png", 254);

    public static LoadableAsset<Sprite> RetrainSprite { get; } =
        new LoadableResourceAsset($"{ShortPath}.Retrain.png", 300);
    
    public static LoadableAsset<Sprite> RetrainCleanSprite { get; } =
        new LoadableResourceAsset($"{ShortPath}.RetrainClean.png", 300);
    
    public static LoadableAsset<Sprite> TribunalSprite { get; } =
        new LoadableResourceAsset($"{ShortPath}.Tribunal.png", 300);
    
    public static LoadableAsset<Sprite> TribunalClearSprite { get; } =
        new LoadableResourceAsset($"{ShortPath}.TribunalClean.png", 300);
    
    public static LoadableAsset<Sprite> Hacked { get; } = new LoadableResourceAsset($"{ShortPath}.Hacked.png");

    public static LoadableAsset<Sprite> BarricadeVentSprite { get; } =
        new LoadableResourceAsset($"{ShortPath}.BarricadeVent.png", 200);

    public static LoadableAsset<Sprite> BarricadeFungleSprite { get; } =
        new LoadableResourceAsset($"{ShortPath}.BarricadePlant.png", 200);

    public static LoadableAsset<Sprite> LighterSprite { get; } = new LoadableResourceAsset($"{ShortPath}.Lighter.png");
    public static LoadableAsset<Sprite> DarkerSprite { get; } = new LoadableResourceAsset($"{ShortPath}.Darker.png");

    public static LoadableAsset<Sprite> AuAvengersSprite { get; } =
        new LoadableResourceAsset($"{ShortPath}.AuAvengers.png", 290);

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
            var name = "Default";
            switch (TownOfUsPlugin.ArrowStyle.Value)
            {
                case 1:
                    name = "Dark Glow";
                    break;
                case 2:
                    name = "Color Glow";
                    break;
                case 3:
                    name = "Legacy";
                    break;
            }
            return name;
        }
    }

    public static LoadableAsset<Sprite> ArrowBasicSprite { get; } = new LoadableResourceAsset($"{ShortPath}.Arrow.png", 110);
    public static LoadableAsset<Sprite> ArrowDarkOutSprite { get; } = new LoadableResourceAsset($"{ShortPath}.ArrowDarkOut.png", 110);
    public static LoadableAsset<Sprite> ArrowLightOutSprite { get; } = new LoadableResourceAsset($"{ShortPath}.ArrowLightOut.png", 110);
    
    public static LoadableAsset<Sprite> ArrowLegacySprite { get; } =
        new LoadableResourceAsset($"{ShortPath}.ArrowLegacy.png");

    public static LoadableAsset<Sprite> CrimeSceneSprite { get; } =
        new LoadableResourceAsset($"{ShortPath}.CrimeScene.png");

    public static LoadableAsset<Sprite> ScreenFlash { get; } =
        new LoadableResourceAsset($"{ShortPath}.ScreenFlash.png");

    public static LoadableAsset<Sprite> KillBG { get; } = new LoadableResourceAsset($"{ShortPath}.KillBackground.png");

    public static LoadableAsset<Sprite> RetributionBG { get; } =
        new LoadableResourceAsset($"{ShortPath}.RetributionBackground.png");

    public static LoadableAsset<Sprite> AbilityCounterPlayerSprite { get; } =
        new LoadableResourceAsset($"{CounterPath}.Player.png");

    public static LoadableAsset<Sprite> AbilityCounterVentSprite { get; } =
        new LoadableResourceAsset($"{CounterPath}.Vent.png");

    public static LoadableAsset<Sprite> AbilityCounterBodySprite { get; } =
        new LoadableResourceAsset($"{CounterPath}.Body.png");

    public static LoadableAsset<Sprite> AbilityCounterBasicSprite { get; } =
        new LoadableResourceAsset($"{CounterPath}.Basic.png");

    public static LoadableAsset<Sprite> GameSummarySprite { get; } =
        new LoadableResourceAsset($"{ShortPath}.GameSummaryButton.png");

    public static LoadableAsset<Sprite> MapVentSprite { get; } =
        new LoadableResourceAsset($"{ShortPath}.MapVent.png", 350);

    public static LoadableAsset<Sprite> MapBodySprite { get; } =
        new LoadableResourceAsset($"{ShortPath}.MapBody.png", 350);

    public static LoadableAsset<Sprite> WikiBgSprite { get; } = new LoadableResourceAsset($"{ShortPath}.WikiBg.png");

    public static LoadableAsset<Sprite> TimerDrawSprite { get; } =
        new LoadableResourceAsset($"{ShortPath}.TimerDraw.png", 300);

    public static LoadableAsset<Sprite> TimerImpSprite { get; } =
        new LoadableResourceAsset($"{ShortPath}.TimerImp.png", 300);

    public static void Initialize()
    {
        AuAvengersAnims.Initialize();
    }
}