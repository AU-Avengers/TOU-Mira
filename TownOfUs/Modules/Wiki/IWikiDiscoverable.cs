using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Modifiers;
using TMPro;
using UnityEngine;

namespace TownOfUs.Modules.Wiki;

public interface IWikiDiscoverable
{
    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities => [];

    public string SecondTabName => TouLocale.Get("WikiAbilitiesTab", "Abilities");
    [HideFromIl2Cpp] public bool IsHiddenFromList => MiscUtils.CurrentGamemode() is not TouGamemode.Normal;

    public uint FakeTypeId => ModifierManager.GetModifierTypeId(GetType()) ??
                              throw new InvalidOperationException("Modifier is not registered.");

    public string GetAdvancedDescription()
    {
        return MiscUtils.AppendOptionsText(GetType());
    }

    public virtual bool CanShowSecondTab => Abilities.Count > 0;

    public virtual float ShowAbilitiesTab(Transform abilityTemplate, Transform abilityTemplateLong, Transform abilityScroller)
    {
        if (Abilities.Count > 0)
        {
            foreach (var ability in Abilities)
            {
                var newAbility = UnityEngine.Object.Instantiate(abilityTemplate, abilityScroller);
                var icon = newAbility.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
                var text = newAbility.GetChild(1).GetComponent<TextMeshPro>();
                var desc = newAbility.GetChild(2).GetComponent<TextMeshPro>();

                icon.sprite = ability.Icon.LoadAsset();
                icon.size = new Vector2(0.8f, 0.8f * icon.sprite.bounds.size.y / icon.sprite.bounds.size.x);
                // icon.tileMode = SpriteTileMode.Adaptive;

                text.text =
                    $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Chat Message Masked\">{ability.Name}</font>";
                desc.text =
                    $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Chat Message Masked\">{ability.Description}</font>";
                newAbility.gameObject.SetActive(true);
            }

            return Mathf.Max(0f, Abilities.Count * 0.875f);
        }

        return 0;
    }
}

public record struct CustomButtonWikiDescription(string Name, string Description, LoadableAsset<Sprite> Icon);