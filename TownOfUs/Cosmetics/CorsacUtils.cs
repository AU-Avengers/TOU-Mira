using CorsacCosmetics;
using CorsacCosmetics.Cosmetics;
using CorsacCosmetics.Cosmetics.AssetReaders;
using CorsacCosmetics.Cosmetics.Pets;
using CorsacCosmetics.Cosmetics.Sources;
using UnityEngine;

namespace TownOfUs.Cosmetics;

public static class CorsacUtils
{
    public struct QuickPetMetadata
    {
        public string Name { get; set; } = "Custom Pet";

        public string ReferenceName { get; set; } = "CustomPet.prefab";

        public bool MatchPlayerColor { get; set; } = false;

        public QuickPetMetadata()
        {
        }

        public QuickPetMetadata(string name, string refName, bool matchPlayerColor)
        {
            Name = name;
            ReferenceName = refName;
            MatchPlayerColor = matchPlayerColor;
        }
    }

    public static void AddMultiplePets(string sourceId,
        string group, QuickPetMetadata[] metadata, AssetBundle assetBundle)
    {
        List<CosmeticDescriptor> descriptors = [];
        foreach (var metadataItem in metadata)
        {
            descriptors.Add(CreatePetDescriptor(sourceId, group, metadataItem, assetBundle));
        }

        SourceRegistry.Instance.RegisterSource(new ManualCosmeticSource(descriptors));
    }

    private static Dictionary<AssetBundle, AssetBundleReader> _bundleReaders = [];

    public static CosmeticDescriptor CreatePetDescriptor(string sourceId,
        string group,
        QuickPetMetadata quickMetadata,
        AssetBundle assetBundle)
    {
        var metadata = new PetMetadata()
        {
            Name = quickMetadata.Name,
            ReferenceName = quickMetadata.ReferenceName,
            MatchPlayerColor = quickMetadata.MatchPlayerColor,
        };
        if (!_bundleReaders.TryGetValue(assetBundle, out var reader))
        {
            reader = new AssetBundleReader(assetBundle);
            _bundleReaders.Add(assetBundle, reader);
        }

        return new CosmeticDescriptor(sourceId, group, quickMetadata.Name, CorsacCosmetics.Cosmetics.CosmeticType.Pet,
            metadata, reader);
    }

    public static void TryDownloadCosmeticBundle(string bundlePath, string downloadLink, string? outputFolder = null)
    {
        if (File.Exists(bundlePath))
        {
            return;
        }

        PluginCompat.QueueBundleDownload(downloadLink, outputFolder);
    }
}
