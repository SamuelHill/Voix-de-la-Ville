namespace TotT.TextGenerator
{
    public static class Generators
    {
        public static TextGenerator Choice(params Choice.Option[] options) => new Choice(options);
       
        public static TextGenerator OneOf(params OneOf.Choice[] choices) => new OneOf(choices);

        public static TextGenerator Sequence(params TextGenerator[] segments) => new Sequence(segments);

        public static readonly TextGenerator CatholicLiturgicalName = 
                        Choice(
                            Sequence("Saint ",
                                        OneOf(
                                            "Agnes", "Anna", "Anthony", "Agatha", "Augustine", 
                                            "Benedict", "Boniface",
                                            "Constantine", "Columba",
                                            "Edmund", "Edward",
                                            "George",
                                            "Helena",
                                            "John",
                                            "Luke", "Lawrence",
                                            "Matthias", "Martin",
                                            "Patrick"
                                        )),
                        Sequence("Our Lady of ",
                                            OneOf(
                                                "Comfort",
                                                "Mercy",
                                                "Sorrows",
                                                "Charity",
                                                "Providence",
                                                "Random",     // This is apparently real
                                                "Solitude",
                                                "Confidence",
                                                "Consolation",
                                                "Good Success",
                                                "Guidance",
                                                "the Hens",
                                                "Peace",
                                                "Perpetual Help",
                                                "Prompt Succor",
                                            (0.2, "the Rocks")
                        )));

        public static readonly TextGenerator Church = 
            Sequence(CatholicLiturgicalName, " Catholic Church");

        public static TextGenerator President = new OneOf(nameof(President),
            (10, "John F. Kennedy"),
            "Richard M. Nixon",
            "Donald J. Trump",
            "Barack H. Obama",
            "Franklin D. Roosevelt",
            "Harry S. Truman",
            "Dwight D. Eisenhower",
            (10, "George Washington"),
            (5, "Thomas Jefferson"),
            "Madison",
            "Andrew Jackson",
            "Millard Fillmore",
            (10, "Lincoln"),
            "Grant",
            "Garfield",
            "Gover Cleveland",
            "McKinley",
            "Theodore Roosevelt",
            "William Howard Taft",
            "Woodrow Wilson");

        public static TextGenerator ElementarySchool = Sequence(
            President, 
            OneOf("elementary school names",
                " Elementary School",
                " Grammar School",
                " School"));
    }
}
