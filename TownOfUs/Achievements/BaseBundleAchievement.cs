    using System.Reflection;
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
        public override Sprite MenuSubIcon => MiraSubIcon.LoadAsset();
        public override Vector3 MenuSubIconOffset => new(-40f, -70f);
        public override Vector3 MenuSubIconScale => new(0.6f, 0.6f, 1);

        public override Vector3 MenuIconOffset => new(10f, -10f);
        public override Vector3 MenuTitleOffset => new(10f, -10f);
        public override Vector3 MenuDescOffset => new(10f, -10f);
        public override Vector3 ToastIconOffset => new(10f, -10f);

        public BaseBundleAchievement(string name, string description, LoadableAsset<Sprite> icon, LoadableAsset<Sprite> subIcon, int rarity = 0,
            bool hidden = false, bool hideRarity = true, Assembly? assembly = null) : base(name, description, rarity,
            hidden, hideRarity, assembly)
        {
            Name = name;
            Description = description;
            Assembly = assembly ?? Assembly.GetCallingAssembly();
            MiraSubIcon = subIcon;
            MiraIcon = icon;
            Id = Assembly.GetName().Name + "_" + Name;
            Rarity = rarity;
            Hidden = hidden;
            HideRarity = hideRarity;
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
    public override Sprite MenuSubIcon => MiraSubIcon.LoadAsset();
    public override Vector3 MenuSubIconOffset => new(-40f, -70f);
    public override Vector3 MenuSubIconScale => new(0.6f, 0.6f, 1);

    public override Vector3 MenuIconOffset => new(10f, -10f);
    public override Vector3 MenuTitleOffset => new(10f, -10f);
    public override Vector3 MenuDescOffset => new(10f, -10f);
    public override Vector3 ToastIconOffset => new(10f, -10f);

    public CountBundleAchievement(string name, string description, LoadableAsset<Sprite> icon, LoadableAsset<Sprite> subIcon, int currentValue,
        int requiredValue, bool progressPersists = true, int rarity = 0, bool hidden = false, bool hideRarity = true,
        bool hideProgress = false) : base(name, description, currentValue, requiredValue, progressPersists, rarity,
        hidden, hideRarity, hideProgress)
    {
        MiraSubIcon = subIcon;
        MiraIcon = icon;
        CurrentValue = currentValue;
        RequiredValue = requiredValue;
        ProgressPersists = progressPersists;
        HideProgress = hideProgress;
    }
}