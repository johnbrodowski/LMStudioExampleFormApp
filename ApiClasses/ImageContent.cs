using LMStudioExampleFormApp.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LMStudioExampleFormApp.ApiClasses
{
    public class ImageContent : IMessageContent
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; } = MessageType.ImageUrl;

        [JsonPropertyName("image_url")]
        public ImageUrlData? ImageUrl { get; set; }
    }
}
