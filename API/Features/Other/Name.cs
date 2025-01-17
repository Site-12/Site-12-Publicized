namespace Site12.API.Features.Other;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Attributes;
using CommandSystem;
using CustomItems;
using Extensions;
using Items;

public abstract class Name
{
    public static bool IsEnabled;

    // I'll shove this into somewhere else later
    public static readonly List<string> FirstNames =
    [
        // Male First Names
        "Winston", "Evan", "Greyson", "Kareem", "Bryce", "Lucas", "Antony", "Jamir", "Shamar", "Urijah", "Parker", "Blaine",
        "Jon", "Walker", "Ishaan", "Trent", "Marvin", "Diego", "Michael", "Anton", "Nash", "Cyrus", "Stephen", "Caiden",
        "Charles", "Kody", "Josh", "Koen", "Sterling", "Ryan", "Jase", "Rudy", "Cash", "Kenneth", "Leonidas",
        "Tristian", "Armani", "Chance", "Jakobe", "Terrence", "Jeremiah", "Braylon", "Sullivan", "Cannon", "Damari",
        "Giovani", "Edgar", "Tucker", "Hugo", "Shawn", "Mateo", "Jasper", "Chandler", "Billy", "James", "Conor",
        "Tripp", "Antoine", "Trystan", "Gael", "Nikhil", "Zayne", "Johnathon", "Lance", "Jesse", "Reagan", "Trevon",
        "Gianni", "Reilly", "Pierce", "Jagger", "Carl", "Jaiden", "Gunnar", "Mekhi", "Khalil", "Rex", "Rhys", "Ramon",
        "Owen", "Bruce", "Efrain", "Braiden", "Tristen", "Jensen", "Javon", "Kadyn", "Augustus", "Alonzo", "Marquise",
        "Dereon", "Giancarlo", "Phoenix", "Jorge", "Benjamin", "Franco", "Killian", "Kash", "Emanuel", "Kasey", "Brett",
        "Jack", "Christian", "Gavyn", "Adonis", "Will", "Ace", "River", "Jason", "Leonel", "Alexander", "Keyon", "Jude",
        "Alec", "Marques", "Marcel", "Aaron", "Douglas", "Zachariah", "Raymond", "Keaton", "Adam", "August", "Henry",
        "Fabian", "Karter", "Zechariah", "Antonio", "Kyan", "Davian", "Kellen", "Chaim", "Andy", "Sawyer", "Declan",
        "Rohan", "Deacon", "Brenden", "Leandro", "London", "Joaquin", "Santino", "Milo", "Saul", "Kayden", "Kade",
        "Solomon", "Quinten", "Moshe", "Amare", "Jermaine", "Makai", "Matthias", "Ryland", "Davion", "Bryson",
        "Muhammad", "Alfred", "Ismael", "Jakob", "Beckett", "Jeffery", "Geovanni", "Glenn", "Gerald", "George",
        "Melvin", "Dayton", "Jaydin", "Zaiden", "Philip", "Mario", "Tanner", "Giovanny", "Taylor", "Kamden",
        "Valentino", "Kaleb", "Tyrell", "Damarion", "Kaeden", "Kobe", "Isaiah", "Camden", "Landin", "Jamarion",
        "Jarrett", "Lawson", "Jaidyn", "Evan", "Colin", "Ezekiel", "Nikolai", "Troy", "Samir", "Izaiah", "Tyrone",
        "Hassan", "Miles", "Braylen", "Cassius", "Bernard", "Alejandro", "David", "Julio", "Darnell", "Vincent",
        "Rashad", "Keenan", "Malakai", "Houston", "Myles", "Ernest", "Emmett", "Luka", "Jamal", "Corbin", "Ahmed",
        "Darion", "Mohammed", "Martin", "Kelvin", "Aidan", "Eddie", "Lennon", "Carter", "Denzel", "Mark", "Isaias",
        "Jaeden", "Maximillian", "Jordan", "Erick", "Donald", "Axel", "Ulises", "Quinton", "Ayden", "Camryn", "Allen",
        "Jordon", "Nathanael", "Arthur", "Sonny", "Elias", "Drew", "Bronson", "Cole", "Mike", "Pablo", "Santiago",
        "Aden", "Hunter", "Adrian", "Shaun", "Jaylin", "Nelson", "Deon", "Kingston", "Demarcus", "Jaxson", "Jadiel",
        "Quinn", "Bridger", "Prince", "Reece", "Boston", "Coby", "Jaylen", "Addison", "Gideon", "Branden", "Kristopher",
        "Jan", "Freddy", "Kai", "Gage", "Tony", "Barrett", "Phillip", "Mohamed", "Carsen", "Lamar", "Braden", "Carlo",
        "Samson", "Guillermo", "Rogelio", "Colby", "Bryan", "Teagan", "Israel", "Jasiah", "Devon", "Clayton", "Kamren",
        "Cristofer", "Derek", "Armando", "Dalton", "Chaz", "Toby", "Jamie", "Rishi", "Terry", "Sean", "Cordell", "Levi",
        "Jeramiah", "Peter", "Anderson", "Kane", "Romeo", "Uriel", "Wade", "Abdullah", "Aldo", "Zander", "Xander",
        "Cody", "Moises", "Bo", "Walter", "Aaden", "Jay", "Deandre", "Richard", "Raiden", "Van", "Jaden", "Ernesto",
        "Garrett", "Abram", "Camren", "Finnegan", "Atticus", "Dashawn", "Adriel", "Jovan", "Rowan", "Randy", "Keith",
        "Abel", "Kadin", "Damon", "Jeffrey", "Dawson", "Jayden", "Derick", "Davis", "Immanuel", "Aarav", "Byron",
        "Lukas", "Donte", "Nathanial", "Ronin", "Beckham", "Bradley", "Marquis", "Konner", "Silas", "Enrique", "Felix",
        "King", "Cesar", "Adrien", "Kason", "Arjun", "Lewis", "Quintin", "Jessie", "Aryan", "Trace", "Zavier", "Talan",
        "Jared", "Nathan", "Wayne", "Roberto", "Brandon", "Karson", "Jacob", "Agustin", "Salvatore", "Kyson", "Korbin",
        "Case", "Zachary", "Grady", "Patrick", "Jesus", "Allan", "Lucian", "Harper", "Messiah", "Louis", "Yahir",
        "Jean", "Cason", "Braedon", "Brennen", "Brennan", "Elliott", "Rylee", "Sergio", "Giovanni", "Justin", "Maddox",
        "Bryant", "Brayan", "Ezra", "Jonathon", "Darren", "Rhett", "Gauge", "Jaron", "Thomas", "Corey", "Kenyon",
        "Brock", "Fisher", "Ignacio", "Gavin", "Camron", "Marcos", "Dario", "Soren", "Travis", "Jamar", "Finn", "Avery",
        "Steve", "Adolfo", "Dominick", "Ray", "Donovan", "Zayden", "Jonah", "Keon", "Ari", "Dangelo", "Trevor", "Zion",
        "Rocco", "Micheal", "Waylon", "Nicolas", "Cale", "Robert", "Konnor", "Cory", "Arnav", "Darryl", "Turner",
        "Asher", "Craig", "Deangelo", "Kendrick", "Justus", "Lincoln", "Devyn", "Braeden", "Riley", "Seth", "Elian",
        "Harley", "Salvador", "Cullen", "Xavier", "Memphis", "Maverick", "Mason", "Luciano", "Issac", "Maximo", "Nolan",
        "Hector", "Conner", "Alex", "Aidyn", "Ariel", "Reynaldo", "Rodney", "Dominic", "Elijah", "Luca", "Nick",
        "Jaquan", "Joseph", "Bradyn", "Kolby", "Kyle", "Cade", "Rafael", "Kelton", "Roger", "Layton", "Russell",
        "Clinton", "Heath", "Marky", "Visual",

        // Female Names
        "Dana", "Marissa", "Faith", "Miah", "Mariyah", "Lina", "Miley", "Katrina", "Laura",
        "Maritza", "Brianna", "Aliana", "Dakota", "Rebecca", "Marlene", "Briley", "Tamia", "Aileen", "Rory", "Sienna",
        "Lyric", "Celia", "Anne", "Rosa", "Kamryn", "Alia", "Madilynn", "Bridget", "Naima", "Sharon", "Frances",
        "Xiomara", "Kathleen", "Clare", "Jaslyn", "Melanie", "Jaidyn", "Sarahi", "Justine", "Marianna", "Tamara",
        "Hannah", "Maggie", "Gracie", "Chelsea", "Neveah", "Emelia", "Erin", "Lila", "Paisley", "Kasey", "Heidy",
        "Araceli", "Shea", "Moriah", "Camille", "Kaylen", "Ashley", "Skyla", "Josephine", "Kyra", "Macey", "Carley",
        "Ally", "Irene", "Amber", "Elisabeth", "Maddison", "Jaden", "Esperanza", "Paula", "Cierra", "Shyann", "Kianna",
        "Sonia", "Jordyn", "Jayleen", "Marie", "Juliet", "Jennifer", "Paris", "Katie", "Halle", "Milagros", "Lilly",
        "Alison", "Janiah", "Tia", "Amelie", "Evelin", "Paityn", "Addyson", "Carissa", "Yareli", "Abagail", "Sydney",
        "Layla", "Nola", "Anabelle", "Catalina", "Olive", "Trinity", "Shiloh", "Karina", "Giovanna", "Keely", "Yasmin",
        "Ingrid", "Maren", "Monica", "Kimberly", "Kierra", "Aaliyah", "Amari", "Heather", "Danna", "Miriam", "Areli",
        "Stella", "Crystal", "Kira", "Ayana", "Saniya", "Isabela", "Elaina", "Janiya", "June", "Mila", "Chaya",
        "Samantha", "Tianna", "Marisol", "Teresa", "Alma", "Tiara", "Ryan", "Shannon", "Emily", "Lizbeth", "Riya",
        "Savanah", "Evelyn", "Lexi", "Iris", "Ana", "Nathaly", "Janiyah", "Campbell", "Lilian", "Marley", "Fernanda",
        "Skylar", "Shayna", "Shyla", "Andrea", "Celeste", "Alondra", "Briana", "Jadyn", "Jaelynn", "Karma", "Alyson",
        "Adelaide", "Yaretzi", "Kaley", "Cristina", "Ava", "Raegan", "Kiara", "Janet", "Hailee", "Lea", "Elaine",
        "Cara", "Hallie", "Madeleine", "Elianna", "Lauryn", "Keyla", "Mariam", "Kailee", "Saige", "Paola", "Kaitlyn",
        "Jakayla", "Nora", "Melany", "Kaya", "Karissa", "Hillary", "Aracely", "Jacey", "Kassidy", "Aimee", "Lara",
        "Myla", "Shelby", "Bailee", "Mackenzie", "India", "Corinne", "Beatrice", "Deanna", "Jasmine", "Bria", "Amanda",
        "Luz", "Jaylen", "Belinda", "Leilani", "Taliyah", "Amira", "Mattie", "Ashlyn", "Kelsie", "Charlie", "Dahlia",
        "Sariah", "Jordin", "Luciana", "Caroline", "Gloria", "Lillie", "Emilia", "Valerie", "Mariela", "Allyson",
        "Stephany", "Carina", "Londyn", "Litzy", "Michaela", "Adalynn", "Amelia", "Veronica", "Cherish", "Kaitlynn",
        "Estrella", "Kaiya", "Abby", "Evie", "Brooklynn", "Emely", "Angeline", "Carolina", "Kailyn", "Reina", "Lainey",
        "Salma", "Lisa", "Leah", "Pamela", "Naomi", "Edith", "Juliette", "Ella", "Mikaela", "Catherine", "Adelyn",
        "Meredith", "Lilia", "Cassandra", "Sofia", "Phoenix", "Aubrie", "Kaylyn", "Rowan", "Gisselle", "Brisa", "Kate",
        "Zariah", "Riley", "Gillian", "Kendal", "Hailie", "Aiyana", "Hadley", "Charlotte", "Nyasia", "Janelle",
        "Mareli", "Rachel", "Yamilet", "Talia", "Alisson", "Daniela", "Valeria", "Alivia", "Selina", "Ellen",
        "Elizabeth", "Kayley", "Maci", "Aleah", "Martha", "Carlie", "Makena", "Jazmine", "Charity", "Valentina", "Elsa",
        "Emilie", "Mayra", "Baylee", "Payten", "Caitlin", "Kristina", "Amaris", "Karsyn", "Lillianna", "Ashlee",
        "Emerson", "Azul", "Princess", "Giada", "Lydia", "Amy", "Eden", "Joslyn", "Delilah", "Kaliyah", "Reagan",
        "Khloe", "Gianna", "Giselle", "Casey", "Reese", "Lillian", "Shirley", "Charlize", "Jaylin", "Kiana", "Aurora",
        "Heaven", "Aubree", "Addison", "Alexus", "Gabrielle", "Abigail", "Vivian", "Sierra", "Anabel", "Ireland",
        "Kaila", "Mariana", "Nicole", "Sidney", "Carolyn", "Barbara", "Isis", "Julie", "Bryanna", "Kyleigh", "Mylie",
        "Miranda", "Evangeline", "Mira", "Nathalie", "Ariana", "Leticia", "Mia", "Jazmin", "Lia", "Dylan", "Harmony",
        "Eve", "Lorelai", "Leanna", "Gabriella", "Rayne", "Grace", "Ashly", "Savanna", "Paloma", "Bailey", "Kimora",
        "Rayna", "Cora", "Sophie", "Luna", "Liberty", "Gia", "Ada", "Juliana", "Dalia", "Myah", "Jayla", "Bethany",
        "Alexa", "Cecilia", "Savannah", "Mariah", "Ariel", "Isabelle", "Chanel", "Morgan", "April", "Tiffany", "Shayla",
        "Raquel", "Cadence", "Sandra", "Zara", "Karly", "Genesis", "Lailah", "Camila", "Clara", "Micaela", "Eleanor",
        "Annika", "Zaria", "Sarah", "Holly", "Jaylynn", "Laylah", "Rihanna", "Serena", "Yaritza", "Anastasia", "Kelsey",
        "Daniella", "Anabella", "Joyce", "Theresa", "Madelyn", "Hadassah", "Julianne", "Tania", "Aniyah", "Abigayle",
        "Greta", "Kayleigh", "Denisse", "Kaydence", "Alyssa", "Jaylyn", "Christine", "Camryn", "Lauren", "Priscilla",
        "Rylee", "Makenna", "Itzel", "Mckayla", "Daphne", "Tori", "Emmy", "Jessie", "Noemi", "Shyanne", "Helen",
        "Abbie", "Tessa", "Rachael", "Guadalupe", "Valery", "Ashanti", "Tess", "Elisa", "Alejandra", "Hailey", "Kamari",
        "Shaylee", "Isabell", "Autumn", "Anahi", "Tanya", "Wendy", "Averi", "Akira", "Lily", "Iliana", "Virginia",
        "Cristal", "Kassandra", "Kayden", "Brenna", "Lesly", "Hanna", "Serenity", "Aniya", "Imani", "Yazmin", "Kaia",
        "Eliza", "Kali", "Kiersten", "Angelina", "Saniyah", "Emery", "Makayla", "Penelope", "Denise", "Rose", "Cameron",
        "Elyse", "Teagan", "Haleigh", "Precious"
    ];

