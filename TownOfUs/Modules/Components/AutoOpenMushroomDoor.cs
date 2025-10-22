using Hazel;
using Reactor.Utilities.Attributes;
using UnityEngine;


namespace TownOfUs.Modules.Components;

[RegisterInIl2Cpp]
public sealed class AutoOpenMushroomDoor(nint cppPtr) : AutoOpenDoor(cppPtr)
{
    public override bool DoUpdate(float dt)
    {
        CooldownTimer = Math.Max(CooldownTimer - dt, 0f);
        if (ClosedTimer > 0f)
        {
            ClosedTimer = Math.Max(ClosedTimer - dt, 0f);
            if (ClosedTimer == 0f)
            {
                SetDoorway(true);
                return true;
            }
        }
        return false;
    }

    public const float CooldownDuration = 30f;

    private const float ClosedDuration = 10f;

    public float ClosedTimer;

    public float CooldownTimer;
    
	public override bool IsOpen
	{
		get
		{
			return open;
		}
	}

    private void SoundDynamics(AudioSource source)
    {
        if (!PlayerControl.LocalPlayer)
        {
            source.volume = 0f;
            return;
        }

        source.volume = 1f;
        var truePosition = PlayerControl.LocalPlayer.GetTruePosition();
        source.volume = SoundManager.GetSoundVolume(gameObject.transform.position, truePosition, 7f, 50f, 0.5f);
    }

	public override void SetDoorway(bool open)
	{
        if (!open)
        {
            ClosedTimer = 10f;
            CooldownTimer = 30f;
        }
        
		if (this.open == open)
		{
			return;
		}
		this.open = open;
		wallCollider.isTrigger = open;
		shadowColl.enabled = !open;
		if (bottomColl != null)
		{
			bottomColl.enabled = !open;
		}
		foreach (MushroomWallMushroom mushroomWallMushroom in mushrooms)
		{
			if (open)
			{
				mushroomWallMushroom.Hide();
			}
			else
			{
				mushroomWallMushroom.Show();
			}
		}
		if (open)
		{
			if (Constants.ShouldPlaySfx()/* && allowAudio*/)
			{
                var audio = SoundManager.Instance.PlaySound(openSound, false, 1,
                    SoundManager.Instance.SfxChannel);
                SoundDynamics(audio);
			}
			VibrationManager.Vibrate(2.5f, base.transform.position, 3f, 0f, VibrationManager.VibrationFalloff.None, openSound, false);
			return;
		}
		if (Constants.ShouldPlaySfx()/* && allowAudio*/)
		{
            var audio = SoundManager.Instance.PlaySound(closeSound, false, 1,
                SoundManager.Instance.SfxChannel);
            SoundDynamics(audio);
		}
		VibrationManager.Vibrate(2.5f, base.transform.position, 3f, 0f, VibrationManager.VibrationFalloff.None, closeSound, false);
	}

	// Token: 0x06000D37 RID: 3383 RVA: 0x00037C2C File Offset: 0x00035E2C
	private void DoorDynamics(AudioSource source, float dt)
	{
		if (!PlayerControl.LocalPlayer)
		{
			source.volume = 0f;
			return;
		}
		Vector2 vector = base.transform.position;
		Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
		float num = Vector2.Distance(vector, truePosition);
		if (num > 4f)
		{
			source.volume = 0f;
			return;
		}
		float num2 = 1f - num / 4f;
		source.volume = Mathf.Lerp(source.volume, num2, dt);
	}

	public override void Serialize(MessageWriter writer)
	{
		writer.Write(open);
	}

	public override void Deserialize(MessageReader reader)
	{
		SetDoorway(reader.ReadBoolean());
	}

    public BoxCollider2D wallCollider;

    public Collider2D shadowColl;

    public Collider2D bottomColl;

    public MushroomWallMushroom[] mushrooms;

    public AudioClip openSound;

	public AudioClip closeSound;

	private bool open;

	// private bool allowAudio;
}
