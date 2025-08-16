using System.Text.Json.Serialization;

namespace StorySpoilerTests.Models
{
    // This class is representing the structure of a story for creation and editing purposes. 
    internal class StoryDTO
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; } = null;

        [JsonPropertyName("id")]
        public string? Id { get; set; } = null;
    }
}
