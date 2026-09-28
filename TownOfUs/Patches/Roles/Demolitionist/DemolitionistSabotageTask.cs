using Il2CppInterop.Runtime.Attributes;
using Il2CppSystem.Text;
using Reactor.Utilities.Attributes;
using TownOfUs.Utilities;
using MiraAPI.Translation;
using UnityEngine;

namespace TownOfUs.Patches;

[RegisterInIl2Cpp]
public sealed class DemolitionistSabotageTask(nint cppPtr) : SabotageTask(cppPtr)
{
    public override int TaskStep => _isComplete ? 1 : 0;
    public override bool IsComplete => _isComplete;
    private bool _isComplete;

    public override bool ValidConsole(Console console) => false;

    public override void Initialize()
    {
    }

    public override void AppendTaskText(Il2CppSystem.Text.StringBuilder sb)
    {
        if (!DemolitionistSabotageState.IsActive)
        {
            return;
        }

        var color = DemolitionistSabotageState.GetCurrentPulseColor();
        var location = DemolitionistSabotageState.PlantedLocationName;
        var seconds = Mathf.CeilToInt(DemolitionistSabotageState.TimeRemaining);
        var hex = ColorUtility.ToHtmlStringRGB(color);

        var text = MiraLocaleManager
            .Get("TownOfUsMira.Role.Demolitionist.Task.SabotageActive")
            .Replace("<location>", location)
            .Replace("<seconds>", seconds.ToString());

        sb.AppendLine(
            $"<color=#{hex}>{text}</color>");
    }

    public override void Complete()
    {
        if (_isComplete)
        {
            return;
        }

        _isComplete = true;

        if (Owner != null)
        {
            Owner.RemoveTask(this);
        }

        if (gameObject != null)
        {
            UnityEngine.Object.Destroy(gameObject);
        }
    }
}
