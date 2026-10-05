using UnityEngine;

namespace TownOfUs.Assets;

public static class TouChatAssets
{
    private const string ChatPath = "TownOfUs.Resources.Chat";

    public static LoadableAsset<Sprite> ImpBubble { get; } = new LoadableBundleSubAsset("ChatImpBubble", TouAssets.UiSpriteHolder);
    public static LoadableAsset<Sprite> JailBubble { get; } = new LoadableBundleSubAsset("ChatJailBubble", TouAssets.UiSpriteHolder);
    public static LoadableAsset<Sprite> VampBubble { get; } = new LoadableBundleSubAsset("ChatVampBubble", TouAssets.UiSpriteHolder);
    public static LoadableAsset<Sprite> TeamChatIdle { get; } = new LoadableBundleSubAsset("TeamChatIdle", TouAssets.UiSpriteHolder);
    public static LoadableAsset<Sprite> TeamChatHover { get; } = new LoadableBundleSubAsset("TeamChatHover", TouAssets.UiSpriteHolder);
    public static LoadableAsset<Sprite> TeamChatOpen { get; } = new LoadableBundleSubAsset("TeamChatOpen", TouAssets.UiSpriteHolder);

    public static LoadableAsset<Sprite> LoveBubble { get; } = new LoadableBundleSubAsset("ChatLoveBubble", TouAssets.UiSpriteHolder);
    public static LoadableAsset<Sprite> LoveChatIdle { get; } = new LoadableBundleSubAsset("LoveChatIdle", TouAssets.UiSpriteHolder);
    public static LoadableAsset<Sprite> LoveChatHover { get; } = new LoadableBundleSubAsset("LoveChatHover", TouAssets.UiSpriteHolder);
    public static LoadableAsset<Sprite> LoveChatOpen { get; } = new LoadableBundleSubAsset("LoveChatOpen", TouAssets.UiSpriteHolder);
    
    public static LoadableAsset<Sprite> NormalBubble { get; } = new LoadableBundleSubAsset("ChatBubble", TouAssets.UiSpriteHolder);
    public static LoadableAsset<Sprite> NormalChatIdle { get; } = new LoadableBundleSubAsset("NormalChatIdle", TouAssets.UiSpriteHolder);
    public static LoadableAsset<Sprite> NormalChatHover { get; } = new LoadableBundleSubAsset("NormalChatHover", TouAssets.UiSpriteHolder);
    public static LoadableAsset<Sprite> NormalChatOpen { get; } = new LoadableBundleSubAsset("NormalChatOpen", TouAssets.UiSpriteHolder);
}
