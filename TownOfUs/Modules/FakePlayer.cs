using System.Globalization;
using AmongUs.Data;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using PowerTools;
using TMPro;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Options.Modifiers.Universal;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace TownOfUs.Modules;

// Code Review: Should be using a MonoBehaviour
public sealed class FakePlayer : IDisposable
{
    private const string DefaultPetName = "EmptyPet(Clone)";
    private const string NameTextObjName = "NameText_TMP";
    private const string ColorBindTextName = "ColorblindName_TMP";
    public static readonly List<FakePlayer> FakePlayers = [];
    private readonly CosmeticsLayer? _cosmeticsLayer;

    private readonly PlayerCosmicInfo _cosmicInfo;
    private readonly SpriteRenderer? _rend;

    public readonly GameObject? body;
    private TextMeshPro _colorBindText;
    private GameObject? _colorBindTextObj;
    private TextMeshPro _nameTextMaster;
    public int PlayerId;

    public FakePlayer(PlayerControl player)
    {
        var playerOutfit = player.Data.DefaultOutfit;

        _cosmicInfo = new PlayerCosmicInfo
        {
            Cosmetics = player.cosmetics,
            FlipX = player.cosmetics.currentBodySprite.BodySprite.flipX,
            OutfitInfo = playerOutfit,
            ColorInfo = playerOutfit.ColorId
        };

        _cosmicInfo.Cosmetics.Visible = true;

        body = new GameObject($"Fake {player.gameObject.name}");
        PlayerId = player.PlayerId;

        body.layer = LayerMask.NameToLayer("Players");

        CreateNameTextParentObj(player, body, _cosmicInfo);

        _rend = CreateBodyImage(_cosmicInfo);
        _cosmeticsLayer = CreateCosmetics(_rend, _cosmicInfo);

        DataManager.Settings.Accessibility.OnChangedEvent += new Action(SwitchColorName);

        DecorateDummy(_cosmeticsLayer, _cosmicInfo, player);

        SpriteAnimNodeSync[] syncs = body.GetComponentsInChildren<SpriteAnimNodeSync>(true);
        for (var i = 0; i < syncs.Length; ++i)
        {
            var sync = syncs[i];
            if (sync != null)
            {
                Object.Destroy(sync);
            }
        }

        var vector = player.transform.position;
        vector.z = vector.y / 1000f;

        body.transform.position = vector;

        if (player.HasModifier<GiantModifier>())
        {
            body.transform.localScale /= 0.7f;
        }
        else if (player.HasModifier<MiniModifier>())
        {
            body.transform.localScale *= 0.7f;
        }

        if (player.HasModifier<ShyModifier>())
        {
            var colour = player.cosmetics.currentBodySprite.BodySprite.color;
            var transparency = OptionGroupSingleton<ShyOptions>.Instance.FinalTransparency / 100;

            colour.a = transparency;
            _cosmeticsLayer.currentBodySprite.BodySprite.color = colour;

            _cosmeticsLayer.nameText.color = _cosmeticsLayer.nameText.color.SetAlpha(transparency);

            if (DataManager.Settings.Accessibility.ColorBlindMode)
            {
                _cosmeticsLayer.colorBlindText.color = _cosmeticsLayer.colorBlindText.color.SetAlpha(transparency);
            }

            //player.SetHatAndVisorAlpha(transparency);
            _cosmeticsLayer.skin.layer.color = _cosmeticsLayer.skin.layer.color.SetAlpha(transparency);

            foreach (var animation in _cosmeticsLayer.GetComponentsInParent<SpriteRenderer>())
            {
                animation.color = animation.color.SetAlpha(transparency);
            }

            foreach (var animation in _cosmeticsLayer.GetComponentsInChildren<SpriteRenderer>())
            {
                animation.color = animation.color.SetAlpha(transparency);
            }

            foreach (var animation in body.GetComponentsInParent<SpriteRenderer>())
            {
                animation.color = animation.color.SetAlpha(transparency);
            }

            foreach (var animation in body.GetComponentsInChildren<SpriteRenderer>())
            {
                animation.color = animation.color.SetAlpha(transparency);
            }
        }

        var cosmeticsObj = body.transform.GetChild(2).gameObject;
        if (cosmeticsObj != null && cosmeticsObj.transform.GetChildCount() > 4)
        {
            cosmeticsObj.transform.GetChild(3).gameObject.SetActive(false);
            cosmeticsObj.transform.GetChild(4).gameObject.SetActive(false);
            cosmeticsObj.transform.GetChild(5).gameObject.SetActive(false);
        }

        FakePlayers.Add(this);
    }

