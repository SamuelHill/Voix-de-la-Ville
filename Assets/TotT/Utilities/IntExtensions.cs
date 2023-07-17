namespace TotT.Utilities {
    /// <summary>
    /// Adds short form numerals ("One", "Two", ... "Thirteen Thousand Twenty Six", ...) for printing ints.
    /// </summary>
    public static class IntExtensions {
        private static readonly string[] OnesOrdinal = {
            "", "First", "Second", "Third", "Fourth", "Fifth", "Sixth", "Seventh", "Eighth", "Ninth"
        };
        private static readonly string[] TeensOrdinal = {
            "Tenth", "Eleventh", "Twelfth", "Thirteenth", "Fourteenth", "Fifteenth", "Sixteenth", "Seventeenth", "Eighteenth", "Nineteenth"
        };
        private static readonly string[] TensOrdinal = {
            "Twentieth", "Thirtieth", "Fortieth", "Fiftieth", "Sixtieth", "Seventieth", "Eightieth", "Ninetieth"
        };
        private static readonly string[] ThousandsGroupsOrdinal = { "", " Thousandth", " Millionth", " Billionth" };
        private static readonly string[] OnesCardinal = {
            "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine"
        };
        private static readonly string[] TeensCardinal = {
            "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen"
        };
        private static readonly string[] TensCardinal = {
            "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety"
        };
        private static readonly string[] ThousandsGroupsCardinal = { "", " Thousand", " Million", " Billion" };

        private static string NaturalCardinalNumeral(int n, string leftDigits = "", int thousands = 0) {
            if (n == 0) return leftDigits;
            if (leftDigits.Length > 0) leftDigits += " ";
            switch (n) {
                case < 10:
                    leftDigits += OnesCardinal[n];
                    break;
                case < 20:
                    leftDigits += TeensCardinal[n - 10];
                    break;
                case < 100:
                    leftDigits += NaturalCardinalNumeral(n % 10, TensCardinal[n / 10 - 2]);
                    break;
                case < 1000:
                    leftDigits += NaturalCardinalNumeral(n % 100, OnesCardinal[n / 100] + " Hundred");
                    break;
                default: {
                    leftDigits += NaturalCardinalNumeral(n % 1000, NaturalCardinalNumeral(n / 1000, "", thousands + 1));
                    if (n % 1000 == 0) return leftDigits;
                    break;
                }
            }
            return leftDigits + ThousandsGroupsCardinal[thousands];
        }

        private static string NaturalOrdinalNumeral(int n, string leftDigits = "", int thousands = 0) {
            if (n == 0) return leftDigits;
            if (leftDigits.Length > 0) leftDigits += " ";
            switch (n) {
                case < 10:
                    leftDigits += OnesOrdinal[n];
                    break;
                case < 20:
                    leftDigits += TeensOrdinal[n - 10];
                    break;
                case 20:
                    leftDigits += TensOrdinal[0];
                    break;
                case 30:
                    leftDigits += TensOrdinal[1];
                    break;
                case 40:
                    leftDigits += TensOrdinal[2];
                    break;
                case 50:
                    leftDigits += TensOrdinal[3];
                    break;
                case 60:
                    leftDigits += TensOrdinal[4];
                    break;
                case 70:
                    leftDigits += TensOrdinal[5];
                    break;
                case 80:
                    leftDigits += TensOrdinal[6];
                    break;
                case 90:
                    leftDigits += TensOrdinal[7];
                    break;
                case < 100:
                    leftDigits += NaturalOrdinalNumeral(n % 10, TensCardinal[n / 10 - 2]);
                    break;
                case < 1000:
                    leftDigits += NaturalOrdinalNumeral(n % 100, OnesCardinal[n / 100] + " Hundredth");
                    break;
                default: {
                    leftDigits += NaturalOrdinalNumeral(n % 1000, NaturalOrdinalNumeral(n / 1000, "", thousands + 1));
                    if (n % 1000 == 0) return leftDigits;
                    break;
                }
            }
            return leftDigits + ThousandsGroupsOrdinal[thousands];
        }

        public static string ToCardinal(this int num) => num switch {
            0 => "Zero", 
            < 0 => "Negative " + NaturalCardinalNumeral(-num), 
            _ => NaturalCardinalNumeral(num)
        };

        public static string ToOrdinal(this int num) => num switch {
            0 => "Zeroth",
            < 0 => NaturalOrdinalNumeral(-num),
            _ => NaturalOrdinalNumeral(num)
        };
    }
}
