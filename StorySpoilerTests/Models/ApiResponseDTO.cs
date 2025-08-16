using System.Text.Json.Serialization;

namespace StorySpoilerTests.Models
{
    // This class is used to parse common response structures from the API.
    internal class ApiResponseDTO
    {
        [JsonPropertyName("msg")]
        public string? Msg { get; set; }

        [JsonPropertyName("storyId")]
        public string? StoryId { get; set; }
    }
}

