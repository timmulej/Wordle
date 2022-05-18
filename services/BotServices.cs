using Wordle.Models;
using System.Data.SqlClient;


namespace Wordle.services
{
    public class BotServices
    {
        // to je naslov moje baze podatkov
        static string sqlConnStr = "Data Source=DESKTOP-QFE4DUG;Initial Catalog=Wordle;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        public static Beseda NewWord(int id)
            // ta funkcija poišce id-to besedo v bazi Dolzine_pet_skrajsan
        {
            Beseda SelectedWord = new Beseda();
            SqlConnection connection;

            using (connection = new SqlConnection(sqlConnStr))
            {
                using (SqlCommand command = new SqlCommand(
                    $"SELECT * FROM Dolzine_pet_skrajsan WHERE Id = {id};", 
                 connection))
                {
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        reader.Read();
                        SelectedWord.Prva = reader.GetString(1);
                        SelectedWord.Druga = reader.GetString(2);
                        SelectedWord.Tretja = reader.GetString(3);
                        SelectedWord.Cetrta = reader.GetString(4);
                        SelectedWord.Peta = reader.GetString(5);



                    }
                    connection.Close();
                }
                Console.WriteLine($"{SelectedWord.Prva}{SelectedWord.Druga}{SelectedWord.Tretja}{SelectedWord.Cetrta}{SelectedWord.Peta}");
                return SelectedWord;
            }

        }

        public static List<int> Levels(int i)
            // ta funkcija sprejme indeks možne kombinacije barve in vrne list levelov npr 1 vrne 0,0,0,0,1 kar predstavlja vse sive razen zadnja je rumena
        {
            int n = 0;
            List<int> levels = new List<int>();
            for (int j = 4; j >= 0; j--)
            {
                n = i / Convert.ToInt32(Math.Pow(3,j));
                levels.Add(n);
                i = Convert.ToInt32(i - n * Math.Pow(3,j));
                
            }
            //Console.WriteLine($"{ levels[0]} + { levels[1]} + { levels[2]} + { levels[3]} + { levels[4]}");
            return levels;
        }

        public static int Probability(Beseda Word, int i)
            // ta funkcija sprejme besedo in barve črk, potem pa vrne koliko še besed v seznamu Dolžine_pet_skrajsan 
        {
            int numberOfHitts = 0;
            List<int> levels = Levels(i);
            string WhereString = CreateString.CommandString(levels, Word);
            if (WhereString.Contains("nemogoce"))
            {
                return 0;
            }
            //Console.WriteLine(WhereString);
            string CommandString = "SELECT COUNT(*) FROM Dolzine_pet_skrajsan WHERE " + WhereString;

            SqlConnection connection;

            using (connection = new SqlConnection(sqlConnStr))
            {
                using (SqlCommand command = new SqlCommand( CommandString,connection))
                {
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        reader.Read();
                        numberOfHitts = reader.GetInt32(0);

                    }
                    connection.Close();
                }
                //Console.WriteLine(numberOfHitts);
                return numberOfHitts;
            }

        }

        public static int AllWords(string baza)
            // ta funkcija sprejme bazo in vrne koliko je besed v tej bazi
        {
            int numberOfHitts = 0;
            //Console.WriteLine(WhereString);
            string CommandString = $"SELECT COUNT(*) FROM {baza};";

            SqlConnection connection;

            using (connection = new SqlConnection(sqlConnStr))
            {
                using (SqlCommand command = new SqlCommand(CommandString, connection))
                {
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        reader.Read();
                        numberOfHitts = reader.GetInt32(0);

                    }
                    connection.Close();
                }
                return numberOfHitts;
            }

        }
        public static double Information(double words, double AllWords)
            // ta funkcija sprejme stevilo besed, ki jih izloči ena beseda in število besed, katerih je še bilo v bazi in vrne informacijo, ki smo jo dobili z to besedo
        {
            return Math.Log2(AllWords / words);
        }
        public static double ExpectedInformation(Beseda Word, double AllWords)
            // Ta funkcija sprejme besedo in število vseh besed, ki jih je v bazi in vrne koliko je pričakovana informacija, ki naj bi jo dobili iz te besede
        {
            int sum_n = 0;
            double E = 0;
            for (int i = 0; i < 243; i++)
            {
                int n = Probability(Word, i);
                sum_n = sum_n + n;
                double P = n/AllWords;
                //Console.WriteLine(P.ToString());
                if (!(P == 0))
                {
                    double Information = Math.Log2(1 / P);
                    E = E + P * Information;
                } 
            }
            //Console.WriteLine(AllWords);
            //Console.WriteLine(sum_n);
            return E;
        }

        public static Beseda BestGues()
            // ta funkcija za vse besede izračuna pričakovano informacijo in vrne tisto z najvišjo pričakovano vrednostjo
        {
            double n = AllWords("Dolzine_pet_skrajsan");
            double bestExpectedInformation = 0;
            Beseda BestWord = new Beseda();
            for (int i = 1; i <= n; i++)
            {
                Console.WriteLine(i.ToString());
                Beseda word = NewWord(i);
                double E = ExpectedInformation(word, n);
                Console.WriteLine(E);
                if (E > bestExpectedInformation)
                {
                    bestExpectedInformation = E;
                    BestWord = word;
                }
            }
            Console.WriteLine($"Najboljša beseda je { BestWord.Prva}{ BestWord.Druga}{ BestWord.Tretja}{ BestWord.Cetrta}{ BestWord.Peta}");
            Console.WriteLine(bestExpectedInformation);
            return BestWord;

        }

        public static void Delite(Beseda Word)
            // Ta funkcija sprejme besedo in iz baze izloči vse besede, ki ne ustrezajo vzorcu, ki ga da ta beseda
        {
            List<int> levels = new List<int>();
            levels = Check.GetList(Check.GetLevel(Word, GameServices.GetSelectedWord()));
            Console.WriteLine(levels[0] + "," + levels[1] + "," + levels[2] + ","  + levels[3] + ","+  levels[4]);
        
            string WhereString = CreateString.CommandString(levels, Word);
            // vem da se ta ne bo nikili izvedel
            if (WhereString.Contains("nemogoce"))
            {
                return;
            }
            //Console.WriteLine(WhereString);
            string CommandString = "DELETE FROM Dolzine_pet_skrajsan WHERE NOT (" + WhereString.Substring(0, WhereString.Length - 1) + ");";

            SqlConnection connection;

            using (connection = new SqlConnection(sqlConnStr))
            {
                using (SqlCommand command = new SqlCommand(CommandString, connection))
                {
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        reader.Read();
                        
                    }
                    connection.Close();
                }
               
            }
            GameServices.Copy("new");

            return;


        }


        public static void Izpisi(Beseda Word)
            // to funkcijo sem uporabljal za testiranje kode
        {
            List<Beseda> words = Duplicate.ListOfWords(Word);
            List<Beseda> nove = Duplicate.Podvojene(words);
            foreach (Beseda word in nove)
            {
                Console.WriteLine($"{word.Prva}{word.Druga}{word.Tretja}{word.Cetrta}{word.Peta}");
            }

        }

    }
}

