using UnityEngine;
using UnityEngine.UI;

namespace Game.View
{
    public class Board : MonoBehaviour
    {
        [SerializeField] private RectTransform boardParent;
        [SerializeField] private GridLayoutGroup gridLayoutGroup;
        [SerializeField] private RectTransform dragLayer;

        public RectTransform BoardParent
        {
            get { return boardParent; }
        }

        public GridLayoutGroup Grid
        {
            get { return gridLayoutGroup; }
        }

        public RectTransform DragLayer
        {
            get { return dragLayer; }
        }
    }
}