using TownOfUs.Modules;

namespace TownOfUs.Roles;

public interface IBodyReport
{
    PlayerControl Player { get; }

    bool ReportEnabled { get; }

    void ReportBody(BodyReport br);
}

public interface IBodyReportChat : IBodyReport
{
    string BodyReportTitle { get; }

    string ParseReport(BodyReport br);

    void IBodyReport.ReportBody(BodyReport br)
    {
        var reportMsg = ParseReport(br);

        if (string.IsNullOrWhiteSpace(reportMsg))
        {
            return;
        }

        var reported = Player;
        if (br.Body != null)
        {
            reported = br.Body;
        }

        MiscUtils.AddFakeChat(reported.Data, BodyReportTitle, reportMsg, false, true);
    }
}
