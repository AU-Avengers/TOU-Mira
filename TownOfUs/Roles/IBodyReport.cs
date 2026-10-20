using TownOfUs.Modules;

namespace TownOfUs.Roles;

public interface IBodyReport
{
    PlayerControl Player { get; }

    bool ReportEnabled { get; }

    void ReportBody(BodyReport br);
}
