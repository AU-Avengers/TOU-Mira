using System.Collections;
using AmongUs.Data;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using PowerTools;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Options.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Modules;

[RegisterInIl2Cpp]
public sealed class StonedPlayer(IntPtr cppPtr) : BaseFakePlayer(cppPtr)
{
    private SpriteRenderer _stoneRend { get; set; }
    private SpriteAnim _stoneAnim { get; set; }

    public StoneStage ProgressStage { get; private set; } = StoneStage.Frozen;

    public IEnumerator? CurrentCoroutine;
    [HideFromIl2Cpp]
    public IEnumerator CoStartStone()
    {
        var isShy = OriginalPlayer.HasModifier<ShyModifier>();
        var opts = OptionGroupSingleton<MedusaOptions>.Instance;
        var otherTime = opts.StoneCompletion - 6.5f;
        yield return new WaitForSeconds(opts.StoneDelay);
        ProgressStage = StoneStage.Petrified;
        if (_cosmeticsLayer)
        {
            if (isShy)
            {
                var textAlpha = _nameTextMaster.color.a;
                var colorAlpha = _colorBindText.color.a;

                while (textAlpha < 1 || colorAlpha < 1)
                {
                    textAlpha = Mathf.Min(_nameTextMaster.color.a + 0.01f, 1f); // Ensure it doesn't go above 1
                    _nameTextMaster.color = _nameTextMaster.color.SetAlpha(textAlpha);

                    colorAlpha = Mathf.Min(_colorBindText.color.a + 0.01f, 1f); // Ensure it doesn't go above 1
                    if (DataManager.Settings.Accessibility.ColorBlindMode)
                    {
                        _colorBindText.color = _colorBindText.color.SetAlpha(colorAlpha);
                    }

                    otherTime -= 0.01f;
                    yield return new WaitForSeconds(0.01f);
                }
            }

            _nameTextMaster.color = _nameTextMaster.color.SetAlpha(1);
            _colorBindText.color = _colorBindText.color.SetAlpha(1);
            SpriteRenderer[] rends =
            [
                _rend, _cosmeticsLayer.hat.FrontLayer, _cosmeticsLayer.hat.BackLayer, _cosmeticsLayer.visor.Image,
                _cosmeticsLayer.skin.layer
            ];
            var tmpIn = _stoneRend.color;
            tmpIn.a = 0;
            var tmpOut = _rend.color;

            while (tmpOut.a > 0 || tmpIn.a < 1)
            {
                tmpOut.a = Mathf.Max(tmpOut.a - 0.01f, 0f); // Ensure it doesn't go below 0
                foreach (var rend in rends)
                {
                    rend.color = tmpOut;
                }

                tmpIn.a = Mathf.Min(tmpIn.a + 0.01f, 1f); // Ensure it doesn't go above 1
                _stoneRend.color = tmpIn;

                otherTime -= 0.03f;
                yield return new WaitForSeconds(0.03f);
            }
        }
        else
        {
            otherTime -= 3f;
            yield return MiscUtils.FadeInOutPair(_stoneRend, _rend, 0.03f);
        }

        yield return new WaitForSeconds(1f);
        ProgressStage = StoneStage.Moving;
        _stoneAnim.Play(TouAssets.MedusaStoneMove);
        yield return new WaitForSeconds(_stoneAnim.m_currAnim.length);
        if (MeetingHud.Instance || ExileController.Instance)
        {
            ProgressStage = StoneStage.Permanent;
            SetStonedName();
            yield break;
        }

        yield return new WaitForSeconds(1f);
        ProgressStage = StoneStage.Crack;
        var crackSpeed = 0.25f / otherTime;
        _stoneAnim.Play(TouAssets.MesudaStoneCrack, crackSpeed);
        Warning($"Seconds: {otherTime} | Multiplier: {0.25f * crackSpeed}");
        yield return new WaitForSeconds(otherTime);
        if (MeetingHud.Instance || ExileController.Instance)
        {
            ProgressStage = StoneStage.Permanent;
            SetStonedName();
            yield break;
        }

        yield return new WaitForSeconds(1f);
        ProgressStage = StoneStage.Visor;
        _stoneAnim.Play(TouAssets.MesudaStoneVisor);
        yield return new WaitForSeconds(_stoneAnim.m_currAnim.length);
        if (MeetingHud.Instance || ExileController.Instance)
        {
            ProgressStage = StoneStage.Permanent;
            SetStonedName();
            yield break;
        }

        yield return new WaitForSeconds(3f);
        _stoneAnim.Play(TouAssets.MesudaStoneShatter);

        ProgressStage = StoneStage.Shatter;
    }

    public void ForceStone()
    {
        if (_cosmeticsLayer)
        {
            _nameTextMaster.color = _nameTextMaster.color.SetAlpha(1);
            _colorBindText.color = _colorBindText.color.SetAlpha(1);
            SpriteRenderer[] rends =
            [
                _rend, _cosmeticsLayer.hat.FrontLayer, _cosmeticsLayer.hat.BackLayer, _cosmeticsLayer.visor.Image,
                _cosmeticsLayer.skin.layer
            ];
            foreach (var rend in rends)
            {
                rend.color = rend.color.SetAlpha(0);
            }
            _stoneRend.color = _stoneRend.color.SetAlpha(1);
        }
        else
        {
            _stoneRend.color = _stoneRend.color.SetAlpha(1);
            _rend.color = _rend.color.SetAlpha(0);
        }
    }

    public static StonedPlayer CreateStone(PlayerControl player)
    {
        return BaseFakePlayerExt.CreateBaseFake<StonedPlayer>(player);
    }

    public override void Start()
    {
        base.Start();

        var stone = Instantiate(TouAssets.MedusaStonedPlayer.LoadAsset(), body.transform);
        stone.transform.localPosition = new Vector3(_cosmicInfo.FlipX ? 0.05f : -0.01f, 0.2f);

        _stoneAnim = stone.GetComponent<SpriteAnim>();
        _stoneRend = stone.GetComponent<SpriteRenderer>();
        _stoneRend.color = new Color(1, 1, 1, 0);
        _stoneRend.flipX = _cosmicInfo.FlipX;
        CurrentCoroutine = Coroutines.Start(CoStartStone());
    }

    protected override bool HandleCamo()
    {
        return base.HandleCamo() && ProgressStage == StoneStage.Frozen;
    }

    public void SetStonedName()
    {
        if (!_cosmeticsLayer)
        {
            return;
        }

        _cosmeticsLayer.nameText.color = _cosmeticsLayer.nameText.color.SetAlpha(1f);

        _colorBindTextObj.SetActive(false);
        _nameTextMaster.text = MiraLocaleManager.Get("DiedToMedusa");
        _nameTextMaster.color = Color.grey;
    }

    public override bool Destroy(bool fullClear = false)
    {
        if (fullClear || ProgressStage is StoneStage.Shatter)
        {
            return base.Destroy(fullClear);
        }

        if (CurrentCoroutine != null)
        {
            Coroutines.Stop(CurrentCoroutine);
        }
        // This forces any players not fully stoned to get the stoned sprite immediately.
        if (ProgressStage is StoneStage.Frozen or StoneStage.Petrified)
        {
            ForceStone();
        }
        ProgressStage = StoneStage.Permanent;
        SetStonedName();
        return false;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (CurrentCoroutine != null)
        {
            Coroutines.Stop(CurrentCoroutine);
        }
    }
}

public enum StoneStage
{
    Frozen,
    Petrified,
    Moving,
    Crack,
    Visor,
    Shatter,
    Permanent
}