namespace TotT.Utilities.CFG {
    public static class NameGrammars {
        // ReSharper disable InconsistentNaming

        private static readonly Constant St = new("St");
        private static readonly Constant Memorial = new("Memorial");

        // Ars Goetia Demon Kings... So not liturgical but also 'liturgical'
        private static readonly Terminal Liturgical = new(
            "Asmodeus", "Bael", "Balam", "Beleth", "Belial", "Paimon", "Purson", "Zagon");
        private static readonly Terminal HospitalAdj = new("Health Care", "Medical", "Wellness");
        private static readonly Terminal MedicalCenter = new("Center", "Clinic", "Hospital");

        private static readonly Production SaintSomething = new(St, Liturgical);
        private static readonly Production HospitalName = new(HospitalAdj, MedicalCenter);
        private static readonly Production SaintHospitalName = new(SaintSomething, HospitalName);
        private static readonly Production MemorialHospitalName = new(Memorial, HospitalName);

        public static readonly Grammar HospitalNames = new(SaintSomething, SaintHospitalName, MemorialHospitalName);

        private static readonly Constant The = new("The");

        private static readonly Terminal ChildCareCenters = new(
            "Academy", "Child Care", "Daycare", "Learning Center", "Nursery", "Preschool");
        private static readonly Terminal ChildCareAdj = new(
            "Sunshine", "Rainbow", "Pumpkin", "Shinning Stars", "Fantastic Friends", "Little Wonders",
            "Dragonfly", "Friendly Faces", "New Beginnings", "Wildflower", "Butterfly", "Precious Angel");

        private static readonly Production ChildCare = new(ChildCareAdj, ChildCareCenters);
        private static readonly Production TheChildCare = new(The, ChildCare);

        public static readonly Grammar DaycareNames = new(ChildCare, TheChildCare);

        private static readonly Constant Cemetery = new("Cemetery");


    }
}
