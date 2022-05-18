using System.Net;
using System.Data.SqlClient;

namespace Wordle.services;

public class GetServices
{
    static List<string> Besede { get; }


    static GetServices()
    {
        Besede = new List<string>
        { };

    }


    static string sqlConnStr = "Data Source=DESKTOP-QFE4DUG;Initial Catalog=Wordle;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
    // to je povezava do baze podatkov

    public static string GetText()
    // ta funkcija pobere vse podatke iz spletne strani http://bos.zrc-sazu.si/sbsj.html in jih pretvori v tekst.
    {
        var client = new WebClient();

        var text = client.DownloadString("http://bos.zrc-sazu.si/sbsj.html");
        return text;
    }

    public static (string, string, int) CutBefore(string text)
        //ta funkcija sprejme tekst in in vrne tekst brez prve besede, prvo besedo in njeno dolžino
    {
        int len_word = text.IndexOf("<br>");
        string word = text.Substring(0, len_word);
        text = text.Substring(len_word + 6);
        return (text, word, len_word);
    }

    public static (string, string) Find_nLenWord(string text, int n)
        // ta funkcija sprejme tekst in dolžino zeljene iskane besede in vrne tekst po prvi besedi s to dolžino in samo besedo
    {
        string word = "";
        int len_word = 0; 
        while (1 > 0)
        {
            (text,word,len_word) = CutBefore(text);
            if (len_word == n)
            {
                return (text, word);
            }
        }
        
    }

    

    public static void InsertToSQL(string word)
        // ta funkcija sprejme sting word in ga shrani v SQLbazo
    {
        Console.WriteLine(word);
        SqlConnection connection;
        using (connection = new SqlConnection(sqlConnStr))
        {
            connection.Open();

            string queryString = " INSERT INTO Dolzine_pet " +
            "(Prva, Druga, Tretja, Cetrta, Peta)" +
            $"Values ('{word[0]}','{word[1]}','{word[2]}','{word[3]}','{word[4]}' );";

            using (SqlCommand command = new SqlCommand(queryString, connection))
            {
                Int32 recordsAffected = command.ExecuteNonQuery();
            }

            connection.Close();
        }
    }

    public static void InsertMany()
    // ta funkcija vstavi vse besede dolžine 5 iz spletne strani http://bos.zrc-sazu.si/sbsj.html v SQL bazo.
    {
        string word = "";
        string text = GetServices.GetText();
        while (text.Length > 3)
        {
            (text, word) = GetServices.Find_nLenWord(text, 5);
            InsertToSQL(word);
        }
        
    }

    public static void Delite()
        // ta funcija izloči vse besede iz SQL baze, ki vsebujejo znake x,y,q,w,#,-,.
    {
        List<string> cundneCrke = new List<string>();
        cundneCrke.Add("x");
        cundneCrke.Add("y");
        cundneCrke.Add("q");
        cundneCrke.Add("w");
        cundneCrke.Add("#");
        cundneCrke.Add("-");
        cundneCrke.Add(".");

        List<string> collums = new List<string>();
        collums.Add("Prva");
        collums.Add("Druga");
        collums.Add("Tretja");
        collums.Add("Cetrta");
        collums.Add("Peta");

        foreach (string crka in cundneCrke)
        {
            string CommandString = "DELETE FROM Dolzine_pet WHERE NOT (" + CreateString.Gray(collums, crka) + ");";

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
                return;
            }
        } 
    }
        
}
