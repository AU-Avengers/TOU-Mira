    using System.Reflection;
    using System.Runtime.CompilerServices;
    using AchievementsAPI.API;
    using UnityEngine;

    namespace TownOfUs.Achievements;

    /// <summary>
    /// Base Achievement class, used to define achievements.
    /// </summary>
    public class BaseBundleAchievement : BaseAchievement
    {
        /// <summary>
        /// The achievement's icon through MiraAPI.
        /// </summary>
        public LoadableAsset<Sprite> MiraIcon;

        public override Sprite Icon => MiraIcon.LoadAsset();
        public override Sprite MenuBgSprite => TouAchAssets.AchievementBox.LoadAsset();
        public override Sprite ToastBgSprite => TouAchAssets.AchievementToast.LoadAsset();
        /// <summary>
        /// The achievement's sub icon through MiraAPI.
        /// </summary>
        public LoadableAsset<Sprite> MiraSubIcon;
        public override Sprite? MenuSubIcon => MiraSubIcon != null ? MiraSubIcon.LoadAsset() : null;
        public override Vector3 MenuSubIconOffset => new(-40f, -70f);
        public override Vector3 MenuSubIconScale => new(0.6f, 0.6f, 1);

        public override Vector3 MenuIconOffset => new(10f, -10f);
        public override Vector3 MenuTitleOffset => new(10f, -10f);
        public override Vector3 MenuDescOffset => new(10f, -10f);
        public override Vector3 ToastIconOffset => new(10f, -10f);

        /// <summary>
        /// Creates an achievement whose name and description are resolved through the Mira
        /// locale manager as <c>TownOfUsMira.Achievement.{memberName}.Name/Description</c>
        /// (falling back to the provided text when no key exists).
        /// </summary>
        public BaseBundleAchievement(string name, string description, LoadableAsset<Sprite> icon, LoadableAsset<Sprite> subIcon, int rarity = 0,
            bool hidden = false, bool hideRarity = false, Assembly? assembly = null,
            [CallerMemberName] string memberName = "")
            : base(AchievementLocale.Key(memberName, name, "Name"), AchievementLocale.Key(memberName, description, "Description"), rarity,
                hidden, hideRarity, assembly)
        {
            MiraSubIcon = subIcon;
            MiraIcon = icon;
            Id = Assembly.GetName().Name + "_" + name;
        }
    }

/// <summary>
/// Achievement class for achievements that can increment.
/// </summary>
public class CountBundleAchievement : CountAchievement
{
    /// <summary>
    /// The achievement's icon through MiraAPI.
    /// </summary>
    public LoadableAsset<Sprite> MiraIcon;

    public override Sprite Icon => MiraIcon.LoadAsset();
    public override Sprite MenuBgSprite => TouAchAssets.AchievementBox.LoadAsset();
    public override Sprite ToastBgSprite => TouAchAssets.AchievementToast.LoadAsset();

    /// <summary>
    /// The achievement's sub icon through MiraAPI.
    /// </summary>
    public LoadableAsset<Sprite> MiraSubIcon;
    public override Sprite? MenuSubIcon => MiraSubIcon != null ? MiraSubIcon.LoadAsset() : null;
    public override Vector3 MenuSubIconOffset => new(-40f, -70f);
    public override Vector3 MenuSubIconScale => new(0.6f, 0.6f, 1);

    public override Vector3 MenuIconOffset => new(10f, -10f);
    public override Vector3 MenuTitleOffset => new(10f, -10f);
    public override Vector3 MenuDescOffset => new(10f, -10f);
    public override Vector3 ToastIconOffset => new(10f, -10f);

    /// <inheritdoc cref="BaseBundleAchievement(string, string, LoadableAsset{Sprite}, LoadableAsset{Sprite}, int, bool, bool, Assembly, string)"/>
    public CountBundleAchievement(string name, string description, LoadableAsset<Sprite> icon, LoadableAsset<Sprite> subIcon, int currentValue,
        int requiredValue, AchPersistence progressPersists = AchPersistence.ThroughoutSessions, int rarity = 0, bool hidden = false, bool hideRarity = false,
        bool hideProgress = false, [CallerMemberName] string memberName = "")
        : base(AchievementLocale.Key(memberName, name, "Name"), AchievementLocale.Key(memberName, description, "Description"), currentValue,
            requiredValue, progressPersists, rarity, hidden, hideRarity, hideProgress)
    {
        MiraSubIcon = subIcon;
        MiraIcon = icon;
        Id = Assembly.GetName().Name + "_" + name;
    }
}

internal static class AchievementLocale
{
    /// <summary>
    /// Builds a translatable <see cref="StringNames"/> keyed
    /// <c>TownOfUsMira.Achievement.{memberName}.{part}</c>. When no member name is known
    /// (non-property call sites), the display text itself is used as the key so English
    /// still resolves correctly.
    /// </summary>
    public static StringNames Key(string memberName, string text, string part) =>
        MiraLocaleManager.GetOrCreateLocaleString(memberName.Length == 0 ? text : $"TownOfUsMira.Achievement.{memberName}.{part}");
}
