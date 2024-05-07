using System.Text.Json.Serialization;

namespace SpeakerGroupingTool.Models
{

    public class GetStory
    {
        public string type { get; set; }
        public string path { get; set; }
        public string[] previous_dataset { get; set; }
    }


    public class GetStoryResponse
    {
        public string type { get; set; }
        public Story[] data { get; set; }
    }

    public class Story
    {
        public string knn_result { get; set; }
        public string estimated_speaker { get; set; }
        public string 人物 { get; set; }
        public string 人物台词 { get; set; }
        public string 开始时间 { get; set; }
        public string image { get; set; }
        public string audio { get; set; }
        [JsonIgnore]
        public string AIM =>
            estimated_speaker.Contains("_") ? estimated_speaker.Split("_")[0] : "";
        [JsonPropertyName("Row Index")]
        public int RowIndex { get; set; }
    }
}
