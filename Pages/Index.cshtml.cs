using Wordle.Models;
using Wordle.services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Wordle.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public Beseda Word { get; set; }

        public List<Beseda> Wordss = new List<Beseda>();

        public List<string> abeceda = new List<string>() 
        { "a", "b", "c", "č", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "r", "s", "š", "t", "u", "v", "z", "ž" };

        public List<string> Zelene = new List<string>();

        public List<string> Rumene = new List<string>();

        public List<string> Sive = new List<string>();

        public string Status = "";
        public void OnGet()
        {
            //GetServices.InsertMany();
            //GetServices.Delite();

            if (PageContext.HttpContext.Request.Query.Keys.Contains("IsWord"))
            {
                Wordss = GameServices.GetGuessWords();
                if (!bool.Parse(PageContext.HttpContext.Request.Query["IsWord"])) Status = "Neveljavna beseda";

                if (GameServices.Win()) Status = "Zmagali ste";

                if (!GameServices.StilPlaying() && !GameServices.Win()) Status = "Porazno";

            }
            if (!PageContext.HttpContext.Request.Query.Keys.Contains("IsWord"))
            {
                GameServices.Delite();
                GameServices.Copy(null);
                GameServices.NewWord();
                GameServices.ClearLetters();
                

                // testing
                //Word = BotServices.NewWord(7499);
                //Console.WriteLine($"{Word.Prva}{Word.Druga}{Word.Tretja}{Word.Cetrta}{Word.Peta}");
                //BotServices.Probability(Word, 30 + 81);
                //Word = BotServices.BestGues();
            
                //Console.WriteLine(BotServices.ExpectedInformation(Word,BotServices.AllWords()));
                //Console.WriteLine(BotServices.Probability(Word, 3 + 81));
                //BotServices.Izpisi(Word);

            }

            Zelene = GameServices.IsteBarve(Level.Zelena);
            Rumene = GameServices.IsteBarve(Level.Rumena);
            Sive = GameServices.IsteBarve(Level.Siva);


        }

        public IActionResult OnPostTry()
        {
            Console.WriteLine(Word.Prva);
            if (GameServices.IsWord(Word))
            {

                double AllWords = BotServices.AllWords("Dolzine_pet_skrajsan");
                

                Word.ExpectedInformation = BotServices.ExpectedInformation(Word, AllWords);
                
                GameServices.Add(Word);
                BotServices.Delite(Word);

                double LeftWords = BotServices.AllWords("Dolzine_pet_skrajsan");
                Word.Information = BotServices.Information(LeftWords, AllWords);

                return RedirectToAction("Get", new { IsWord = true });
            }

            return RedirectToAction("Get", new { IsWord = false });
        }

        public IActionResult OnPostHint()
        {
            double AllWords = BotServices.AllWords("Dolzine_pet_skrajsan");
            if (AllWords == 1)
            {
                Word = BotServices.NewWord(1);
                GameServices.Add(Word);
                return RedirectToAction("Get", new { IsWord = true });
            }
            Wordss = GameServices.GetGuessWords();
            if (Wordss.Count == 0)
            {
                Word.Prva = 'k'.ToString();
                Word.Druga = 'a'.ToString();
                Word.Tretja = 'r'.ToString();
                Word.Cetrta = 'e'.ToString();
                Word.Peta = 'n'.ToString();
            }
            else
            {
                Word = BotServices.BestGues();
            }
            

            Word.ExpectedInformation = BotServices.ExpectedInformation(Word, AllWords);

            GameServices.Add(Word);
            BotServices.Delite(Word);

            double Words = BotServices.AllWords("Dolzine_pet_skrajsan");
            Word.Information = BotServices.Information(Words, AllWords);

            return RedirectToAction("Get", new { IsWord = true });
        }

        public IActionResult OnPostNewGame()
        {
            return RedirectToAction("Get");
        }
    }
}