    public static readonly List<string> LastNames =
    [
        "Mendez", "Mccann", "Drop", "Cayen", "Roberts", "Robinson", "Robles", "Bender", "Davis", "Burgess", "Craig", "Nguyen", "Burns",
        "Aguilar", "Torres", "Bean", "Christian", "Watson", "Alexander", "Avery", "Kramer", "Douglas", "Castillo",
        "Castro", "Walker", "Wang", "Glenn", "Yoder", "Hahn", "Sampson", "Shepard", "Padilla", "Macdonald", "Shields",
        "Bowen", "Shelton", "Klein", "Best", "Gamble", "Duran", "Serrano", "Huff", "Brown", "Wilkins", "Romero",
        "Russell", "Hanna", "Riley", "Summers", "Woodard", "Zavala", "Fry", "Johnson", "Johnston", "Small", "Berger",
        "Booth", "Marsh", "Mccormick", "Collier", "Holloway", "Vaughn", "Carney", "Rios", "Flores", "Harrison",
        "Michael", "Cole", "Macias", "Roach", "Christensen", "Chang", "Morgan", "Pineda", "Davies", "Clark", "Parker",
        "Key", "Krueger", "Parks", "Knight", "Preston", "Barry", "Sweeney", "English", "Anthony", "Holden", "Shannon",
        "Sandoval", "Harrington", "Santana", "Middleton", "Matthews", "Willis", "Kelly", "Brennan", "Callahan",
        "Harper", "Fletcher", "Finley", "Thornton", "Villegas", "Nunez", "Valenzuela", "Horne", "Clarke", "Wall",
        "Nicholson", "Duke", "Hampton", "Calhoun", "Lynch", "Osborne", "Cross", "Gregory", "Blackwell", "Armstrong",
        "Waters", "Nichols", "Lowery", "Oneal", "Foster", "Pittman", "Holland", "Mitchell", "Medina", "Alvarado",
        "Whitney", "Ramirez", "Mcclure", "Humphrey", "Andrade", "Donovan", "Molina", "Kennedy", "Patton", "Joyce",
        "Velez", "Henson", "Manning", "Maddox", "George", "Stone", "Mcdonald", "Graves", "Ramsey", "Ingram", "Rice",
        "Meyers", "Osborn", "Vargas", "Young", "Beck", "Wu", "Hopkins", "Harding", "Bauer", "Myers", "Lopez", "Cantu",
        "Gibson", "Golden", "Norton", "Dyer", "Orr", "Fitzpatrick", "Holmes", "Ray", "Estes", "Carter", "Flowers",
        "Mata", "Lee", "Carrillo", "Mercado", "Olson", "Rosario", "Deleon", "Vang", "Payne", "Forbes", "Powell",
        "Horton", "Mueller", "Peters", "Gordon", "Hamilton", "Khan", "Jensen", "Dean", "Brewer", "Solomon", "Goodwin",
        "Hobbs", "West", "Kim", "Rubio", "Mcpherson", "Singh", "Coffey", "Wilson", "Bartlett", "Bishop", "Cohen",
        "Gaines", "Calderon", "Barr", "Mclean", "Livingston", "Leblanc", "Li", "Delacruz", "Cherry", "Combs", "Simon",
        "Banks", "Arias", "Cortez", "Hughes", "Cruz", "Miller", "Palmer", "Mcintosh", "Trevino", "Compton", "Adams",
        "Pacheco", "Elliott", "Moreno", "Ellison", "Pierce", "Meyer", "Saunders", "Reyes", "Krause", "Cantrell",
        "Merritt", "Gutierrez", "Bullock", "Cardenas", "Welch", "Bentley", "Novak", "Malone", "Murray", "Reid", "Wiley",
        "Silva", "Butler", "Blake", "Moody", "Randall", "Rodriguez", "Trujillo", "Mack", "Horn", "Figueroa", "Arnold",
        "Huffman", "Thompson", "Avila", "Smith", "Lara", "Herman", "Stevens", "Warner", "Eaton", "Rivers", "Hood",
        "Logan", "Hess", "Melendez", "Bailey", "Huber", "Roberson", "Gilmore", "Larson", "Underwood", "Mcdowell",
        "Lucero", "Dodson", "Nixon", "Chandler", "Andersen", "Francis", "Hurley", "Carr", "Cowan", "Roman", "Graham",
        "Stein", "Miranda", "Conrad", "Villarreal", "Duarte", "Vasquez", "Martinez", "Moon", "Levy", "Rowe", "Steele",
        "Harvey", "Carlson", "Cabrera", "Frost", "Strickland", "Glover", "Hogan", "Baker", "Santos", "Marshall",
        "Bryant", "Robbins", "Walls", "Friedman", "Taylor", "Davenport", "Haney", "Mercer", "Sanders", "Lawrence",
        "Owens", "Levine", "Mckenzie", "Garcia", "Stout", "Spears", "Powers", "Moss", "Sharp", "Burke", "Randolph",
        "Chan", "Castaneda", "Mills", "Le", "Sexton", "Tyler", "Campbell", "Hutchinson", "Jacobs", "Mullins", "Salas",
        "Barton", "Rocha", "Watts", "Maxwell", "Villa", "Wheeler", "Conner", "Baird", "Ayala", "Cooper", "Olsen",
        "Sosa", "Huang", "Caldwell", "Rosales", "Salinas", "Ross", "Sims", "Orozco", "Winters", "Parrish", "Barrett",
        "Richard", "Shaffer", "Rush", "Moore", "Schroeder", "Ramos", "Freeman", "Vega", "Mathews", "Copeland",
        "Jimenez", "Wyatt", "Bass", "Cline", "Tanner", "Bennett", "Fischer", "Schultz", "Archer", "Richardson",
        "Gillespie", "Herrera", "Riggs", "Hicks", "Lewis", "Duffy", "Frank", "Cooke", "Jefferson", "Dixon", "Ferguson",
        "Henderson", "Daniel", "Kirby", "Oneill", "Alvarez", "Leon", "Hinton", "Sellers", "Hoffman", "Wagner", "Hodge",
        "Sutton", "Jennings", "Aguirre", "Tran", "Brooks", "Stephenson", "Rasmussen", "Benson", "Costa", "Todd",
        "Little", "Norris", "Mooney", "Wiggins", "Grimes", "Huerta", "Lynn", "Jacobson", "Wolfe", "Mccall", "Hartman",
        "Stephens", "Fuentes", "Sanchez", "Travis", "Ballard", "Houston", "Rivas", "Beard", "Anderson", "Richmond",
        "Hart", "Jenkins", "Gay", "Hunt", "Villanueva", "Price", "Irwin", "Lin", "Pham", "Kerr", "Buchanan", "Wise",
        "Hoover", "Schmidt", "Kaiser", "Espinoza", "White", "Maynard", "Berg", "Strong", "Andrews", "Zhang", "Pratt",
        "Vincent", "Ochoa", "Pace", "Mann", "Adkins", "King", "Cisneros", "Paul", "Velazquez", "Morales", "Bolton",
        "Faulkner", "Braun", "Meza", "Diaz", "Leonard", "Booker", "Cooley", "Hatfield", "Mclaughlin", "Barker", "James",
        "Murillo", "Cummings", "Barnes", "Mcconnell", "Russo", "Potter", "Joseph", "Johns", "Perkins", "Jarvis",
        "Barber", "Webb", "Cameron", "Garner", "Waller", "Bonilla", "Miles", "Gallegos", "Simmons", "Conway", "Page",
        "Rhodes", "Ford", "Watkins", "French", "Luna", "Pearson", "Williamson", "Dudley", "Dalton", "Mayer", "Hester",
        "Frey", "Bird", "Williams", "Monroe", "Bowers", "Harmon", "Schmitt", "Odonnell", "Sparks", "Terry", "Barnett",
        "Mcclain", "Rojas", "Gates", "Blackburn", "Koch", "Franco", "Davila", "Meadows", "Vaughan", "Frederick",
        "Bridges", "Shaw", "Boyd", "Weber", "Moses", "Duncan", "Warren", "Bush", "Thomas", "Montgomery", "Giles",
        "Hurst", "Acosta", "Chapman", "Kemp", "Atkins", "Burton", "Casey", "Boyer", "Hernandez", "Tucker", "Pugh",
        "Solis", "Morton", "Knapp", "Henry", "Parsons", "Long", "Ware", "Mccullough", "Delgado", "Knox", "Whitehead",
        "Bradley", "Gill", "Montoya", "Cox", "Browning", "Spence", "Sherman", "Haynes", "Becker", "Shah", "Mckinney",
        "Mason", "Kidd", "Ferrell", "Allen", "Fritz", "Lutz", "Stewart", "Higgins", "Webster", "Blevins", "Ellis",
        "Nolan", "Lozano", "Schneider", "Lindsey", "Odom", "Yang", "Conley", "Love", "Blair", "Stokes", "Lowe", "Chase",
        "Baxter", "Pennington", "Oconnell", "Glass", "Mccoy", "Moran", "Turner", "Woodward", "Hebert", "Massey",
        "Patel", "Decker", "Curtis", "Garrett", "Abbott", "Ruiz", "Clay", "Franklin", "Walsh", "Lloyd", "Munoz",
        "Obrien", "Morrow", "Mccarthy", "Haley", "Burnett", "Kane", "Buck", "Wood", "Benitez", "Barajas", "Campos",
        "Ashley", "Montes", "Daugherty", "Werner", "Holt", "Carey", "Rodgers", "Sawyer", "Wade", "Hubbard", "Guerra",
        "Chen", "Madden", "Berry", "Townsend", "Mosley", "Burch", "Snyder", "Boyle", "Fleming", "Donaldson", "Stanton",
        "Rowland", "Cervantes", "Simpson", "Harris", "Singleton", "Reilly", "Huynh", "Proctor", "Kent", "Boone",
        "Woods", "Sheppard", "Dougherty", "Bell", "Durham", "Foley", "Dorsey", "Good", "Gray", "Brady", "Dunlap",
        "Robertson", "Hooper", "Holder", "Yates", "Schaefer", "Hammond", "Schwartz", "Evans", "Velasquez", "Short",
        "Estrada", "Wilkerson", "Floyd", "Potts", "Landry", "Barrera", "David", "Roy", "Jackson", "Dickson", "Salazar",
        "Mcneil", "Haas", "Branch", "Lane", "Crane", "Oconnor", "Baldwin", "Lamb", "Bray", "Keller", "Bruce", "Rogers",
        "Mora", "Fitzgerald", "Herring", "Hensley", "Bowman", "Rose", "Fowler", "Goodman", "Phillips", "Wilcox",
        "Carroll", "Lambert", "Morris", "Fields", "Grant", "Kirk", "Erickson", "Ibarra", "Oliver", "Lester", "Rollins",
        "Kelley", "Carson", "Farmer", "Wright", "Gonzales", "Church", "Nelson", "Walton", "Suarez", "Porter", "Stark",
        "Barron", "Mccarty", "Wong", "Morse", "Scott", "Navarro", "Snow", "Cannon", "Clayton", "Mendoza", "Nielsen",
        "Galvan", "Cunningham", "Hardin", "Lawson", "Esparza", "Gardner", "Ritter", "Raymond", "Wells", "Garza",
        "Farley", "Keith", "Camacho", "Ryan", "Park", "Hale", "Benton", "Beasley", "Bright", "Atkinson", "Harrell",
        "Weeks", "Bradford", "Charles", "Hardy", "Fisher", "Dawson", "Reynolds", "Stuart", "Tate", "Gonzalez",
        "Bautista", "Day", "Soto", "Benjamin", "Hanson", "Galloway", "Allison", "Garrison", "Clements", "Peck", "Petty",
        "Poole", "Sloan", "Cordova", "May", "Zamora", "Roth", "Hancock", "Hodges", "Melton", "Whitaker", "Norman",
        "Frazier", "Mayo", "Walter", "Dennis", "Murphy", "Pope", "Hall", "Newman", "York", "Petersen", "Collins",
        "Gentry", "Washington", "Valencia", "Wilkinson", "Gould", "Carpenter", "Crosby", "Nash", "Everett", "Flynn",
        "Howell", "Crawford", "Escobar", "Mcknight", "Mcdaniel", "Spencer", "Maldonado", "Mejia", "Austin", "Case",
        "Drake", "Ball", "Jones", "Mckee", "Howe", "Doyle", "Beltran", "Swanson", "Bond", "Shepherd", "Morrison", "Liu",
        "Ortiz", "Cain", "Briggs", "Brock", "Griffith", "Hill", "Guerrero", "Arellano", "Fernandez", "Stanley",
        "Chavez", "Ho", "Bates", "Curry", "Cuevas", "Blanchard", "Reed", "Mcguire", "Chung", "Bryan", "Hayes", "Ewing",
        "Lucas", "Stafford", "Lyons", "Rich", "Hays", "Mullen", "Riddle", "Neal", "Cook", "Skinner", "Daniels",
        "Greene", "Gross", "Santiago", "House", "Wolf", "Weiss", "Perry", "Hunter", "Cobb", "Dunn", "Griffin",
        "Patrick", "Moyer", "Gibbs", "Tapia", "Marks", "Lam", "Zimmerman", "Savage", "Ponce", "Choi", "Kline",
        "Bradshaw", "Frye", "Hudson", "Green", "Mathis", "Leach", "Byrd", "Walters", "Black", "Marquez", "Noble",
        "Mcbride", "Newton", "Howard", "Dominguez", "Buckley", "Coleman", "Sullivan", "Pruitt", "Contreras", "Rangel",
        "Ortega", "Mcintyre", "Valdez", "Kaufman", "Vance", "Gilbert", "Heath", "Colon", "Mcfarland", "Hendricks",
        "Perez", "Prince", "Cochran", "Pollard", "Vazquez", "Fuller", "Patterson", "Jordan", "Davidson", "Guzman",
        "Peterson", "Pena", "Reese", "Larsen", "Martin", "Arroyo", "Mcgrath", "Chambers", "Hayden", "Phelps", "Yu",
        "Hull", "Blankenship", "Greer", "Shea", "Valentine", "Richards", "Pitts", "Mcgee", "Fox", "Downs", "Ayers",
        "Stevenson", "Gallagher", "Mcmillan", "Acevedo", "Weaver", "Reeves", "Farrell", "Mcmahon", "Mahoney", "Mckay",
        "Hendrix", "Dickerson", "Quinn", "Chaney", "Wallace", "Terrell", "Hawkins", "Juarez", "Owen", "Sanford",
        "Dillon", "Mays", "Rivera", "Lang", "Ali", "Edwards", "Gomez", "Hansen", "Hines", "Brandt", "Zuniga", "Ward",
        "Hickman", "Bernard", "Aribus"
    ];