    private static Vector3 Scale => new(0.35f, 0.35f, 0.35f);
    private static Vector2 PosOffset => new(-0.045f, 0.575f);

    public void SwitchColorName()
    {
        _colorBindTextObj?.SetActive(DataManager.Settings.Accessibility.ColorBlindMode);
    }

    public void Camo()
    {
        _cosmeticsLayer!.SetHat(string.Empty, _cosmicInfo.ColorInfo);
        _cosmeticsLayer.SetVisor(string.Empty, _cosmicInfo.ColorInfo);
        _cosmeticsLayer.SetSkin(string.Empty, _cosmicInfo.ColorInfo);

        PlayerMaterial.SetColors(Color.grey, _cosmeticsLayer.currentBodySprite.BodySprite);

        _nameTextMaster!.color = Color.clear;
        _colorBindText!.color = Color.clear;
    }

    public void UnCamo()
    {
        _cosmeticsLayer!.SetHat(_cosmicInfo.OutfitInfo.HatId, _cosmicInfo.ColorInfo);
        _cosmeticsLayer.SetVisor(_cosmicInfo.OutfitInfo.VisorId, _cosmicInfo.ColorInfo);
        _cosmeticsLayer.SetSkin(_cosmicInfo.OutfitInfo.SkinId, _cosmicInfo.ColorInfo);
        _cosmeticsLayer.SetColor(_cosmicInfo.ColorInfo);

        _nameTextMaster!.color = Color.white;
        _colorBindText!.color = Color.white;
    }

    private SpriteRenderer CreateBodyImage(PlayerCosmicInfo info)
    {
        var spriteRenderer = Object.Instantiate(info.Cosmetics.currentBodySprite.BodySprite, body!.transform);

        spriteRenderer.flipX = info.FlipX;
        spriteRenderer.transform.localScale = Scale;

        return spriteRenderer;
    }

    private CosmeticsLayer CreateCosmetics(SpriteRenderer playerImage, PlayerCosmicInfo info)
    {
        var cosmeticsLayer = Object.Instantiate(AmongUsClient.Instance.PlayerPrefab.cosmetics, body!.transform);
        var basePayerBodySprite = info.Cosmetics.currentBodySprite;

        var playerBodySprite = new PlayerBodySprite
        {
            BodySprite = playerImage,
            Type = basePayerBodySprite.Type,
            flippedCosmeticOffset = basePayerBodySprite.flippedCosmeticOffset,
            LongModeParts =
                new Il2CppReferenceArray<SpriteRenderer>(info.Cosmetics.currentBodySprite.LongModeParts.Length)
        };

        for (var i = 0; i < info.Cosmetics.currentBodySprite.LongModeParts.Length; ++i)
        {
            var newSprite = Object.Instantiate(
                info.Cosmetics.currentBodySprite.LongModeParts[i],
                cosmeticsLayer.transform);

            playerBodySprite.LongModeParts[i] = newSprite;
        }

        cosmeticsLayer.currentBodySprite = playerBodySprite;
        cosmeticsLayer.hat.Parent = playerImage;
        cosmeticsLayer.hat.transform.localPosition = PosOffset;
        cosmeticsLayer.visor.transform.localPosition = PosOffset;
        cosmeticsLayer.petParent = body.transform;
        cosmeticsLayer.transform.localScale = Scale;
        cosmeticsLayer.ResetCosmetics();

        return cosmeticsLayer;
    }

