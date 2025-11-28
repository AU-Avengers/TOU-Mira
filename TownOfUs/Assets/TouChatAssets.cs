using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace TownOfUs.Assets;

public static class TouChatAssets
{
    private const string ChatPath = "TownOfUs.Resources.Chat";

    public static LoadableAsset<Sprite> ImpBubble { get; } = new LoadableResourceAsset($"{ChatPath}.ChatImpBubble.png");
    public static LoadableAsset<Sprite> JailBubble { get; } = new LoadableResourceAsset($"{ChatPath}.ChatJailBubble.png");
    public static LoadableAsset<Sprite> VampBubble { get; } = new LoadableResourceAsset($"{ChatPath}.ChatVampBubble.png");
    public static LoadableAsset<Sprite> TeamChatIdle { get; } = new LoadableResourceAsset($"{ChatPath}.TeamChatIdle.png");
    public static LoadableAsset<Sprite> TeamChatHover { get; } = new LoadableResourceAsset($"{ChatPath}.TeamChatHover.png");
    public static LoadableAsset<Sprite> TeamChatOpen { get; } = new LoadableResourceAsset($"{ChatPath}.TeamChatOpen.png");

    public static LoadableAsset<Sprite> LoveBubble { get; } = new LoadableResourceAsset($"{ChatPath}.ChatLoveBubble.png");
    public static LoadableAsset<Sprite> LoveChatIdle { get; } = new LoadableResourceAsset($"{ChatPath}.LoveChatIdle.png");
    public static LoadableAsset<Sprite> LoveChatHover { get; } = new LoadableResourceAsset($"{ChatPath}.LoveChatHover.png");
    public static LoadableAsset<Sprite> LoveChatOpen { get; } = new LoadableResourceAsset($"{ChatPath}.LoveChatOpen.png");
    
    public static LoadableAsset<Sprite> NormalBubble { get; } = new LoadableResourceAsset($"{ChatPath}.ChatBubble.png");
    public static LoadableAsset<Sprite> NormalChatIdle { get; } = new LoadableResourceAsset($"{ChatPath}.NormalChatIdle.png");
    public static LoadableAsset<Sprite> NormalChatHover { get; } = new LoadableResourceAsset($"{ChatPath}.NormalChatHover.png");
    public static LoadableAsset<Sprite> NormalChatOpen { get; } = new LoadableResourceAsset($"{ChatPath}.NormalChatOpen.png");
}
