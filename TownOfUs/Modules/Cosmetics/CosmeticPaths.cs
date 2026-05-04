using BepInEx;

namespace TownOfUs.Modules.Cosmetics;

public static class CosmeticPaths
{
    public static string StarlightPath => Environment.GetEnvironmentVariable("STAR_DATA_PATH")!;
    
    public static string BasePath { get; } = Path.Combine(
        OperatingSystem.IsAndroid() ? StarlightPath : Paths.GameRootPath,
        "CorsacCosmetics"
    );

    public static string PetPath { get; } = Path.Combine(BasePath, "Pets");

    public static void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(BasePath);
        Directory.CreateDirectory(PetPath);
    }
}