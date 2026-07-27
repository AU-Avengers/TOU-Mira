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

        public BaseBundleAchievement(string name, string description, LoadableAsset<Sprite> icon, int rarity = 0,
            bool hidden = false, bool hideRarity = true, Assembly? assembly = null) : base(name, description, rarity,
            hidden, hideRarity, assembly)
        {
            Name = name;
            Description = description;
            Assembly = assembly ?? Assembly.GetCallingAssembly();
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

    public CountBundleAchievement(string name, string description, LoadableAsset<Sprite> icon, int currentValue,
        int requiredValue, bool progressPersists = true, int rarity = 0, bool hidden = false, bool hideRarity = true,
        bool hideProgress = false) : base(name, description, currentValue, requiredValue, progressPersists, rarity,
        hidden, hideRarity, hideProgress)
    {
        MiraIcon = icon;
        CurrentValue = currentValue;
        RequiredValue = requiredValue;
        ProgressPersists = progressPersists;
        HideProgress = hideProgress;
    }
}