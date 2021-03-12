namespace GeneticPcb.Models
{
    public class PcbBoard
    {
        public int Width { get; }
        public int Height { get; }

        public SolderingPoint[] SolderingPoints { get; }

        public PcbBoard(int width, int height, SolderingPoint[] solderingPoints)
        {
            Width = width;
            Height = height;

            SolderingPoints = solderingPoints;
        }
    }
}