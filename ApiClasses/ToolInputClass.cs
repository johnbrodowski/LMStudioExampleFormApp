using System.Text.Json.Serialization;

namespace LMStudioExampleFormApp.ApiClasses
{

    public class ToolInput
    {
        [JsonPropertyName("get_weather")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public GetWeather? get_weather { get; set; }
    }

    public class GetWeather()
    {
        [JsonPropertyName("city")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? City { get; set; } = "Boston";

        [JsonPropertyName("temperature")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Temperature { get; set; } = "72";

        [JsonPropertyName("forecast")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Forecast { get; set; } = "Sunny";

        [JsonPropertyName("unit")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Unit { get; set; } = "fahrenheit";
    }
}