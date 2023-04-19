using System.Globalization;
using System.Threading;

public static class Utils {
    public static readonly CultureInfo CultureInfo = Thread.CurrentThread.CurrentCulture;
    public static readonly TextInfo TextInfo = CultureInfo.TextInfo;

    public static string Title(string title) => TextInfo.ToTitleCase(title);
}
