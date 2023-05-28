namespace TotT.Utilities {
    public static class ByteExtensions {
        public static string SuffixedDate(this byte num) {
            var number = num.ToString();
            return number.EndsWith("11") ? number + "th" :
                number.EndsWith("12") ? number + "th" :
                number.EndsWith("13") ? number + "th" :
                number.EndsWith("1") ? number + "st" :
                number.EndsWith("2") ? number + "nd" :
                number.EndsWith("3") ? number + "rd" :
                number + "th";
        }
    }
}