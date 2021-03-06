namespace PokerTell.User.Interfaces
{
    using Infrastructure.Interfaces;

    public interface IReporter : IFluentInterface
    {
        string LogfileContent { get; }

        string ScreenShotFile { get; }

        IReporter DeleteReportingTempFolder();

        IReporter PrepareReport();

        IReporter SendReport(string caption, string comments, bool includeScreenshot);
    }
}