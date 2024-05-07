namespace SpeakerGroupingTool.Models
{
    public class UpdateStory
    {
        public string type { get; set; }

        public LabelData[] data { get; set; }
    }

    public class LabelData
    {
        public int id { get; set; }
        public string 人物 { get; set; }
    }
    public class UpdateStoryResponse
    {
        public string type { get; set; }
        public Story[] data { get; set; }
    }
}
