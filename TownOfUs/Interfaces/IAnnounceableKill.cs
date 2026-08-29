namespace TownOfUs.Interfaces;

public interface IAnnounceableKill
{
    void AnnounceKill(PlayerControl source, PlayerControl victim);
}