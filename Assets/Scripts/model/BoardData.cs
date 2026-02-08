namespace Game.Model
{
    public enum DropResult
    {
        Rejected = 0,
        Moved = 1,
        Merged = 2
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

        public DropResult ApplyDrop(Position from, Position to)
        {
            if (!IsInside(from) || !IsInside(to))
            {
                return DropResult.Rejected;
            }

            if (from == to)
            {
                return DropResult.Rejected;
            }

            TileData fromTile = GetTile(from);
            if (fromTile == null)
            {
                return DropResult.Rejected;
            }

            if (IsEmpty(to))
            {
                grid[to.Column, to.Row] = fromTile;
                grid[from.Column, from.Row] = null;
                return DropResult.Moved;
            }

            TileData toTile = GetTile(to);

            if (fromTile.IsGenerator || toTile.IsGenerator)
            {
                return DropResult.Rejected;
            }

            if (fromTile.Definition == null || toTile.Definition == null)
            {
                return DropResult.Rejected;
            }

            if (fromTile.Definition != toTile.Definition)
            {
                return DropResult.Rejected;
            }

            if (fromTile.EvolutionIndex != toTile.EvolutionIndex)
            {
                return DropResult.Rejected;
            }

            if (!toTile.TryEvolve())
            {
                return DropResult.Rejected;
            }

            grid[from.Column, from.Row] = null;
            return DropResult.Merged;
        }
    }
}