using System.Collections.Generic;
using System.Linq;
using TED;
using VdlV.Utilities;
using VdlV.ValueTypes;
using Vector2Int = UnityEngine.Vector2Int;

namespace VdlV.TextGenerator {
    using static LocationType;
    using static IntExtensions;
    using static Randomize;

    // ReSharper disable InconsistentNaming
    public static class Generators {
        private static TextGenerator Choice(params Choice.Option[] options) => new Choice(options);
        private static TextGenerator OneOf(params OneOf.Choice[] choices) => new OneOf(choices);
        private static TextGenerator Sequence(params TextGenerator[] segments) => new Sequence(segments);

        #region Location naming
        #region Vector2Int based naming
        public static Parameter<Vector2Int> PositionXComponentName = new(nameof(PositionXComponentName),
            position => {
                var ord = position.x.ToOrdinal();
                var street = position.x == 0 ? "Main" : position.x > 0 ? $"East {ord}" : $"West {ord}";
                return $"{street} Street";
            });
        public static Parameter<Vector2Int> PositionYComponentName = new(nameof(PositionYComponentName),
            position => {
                var ord = position.y.ToOrdinal();
                return position.y == 0 ? "Main Avenue" : position.y > 0 ? $"{ord} Avenue North" : $"{ord} Avenue South";
            });
        private static readonly TextGenerator PositionName = Choice(PositionXComponentName, PositionYComponentName);

        public static Function<Vector2Int, string> NamedPosition {
            get {
                var rng = MakeRng();
                return new Function<Vector2Int, string>(nameof(NamedPosition), v => {
                    var bind = new BindingList(PositionXComponentName, v, null).Bind(PositionYComponentName, v);
                    return PositionName.Generate(bind, rng);
                }, false);
            }
        }
        #endregion

        public static Parameter<string> TownName = new(nameof(TownName));

        private static readonly TextGenerator Animal = OneOf(
            "Bear", "Deer", "Beaver", "Elk", "Squirrel", "Eagle");
        public static readonly TextGenerator PossibleTownName = Choice(
            Sequence(Animal, OneOf("ville", "ton")),
            Sequence(Animal, " ", OneOf("Crossing", "Fork", "Bend", "Hill",
                                        "Mound", "River", "Creek", "Lake", "Landing")));

        private static readonly TextGenerator The = OneOf("The ", "");
        private static readonly TextGenerator TheTownName = Sequence(The, TownName, " ");

        private static readonly TextGenerator CemeteryName = Sequence(TheTownName, "Cemetery");
        private static readonly TextGenerator CityHallName = Sequence(TheTownName, "City Hall");
        private static readonly TextGenerator PostOfficeName = Sequence(TheTownName, "Post Office");
        private static readonly TextGenerator PoliceStationName = Sequence(TheTownName, "Police Station");
        private static readonly TextGenerator FireStationName = Sequence(TheTownName, "Fire Station");
        private static readonly TextGenerator BankName = Sequence(TheTownName, "Bank");
        private static readonly TextGenerator CommunityCenterName = Sequence(TheTownName, "Community Center");

        public static Parameter<string> RandomNumber {
            get {
                var rng = MakeRng();
                return new Parameter<string>(nameof(RandomNumber), _ => Byte(rng, 55).ToString());
            }
        }

        private static readonly TextGenerator StreetNumbers = OneOf(Enumerable.Range(1, 56).Select(i => (OneOf.Choice)i.ToString()).ToArray());

        private static readonly TextGenerator StreetName = Sequence(
            OneOf("Elm", "Oak", "Maple", "Pine", "Cedar", "Walnut", "Spruce", "Ash", "Birch", "Chestnut", "Sycamore", 
                  "Hickory", "Beech", "Poplar", "Willow", "Juniper", "Pinecone", "Redwood", "Cypress"), " ", 
            OneOf("Street", "Avenue", "Lane", "Drive", "Boulevard"));
        private static readonly TextGenerator AddressedStreet = Sequence(StreetNumbers, " ", StreetName);

