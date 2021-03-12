namespace GeneticPcb.Models
{
    public class PcbBoard
    {
        public int Width { get; }
        public int Height { get; }

        public PcbBoard(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}