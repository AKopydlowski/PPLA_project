namespace PPLA.Project.Core.Metar
{
    public class MetarReport
    {
        public string Raw { get; set; } = string.Empty;
        public string? Wind { get; set; }
        public string? Visibility { get; set; }
        public string? Clouds { get; set; }
        public string? Qnh { get; set; }
    }
}
