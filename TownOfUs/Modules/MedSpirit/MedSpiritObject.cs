using System.Collections;
using Hazel;
using Il2CppInterop.Runtime.Attributes;
using InnerNet;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modules.MedSpirit;

[RegisterInIl2Cpp]
public sealed class MedSpiritObject : InnerNetObject
{
    // Components
    public PlayerControl? Owner { get; private set; }
    public Rigidbody2D Rigidbody { get; private set; } = null!;
    public MedSpiritNetTransform NetTransform { get; private set; } = null!;
    public SpriteRenderer Rend { get; private set; } = null!;
    public SpecialInputHandler InputHandler { get; private set; } = null!;
    private static Color32 BodyColor => new(129, 85, 142, 255);
    private static Color32 BackColor => new(112, 69, 124, 255);

    // Movement
    public float CurrentSpeed { get; private set; }
    public bool Moveable { get; private set; }
    public float AccelerationRate { get; private set; } = 3.25f;
    public float DecelerationRate { get; private set; } = 3.75f;
    public float TurnSharpness { get; private set; } = 10000f;
    public float MaxSpeed { get; private set; }

    public MedSpiritObject(IntPtr cppPtr) : base(cppPtr)
    {
    }

    public override void ClearOrDecrementDirt()
    {
        // Not needed, but must be implemented
    }

    public override bool Serialize(MessageWriter writer, bool initialState)
    {
        // Not needed, but must be implemented
        return false;
    }

    public override void Deserialize(MessageReader reader, bool initialState)
    {
        // Not needed, but must be implemented
    }

    public override void HandleRpc(byte callId, MessageReader reader)
    {
        // Not needed, but must be implemented
    }

    private void Awake()
    {
        /*var baseSpeed = 1f;
        
        if (GameManager.Instance.LogicOptions.currentGameOptions.TryGetFloat(FloatOptionNames.PlayerSpeedMod, out var result))
        {
            baseSpeed = result;
        }*/
        MaxSpeed = OptionGroupSingleton<MediumOptions>.Instance.MediatingSpeed.Value/* * baseSpeed * 1.5f*/;
        Rigidbody = GetComponent<Rigidbody2D>();
        Rend = transform.GetChild(0).GetComponent<SpriteRenderer>();
        NetTransform = GetComponent<MedSpiritNetTransform>();

        if (InputHandler != null) return;
        InputHandler = gameObject.AddComponent<SpecialInputHandler>();
        InputHandler.disableVirtualCursor = true;
        InputHandler.enabled = false;
    }

    private void Start()
    {
        Owner = AmongUsClient.Instance.GetClient(OwnerId).Character;

        if (Owner == null)
        {
            Error("null owner for Medium Spirit");
            return;
        }

        if (Owner.Data.Role is MediumRole detonator)
        {
            detonator.Spirit = this;
        }

        NetTransform.SnapTo(Owner.transform.position - new Vector3(0f, 0.1f * (Owner.transform.localScale.y / 0.7f)));

        Rend.flipX = Owner.MyPhysics.FlipX;
        Rend.material = HatManager.Instance.PlayerMaterial;

        // Mod compatibility - get the current play material colors, rather than their colorid's colors...
        var targetPlayer = Owner;
        var materialColor =
            targetPlayer!.cosmetics.currentBodySprite.BodySprite.material.GetColor(ShaderID.BodyColor);
        var visorColor =
            targetPlayer!.cosmetics.currentBodySprite.BodySprite.material.GetColor(ShaderID.VisorColor);

        PlayerMaterial.SetColors(materialColor, Rend);
        // For some reason the visor color gets set to `Color32.op_Implicit(Palette.VisorColor)` in the above function...
        Rend.material.SetColor(ShaderID.VisorColor, visorColor);
        Rend.material.SetColor(ShaderID.BackColor, BackColor);
        Rend.material.SetColor(ShaderID.BodyColor, BodyColor);
        Coroutines.Start(CoFadeIn());

        if (!AmOwner) return;

        Owner.moveable = false;
        Owner.MyPhysics.ResetMoveState();
        Owner.MyPhysics.body.velocity = Vector2.zero;
        Owner.NetTransform.SetPaused(true);
        Owner.NetTransform.ClearPositionQueues();
        Owner.SetKinematic(true);

        HudManager.Instance.PlayerCam.SetTarget(this);

        Owner.lightSource.transform.parent = transform;
        Owner.lightSource.Initialize(Owner.Collider.offset / 2f);
        HudManager.Instance.ShadowQuad.gameObject.SetActive(false);

        Moveable = true;
    }