        private static readonly TextGenerator FarmName = Sequence(
            OneOf("Tumbleweed", "Prairie", "Wildflower", "Lone Oak", "Crown", "Happy Horse", "Sweet Dreams", 
                  "Deer Hill", "Meadowbrooke", "Whitewater", "Pleasant View", "Silver Tree", "Rainbow Hill", 
                  "Grizzly Bear", "Eagle's Nest", "Rooster", "Whispering Willow", "Mossy Pine", "New Hope", 
                  "Angry Beaver", "Riverrock", "Heartsong", "Dandelion", "Blue Moon", "Maplewood", "New Morning", 
                  "Grumpy Llama", "White Willow", "Oakey Dokey", "Bearded Goat", "Haxelwood", "Black Sheep", "Red Dog", 
                  "Mountain Ridge", "Furball", "Sleepy Hollow", "Little Wolf", "Sycamore", "Small Paws", "Crooked Kitten"), " ", 
            OneOf("Ranch", "Acres", "Fields", "Farmstead", "Range", "Grange", "Pastures", "Meadow", "Farm"));

        private static readonly TextGenerator ParkName = Sequence(
            OneOf("Lincoln", "Forest", "Washington", "Riverside", "Elmwood", "Garfield", "Victory", "Euclid", "Central", 
                  "Prospect", "Meadowview", "Whispering Pines", "Willowbrook", "Lakeside Retreat", "Oakwood Grove", "Riverside Meadows", 
                  "Pinehurst", "Maplewood", "Cedar Creek", "Sunnydale", "Willow Lake", "Bellwood Gardens", "Elmwood Heights", "Forestwood", 
                  "Briarcliff", "Wildwood Springs", "Shady Hollow", "Sunnyside Meadows", "Riverside", "Cherrywood"), " Park");

        private static readonly TextGenerator OrchardName = Sequence(
            OneOf("Maple Acres", "Oak Hill", "Applewood", "Pine Grove", "Willow Springs", "Cedar Ridge", "Elmwood", "Walnut Grove", 
                  "Hickory Hills", "Peachtree", "Cherry Blossom", "Golden Harvest", "Plum Grove", "Berry Patch", "Pear Tree",
                  "Harvest Moon", "Red Barn", "Fox Hollow", "Mulberry Creek", "Sycamore Grove", "Meadowview", "Whispering Pines", 
                  "Willowbrook", "Oakwood Grove", "Maplewood", "Cedar Creek", "Cherrywood"), " Orchard");

        private static readonly TextGenerator President = OneOf(
            (10, "John F. Kennedy"), (10, "George Washington"), (10, "Lincoln"), (5, "Thomas Jefferson"),
            "Franklin D. Roosevelt", "Harry S. Truman", "Dwight D. Eisenhower", "Madison", "Andrew Jackson", "Millard Fillmore",
            "Grant", "Garfield", "Grover Cleveland", "McKinley", "Theodore Roosevelt", "William Howard Taft", "Woodrow Wilson");

        private static readonly TextGenerator HighSchoolName = Sequence(
            Choice(President, TownName), " High School");

        private static readonly TextGenerator ChildCare = Sequence(
            OneOf("Sunshine", "Rainbow", "Pumpkin", "Shinning Stars", "Fantastic Friends", "Little Wonders",
                  "Dragonfly", "Friendly Faces", "New Beginnings", "Wildflower", "Butterfly", "Precious Angel"),
            OneOf("Academy", "Child Care", "Daycare", "Learning Center", "Nursery", "Preschool"));
        private static readonly TextGenerator DaycareName = Sequence(The, ChildCare);

        private static readonly TextGenerator MidwestDescriptor = Choice(TheTownName,
            (4, Sequence(The, OneOf("Midwest", "Prairie", "Heartland", "Elmwood",
                                    "Highland", "Oakhurst", "Pinehurst"), " ")));

        private static readonly TextGenerator InnName = Sequence(MidwestDescriptor, 
            OneOf("Inn", "Hotel", "Lodge", "Retreat", "Cottage"));

        private static readonly TextGenerator DoctorOfficeName = Sequence(MidwestDescriptor, 
            OneOf("Health Clinic", "Family Medicine", "Health Services", "Family Physicians", "Physicians Group"));

        private static readonly TextGenerator PharmacyName = Sequence(MidwestDescriptor,
            OneOf("Pharmacy", "Drug Store", "Drug Co.", "Apothecary"));

        private static readonly TextGenerator DentistOfficeName = Sequence(MidwestDescriptor,
            OneOf("Dental Associates", "Dental Clinic", "Dental Studio", "Dentistry", 
                  "Dental Practice", "Dental Care", "Dental Health", "Dental Group"));

