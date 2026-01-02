using System.Reflection;
using UnityEngine;

namespace TownOfUs.Modules.TimeLord;

/// <summary>
/// Utilities for animation handling in Time Lord rewind system.
/// </summary>
internal static class TimeLordAnimationUtilities
{
    private static readonly MethodInfo? NetTransformInvisibleAnimMethod =
#pragma warning disable S3011
        typeof(CustomNetworkTransform).GetMethod("IsInMiddleOfAnimationThatMakesPlayerInvisible",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
#pragma warning restore S3011

    private sealed class SpecialClipSet
    {
        public AnimationClip? LadderAny { get; set; }
        public AnimationClip? LadderUp { get; set; }
        public AnimationClip? LadderDown { get; set; }
    }

    private static readonly Dictionary<int, SpecialClipSet> SpecialClipsByGroupHash = new();

    public static bool IsInInvisibleAnimation(PlayerControl lp)
    {
        try
        {
            if (NetTransformInvisibleAnimMethod != null && lp.NetTransform != null)
            {
                return (bool)NetTransformInvisibleAnimMethod.Invoke(lp.NetTransform, null)!;
            }
        }
        catch
        {
            // ignored
        }

        return false;
    }

    public static void ApplySpecialAnimation(PlayerPhysics physics, SpecialAnim anim, Vector2 delta)
    {
        if (physics?.Animations?.Animator == null)
        {
            return;
        }

        var animator = physics.Animations.Animator;
        var group = physics.Animations.group;
        if (group == null)
        {
            return;
        }

        var hash = group.GetHashCode();
        if (!SpecialClipsByGroupHash.TryGetValue(hash, out var set))
        {
            set = BuildSpecialClipSet(group);
            SpecialClipsByGroupHash[hash] = set;
        }

        AnimationClip? desired = null;
        var goingUp = delta.y > 0.001f;
        var goingDown = delta.y < -0.001f;

        if (anim == SpecialAnim.Ladder)
        {
            if (goingUp)
            {
                desired = set.LadderDown ?? set.LadderAny;
            }
            else if (goingDown)
            {
                desired = set.LadderUp ?? set.LadderAny;
            }
        }
        else
        {
            return;
        }

        if (desired == null)
        {
            return;
        }

        try
        {
            var cur = animator.GetCurrentAnimation();
            if (cur != desired)
            {
                animator.Play(desired);
            }
        }
        catch
        {
            try
            {
                animator.Play(desired);
            }
            catch
            {
                // ignored
            }
        }
    }

    private static SpecialClipSet BuildSpecialClipSet(object group)
    {
        var set = new SpecialClipSet();
        var groupType = group.GetType();

#pragma warning disable S3011
        foreach (var field in groupType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (field.FieldType != typeof(AnimationClip))
            {
                continue;
            }

            var clip = field.GetValue(group) as AnimationClip;
            if (clip == null)
            {
                continue;
            }

            ClassifyClip(set, field.Name, clip);
        }

        foreach (var prop in groupType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
#pragma warning restore S3011
        {
            if (!prop.CanRead || prop.GetIndexParameters().Length != 0 || prop.PropertyType != typeof(AnimationClip))
            {
                continue;
            }

            AnimationClip? clip = null;
            try
            {
                clip = prop.GetValue(group, null) as AnimationClip;
            }
            catch
            {
                // ignored
            }

            if (clip == null)
            {
                continue;
            }

            ClassifyClip(set, prop.Name, clip);
        }

        return set;
    }

    private static void ClassifyClip(SpecialClipSet set, string memberName, AnimationClip clip)
    {
        var memberLower = (memberName ?? string.Empty).ToLowerInvariant();
        var clipLower = (clip.name ?? string.Empty).ToLowerInvariant();

        bool AnyContains(string s) => memberLower.Contains(s) || clipLower.Contains(s);

        if (AnyContains("ladder") || AnyContains("climb"))
        {
            set.LadderAny ??= clip;
            if (AnyContains("up") || AnyContains("top"))
            {
                set.LadderUp ??= clip;
            }
            if (AnyContains("down") || AnyContains("bottom"))
            {
                set.LadderDown ??= clip;
            }
        }
    }
}