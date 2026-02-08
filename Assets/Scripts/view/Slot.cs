using UnityEngine;
using UnityEngine.UI;
using Game.Model;

namespace Game.View
{
    public class Slot : MonoBehaviour
    {
        public Position Position { get; private set; }

        [SerializeField] private Image backgroundImage;

        public void Initialize(Position position, Color backgroundColor)
        {
            Position = position;

            if (backgroundImage != null)
            {
                backgroundImage.color = backgroundColor;
            }
        }
    }
}