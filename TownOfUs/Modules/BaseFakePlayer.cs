using System.Collections;
using AmongUs.Data;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using PowerTools;
using Reactor.Utilities;
using TMPro;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Options.Modifiers.Universal;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TownOfUs.Modules;

public abstract class BaseFakePlayer(IntPtr cppPtr) : MonoBehaviour(cppPtr)
{
    private const string DefaultPetName = "EmptyPet(Clone)";
    private const string NameTextObjName = "NameText_TMP";
    private const string ColorBindTextName = "ColorblindName_TMP";

    public static readonly List<BaseFakePlayer> FakePlayers = [];
    protected CosmeticsLayer _cosmeticsLayer { get; set; }

    protected PlayerCosmicInfo _cosmicInfo { get; set; }
    protected SpriteRenderer _rend { get; set; }

    public GameObject body { get; internal set; }
    protected TextMeshPro _colorBindText;
    protected GameObject _colorBindTextObj;
    protected TextMeshPro _nameTextMaster;
    public int PlayerId;
    public bool InCamo;
    public PlayerControl OriginalPlayer;
    public bool IsMiniPlayer { get; private set; }

    public virtual void Start()
    {
        var player = OriginalPlayer;
        var playerOutfit = player.Data.DefaultOutfit;

        _cosmicInfo = new PlayerCosmicInfo
        {
            Cosmetics = player.cosmetics,
            FlipX = player.cosmetics.currentBodySprite.BodySprite.flipX,
            OutfitInfo = playerOutfit,
            ColorInfo = playerOutfit.ColorId
        };

        _cosmicInfo.Cosmetics.Visible = true;
        var isClassic =
            _cosmicInfo.Cosmetics.bodyType is PlayerBodyTypes.Classic;
        if (isClassic)
        {
            _cosmicInfo.Cosmetics.SetSkin("skin_None", _cosmicInfo.ColorInfo);
            _cosmicInfo.OutfitInfo.SkinId = "skin_None";
        }

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
                Destroy(sync);
            }
        }

        if (isClassic)
        {
            _cosmeticsLayer.currentBodySprite.BodySprite.transform.localScale *= 2f;
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
            IsMiniPlayer = true;
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

    public static void UpdateFakePlayerText(bool waitForRole = false)
    {
        Coroutines.Start(CyclePlayerNames(waitForRole));
    }

    private static IEnumerator CyclePlayerNames(bool waitForRole)
    {
        if (waitForRole)
        {
            yield return new WaitForSeconds(0.05f);
        }
        yield return new WaitForEndOfFrame();
        foreach (var fake in FakePlayers)
        {
            if (!fake._nameTextMaster || !fake.HandleCamo() || fake.InCamo) continue;
            if (fake.OriginalPlayer)
            {
                fake._nameTextMaster.text = fake.OriginalPlayer.cosmetics.nameText.text;
            }
            fake._nameTextMaster.color = Color.white;
        }
    }

    protected virtual bool HandleCamo()
    {
        return _cosmeticsLayer;
    }

    public void Camo()
    {
        InCamo = true;
        if (!HandleCamo()) return;

        _cosmeticsLayer.SetHat(string.Empty, _cosmicInfo.ColorInfo);
        _cosmeticsLayer.SetVisor(string.Empty, _cosmicInfo.ColorInfo);
        _cosmeticsLayer.SetSkin(string.Empty, _cosmicInfo.ColorInfo);

        PlayerMaterial.SetColors(Color.grey, _cosmeticsLayer.currentBodySprite.BodySprite);

        _nameTextMaster.color = Color.clear;
        _colorBindText.color = Color.clear;
    }

    public void UnCamo()
    {
        InCamo = false;
        if (!HandleCamo()) return;

        _cosmeticsLayer.SetHat(_cosmicInfo.OutfitInfo.HatId, _cosmicInfo.ColorInfo);
        _cosmeticsLayer.SetVisor(_cosmicInfo.OutfitInfo.VisorId, _cosmicInfo.ColorInfo);
        _cosmeticsLayer.SetSkin(_cosmicInfo.OutfitInfo.SkinId, _cosmicInfo.ColorInfo);
        _cosmeticsLayer.SetColor(_cosmicInfo.ColorInfo);

        _nameTextMaster.color = Color.white;
        _colorBindText.color = Color.white;
    }

    private SpriteRenderer CreateBodyImage(PlayerCosmicInfo info)
    {
        var spriteRenderer = Instantiate(info.Cosmetics.currentBodySprite.BodySprite, body!.transform);

        spriteRenderer.flipX = info.FlipX;
        spriteRenderer.transform.localScale = Scale;

        return spriteRenderer;
    }

    private CosmeticsLayer CreateCosmetics(SpriteRenderer playerImage, PlayerCosmicInfo info)
    {
        var cosmeticsLayer = Instantiate(AmongUsClient.Instance.PlayerPrefab.cosmetics, body!.transform);
        var basePayerBodySprite = info.Cosmetics.currentBodySprite;

        var playerBodySprite = new PlayerBodySprite
        {
            BodySprite = playerImage,
            Type = basePayerBodySprite.Type,
            flippedCosmeticOffset = basePayerBodySprite.flippedCosmeticOffset,
            LongModeParts = new SpriteRenderer[info.Cosmetics.currentBodySprite.LongModeParts.Length]
        };

        for (var i = 0; i < info.Cosmetics.currentBodySprite.LongModeParts.Length; ++i)
        {
            var newSprite = Instantiate(
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
            Destroy(emptyPet.gameObject);
        }

        var petId = cosmicInfo.OutfitInfo.PetId;

        if (petId != PetData.EmptyId)
        {
            var preBehaviourPrefab = ShipStatus.Instance.CosmeticsCache.GetPet(petId);

            var petBehaviour = Instantiate(preBehaviourPrefab, body.transform);
            petBehaviour.SetCrewmateColor(colorId);
            petBehaviour.transform.localPosition = Vector2.zero +
                                                   (flipX
                                                       ? Vector2.right * Random.RandomRange(0, 0.2f)
                                                       : Vector2.left * Random.RandomRange(0, 0.2f)) +
                                                   Vector2.down * Random.RandomRange(-0.05f, 0.15f);
            petBehaviour.transform.localScale = Scale;
            petBehaviour.FlipX = flipX;

            MiscUtils.RemovePet(playerRef);

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

        var nameObj = Instantiate(baseObject, parent.transform);
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
            nameText.alignment = TextAlignmentOptions.Bottom;
            ChangeDummyName(nameText, baseNameText, info);
            if (player.HasModifier<ShyModifier>())
            {
                nameText.GetComponent<TextMeshPro>().color =
                    nameText.GetComponent<TextMeshPro>().color.SetAlpha(transparency);
            }
        }

        if (_colorBindText && baseColorBindText != null)
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
            UnityEngine.Object.Destroy(info.gameObject);
        }
    }

    private static void UpdateColorName(TextMeshPro colorText, TextMeshPro baseColorText, int colorId)
    {
        var array = TranslationController.Instance
            .GetString(Palette.ColorNames[colorId]).ToCharArray();

        if (array.Length != 0)
        {
            array[0] = char.ToUpper(array[0], TownOfUsPlugin.Culture);
            for (var i = 1; i < array.Length; i++)
            {
                array[i] = char.ToLower(array[i], TownOfUsPlugin.Culture);
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
            UnityEngine.Object.Destroy(component);
        }
    }

    private static void FitTextMeshPro(TextMeshPro a, TextMeshPro b)
    {
        a.transform.localPosition = b.transform.localPosition;
        a.transform.localScale = b.transform.localScale;
        a.fontSize = a.fontSizeMax = a.fontSizeMin = b.fontSizeMax = b.fontSizeMin = b.fontSize;
    }

    public static void ClearAll(bool fullClear = false)
    {
        FakePlayers.ProcessAndClear(x => x.Destroy(fullClear));
    }

    public virtual bool Destroy(bool fullClear = false)
    {
        if (body)
        {
            DestroyObject(body);
        }
        FakePlayers.Remove(this);
        // Do not try to remove the item from the list again.
        return false;
    }

    public virtual void OnDestroy()
    {
        DataManager.Settings.Accessibility.OnChangedEvent -= new Action(SwitchColorName);
    }

    protected struct PlayerCosmicInfo
    {
        public CosmeticsLayer Cosmetics;

        public NetworkedPlayerInfo.PlayerOutfit OutfitInfo;

        public bool FlipX;

        public int ColorInfo;
    }
}

public static class BaseFakePlayerExt
{
    public static TFake CreateBaseFake<TFake>(PlayerControl player) where TFake : BaseFakePlayer
    {
        var obj = new GameObject($"Fake {player.gameObject.name}")
        {
            layer = LayerMask.NameToLayer("Players")
        };
        var fake = obj.AddComponent<TFake>();
        fake.PlayerId = player.PlayerId;
        fake.OriginalPlayer = player;
        fake.body = obj;
        return fake;
    }
}
