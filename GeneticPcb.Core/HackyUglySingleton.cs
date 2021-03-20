namespace GeneticPcb.Core
{
    public static class HackyUglySingleton
    {
        public static int LengthWeight { get; set; } = 1;

        public static int SegmentWeight { get; set; } = 1;

        public static int IntersectionWeight { get; set; } = 5000;

        public static int OutOfBoundsWeight { get; set; } = 5000;
    }
}