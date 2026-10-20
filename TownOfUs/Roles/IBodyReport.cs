using TownOfUs.Modules;

namespace TownOfUs.Roles;

public interface IBaseBodyReport
{
    PlayerControl Player { get; }

    bool ReportEnabled { get; }
}

public interface IBodyReport : IBaseBodyReport
{
    void ReportBody(BodyReport br);
}

public interface IBodyReportChat : IBaseBodyReport
{
    string BodyReportTitle { get; }

    string ParseReport(BodyReport br);
}
