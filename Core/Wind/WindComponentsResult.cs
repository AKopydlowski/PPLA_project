namespace PPLA.Project.Core.Wind
{
    public class WindComponentsResult
    {
        public double HeadwindKt { get; set; }
        public double CrosswindKt { get; set; }
        public bool ExceedsCrosswindLimit { get; set; }
    }
}
