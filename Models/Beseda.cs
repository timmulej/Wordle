using System.ComponentModel.DataAnnotations;

namespace Wordle.Models
{
    public class Beseda
    {
        [Required, MaxLength(1)]
        public string Prva { get; set; }

        public Level PrviLevel { get; set; }

        [Required, MaxLength(1)]
        public string Druga { get; set; }

        public Level DrugiLevel { get; set; }

        [Required, MaxLength(1)]
        public string Tretja { get; set; }

        public Level tretjiLevel { get; set; }

        [Required, MaxLength(1)]
        public string Cetrta { get; set; }

        public Level CetrtiLevel { get; set; }

        [Required, MaxLength(1)]
        public string Peta { get; set; }
         
        public Level PetiLevel { get; set; }

        public double ExpectedInformation { get; set; }

        public double Information { get; set; }

    }

    public enum Level { Siva, Rumena, Zelena }
}

