using Reactor.Utilities.Attributes;

namespace TownOfUs.Modules;

[RegisterInIl2Cpp]
public sealed class FakePlayer(IntPtr cppPtr) : BaseFakePlayer(cppPtr)
{
    public static FakePlayer CreateFakePlayer(PlayerControl player)
    {
        return BaseFakePlayerExt.CreateBaseFake<FakePlayer>(player);
    }
}