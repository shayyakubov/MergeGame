using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Game.Model;
using Game.View;

namespace Game.Controller
{
    public class BoardController
    {
        private readonly BoardData boardData;
        private readonly Board boardView;

        private readonly int columns;
        private readonly int rows;

        private readonly Slot[,] slots;

        private readonly Dictionary<Position, Tile> tilesByPosition;
        private readonly Dictionary<Tile, Position> positionByTile;

        private readonly GameObject slotPrefab;
        private readonly GameObject tilePrefab;

        private readonly Color lightSlotColor = new Color(0.90f, 0.90f, 0.90f, 1.0f);
        private readonly Color darkSlotColor = new Color(0.78f, 0.78f, 0.78f, 1.0f);

        private const float GeneratorFlyDurationSeconds = 0.40f;

        private bool isDraggingLocked;

        public BoardController(BoardData boardData, Board boardView, GameObject slotPrefab, GameObject tilePrefab)
        {
            this.boardData = boardData;
            this.boardView = boardView;
            this.slotPrefab = slotPrefab;
            this.tilePrefab = tilePrefab;

            columns = boardData.Columns;
            rows = boardData.Rows;

            slots = new Slot[columns, rows];

            tilesByPosition = new Dictionary<Position, Tile>(columns * rows);
            positionByTile = new Dictionary<Tile, Position>(columns * rows);

            isDraggingLocked = false;
        }

        public void BuildBoard()
        {
            boardView.Grid.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
            boardView.Grid.constraintCount = columns;

            RectTransform parent = boardView.BoardParent;

            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(parent.GetChild(i).gameObject);
            }

            tilesByPosition.Clear();
            positionByTile.Clear();

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    GameObject slotObject = Object.Instantiate(slotPrefab, parent);
                    Slot slot = slotObject.GetComponent<Slot>();

                    Position position = new Position(column, row);

                    bool isDark = ((column + row) % 2) == 1;
                    Color backgroundColor = isDark ? darkSlotColor : lightSlotColor;

                    slot.Initialize(position, backgroundColor);
                    slots[column, row] = slot;
                }
            }
        }

        public void SpawnTile(Position position, TileDefinition definition, int evolutionIndex)
        {
            if (!boardData.IsInside(position))
            {
                return;
            }

            if (!boardData.IsEmpty(position))
            {
                return;
            }

            boardData.SetTile(position, new TileData(definition, evolutionIndex));
            CreateTileView(position, definition, evolutionIndex);
        }

        private Tile CreateTileView(Position position, TileDefinition definition, int evolutionIndex)
        {
            Transform slotTransform = slots[position.Column, position.Row].transform;

            GameObject tileObject = Object.Instantiate(tilePrefab, slotTransform, false);
            Tile tile = tileObject.GetComponent<Tile>();

            tile.Initialize(boardView.DragLayer, true);
            tile.SetSprite(definition != null ? definition.GetSprite(evolutionIndex) : null);

            tile.Dropped += HandleTileDropped;
            tile.Clicked += HandleTileClicked;

            tilesByPosition[position] = tile;
            positionByTile[tile] = position;

            tile.AttachTo(slotTransform);
            tile.SetDraggable(!isDraggingLocked);

            return tile;
        }

        private void RemoveTileView(Tile tile)
        {
            if (tile == null)
            {
                return;
            }

            tile.Dropped -= HandleTileDropped;
            tile.Clicked -= HandleTileClicked;

            if (positionByTile.TryGetValue(tile, out Position position))
            {
                positionByTile.Remove(tile);

                if (tilesByPosition.TryGetValue(position, out Tile existing) && existing == tile)
                {
                    tilesByPosition.Remove(position);
                }
            }

            Object.Destroy(tile.gameObject);
        }

        private void SetDraggingLocked(bool locked)
        {
            isDraggingLocked = locked;

            foreach (Tile tile in positionByTile.Keys)
            {
                if (tile != null)
                {
                    tile.SetDraggable(!locked);
                }
            }
        }

        private void HandleTileClicked(Tile clickedTile)
        {
            if (!positionByTile.TryGetValue(clickedTile, out Position generatorPosition))
            {
                return;
            }

            TileData generatorData = boardData.GetTile(generatorPosition);
            if (generatorData == null)
            {
                return;
            }

            if (generatorData.Definition == null || !generatorData.Definition.IsGenerator)
            {
                return;
            }

            TileDefinition outputDefinition = generatorData.Definition.GeneratedOutputDefinition;
            if (outputDefinition == null)
            {
                return;
            }

            Position emptyPosition;
            bool found = TryFindFirstEmptySlot(out emptyPosition);
            if (!found)
            {
                return;
            }

            boardData.SetTile(emptyPosition, new TileData(outputDefinition, 0));

            Tile spawnedTile = CreateTileView(emptyPosition, outputDefinition, 0);

            Transform destinationSlot = slots[emptyPosition.Column, emptyPosition.Row].transform;

            Vector3 from = clickedTile.transform.position;
            Vector3 destination = destinationSlot.position;

            SetDraggingLocked(true);

            spawnedTile.PlayFlyAnimation(from, destination, GeneratorFlyDurationSeconds, () =>
            {
                spawnedTile.AttachTo(destinationSlot);
                
                // TODO: if there is another spawn in progress during the end of this one
                // this may disable the lock and allow tiles movement [bug :( ]
                SetDraggingLocked(false);
            });
        }

        private bool TryFindFirstEmptySlot(out Position position)
        {
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    Position candidate = new Position(column, row);
                    if (boardData.IsEmpty(candidate))
                    {
                        position = candidate;
                        return true;
                    }
                }
            }

            position = new Position(0, 0);
            return false;
        }

        private void HandleTileDropped(Tile draggedTile, PointerEventData eventData)
        {
            if (isDraggingLocked)
            {
                draggedTile.SnapBack();
                return;
            }

            if (!positionByTile.TryGetValue(draggedTile, out Position from))
            {
                draggedTile.SnapBack();
                return;
            }

            GameObject raycastObject = eventData.pointerCurrentRaycast.gameObject;
            if (raycastObject == null)
            {
                draggedTile.SnapBack();
                return;
            }

            Slot targetSlot = raycastObject.GetComponentInParent<Slot>();
            if (targetSlot == null)
            {
                draggedTile.SnapBack();
                return;
            }

            Position destination = targetSlot.Position;

            DropResult result = boardData.ApplyDrop(from, destination);

            if (result == DropResult.Moved)
            {
                tilesByPosition.Remove(from);
                tilesByPosition[destination] = draggedTile;

                positionByTile[draggedTile] = destination;

                draggedTile.AttachTo(slots[destination.Column, destination.Row].transform);
                return;
            }

            if (result == DropResult.Merged)
            {
                RemoveTileView(draggedTile);

                TileData targetData = boardData.GetTile(destination);
                if (targetData != null && tilesByPosition.TryGetValue(destination, out Tile targetTile) && targetTile != null)
                {
                    targetTile.SetSprite(targetData.Definition.GetSprite(targetData.EvolutionIndex));
                }

                return;
            }

            draggedTile.SnapBack();
        }
    }
}