        private static readonly TextGenerator OptometryClinicName = Sequence(MidwestDescriptor,
            OneOf("Vision Center", "Optometry Clinic", "Eye Care Center", "Vision Clinic"));

        private static readonly TextGenerator ArsGoetiaKings = Sequence("St ", 
            OneOf("Asmodeus", "Bael", "Balam", "Beleth", "Belial", "Paimon", "Purson", "Zagan"));

        private static readonly TextGenerator MedicalName = Sequence(OneOf("Memorial ", ""), 
            OneOf("Health Care", "Medical", "Wellness"), " ", OneOf("Center", "Clinic", "Hospital"));

        private static readonly TextGenerator HospitalName = Choice(ArsGoetiaKings, 
            Sequence(Choice(TownName, ArsGoetiaKings), MedicalName));

        private static readonly TextGenerator CatholicLiturgicalName = Choice(
            Sequence("Saint ", OneOf(
                         "Agnes", "Anna", "Anthony", "Agatha", "Augustine",
                         "Benedict", "Boniface", "Constantine", "Columba", "Edmund",
                         "Edward", "George", "Helena", "John", "Luke",
                         "Lawrence", "Matthias", "Martin", "Patrick")),
            Sequence("Our Lady of ", OneOf(
                         "Comfort", "Mercy", "Sorrows", "Charity", "Providence", "Random",
                         "Solitude", "Confidence", "Consolation", "Good Success", "Guidance",
                         "the Hens", "Peace", "Perpetual Help", "Prompt Succor", (0.2, "the Rocks"))));

        public static readonly TextGenerator CatholicChurchName =
            Sequence(CatholicLiturgicalName, " Catholic Church");

        private static readonly TextGenerator BreweryName = Sequence(
            OneOf("The Veil", "Steam Bell", "Stone", "Strangeways", "Main Line", "Sketchbook",
                  "Whistlestop", "Prairie Gold", "Heartland", "Amber Grains", "Final Gravity"), " ",
            OneOf((5, "Brewery"), (2, "Brewing"), "Aleworks"));

        private static readonly TextGenerator DistilleryName = Sequence(
            OneOf("Old Crow", "Buffalo Trace", "Eagle Rare", "Four Roses", "Beam Suntory", 
                  "Seagram's", "Pappy Van Winkle", "Balcones"), " Distillery");

        private static readonly TextGenerator BarName = Choice(
            Sequence(The, 
                     OneOf("Cozy", "Cheers", "Rustic", "Lively", "Friendly", "Local", "Neighborhood", "Happy Hour"), " ", 
                     OneOf("Pub", "Tavern", "Ale House", "Bar", "Hangout", "Barrel", "Lounge", "Speakeasy")),
            OneOf("Quench", "For The Table", "Lucky Spot", "Drunken Doozy", "Marvelous Mugs", "Table Toast",
                  "The In Zone", "Double Draft", "Make A Toast", "Hair Of The Dog", "Top Notch Toast", "Happy Hours",
                  "Double Dipped", "Warm Welcome", "Tipsy Table", "Next Round", "Secret Spot", "Sunshine Saloon",
                  "Lunar Saloon", "Sip And Savor", "The Black Sheep", "Well Drafted", "Something Shared", "Draft Room",
                  "Double Up Drafts", "Loony Saloon", "Drink And Delight", "Top Tabs", "No Holds Barred", "First Round",
                  "Draft Depot", "Starting Pitcher", "Welcome Wagon", "Pitcher Perfect", "Raise A Glass", "Drafting Table",
                  "Bar None", "A Bar Above", "Hell Club", "Triple Crossing"));

        private static readonly TextGenerator EateryDescriptors = OneOf(
            "Bluebell", "Red Rooster", "Golden Spoon", "Silver Star", "Rusty Fork", "Cornfield",
            "Wheatfield", "Sunflower", "Cattleman's", "Harvest Moon", "Prairie Dog", "Prairie Rose",
            "Blue Moon", "Homestead", "Harvest House", "Stagecoach", "Millwright's", "Maplewood");

        private static readonly TextGenerator DinerName = Sequence(
            The, EateryDescriptors, " ", OneOf("Diner", "Grill", "Kitchen"));

