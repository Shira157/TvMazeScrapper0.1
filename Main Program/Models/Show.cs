using System.Text.Json.Serialization;

namespace TvMazeScrapper.Models
{
    public class Show
    {
        
        public int id { get; set; }
        
        public string name { get; set; }
        public ICollection<CastPerson> CastPerson { get; set; }
    }
}
