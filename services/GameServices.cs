using System.Net;
using Wordle.Models;
using System.Data.SqlClient;

namespace Wordle.services
{
    public class GameServices
        // tukaj imam shranjene funkcije, ki urejajo pravila igre
    {
        static List<Beseda> Besede { get; }
        // v tem listu imam shranjene besede, ki sem jih že poizkusil

        static Beseda SelectedWord { get; }
        // to je beseda, ki jo igralec poizkusa uganiti

        static List<string> Zelene { get; }
        // tukaj imam shranjene črke, ki vemo da so zelene

        static List<string> Rumene { get; }
        // tukaj imam shranjene črke za katere vem da so rumene

        static List<string> Sive { get; }
        // tukaj imam shranjene črke za katere vem da so sive

        static GameServices()
            // tukaj vsakič ko zazenem aplikacijo nastavim vse liste, da so prazni
        {
            Besede = new List<Beseda>
            { };
            SelectedWord = new Beseda
            { };
            Zelene = new List<string>
            { };
            Rumene = new List<string>
            { };
            Sive = new List<string>
            { };

        }

        static string sqlConnStr = "Data Source=DESKTOP-QFE4DUG;Initial Catalog=Wordle;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        // to je povezava do baze podatkov

        public static Beseda GetSelectedWord()
            // ta funkcija vrne SelectedWord (besedo, ki jo je treba uganit).
        {
            return SelectedWord;
        }
        public static bool IsWord(Beseda beseda)
            // ta funkcija sprejme besedo in pogleda v bazi, če je veljavna beseda
        {
            if(beseda == null)
                return false;

            SqlConnection connection;
         
            using (connection = new SqlConnection(sqlConnStr))
            {
                using (SqlCommand command = new SqlCommand(
                    $"SELECT * FROM Dolzine_pet WHERE Prva = '{beseda.Prva}' AND " +
                    $"Druga = '{beseda.Druga}' AND Tretja = '{beseda.Tretja}' AND " +
                    $"Cetrta = '{beseda.Cetrta}' AND Peta = '{beseda.Peta}';",
                 connection))
                {
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {

                            return true;
                        }
                        else
                        {
                            return false;
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
        }
  
        public static void Add(Beseda Word)
            // ta funkcija sprejme besedo in jo doda me že probane besede in doba njene črke med že poizkušene črke
        {
            List<int> leveli = new List<int>();
            List<string> Sez = new List<string>();
            Word = Check.GetLevel(Word, SelectedWord);

            leveli = Check.GetList(Word);

            Sez.Add(Word.Prva);
            Sez.Add(Word.Druga);
            Sez.Add(Word.Tretja);
            Sez.Add(Word.Cetrta);
            Sez.Add(Word.Peta);
            for (int i = 0; i < 5; i++)
            {
                if (leveli[i] == 0) Sive.Add(Sez[i]);
                if (leveli[i] == 1) Rumene.Add(Sez[i]);
                if (leveli[i] == 2) Zelene.Add(Sez[i]);
            }
            //Console.WriteLine($"{leveli[0]},{leveli[1]},{leveli[2]},{leveli[3]},{leveli[4]},");
            //foreach (string crka in Zelene) Console.WriteLine(crka);

            Besede.Add(Word);
        }

        public static List<Beseda> GetGuessWords()
            // ta funkcija vrne ze poizkušene besede
        {
            return Besede;
        }
    
        public static void NewWord()
            // ta funkcija vrne novo naključno besedo iz baze.
        {
          
            SqlConnection connection;

            using (connection = new SqlConnection(sqlConnStr))
            {
                using (SqlCommand command = new SqlCommand(
                    "SELECT TOP 1 * FROM Dolzine_pet ORDER BY NEWID();",
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
                //SelectedWord.Prva = "k";
                //SelectedWord.Druga = "r";
                //SelectedWord.Tretja = "i";
                //SelectedWord.Cetrta = "k";
                //SelectedWord.Peta = "a";
                Console.WriteLine($"{SelectedWord.Prva}{SelectedWord.Druga}{SelectedWord.Tretja}{SelectedWord.Cetrta}{SelectedWord.Peta}");
            }
            

        }
        
        public static bool Win()
            // ta funkcija preveri, če si zmagal igro
        {
            if (Besede.Count == 0) return false;
            Beseda LastWord = Besede.Last();
            if (LastWord.PrviLevel == Level.Zelena &&
                LastWord.DrugiLevel == Level.Zelena &&
                LastWord.tretjiLevel == Level.Zelena &&
                LastWord.CetrtiLevel == Level.Zelena &&
                LastWord.PetiLevel == Level.Zelena) return true;
            return false;

        }

        public static bool StilPlaying()
            // ta funkcija preveri če si že prekoračil število možnih poizkusov
        {
            if (Besede.Count() < 6) return true;
            return false;
        }

        public static void Delite()
            // ta funkcija pobriše vse poizkušene besede
        {
            Besede.Clear();
        }

        public static void Copy(string? novo)
            // ta funkcija sprejme string in če ni prazen kopira bazo Dolzine_pet v Dolzine_pet_skrajsan, če 
            // pa ni prazen pa resetira indeks v bazi Dolzine_pet_skrajsan
        {
            string CommandString = "INSERT INTO Dolzine_pet_skrajsan (Prva, Druga, Tretja, Cetrta, Peta)" +
                "SELECT Prva, Druga, Tretja, Cetrta, Peta FROM Dolzine_pet;";
            string CommandString2 = "DELETE FROM Dolzine_pet_skrajsan;";
            
            if (!string.IsNullOrEmpty(novo))
            {
                CommandString = "INSERT INTO NewDolzine_pet_skrajsan (Prva, Druga, Tretja, Cetrta, Peta)" +
                "SELECT Prva, Druga, Tretja, Cetrta, Peta FROM Dolzine_pet_skrajsan;";
                CommandString2 = "DELETE  FROM newDolzine_pet_skrajsan;";
            }

            SqlConnection connection;

            using (connection = new SqlConnection(sqlConnStr))
            {
                using (SqlCommand command = new SqlCommand(CommandString2, connection))
                {
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        reader.Read();
                    }
                    connection.Close();
                }


            }

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

            if (!string.IsNullOrEmpty(novo))
            {
                CommandString2 = "DELETE FROM Dolzine_pet_skrajsan; " +
                    "DBCC CHECKIDENT ('Dolzine_pet_skrajsan', RESEED, 0); " +
                    "INSERT INTO Dolzine_pet_skrajsan (Prva, Druga, Tretja, Cetrta, Peta)" +
                    " SELECT Prva, Druga, Tretja, cetrta, Peta FROM newDolzine_pet_skrajsan ORDER BY id ASC;";

                using (connection = new SqlConnection(sqlConnStr))
                {
                    using (SqlCommand command = new SqlCommand(CommandString2, connection))
                    {
                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            reader.Read();
                        }
                        connection.Close();
                    }

                }
            }

        }

        public static List<string> IsteBarve(Level level)
            // ta funkcija sprejme level črke in vrne njeno barvo
        {
            if (level == Level.Siva) return Sive;
            if (level == Level.Rumena) return Rumene;
            else return Zelene;
         
        }

        public static void ClearLetters()
            // ta funkcija pobriše vse ze uganjene črke
        {
            Zelene.Clear();
            Rumene.Clear();
            Sive.Clear();
        }

    }
}

static class Check
    // ta razred preveri kakšne barve so črke v uganjeni besedi
{
    public static List<string> AllExceptOne(Beseda Word, int i)
    {
        // vrne seznam črk, ki so v besedi razen i-te crke
        List<string> Sez = new List<string>();
        Sez.Add(Word.Prva);
        Sez.Add(Word.Druga);
        Sez.Add(Word.Tretja);
        Sez.Add(Word.Cetrta);
        Sez.Add(Word.Peta);
        Sez.RemoveAt(i);
        return Sez;

    }
    public static Beseda IsZelena(Beseda Word, Beseda SelectedWord, int i)
    {
        // v selectedWord zamenja i-to crko z *, če je v wordu na istem mestu
        string beseda = $"{Word.Prva}{Word.Druga}{Word.Tretja}{Word.Cetrta}{Word.Peta}";
        string izbranaBeseda = $"{SelectedWord.Prva}{SelectedWord.Druga}{SelectedWord.Tretja}{SelectedWord.Cetrta}{SelectedWord.Peta}";

        if (beseda.Substring(i, 1) == izbranaBeseda.Substring(i, 1))
        {
            izbranaBeseda = izbranaBeseda.Remove(i, 1);
            izbranaBeseda = izbranaBeseda.Insert(i, "*");
        }
       
        return ToBeseda(izbranaBeseda);
    }
    public static Beseda IsRumena(Beseda Word, Beseda SelectedWord, int i)
    {
        // v SelectedWord zamenja crko, ki je enaka i-ti v wordu z *
        List<string> Sez = AllExceptOne(SelectedWord, i);
        string beseda = $"{Word.Prva}{Word.Druga}{Word.Tretja}{Word.Cetrta}{Word.Peta}";
        string izbranaBeseda = $"{SelectedWord.Prva}{SelectedWord.Druga}{SelectedWord.Tretja}{SelectedWord.Cetrta}{SelectedWord.Peta}";
        string crka = beseda.Substring(i, 1);

        if (Sez.Contains(crka))
        {
            int index = izbranaBeseda.IndexOf(crka);
            izbranaBeseda = izbranaBeseda.Remove(index, 1);
            izbranaBeseda = izbranaBeseda.Insert(index, "*");
   
        }
 
        return ToBeseda(izbranaBeseda);
    }

    public static Beseda CheckZelena(Beseda Word, Beseda IzbranaBeseda)
    {
        // v selectedWord zamenja vse črke z *, ki se ujemanjo z črkami v Wordu
        IzbranaBeseda = IsZelena(Word, IzbranaBeseda, 0);
        IzbranaBeseda = IsZelena(Word, IzbranaBeseda, 1);
        IzbranaBeseda = IsZelena(Word, IzbranaBeseda, 2);
        IzbranaBeseda = IsZelena(Word, IzbranaBeseda, 3);
        IzbranaBeseda = IsZelena(Word, IzbranaBeseda, 4);
        return IzbranaBeseda;
    }
    public static List<int> CheckRumena(Beseda Word, Beseda SelectedWord, List<int> zelene)
    {
        // pove indexe črk ki jih je zamenjala z *, ki simbolizirajo rumeno barvo
        string izbranaBeseda = $"{SelectedWord.Prva}{SelectedWord.Druga}{SelectedWord.Tretja}{SelectedWord.Cetrta}{SelectedWord.Peta}";
        List<int> list = new List<int>();
        Beseda Nova = SelectedWord;
        string beseda = "";
        for (int i = 0; i < 5; i++)
        {
            if (!zelene.Contains(i))
            {
                if (i == 0)
                { Nova = IsRumena(Word, SelectedWord, i); }

                else
                { Nova = IsRumena(Word, Nova, i); }

                beseda = $"{Nova.Prva}{Nova.Druga}{Nova.Tretja}{Nova.Cetrta}{Nova.Peta}";
                if (beseda != izbranaBeseda)
                {
                    list.Add(i);
                    izbranaBeseda = beseda;
                }
            }
        }
        

        return list;
    }

    public static Beseda GetLevel(Beseda Word, Beseda SelectedWord)
        // ta funkcija sprejme uganjeno besedo in izbrano besedo in vrne barve uganjene besede
    {
        Beseda Nova = new Beseda();
        List<int> zelene = new List<int>();
        Nova = CheckZelena(Word, SelectedWord);

        string beseda = $"{Nova.Prva}{Nova.Druga}{Nova.Tretja}{Nova.Cetrta}{Nova.Peta}";
        string izbranaBeseda = $"{SelectedWord.Prva}{SelectedWord.Druga}{SelectedWord.Tretja}{SelectedWord.Cetrta}{SelectedWord.Peta}";

        for (int j = 0; j < 5; j++)
        {
            if (beseda.Substring(j, 1) != izbranaBeseda.Substring(j, 1))
            {
                Word = SetLevel(Word, Level.Zelena, j);
                zelene.Add(j);
            }
        }

        List<int> rumene = new List<int>();
        rumene = CheckRumena(Word, Nova,zelene);

        foreach (int j in rumene)
        {
            Console.WriteLine(j);
            if (!zelene.Contains(j))
            {
                Word = SetLevel(Word, Level.Rumena, j);
            }
        }
        
        return Word;   

    }

    public static Beseda SetLevel(Beseda Word, Level level, int i)
        // ta funkcija sprejme besedo, level in int in nastavi v tej besedi i-to črko na izbran level
    {
        if (i == 0)
        {
            Word.PrviLevel = level;
        }
        if (i == 1)
        {
            Word.DrugiLevel = level;
        }
        if (i == 2)
        {
            Word.tretjiLevel = level;
        }
        if (i == 3)
        {
            Word.CetrtiLevel = level;
        }
        if (i == 4)
        {
            Word.PetiLevel = level;
        }
        return Word;
    }

    public static Beseda ToBeseda(string word)
        // ta funkcija iz stringa naredi besedo
    {
        Beseda Word = new Beseda();
        Word.Prva = word.Substring(0,1);
        Word.Druga = word.Substring(1, 1);
        Word.Tretja = word.Substring(2, 1);
        Word.Cetrta = word.Substring(3, 1);
        Word.Peta = word.Substring(4, 1);
        return Word;
    }

    public static int ToInt(Level lvl)
        // ta funkcija levele pretvarja v int, Zelena = 2, Rumena = 1, Siva = 0
    {
        if (lvl == Level.Siva)
        {
            return 0;
        }
        else if (lvl == Level.Rumena)
        {
            return 1;
        }
        else if  (lvl == Level.Zelena)
        {
            return 2;
        }
        else return 3;
    }

    public static Level GetLevelWord(Beseda Word, int i)
        // ta funkcija sprejme besedo in int in vrne level i-te črke v tej besedi
    {
        if (i == 0)
        {
            return Word.PrviLevel;
        }
        if (i == 1)
        {
            return Word.DrugiLevel;
        }
        if (i == 2)
        {
            return Word.tretjiLevel;
        }
        if (i == 3)
        {
            return Word.CetrtiLevel;
        }
        if (i == 4)
        {
            return Word.PetiLevel;
        }
        return Level.Siva;
    }

    public static List<int> GetList(Beseda word)
        // ta funkcija sprejme besedo in vrne list int v katerem so shranjeni leveli besede.
    {
        List<int> list = new List<int>();
        for (int i=0; i<5; i++)
        {
            list.Add(ToInt(GetLevelWord(word, i)));
        }
        return list;
    }
}
