using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Modifiers.Impostor;

public sealed class ScavengerArrowModifier(PlayerControl owner, Color color) : ArrowTargetModifier(owner, color, 0)
{
    public override string ModifierName => "Scavenger Arrow";

    public override void OnActivate()
    {
        base.OnActivate();
        var popup = GameManagerCreator.Instance.HideAndSeekManagerPrefab.DeathPopupPrefab;
        var item = Object.Instantiate(popup, HudManager.Instance.transform.parent);
        item.Show(Player, 0);
        if (item.text.transform.TryGetComponent<TextTranslatorTMP>(out var tmp))
        {
            tmp.defaultStr = "Is Your Next Target.";
            tmp.TargetText = StringNames.None;
            tmp.ResetText();
        }
    }
    public override void OnMeetingStart()
    {
        base.OnMeetingStart();
        ModifierComponent!.RemoveModifier(this);
    }
}