using AmongUs.Data;
using HarmonyLib;
using TownOfUs.Modules.Components;
using TownOfUs.Modules.Cosmetics;
using UnityEngine;
using UnityEngine.Events;

namespace TownOfUs.Patches.Cosmetics;

#pragma warning disable S3398
public static class PetsTabPatches
{
    private static InventoryTabPaginationBehaviour _pagination = null!;

    private static string GetText()
    {
        string name;
        if (!_pagination || _pagination.CurrentTab == 0)
        {
            name = TranslationController.Instance.GetString(StringNames.PetLabel);
        }
        else
        {
            name = CosmeticsLoader.Instance.PetGroups.GetGroupNameByIndex(_pagination.CurrentTab - 1);
        }

        var max = CosmeticsLoader.Instance.PetGroups.Count;
        return $"{name} ({_pagination.CurrentTab} / {max})";
    }

    private static bool ShowOnPage(string id)
    {
        if (!_pagination) return true;
        
        if (_pagination.CurrentTab == 0) return !id.StartsWith("toum", StringComparison.InvariantCulture);

        if (!id.StartsWith("toum", StringComparison.InvariantCulture)) return false;
        
        var group = Names.GetGroup(id);
        var currentGroup = CosmeticsLoader.Instance.PetGroups.GetGroupIdByIndex(_pagination.CurrentTab - 1);

        return currentGroup == group;
    }

    [HarmonyPatch(typeof(PetsTab), nameof(PetsTab.OnEnable))]
    public static class PetsTabOnEnablePatch
    {
        public static bool Prefix(PetsTab __instance)
        {
            __instance.initialized = false;
            // --------- Pagination ----------------
            _pagination = __instance.GetComponent<InventoryTabPaginationBehaviour>();
            if (!_pagination)
            {
                _pagination = __instance.gameObject.AddComponent<InventoryTabPaginationBehaviour>();
            }

            _pagination.Setup(
                __instance,
                CosmeticsLoader.Instance.PetGroups.Count,
                GetText);

            // ---------- Original Game Code -----------
            InventoryTabReversePatch.OnEnable(__instance);

            PetData[] unlockedPets = HatManager.Instance.GetUnlockedPets();

            var num = 0;
            foreach (var hat in unlockedPets)
            {
                if (!ShowOnPage(hat.ProductId)) continue;

                var num2 = __instance.XRange.Lerp(num % __instance.NumPerRow / (__instance.NumPerRow - 1f));
                var num3 = __instance.YStart - num / __instance.NumPerRow * __instance.YOffset;
                var colorChip = UnityEngine.Object.Instantiate(__instance.ColorTabPrefab, __instance.scroller.Inner);
                colorChip.transform.localPosition = new Vector3(num2, num3, -1f);
                colorChip.PlayerEquippedForeground.SetActive(hat.ProdId == DataManager.Player.Customization.Pet);
                colorChip.SelectionHighlight.gameObject.SetActive(false);
                if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Keyboard)
                {
                    colorChip.Button.OnMouseOver.AddListener((UnityAction)(()=>
                    {
                        __instance.SelectPet(colorChip, hat);
                    }));
                    colorChip.Button.OnMouseOut.AddListener((UnityAction)(()=>
                    {
                        __instance.SelectPet(colorChip, HatManager.Instance.GetPetById(DataManager.Player.Customization.Pet));
                    }));
                    colorChip.Button.OnClick.AddListener((UnityAction)(()=>
                    {
                        __instance.ClickEquip();
                    }));
                }
                else
                {
                    colorChip.Button.OnClick.AddListener((UnityAction)(()=>
                    {
                        __instance.SelectPet(colorChip, hat);
                    }));
                }
                colorChip.Button.ClickMask = __instance.scroller.Hitbox;
                colorChip.Tag = hat;
                UpdateMaterials(colorChip.Inner.FrontLayer, hat);
                hat.SetPreview(colorChip.Inner.FrontLayer, __instance.GetDisplayColor());
                colorChip.Inner.SetMaskType(PlayerMaterial.MaskType.SimpleUI);
                colorChip.SelectionHighlight.gameObject.SetActive(false);
                __instance.ColorChips.Add(colorChip);
                num++;
                if (!HatManager.Instance.CheckLongModeValidCosmetic(hat.ProdId, __instance.PlayerPreview.GetIgnoreLongMode()))
                {
                    colorChip.SetUnavailable();
                }
            }

            __instance.petId = DataManager.Player.Customization.Pet;
            __instance.currentPetIsEquipped = true;
            __instance.SetScrollerBounds();
            __instance.initialized = true;
            return false;
        }
    }
    private static void UpdateMaterials(SpriteRenderer spriteRenderer, PetData data)
    {
        if (!data.PreviewCrewmateColor)
        {
            spriteRenderer.sharedMaterial = HatManager.Instance.DefaultShader;
            return;
        }
        spriteRenderer.sharedMaterial = new Material(HatManager.Instance.PlayerMaterial);
    }
}
#pragma warning restore S3398
