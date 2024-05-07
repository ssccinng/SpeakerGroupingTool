namespace SpeakerGroupingTool.Models
{
    //public class Init
    //{
    //}


    public class Init
    {
        public string type { get; set; }
        public Data data { get; set; }
    }

    public class Data
    {
        public string username { get; set; }
        public string[] parquets { get; set; }
    }

}