    private void DecorateDummy(CosmeticsLayer cosmetics, PlayerCosmicInfo cosmicInfo, PlayerControl playerRef)
    {
        var colorId = cosmicInfo.ColorInfo;
        var flipX = cosmicInfo.FlipX;

        cosmetics.SetNameMask(true);
        cosmetics.SetHat(cosmicInfo.OutfitInfo.HatId, colorId);
        cosmetics.SetVisor(cosmicInfo.OutfitInfo.VisorId, colorId);
        cosmetics.SetSkin(cosmicInfo.OutfitInfo.SkinId, colorId);
        cosmetics.SetFlipX(flipX);

        var emptyPet = body!.transform.Find(DefaultPetName);
        if (emptyPet != null)
        {
            Object.Destroy(emptyPet.gameObject);
        }

        var petId = cosmicInfo.OutfitInfo.PetId;

        if (petId != PetData.EmptyId)
        {
            var preBehaviourPrefab = ShipStatus.Instance.CosmeticsCache.GetPet(petId);

            var petBehaviour = Object.Instantiate(preBehaviourPrefab, body.transform);
            petBehaviour.SetCrewmateColor(colorId);
            petBehaviour.transform.localPosition = Vector2.zero +
                                                   (flipX
                                                       ? Vector2.right * Random.RandomRange(0, 0.2f)
                                                       : Vector2.left * Random.RandomRange(0, 0.2f)) +
                                                   Vector2.down * Random.RandomRange(-0.05f, 0.15f);
            petBehaviour.transform.localScale = Scale;
            petBehaviour.FlipX = flipX;

            RemovePet(playerRef);

            DestroyAllCollider(petBehaviour.gameObject);

            if (playerRef.HasModifier<GiantModifier>())
            {
                petBehaviour.transform.localScale *= 0.7f;
                petBehaviour.transform.localPosition += Vector3.down * 0.1f;
            }
            else if (playerRef.HasModifier<MiniModifier>())
            {
                petBehaviour.transform.localScale /= 0.7f;
                petBehaviour.transform.localPosition += Vector3.down * 0.1f;
            }

            if (playerRef.HasModifier<ShyModifier>())
            {
                var transparency = OptionGroupSingleton<ShyOptions>.Instance.FinalTransparency / 100;

                foreach (var pet in petBehaviour.GetComponentsInParent<SpriteRenderer>())
                {
                    pet.color = pet.color.SetAlpha(transparency);
                }

                foreach (var pet in petBehaviour.GetComponentsInChildren<SpriteRenderer>())
                {
                    pet.color = pet.color.SetAlpha(transparency);
                }
            }
        }

        cosmetics.SetColor(colorId);

        cosmetics.skin.transform.localPosition = cosmicInfo.Cosmetics.skin.transform.localPosition;
        cosmetics.hat.transform.localPosition = PosOffset;
        cosmetics.visor.transform.localPosition = PosOffset;
    }

    private void CreateNameTextParentObj(PlayerControl player, GameObject parent, PlayerCosmicInfo info)
    {
        var baseParentTrans = player.gameObject.transform.FindChild("Names");
        if (baseParentTrans == null)
        {
            return;
        }

        var baseObject = baseParentTrans.gameObject;

        var nameObj = Object.Instantiate(baseObject, parent.transform);
        nameObj.transform.localScale = player.gameObject.transform.localScale;
        nameObj.transform.localPosition = baseObject.transform.localPosition;
        nameObj.transform.localPosition -= new Vector3(0f, 0.247f, 0f);

        if (player.HasModifier<GiantModifier>())
        {
            nameObj.transform.localScale *= 0.7f;
        }
        else if (player.HasModifier<MiniModifier>())
        {
            nameObj.transform.localScale /= 0.7f;
        }

        var nameText = nameObj.transform.FindChild(NameTextObjName).GetComponent<TextMeshPro>();
        var baseNameText = baseObject.transform.FindChild(NameTextObjName).GetComponent<TextMeshPro>();

        _colorBindTextObj = nameObj.transform.FindChild(ColorBindTextName).gameObject;
        _colorBindText = _colorBindTextObj.GetComponent<TextMeshPro>();

        var baseColorBindText = baseObject.transform.FindChild(ColorBindTextName).GetComponent<TextMeshPro>();
        var transparency = OptionGroupSingleton<ShyOptions>.Instance.FinalTransparency / 100;

        if (nameText != null && baseNameText != null)
        {
            ChangeDummyName(nameText, baseNameText, info);
            if (player.HasModifier<ShyModifier>())
            {
                nameText.GetComponent<TextMeshPro>().color =
                    nameText.GetComponent<TextMeshPro>().color.SetAlpha(transparency);
            }
        }

        if (_colorBindText != null && baseColorBindText != null)
        {
            UpdateColorName(_colorBindText, baseColorBindText, info.ColorInfo);
            if (player.HasModifier<ShyModifier>())
            {
                _colorBindText.GetComponent<TextMeshPro>().color =
                    _colorBindText.GetComponent<TextMeshPro>().color.SetAlpha(transparency);
            }
        }

        RemoveRoleInfo(nameObj);
    }