static class CreateString
    // ta razred je namenjen izdelavi WhereStringa
{
    public static string XOR(List<string> elements, string crka)
        // ta funkcija sprejme list pozicij v besedi in črko, ki je na teh pozicijah in vrne string, ki opisuje XOR logično izjavo
    {
        string command = "";
        int n = elements.Count;
        foreach (var element in elements)
        {
            string semiComand = element +" = '"+ crka+"'";
            List<string> newList = new List<string>();
            List<string> elemen = new List<string> { element };
            newList = elements.Except(elemen).ToList();
            foreach (var column in newList)
            {
               semiComand = semiComand + " AND " + column + " != '" + crka + "'"; 
            }
            semiComand = " OR (" + semiComand + ")";
            command = command + semiComand;
        }
        return "(" + command.Substring(3) +")";
    }

    public static string Green(List<string> collumns, string letter)
        // ta funkcija sprejme pozicije na katerih je določena črka (letter) zelena in vrne string, ki opisuje, da te zelene črke
    {
        string command = "";
        foreach (string coll in collumns)
        {
            command = command + " AND (" + coll + " = '" + letter + "')";
        }
        return "(" + command.Substring(4) + ")";
    }

    public static string OR(List<string> Collumns, string letter)
        // ta funkcija sprejme pozicije, kjer je določena črka (letter) in vrne string OR string, ki opisuje da je lahko ta črka na teh mestih
    {
        string command = "";
        foreach(string collumn in Collumns)
        {
            command = command + " OR (" + collumn + " = " + "'" + letter +"')";
        }
        return "(" + command.Substring(3) + ")";
    }

    public static string Yellow(List<string> collumns, List<string> yellowColumns, string letter)
        // ta funkcija sprejme vse se možne pozicije (tiste ki nisko zelene), tiste pozicije, ki so rumene in črko, ki jih opisuje, vrne pa string, ki opisuje kaj pomeni če je ena črka rumena ali pa če jih je več
    {
        string command = "";
        if (yellowColumns.Count() == 1)
        {
            if (collumns.Count == 1)
            {
                command = "(Prva = 'nemogoce')";
            }
            else
            {
                command = OR(collumns.Except(yellowColumns).ToList(), letter) + " AND ("+ yellowColumns[0] + " != '" + letter + "')";
            }  
        }
        else if (yellowColumns.Count() == 2)
        {
           if (collumns.Count <= 3)
            {
                command =  "(Prva = 'nemogoce')";
            }
            if (collumns.Count == 4)
            {
                List<string> Greencollumns = collumns.Except(yellowColumns).ToList();
                command = Green(Greencollumns, letter) + " AND NOT (" + OR(yellowColumns, letter) + ")";
            }

           if (collumns.Count == 5)
            {
                List<string> maybe = collumns.Except(yellowColumns).ToList();
                List<string> not = new List<string>();
                foreach (string coll in maybe)
                {
                    not.Add(coll);
                    command = command + " OR (" + Green(maybe.Except(not).ToList(), letter) + ")";
                    not.Clear();
                }
                command = "(" + command.Substring(3) + ")";
            }

        }
        else if (yellowColumns.Count() >= 3)
        {
            command =  "(Prva = 'nemogoce')";
        }
        return command;
    }

    public static string Gray(List<string> collumns, string letter)
        // ta funkcija sprejme vse možne pozicije (tiste ki niso zelene) in katera črka je siva, in vrne stavek, da na teh možnih pozicijah ni te črke.
    {
        // collumns vsebuje tudi rumene in sive
        string command = "";
        foreach  (string col in collumns)
        {
            command = command + " AND (" + col +" != '"+ letter +"')";
        }
        return "(" + command.Substring(4) +")";
    }

    public static string GrayandYellow(List<string> Collumns, List<string> Gray, List<string> Yellow, string crka)
        // ta funkcija dela z črkami, ki so sive in rumene, torej sprejme vse možne pozicije (tiste ki niso zelene), pozicije v katerih so sive, pozicije v katerih so rumene in to črko
        // vrne pa string, ki utreza tem zahtevam.
    {
        // Gray in Yellow moreta vsebovati vsaj en element
        string command = "";
        if (Yellow.Count == 1)
        {
            if (Collumns.Except(Yellow.Concat(Gray)).ToList().Count == 0)
            {
                command = "(Prva = 'nemogoce')";
            }
            else
            {
                command = XOR(Collumns.Except(Yellow.Concat(Gray)).ToList(), crka) + " AND NOT (" + OR(Yellow.Concat(Gray).ToList(), crka) + ")";
            }
        }
        else if (Yellow.Count == 2)
        {
            if (Collumns.Count == 5)
            {
                if (Gray.Count == 1)
                {
                    List<string> Greencollumns = Collumns.Except(Yellow.Concat(Gray.ToList())).ToList();
                    command = Green(Greencollumns, crka) + " AND NOT (" + OR(Yellow.Concat(Gray).ToList(), crka) + ")";
                }
                else
                {
                    command = "(Prva = 'nemogoce')";
                }
            }
            else
            {
                command = "(Prva = 'nemogoce')";
            }
        }
        else
        {
            command = "(Prva = 'nemogoce')";
        }
        return command;
    }

    public static string GetLetter(Beseda word, string i)
        // ta funkcija sprejme besedo in pozicijo črke in vrne to črko iz besede
    {
        if (i == "Prva")
        {
            return word.Prva;
        }
        if (i == "Druga")
        {
            return word.Druga;
        }
        if (i == "Tretja")
        {
            return word.Tretja;
        }
        if (i == "Cetrta")
        {
            return word.Cetrta;
        }
        if (i == "Peta")
        {
            return word.Peta;
        }
        return "";
    }

    public static List<string> ColumnsWithSameLetter(List<string> list, string letter)
        // ta funkcija sprejme list črk in črko in vrne vse pozicije na katerih je ta črka
    {
        List<string> collumns = new List<string>();
        for (int i = 0; i <= list.Count()-1; i++)
        {
            if (list[i] == letter)
            {
                collumns.Add(list[i - 1]);
            }
        }
        return collumns;
    }

    public static string CommandString(List<int> levels, Beseda Word)
        // ta funkcija sprejme list na katerem so opisane barve in besedo in vrne string na katerem je opisana množica besed, ki ustrezajo tem kriterijem
    {
        List<string> list = new List<string>
        {"Prva","Druga","Tretja","Cetrta","Peta" };
        int id = 0;
        List<string> Greens = new List<string>();
        List<string> Yellows = new List<string>();
        List<string> Grays = new List<string>();

        string command = "";
        foreach (int i in levels)
        {
            if (i == 2)
            {
                Greens.Add(list[id]);
                Greens.Add(GetLetter(Word, list[id]));
                
            }
            if (i == 1)
            {
                Yellows.Add(list[id]);
                Yellows.Add(GetLetter(Word, list[id]));
            }
            if (i == 0)
            {
                Grays.Add(list[id]);
                Grays.Add(GetLetter(Word, list[id]));
            }
            id += 1;
        }

        List<string> NotGreens = new List<string>();
        NotGreens = list.Except(Greens).ToList();
        List<string> GreenLetters = new List<string>();
        List<string> YellowGrayLetters = new List<string>();
        List<string> GrayLetters = new List<string>();

        foreach (string coll in list)
        {
            string letter = "";
            
            if (Greens.Contains(coll))
            {
                letter = GetLetter(Word, coll);
                if (!GreenLetters.Contains(letter))
                {
                    command = command + " AND (" + Green(ColumnsWithSameLetter(Greens, letter), letter) +")";
                }
                GreenLetters.Add(letter);
            }
            else if (Yellows.Contains(coll))
            {
                letter = GetLetter(Word, coll);
                if (GrayLetters.Contains(letter))
                {
                    return "    Prva = 'nemogoce'";
                }
                if (!YellowGrayLetters.Contains(letter))
                {
                    
                    if (Grays.Contains(letter))
                    {
                        //Console.WriteLine($"toliko je rumenih {ColumnsWithSameLetter(Yellows,letter).Count()}");
                        //Console.WriteLine($" toliko je sivih {ColumnsWithSameLetter(Grays, letter).Count()}");
                        //Console.WriteLine($"toliko je nezelenih {NotGreens.Count()}");
                        command = command + " AND (" +  GrayandYellow(NotGreens, ColumnsWithSameLetter(Grays, letter), ColumnsWithSameLetter(Yellows, letter), letter) + ")";
                    }
                    else
                    {
                        command = command + " AND (" + Yellow(NotGreens,ColumnsWithSameLetter(Yellows, letter), letter) +")";
                    }
                    
                }
                YellowGrayLetters.Add(letter);
                
            }
            else if (Grays.Contains(coll))
            {
                letter = GetLetter(Word, coll);
                if (!YellowGrayLetters.Contains(letter))
                {
                    command = command + " AND (" + Gray(NotGreens,letter) + ")";
                }
                YellowGrayLetters.Add(letter);
                GrayLetters.Add(letter);
            }

        }

        return command.Substring(4) + ";";
    }

}

