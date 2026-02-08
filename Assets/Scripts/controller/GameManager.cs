// Assets/Scripts/Controller/GameManager.cs
using UnityEngine;
using Game.Model;
using Game.View;

namespace Game.Controller
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private int columns = 5;
        [SerializeField] private int rows = 6;

        [SerializeField] private Board boardView;

        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private GameObject tilePrefab;

        [Header("Test")]
        [SerializeField] private TileDefinition normalItemDefinition;
        [SerializeField] private TileDefinition generatorDefinition;

        private BoardController boardController;

        private void Awake()
        {
            BoardData boardData = new BoardData(columns, rows);

            boardController = new BoardController(
                boardData,
                boardView,
                slotPrefab,
                tilePrefab
            );

            boardController.BuildBoard();

            if (normalItemDefinition != null)
            {
                boardController.SpawnTile(new Position(0, 0), normalItemDefinition, 0);
                boardController.SpawnTile(new Position(1, 0), normalItemDefinition, 0);
            }

            if (generatorDefinition != null)
            {
                boardController.SpawnTile(new Position(0, 1), generatorDefinition, 0);
            }
        }
    }
}