    private void ChangeDummyName(TextMeshPro nameText, TextMeshPro baseNameText, PlayerCosmicInfo info)
    {
        FitTextMeshPro(nameText, baseNameText);

        nameText.text = info.OutfitInfo.PlayerName;
        nameText.color = Palette.White;

        _nameTextMaster = nameText;
    }

    private static void RemoveRoleInfo(GameObject nameTextObjct)
    {
        var info = nameTextObjct.transform.FindChild("Info");
        if (info != null)
        {
            Object.Destroy(info.gameObject);
        }
    }

    private static void UpdateColorName(TextMeshPro colorText, TextMeshPro baseColorText, int colorId)
    {
        var array = TranslationController.Instance
            .GetString(Palette.ColorNames[colorId], Array.Empty<Il2CppSystem.Object>()).ToCharArray();

        if (array.Length != 0)
        {
            array[0] = char.ToUpper(array[0], CultureInfo.InvariantCulture);
            for (var i = 1; i < array.Length; i++)
            {
                array[i] = char.ToLower(array[i], CultureInfo.InvariantCulture);
            }
        }

        FitTextMeshPro(colorText, baseColorText);

        colorText.text = new string(array);
    }

    private static void DestroyAllCollider(GameObject obj)
    {
        DestroyCollider<Collider2D>(obj);
        DestroyCollider<PolygonCollider2D>(obj);
        DestroyCollider<BoxCollider2D>(obj);
        DestroyCollider<CircleCollider2D>(obj);
    }

    private static void DestroyCollider<T>(GameObject obj) where T : Collider2D
    {
        var component = obj.GetComponent<T>();
        if (component != null)
        {
            Object.Destroy(component);
        }
    }

    private static void FitTextMeshPro(TextMeshPro a, TextMeshPro b)
    {
        a.transform.localPosition = b.transform.localPosition;
        a.transform.localScale = b.transform.localScale;
        a.fontSize = a.fontSizeMax = a.fontSizeMin = b.fontSizeMax = b.fontSizeMin = b.fontSize;
    }

    public static void ClearAll()
    {
        FakePlayers.Do(x => x.Destroy());
        FakePlayers.Clear();
    }

    public void Destroy()
    {
        DataManager.Settings.Accessibility.OnChangedEvent -= new Action(SwitchColorName);

        Dispose();
    }

    public void Dispose()
    {
        Dispose(true);
    }
    public void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (body != null)
            {
                Object.Destroy(body);
            }

            if (_colorBindTextObj != null)
            {
                Object.Destroy(_colorBindTextObj);
            }

            if (_rend != null)
            {
                Object.Destroy(_rend);
            }
        }
    }

    public static void RemovePet(PlayerControl pc)
    {
        if (pc == null || !pc.Data.IsDead)
        {
            return;
        }

        if (pc.CurrentOutfit.PetId == "")
        {
            return;
        }

        pc.SetPet("");
    }

    private struct PlayerCosmicInfo
    {
        public CosmeticsLayer Cosmetics;

        public NetworkedPlayerInfo.PlayerOutfit OutfitInfo;

        public bool FlipX;

        public int ColorInfo;
    }
}