static class Duplicate
    // ta razred upravlja z bazo se možnih besed
{
    static string sqlConnStr = "Data Source=DESKTOP-QFE4DUG;Initial Catalog=Wordle;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
    public static List<int> Levels(int i)
        // ta funkcija sprejme id levela in vrne njegov list<int> višje mam že enako funkcijo
    {
        int n = 0;
        List<int> levels = new List<int>();
        for (int j = 4; j >= 0; j--)
        {
            n = i / Convert.ToInt32(Math.Pow(3, j));
            levels.Add(n);
            i = Convert.ToInt32(i - n * Math.Pow(3, j));

        }

        return levels;
    }
    public static List<Beseda> Probability(Beseda Word, int i)
        // sem uporabil za testiranje kode
        // ta funkcija sprejme besedo in vrstni red kombinacije in vrne vse bedede ki vtrezajo kriterijem
    {
        List<Beseda> Words = new List<Beseda>();
        Beseda beseda = new Beseda();
        List<int> levels = Levels(i);
        string WhereString = CreateString.CommandString(levels, Word);
        if (WhereString.Contains("nemogoce"))
        {
            return Words;
        }
        string CommandString = "SELECT * FROM Dolzine_pet_skrajsan WHERE " + WhereString;

        SqlConnection connection;

        using (connection = new SqlConnection(sqlConnStr))
        {
            using (SqlCommand command = new SqlCommand(CommandString, connection))
            {
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        beseda.Prva = reader.GetString(1);
                        beseda.Druga = reader.GetString(2);
                        beseda.Tretja = reader.GetString(3);
                        beseda.Cetrta = reader.GetString(4);
                        beseda.Peta = reader.GetString(5);
                        Words.Add(beseda);
                    }


                }
                connection.Close();
            }

            return Words;
        }

    }

    public static List<Beseda> ListOfWords(Beseda Word)
        // sem uporabil za testiranje kode
    {
        List<Beseda> AllWords= new List<Beseda>();
        for (int i = 0; i < 243; i++)
        {
            List<Beseda> vzorec = Probability(Word, i);

            if (vzorec.Count > 0)
            {
                AllWords.AddRange(vzorec);
            }
            
        }
       
        return AllWords;
    }

    public static List<Beseda> Podvojene(List<Beseda> Besede)
        // sem uporabil za testiranje kode
    {
        var query = Besede.GroupBy(x => x)
              .Where(g => g.Count() > 1)
              .Select(y => y.Key)
              .ToList();
        return query;
    }

}