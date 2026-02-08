using UnityEngine;
using UnityEngine.UI;

namespace Game.View
{
    public class Board : MonoBehaviour
    {
        public RectTransform BoardParent => boardParent;
        public GridLayoutGroup Grid => gridLayoutGroup;
        public RectTransform DragLayer => dragLayer;
        
        [SerializeField] private RectTransform boardParent;
        [SerializeField] private GridLayoutGroup gridLayoutGroup;
        [SerializeField] private RectTransform dragLayer;

    }
}