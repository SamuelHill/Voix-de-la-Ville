using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;

public static class Utils {
    public static readonly CultureInfo CultureInfo = Thread.CurrentThread.CurrentCulture;
    public static readonly TextInfo TextInfo = CultureInfo.TextInfo;

    public static string Title(string title) => TextInfo.ToTitleCase(title);
    public static string VariableSpacing(string variable) => 
        string.Join(" ", Regex.Split(variable, @"(?=[A-Z][^A-Z])"));
    public static string Heading(string heading) => Title(VariableSpacing(heading));
}
