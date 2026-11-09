using TownOfUs.Modules;

namespace TownOfUs.Roles;

/// <summary>
/// Interface for roles that require a <see cref="BodyReport"/> when reporting a body.
/// <para/>
/// Should not be implemented directly; implement <see cref="IBodyReport"/> or <see cref="IBodyReportChat"/> instead.
/// </summary>
public interface IBaseBodyReport
{
    PlayerControl Player { get; }

    bool ReportEnabled { get; }
}

/// <summary>
/// Interface for roles that require a <see cref="BodyReport"/> when reporting a body.
/// </summary>
public interface IBodyReport : IBaseBodyReport
{
    void ReportBody(BodyReport br);
}

/// <summary>
/// Interface for roles that require a <see cref="BodyReport"/>
/// when reporting a body to send a message in chat to the reporter.
/// </summary>
public interface IBodyReportChat : IBaseBodyReport
{
    string BodyReportTitle { get; }

    string ParseReport(BodyReport br);
}
