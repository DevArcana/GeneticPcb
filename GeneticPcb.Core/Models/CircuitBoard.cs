namespace GeneticPcb.Core.Models
{
    public sealed record CircuitBoard (uint Width, uint Height, Route[] Routes);
}