        private static readonly TextGenerator RestaurantName = Sequence(The, EateryDescriptors, " ", 
            OneOf("Restaurant", "Steakhouse", "Cafe", "Eatery", "Pizzeria", "Lunchroom"));

        private static readonly TextGenerator BakeryName = Sequence(The, 
            OneOf("Doughy", "Sweet", "Golden Loaf", "Flour Mill", "Sugared", "Bagel", "Muffin", "Cookie"), " ", 
            OneOf("Delights", "Oven", "Bakery", "Confections"));

        private static readonly TextGenerator CandyShopName = Sequence(The,
            OneOf("Sweet Tooth", "Chocolate", "Rainbow", "Soda Fountain", "Caramel", 
                  "Peppermint", "Licorice", "Fudge", "Jellybean", "Truffle", "Honey"), " ",
            OneOf("Confectionery", "Candy Co.", "Sweets", "Parlor", "Candy Shop"));

        private static readonly TextGenerator DairyName = Sequence(MidwestDescriptor, 
            OneOf("Creamery", "Dairy Co.", "Milk Company", "Dairy Farms", "Blue Ribbon Dairy", "Dairy", "Milk Co."));

        private static readonly TextGenerator ButcherShopName = Sequence(MidwestDescriptor,
            OneOf("Meats", "Quality Meats", "Meats & Deli", "Butcher Shop", "Fine Meats", "Butchers", 
                  "Meat Market", "Fine Cuts", "Meats & Sausages", "Premium Meats", "Old-Fashioned Butchery"));

        private static readonly TextGenerator GeneralStoreName = Sequence(TheTownName,
            OneOf("General Store", "Emporium", "Mercantile", "Trading Post", "Corner Store", 
                  "Country Store", "Supply Co.", "Provisions", "Trading Co.", "Variety Store"));

        private static readonly TextGenerator GroceryStoreName = Sequence(Choice(TheTownName, 
            (5, Sequence(The, OneOf("Midwest", "Prairie", "Heartland", "Pioneer", "Harvest", "Root & Stem"), " "))), 
            OneOf("Provisioners", "Grocers", "Pantry", "Provisions", "Food Mart", "Market", "Grocery"));

        private static readonly TextGenerator DepartmentStoreName = OneOf(
            "Marshall Field's", "Hudson's", "Stix, Baer & Fuller", "Dayton's", "Higbee's",
            "Lazarus", "Younkers", "Kaufmann's", "Ayres", "Davison's", "Von Maur", "Famous-Barr");

        private static readonly TextGenerator ClothingStoreName = Sequence(The,
            OneOf("", "Fashion ", "Vintage ", "Tailor's "),
            OneOf("Emporium", "Square", "Palace", "Attire", "Outfitters", 
                  "Shoppe", "Haberdashery", "Millinery", "Boutique", "Bon-Ton"));

        private static readonly TextGenerator ShoemakerShopName = Sequence(MidwestDescriptor, 
            OneOf("Footwear", "Bootmakers", "Shoe Co.", "Shoe Company", "Shoe Emporium", 
                  "Shoe Outlet", "Shoe Repair", "Shoe Shop", "Shoe Store"));

        private static readonly TextGenerator TailorShopName = Sequence(MidwestDescriptor,
            OneOf("Custom Tailors", "Sartorial House", "Bespoke Tailors", "Dressmakers Guild", "Tailoring Emporium", 
                  "Stitch & Sew", "Modern Clothiers", "Fine Tailors", "Couturiers", "Tailoring Co.", "Tailoring Shoppe", 
                  "Tailoring Studio", "Tailored Threads", "Custom Clothiers"));

        private static readonly TextGenerator JeweleryShopName = Sequence(The,
            OneOf("Golden", "Pearl & Rose", "Ruby", "Diamond", "Gem", "Sterling", 
                  "Silver", "Timeless", "Elegant", "Antique", "Artisanal"), " ",
            OneOf("Jewels", "Emporium", "Treasures", "Creations", "Jewelers"));

        private static readonly TextGenerator BarbershopName = Sequence(The,
            OneOf("Gents", "Midwest", "Union", "Old Town", "Pioneer", "Frontier"), " ",
            OneOf("Parlor", "Barber", "Cuts", "Clip", "Trim", "Shears", "Shaves"));

