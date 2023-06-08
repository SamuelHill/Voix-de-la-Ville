namespace TotT.Utilities {
    /// <summary>
    /// Adds short form numerals ("One", "Two", ... "Thirteen Thousand Twenty Six", ...) for printing ints.
    /// </summary>
    public static class IntExtensions {
        private static readonly string[] Ones = {
            "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine"
        };
        private static readonly string[] Teens = {
            "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen"
        };
        private static readonly string[] Tens = {
            "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety"
        };
        private static readonly string[] ThousandsGroups = { "", " Thousand", " Million", " Billion" };

        private static string NaturalNumeral(int n, string leftDigits = "", int thousands = 0) {
            if (n == 0) return leftDigits;
            if (leftDigits.Length > 0) leftDigits += " ";
            switch (n) {
                case < 10:
                    leftDigits += Ones[n];
                    break;
                case < 20:
                    leftDigits += Teens[n - 10];
                    break;
                case < 100:
                    leftDigits += NaturalNumeral(n % 10, Tens[n / 10 - 2]);
                    break;
                case < 1000:
                    leftDigits += NaturalNumeral(n % 100, Ones[n / 100] + " Hundred");
                    break;
                default: {
                    leftDigits += NaturalNumeral(n % 1000, NaturalNumeral(n / 1000, "", thousands + 1));
                    if (n % 1000 == 0) return leftDigits;
                    break;
                }
            }
            return leftDigits + ThousandsGroups[thousands];
        }

        public static string ToNumeral(this int num) => num switch {
            0 => "Zero", 
            < 0 => "Negative " + NaturalNumeral(-num), 
            _ => NaturalNumeral(num)
        };
    }
}
