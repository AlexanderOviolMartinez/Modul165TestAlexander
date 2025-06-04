using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SongApiMM.Models
{
    public class Song
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Year { get; set; } = "";
        public string Genre { get; set; } = "";
        public string[] Artists { get; set; } = Array.Empty<string>();
    }
}