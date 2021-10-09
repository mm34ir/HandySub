namespace HandySub.Models
{
    public class SrtModel
    {
        public int Index { get; set; }
        public int BeginHour { get; set; }
        public int BeginMintue { get; set; }
        public int BeginSecond { get; set; }
        public int BeginMSecond { get; set; }
        public int BeginTime { get; set; }
        public int EndHour { get; set; }
        public int EndMintue { get; set; }
        public int EndSecond { get; set; }
        public int EndMSecond { get; set; }
        public int EndTime { get; set; }
        public string SrtString { get; set; }
    }
}
