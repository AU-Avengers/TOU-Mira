namespace TownOfUs.Roles;

public interface IBasicGhostRole
{
    virtual int SpawnPriority => 0;
    virtual bool ApplyImmediatelyAfterDeath => true;

    public virtual List<PlayerControl> GetAvailableGhosts(List<PlayerControl> basicGhosts, PlayerControl? exiled)
    {
        // empty by default!
        return basicGhosts;
    }
}

public interface IGhostRole : IBasicGhostRole
{
    bool ApplyImmediatelyAfterDeath => false;
    bool Setup { get; set; }
    bool Caught { get; set; }
    bool Faded { get; set; }
    bool CanBeClicked { get; set; }
    bool GhostActive => Setup && !Caught;

    void Spawn();

    void FadeUpdate();

    void Clicked();

    bool CanCatch();
}