        private static readonly TextGenerator HardwareStoreName = Sequence(MidwestDescriptor,
            OneOf("Hardware", "Hardware Co.", "Hardware Depot", "Hardware Emporium", "Hardware Store", 
                  "Hardware Supply", "Hardware & Tools", "Hardware Merchants", "Hardware & Appliance"));

        private static readonly TextGenerator FurnitureStoreName = Sequence(MidwestDescriptor, 
            OneOf((5, ""), "Fine ", "House ", "Home ", "Modern ", "Vintage ", "Heritage ", "Antique "),
            OneOf("Furniture", "Furniture Co.", "Furniture Depot", "Furniture Emporium", 
                  "Furniture Store", "Furnishings", "Home Goods"));

        private static readonly TextGenerator WoodworkDescriptors = OneOf(
            "Midwest Craftsman", "Heartland", "Prairie Pro", "Oak Grove", "Windmill", "Barn and Beam",
            "Pioneers'", "Landmark", "Heritage", "Frontier", "Sawmill", "Timberland", "Country");

        private static readonly TextGenerator CarpentryCompanyName = Sequence(WoodworkDescriptors, " ", 
            OneOf("Carpentry", "Woodworks", "Timberbuilders", "Woodwork Co.", "Renovations", 
                  "Cabinetry", "Woodcrafters", "Restoration Co.", "Woodworking Co."));

        private static readonly TextGenerator LumberMillName = Sequence(WoodworkDescriptors, " ",
            OneOf("Timber Co.", "Lumber Co.", "Mill & Timber", "Timberworks", "Sawmill", "Sawmill Co.", "Lumber Mill"));

        private static readonly TextGenerator FoundryName = Sequence(MidwestDescriptor,
            OneOf("Steel Works", "Foundry", "Iron Works", "Forge Company", "Foundries", "Castings", "Foundry Company",
                  "Brass & Iron Works", "Foundry & Machine Works", "Steel Foundry", "Steel & Iron Works"));

        private static readonly TextGenerator QuarryName = Sequence(
            OneOf("Blue Ridge", "Golden Valley", "Fox Hollow", "Rocky Mountain", "Outcrop", "Redstone", 
                  "Hickory Ridge", "Stony Brook", "Millstone", "Shady Glen"), " Quarry");

        private static readonly TextGenerator TattooParlorName = OneOf((5, "Heroes & Ghosts"), 
            "The Sacred Canvas", "Ink Sanctuary", "Enigma Ink", "Mystic Markings", "Bespoke Body Art");

        public static readonly Dictionary<LocationType, TextGenerator> LocationNames = new() {
            { Cemetery , CemeteryName},
            { CityHall , CityHallName },
            { PostOffice , PostOfficeName },
            { PoliceStation , PoliceStationName},
            { FireStation , FireStationName },
            { Bank , BankName },
            { CommunityCenter , CommunityCenterName },
            { House , AddressedStreet },
            { ApartmentComplex , AddressedStreet },
            { Farm , FarmName },
            { Park , ParkName },
            { Orchard , OrchardName },
            { School , HighSchoolName },
            { DayCare , DaycareName },
            { Inn , InnName },
            { DoctorOffice , DoctorOfficeName },
            { DentistOffice , DentistOfficeName },
            { OptometryClinic , OptometryClinicName },
            { Pharmacy , PharmacyName },
            { Hospital , HospitalName },
            { Brewery , BreweryName },
            { Distillery , DistilleryName },
            { Bar , BarName },
            { Diner , DinerName },
            { Restaurant , RestaurantName },
            { Bakery , BakeryName },
            { CandyShop , CandyShopName },
            { Dairy , DairyName },
            { ButcherShop , ButcherShopName },
            { GeneralStore , GeneralStoreName },
            { GroceryStore , GroceryStoreName },
            { DepartmentStore , DepartmentStoreName },
            { ClothingStore , ClothingStoreName },
            { ShoemakerShop , ShoemakerShopName },
            { TailorShop , TailorShopName },
            { JewelryShop , JeweleryShopName },
            { Barbershop , BarbershopName },
            { HardwareStore , HardwareStoreName },
            { FurnitureStore , FurnitureStoreName },
            { CarpentryCompany , CarpentryCompanyName },
            { LumberMill , LumberMillName },
            { Foundry , FoundryName },
            { Quarry , QuarryName },
            { TattooParlor , TattooParlorName },
        };
        #endregion

    }
}