    private void OnDisable()
    {
        if (!Owner || !Owner!.Data || !Owner.Data.Role) return;
        if (Owner.Data.Role is MediumRole detonator) detonator.Spirit = null;

        if (!AmOwner) return;
        foreach (var modifier in ModifierUtils.GetActiveModifiers<MediumHiddenModifier>())
        {
            modifier.Player.RemoveModifier(modifier);
        }
        var button = CustomButtonSingleton<MediumMediateButton>.Instance;
        button.ResetCooldownAndOrEffect();

        Owner.moveable = true;
        Owner.NetTransform.SetPaused(false);
        Owner.SetKinematic(false);
        Owner.NetTransform.Halt();

        HudManager.Instance.PlayerCam.SetTarget(PlayerControl.LocalPlayer);

        Owner.lightSource.transform.parent = Owner.transform;
        Owner.lightSource.Initialize(Owner.Collider.offset / 2f);

        if (!Owner.HasDied())
        {
            HudManager.Instance.ShadowQuad.gameObject.SetActive(true);
        }
    }

    public void SetNormalizedVelocity(Vector2 direction)
    {
        if (direction.magnitude > 0)
        {
            CurrentSpeed = Mathf.Min(CurrentSpeed + AccelerationRate * Time.deltaTime, MaxSpeed);

            Rigidbody.velocity = direction.normalized * CurrentSpeed;
        }
        else
        {
            Rigidbody.velocity = Vector2.Lerp(Rigidbody.velocity, Vector2.zero, Time.deltaTime * DecelerationRate);
            CurrentSpeed = Mathf.Lerp(CurrentSpeed, 0f, Time.deltaTime * DecelerationRate);
        }
    }

    private void FixedUpdate()
    {
        Rend.flipX = Rigidbody.velocity.x switch
        {
            < -0.01f => true,
            > 0.01f => false,
            _ => Rend.flipX,
        };

        if (Owner && AmOwner && Moveable && GameData.Instance && HudManager.InstanceExists && HudManager.Instance.joystick != null)
        {
            SetNormalizedVelocity(HudManager.Instance.joystick.DeltaL);
        }
    }

    private void LateUpdate()
    {
        var position = transform.position;
        position.z = position.y / 1000f;
        transform.position = position;
    }

    [HideFromIl2Cpp]
    public IEnumerator CoFadeIn()
    {
        yield return new WaitForEndOfFrame();
        Rend.color = Rend.color.SetAlpha(0f);
        if (Owner!.AmOwner)
        {
            Rend.color = Rend.color.SetAlpha(0.1f);
        }
        yield return new WaitForSeconds(OptionGroupSingleton<MediumOptions>.Instance.LivingSeeSpiritTimer.Value);
        yield return MiscUtils.FadeIn(Rend, 0.0001f, 0.05f);
    }

    [HideFromIl2Cpp]
    public IEnumerator CoDestroy()
    {
        Moveable = false;
        Rigidbody.isKinematic = true;
        NetTransform.enabled = false;
        NetTransform.ClearPositionQueues();
        Rigidbody.velocity = Vector2.zero;
        yield return MiscUtils.FadeOut(Rend, 0.0001f, 0.05f);
        yield return new WaitForSeconds(0.1f);

        /*
        if (AmOwner)
        {
            SoundManager.Instance.StopSound(TouAudio.EscapistRecallSound.LoadAsset());
            SoundManager.Instance.PlaySound(TouAudio.EscapistRecallSound.LoadAsset(), false);
        }*/

        AmongUsClient.Instance.DestroyedObjects.Add(NetId);
        AmongUsClient.Instance.RemoveNetObject(this);
        Destroy(gameObject);
    }
}