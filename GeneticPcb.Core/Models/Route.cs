namespace GeneticPcb.Core.Models
{
    public sealed class Route
    {
        public BoardPoint Start { get; }
        public BoardPoint End { get; }
        
        public Path Path { get; }

        public bool IsConnected => Path.End == End;

        public Route(BoardPoint start, BoardPoint end)
        {
            Start = start;
            End = end;

            Path = new Path(start);
        }
    }
}