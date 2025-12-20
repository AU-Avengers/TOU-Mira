using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.BaseFreeplay;

/// <summary>
/// Freeplay-only debug button to apply/toggle specific modifiers.
/// </summary>
public sealed class FreeplaySetModifiersButton : TownOfUsButton
{
    public override string Name => TouLocale.GetParsed("FreeplaySetModifiersButton", "Set Modifiers");
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => 0.001f;
    public override float InitialCooldown => 0.001f;
    public override float EffectDuration => 0.001f;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;
    public override LoadableAsset<Sprite> Sprite => TouAssets.FreeplayModifierSprite;

    public override bool ZeroIsInfinite { get; set; } = true;
    public override bool UsableInDeath => true;

    public override bool Enabled(RoleBehaviour? role)
    {
        return PlayerControl.LocalPlayer != null && TutorialManager.InstanceExists;
    }

    public override void ClickHandler()
    {
        if (!CanClick())
        {
            return;
        }

        OnClick();
    }

    protected override void OnClick()
    {
        PlayerControl.LocalPlayer.NetTransform.Halt();

        if (Minigame.Instance)
        {
            return;
        }

        var playerMenu = CustomPlayerMenu.Create();
        playerMenu.transform.FindChild("PhoneUI").GetChild(0).GetComponent<SpriteRenderer>().material =
            PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;
        playerMenu.transform.FindChild("PhoneUI").GetChild(1).GetComponent<SpriteRenderer>().material =
            PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;

        playerMenu.Begin(
            _ => true,
            plr =>
            {
                playerMenu.ForceClose();
                if (plr == null)
                {
                    return;
                }

                var modMenu = GuesserMenu.Create();

                bool IsModifierVisibleForPlayer(BaseModifier modifier)
                {
                    if (modifier is LoverModifier)
                    {
                        return true;
                    }

                    var modifierType = modifier.GetType();

                    // If the player currently has this modifier type, show it so it can be removed.
                    if (plr.GetModifiers<BaseModifier>().Any(x => x.GetType() == modifierType))
                    {
                        return true;
                    }

                    // Otherwise, only show modifiers we can add without extra parameters.
                    return modifierType.GetConstructor(Type.EmptyTypes) != null;
                }

                modMenu.Begin(
                    _ => false,
                    _ => { },
                    IsModifierVisibleForPlayer,
                    modifier => ApplyModifier(plr, modifier, modMenu));
            });

        foreach (var panel in playerMenu.potentialVictims)
        {
            if (panel.NameText.text != PlayerControl.LocalPlayer.Data.PlayerName)
            {
                panel.NameText.color = Color.white;
            }
        }
    }

    private static void ApplyModifier(PlayerControl target, BaseModifier modifier, Minigame modMenu)
    {
        modMenu.ForceClose();

        if (modifier is LoverModifier)
        {
            OpenLoversPicker(target);
            return;
        }

        //ReviveIfDead(target);

        ToggleModifierByType(target, modifier.GetType());
    }

    private static void ToggleModifierByType(PlayerControl player, Type modifierType)
    {
        var comp = player.GetModifierComponent();
        if (comp == null)
        {
            return;
        }

        var existing = player.GetModifiers<BaseModifier>().Where(x => x.GetType() == modifierType).ToList();
        if (existing.Count > 0)
        {
            foreach (var mod in existing)
            {
                comp.RemoveModifier(mod);
            }
            return;
        }

        // Only support modifiers that can be constructed without parameters.
        if (modifierType.GetConstructor(Type.EmptyTypes) == null)
        {
            // This shouldn't normally be reachable because we hide non-addable modifiers
            // unless they are already present (in which case we'd remove above).
            HudManager.Instance.ShowPopUp($"Cannot add '{modifierType.Name}' - this modifier requires parameters.");
            return;
        }

        if (Activator.CreateInstance(modifierType) is BaseModifier instance)
        {
            comp.AddModifier(instance);
        }
    }

    private static void OpenLoversPicker(PlayerControl loverA)
    {
        if (loverA == null || loverA.Data == null || loverA.Data.Disconnected)
        {
            return;
        }

        ReviveIfDead(loverA);

        var player2Menu = CustomPlayerMenu.Create();
        player2Menu.transform.FindChild("PhoneUI").GetChild(0).GetComponent<SpriteRenderer>().material =
            loverA.cosmetics.currentBodySprite.BodySprite.material;
        player2Menu.transform.FindChild("PhoneUI").GetChild(1).GetComponent<SpriteRenderer>().material =
            loverA.cosmetics.currentBodySprite.BodySprite.material;

        player2Menu.Begin(
            plr2 => plr2 != null && !plr2.Data.Disconnected && plr2 != loverA,
            plr2 =>
            {
                player2Menu.ForceClose();
                if (plr2 == null)
                {
                    return;
                }

                ReviveIfDead(plr2);
                LoverModifier.DebugSetLovers(loverA, plr2, clearExisting: true);
            });

        foreach (var panel in player2Menu.potentialVictims)
        {
            if (panel.NameText.text != PlayerControl.LocalPlayer.Data.PlayerName)
            {
                panel.NameText.color = Color.white;
            }
        }
    }

    private static void ReviveIfDead(PlayerControl plr)
    {
        if (!plr.HasDied())
        {
            return;
        }

        var body = UnityEngine.Object.FindObjectsOfType<DeadBody>().FirstOrDefault(b => b.ParentId == plr.PlayerId);
        var position = new Vector2(PlayerControl.LocalPlayer.transform.localPosition.x,
            PlayerControl.LocalPlayer.transform.localPosition.y);

        if (body != null)
        {
            position = new Vector2(body.transform.localPosition.x, body.transform.localPosition.y + 0.3636f);
            UnityEngine.Object.Destroy(body.gameObject);
        }

        GameHistory.ClearMurder(plr);
        plr.Revive();
        plr.transform.position = new Vector2(position.x, position.y);
    }
}


