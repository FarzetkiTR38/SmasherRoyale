using UnityEngine;

namespace RoyalSmash.Gameplay
{
    /// <summary>Editor composition preview. LevelLoader owns the playable instances.</summary>
    [DefaultExecutionOrder(-10000)]
    public sealed class SkyCitadelPreview : MonoBehaviour
    {
        private void Awake()
        {
            // Disable before any preview target can register itself with LevelManager.
            gameObject.SetActive(false);
        }
    }
}
