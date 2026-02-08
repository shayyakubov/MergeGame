using UnityEngine;

namespace Game.Model
{
    [CreateAssetMenu(menuName = "Merge/TileDefinition")]
    public class TileDefinition : ScriptableObject
    {
        [Header("Visuals (this tile's own evolution chain)")]
        [SerializeField] private Sprite[] evolutions;

        [Header("Generator (optional)")]
        [SerializeField] private bool isGenerator;
        [SerializeField] private TileDefinition generatedOutputDefinition;

        public bool IsGenerator
        {
            get { return isGenerator; }
        }

        // This is NOT related to this tile's own evolutions. It's the output type it spawns.
        public TileDefinition GeneratedOutputDefinition
        {
            get { return generatedOutputDefinition; }
        }

        public int EvolutionCount
        {
            get { return evolutions != null ? evolutions.Length : 0; }
        }

        public Sprite GetSprite(int evolutionIndex)
        {
            if (evolutions == null)
            {
                return null;
            }

            if (evolutionIndex < 0 || evolutionIndex >= evolutions.Length)
            {
                return null;
            }

            return evolutions[evolutionIndex];
        }

        public bool IsMaxEvolution(int evolutionIndex)
        {
            return evolutions != null && evolutions.Length > 0 && evolutionIndex >= evolutions.Length - 1;
        }
    }
}