namespace GeneticPcb.Models
{
    public class SolderingPoint
    {
        public int X { get; }
        public int Y { get; }

        public SolderingPoint(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}