    [OnPluginEnabled]
    public static void InitEvents() => ServerHandlers.WaitingForPlayers += WaitingForPlayers;

    private static void WaitingForPlayers() => IsEnabled = false;
}

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class NameEnable : ICommand
{
    public string Command => "NameEnable";
    public string[] Aliases => ["NameOn", "OnName", "EnableName"];
    public string Description => "Enables the Name Client Command";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        if (!sender.CheckRemoteAdmin(out response))
            return false;

        response = "<color=red>Name Client Command is already Enabled";
        if (Name.IsEnabled)
            return false;

        Name.IsEnabled = !Name.IsEnabled;

        response = "<color=green>Name Client Command is now Enabled";
        return true;
    }
}

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class NameDisable : ICommand
{
    public string Command => "NameDisable";
    public string[] Aliases => ["NameOff", "OffName", "DisableName"];
    public string Description => "Disables the Name Client Command";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        if (!sender.CheckRemoteAdmin(out response))
            return false;

        response = "<color=red>Name Client Command is already Disabled";
        if (!Name.IsEnabled)
            return false;

        Name.IsEnabled = !Name.IsEnabled;

        response = "<color=green>Name Client Command is now Disabled";
        return true;
    }
}


[CommandHandler(typeof(ClientCommandHandler))]
public class NameClient : ICommand
{
    public string Command => "Name";
    public string[] Aliases => ["name", "customname"];
    public string Description => "Gives you a custom name for RP purposes : Name (New Name)";

    private List<string> NameBlacklist { get; } = Plugin.Singleton.Config.BlackList;

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!Name.IsEnabled)
        {
            response = "This feature is currently disabled!";
            return false;
        }

        var player = ExPlayer.Get((CommandSender)sender);
        if (arguments.Count == 0)
        {
            player.DisplayNickname = string.Empty;
            response = "Name reset successfully!";
            return true;
        }
        foreach (var word in arguments)
        foreach (var _ in NameBlacklist.Where(target => word.Equals(target, StringComparison.OrdinalIgnoreCase))) player.Ban(1577000000, "Automated ban for Rule 3. Appeal on the discord if you believe this was false.");

        player.DisplayNickname = string.Join(" ", arguments);

        foreach (var item in player.Items)
            if (CustomItemsManager.Get<KeycardHandler>().Container.HasItem(item.Base, out var idCard))
            {
                idCard.Name = player.DisplayNickname;
                break;
            }

        response = "Name changed successfully!";
        return true;
    }
}
