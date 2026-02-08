namespace Game.Model
{
    public enum DropResult
    {
        Rejected = 0,
        Moved = 1,
        Merged = 2
    }

    public struct GeneratorActivationResult
    {
        public Position SpawnPosition { get; }
        public TileDefinition OutputDefinition { get; }
        public int EvolutionIndex { get; }

        public GeneratorActivationResult(Position spawnPosition, TileDefinition outputDefinition, int evolutionIndex)
        {
            SpawnPosition = spawnPosition;
            OutputDefinition = outputDefinition;
            EvolutionIndex = evolutionIndex;
        }
    }

    public class BoardData
    {
        public readonly int Columns;
        public readonly int Rows;

        private readonly TileData[,] grid;

        public BoardData(int columns, int rows)
        {
            Columns = columns;
            Rows = rows;
            grid = new TileData[columns, rows];
        }

        public bool IsInside(Position position)
        {
            return position.Column >= 0 && position.Column < Columns &&
                   position.Row >= 0 && position.Row < Rows;
        }

        public TileData GetTile(Position position)
        {
            return grid[position.Column, position.Row];
        }

        public bool IsEmpty(Position position)
        {
            return GetTile(position) == null;
        }

        public void SetTile(Position position, TileData tile)
        {
            grid[position.Column, position.Row] = tile;
        }

        public void ClearTile(Position position)
        {
            grid[position.Column, position.Row] = null;
        }

        public DropResult ApplyDrop(Position origin, Position destination)
        {
            if (!IsInside(origin) || !IsInside(destination))
            {
                return DropResult.Rejected;
            }

            if (origin.Column == destination.Column && origin.Row == destination.Row)
            {
                return DropResult.Rejected;
            }

            TileData originTile = GetTile(origin);
            if (originTile == null)
            {
                return DropResult.Rejected;
            }

            if (IsEmpty(destination))
            {
                grid[destination.Column, destination.Row] = originTile;
                grid[origin.Column, origin.Row] = null;
                return DropResult.Moved;
            }

            TileData destinationTile = GetTile(destination);
            if (destinationTile == null)
            {
                return DropResult.Rejected;
            }

            if (originTile.IsGenerator || destinationTile.IsGenerator)
            {
                return DropResult.Rejected;
            }

            if (originTile.Definition == null || destinationTile.Definition == null)
            {
                return DropResult.Rejected;
            }

            if (originTile.Definition != destinationTile.Definition)
            {
                return DropResult.Rejected;
            }

            if (originTile.EvolutionIndex != destinationTile.EvolutionIndex)
            {
                return DropResult.Rejected;
            }

            if (!destinationTile.TryEvolve())
            {
                return DropResult.Rejected;
            }

            grid[origin.Column, origin.Row] = null;
            return DropResult.Merged;
        }

        public bool TryActivateGenerator(Position generatorPosition, out GeneratorActivationResult result)
        {
            result = default;

            if (!IsInside(generatorPosition))
            {
                return false;
            }

            TileData generatorTile = GetTile(generatorPosition);
            if (generatorTile == null)
            {
                return false;
            }

            if (generatorTile.Definition == null || !generatorTile.Definition.IsGenerator)
            {
                return false;
            }

            TileDefinition outputDefinition = generatorTile.Definition.GeneratedOutputDefinition;
            if (outputDefinition == null)
            {
                return false;
            }

            Position spawnPosition;
            bool hasEmptySlot = TryFindFirstEmptySlot(out spawnPosition);
            if (!hasEmptySlot)
            {
                return false;
            }

            int evolutionIndex = 0;

            SetTile(spawnPosition, new TileData(outputDefinition, evolutionIndex));
            result = new GeneratorActivationResult(spawnPosition, outputDefinition, evolutionIndex);
            return true;
        }

        private bool TryFindFirstEmptySlot(out Position position)
        {
            for (int row = 0; row < Rows; row++)
            {
                for (int column = 0; column < Columns; column++)
                {
                    Position candidate = new Position(column, row);
                    if (IsEmpty(candidate))
                    {
                        position = candidate;
                        return true;
                    }
                }
            }

            position = new Position(0, 0);
            return false;
        }
    }
}
