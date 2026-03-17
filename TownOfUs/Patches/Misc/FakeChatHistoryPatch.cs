using HarmonyLib;
using TownOfUs.Utilities;

namespace TownOfUs.Patches.Misc;

/// <summary>
/// Hooks MiscUtils.AddFakeChat to record every call into FakeChatHistory
/// so the /info command can replay them.
/// </summary>
[HarmonyPatch(typeof(MiscUtils), nameof(MiscUtils.AddFakeChat))]
public static class FakeChatHistoryPatch
{
    public static void Prefix(string nameText, string message)
    {
        // Don't re-record while /info is actively replaying entries
        if (FakeChatHistory.IsReplaying)
        {
            return;
        }

        // Don't record the /info "no info" fallback message itself
        var noInfoKey = TouLocale.GetParsed("InfoCommandNoInfo");
        if (message.Contains(noInfoKey))
        {
            return;
        }

        FakeChatHistory.Record(nameText, message);
    }
}