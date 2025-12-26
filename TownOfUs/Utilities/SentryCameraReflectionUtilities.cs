using System.Collections.Concurrent;
using System.Reflection;
using UnityEngine;

namespace TownOfUs.Utilities;

public static class SentryCameraReflectionUtilities
{
    private static readonly ConcurrentDictionary<Type, (Func<object, Camera?>? camGetter, Func<object, object?>? texGetter,
        Action<object, object?>? texSetter)> AccessorCache = new();

    public static (Camera? cameraPrefab, object? texturesObj, Action<object, object?>? texturesSetter)
        TryGetMinigameCameraData(object minigame)
    {
        var t = minigame.GetType();
        var accessors = AccessorCache.GetOrAdd(t, BuildAccessors);
        var cam = accessors.camGetter?.Invoke(minigame);
        var tex = accessors.texGetter?.Invoke(minigame);
        return (cam, tex, accessors.texSetter);
    }

    private static (Func<object, Camera?>? camGetter, Func<object, object?>? texGetter, Action<object, object?>? texSetter)
        BuildAccessors(Type t)
    {
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        Func<object, Camera?>? camGetter = null;
        foreach (var name in new[] { "CameraPrefab", "cameraPrefab", "_cameraPrefab", "CamPrefab" })
        {
            var f = t.GetField(name, flags);
            if (f != null && typeof(Camera).IsAssignableFrom(f.FieldType))
            {
                camGetter = obj => f.GetValue(obj) as Camera;
                break;
            }

            var p = t.GetProperty(name, flags);
            if (p != null && typeof(Camera).IsAssignableFrom(p.PropertyType) && p.GetGetMethod(true) != null)
            {
                camGetter = obj => p.GetValue(obj) as Camera;
                break;
            }
        }

        Func<object, object?>? texGetter = null;
        Action<object, object?>? texSetter = null;
        foreach (var name in new[] { "textures", "Textures", "_textures", "tex", "Tex" })
        {
            var f = t.GetField(name, flags);
            if (f != null)
            {
                texGetter = obj => f.GetValue(obj);
                texSetter = (obj, value) => f.SetValue(obj, value);
                break;
            }

            var p = t.GetProperty(name, flags);
            if (p != null && p.GetGetMethod(true) != null)
            {
                texGetter = obj => p.GetValue(obj);
                if (p.GetSetMethod(true) != null)
                {
                    texSetter = (obj, value) => p.SetValue(obj, value);
                }
                break;
            }
        }

        return (camGetter, texGetter, texSetter);
    }

    public static int GetTexturesLength(object texturesObj)
    {
        return texturesObj switch
        {
            RenderTexture[] arr => arr.Length,
            Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<RenderTexture> ilArr => ilArr.Length,
            _ => 0,
        };
    }

    public static object ResizeTextures(object texturesObj, int newLength)
    {
        switch (texturesObj)
        {
            case RenderTexture[] arr:
                {
                    var resized = new RenderTexture[newLength];
                    Array.Copy(arr, resized, arr.Length);
                    return resized;
                }
            case Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<RenderTexture> ilArr:
                {
                    var resized = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<RenderTexture>(newLength);
                    for (var i = 0; i < ilArr.Length && i < newLength; i++)
                    {
                        resized[i] = ilArr[i];
                    }
                    return resized;
                }
            default:
                return texturesObj;
        }
    }

    public static RenderTexture? GetTextureAt(object texturesObj, int index)
    {
        return texturesObj switch
        {
            RenderTexture[] arr => index >= 0 && index < arr.Length ? arr[index] : null,
            Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<RenderTexture> ilArr => index >= 0 && index < ilArr.Length ? ilArr[index] : null,
            _ => null,
        };
    }

    public static void SetTextureAt(object texturesObj, int index, RenderTexture texture)
    {
        switch (texturesObj)
        {
            case RenderTexture[] arr when index >= 0 && index < arr.Length:
                arr[index] = texture;
                break;
            case Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<RenderTexture> ilArr when index >= 0 && index < ilArr.Length:
                ilArr[index] = texture;
                break;
        }
    }
}

