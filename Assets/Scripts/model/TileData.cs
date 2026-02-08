namespace Game.Model
{
    public class TileData
    {
        public TileDefinition Definition { get; }
        public int EvolutionIndex { get; private set; }

        public TileData(TileDefinition definition, int evolutionIndex)
        {
            Definition = definition;
            EvolutionIndex = evolutionIndex;
        }

        public bool IsGenerator
        {
            get { return Definition != null && Definition.IsGenerator; }
        }

        public bool CanEvolve()
        {
            if (Definition == null)
            {
                return false;
            }

            return !Definition.IsMaxEvolution(EvolutionIndex);
        }

        public bool TryEvolve()
        {
            if (!CanEvolve())
            {
                return false;
            }

            EvolutionIndex += 1;
            return true;
        }
    }
}