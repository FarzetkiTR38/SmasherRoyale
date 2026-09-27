using UnityEngine;

namespace RoyalSmash.Gameplay
{
    /// <summary>
    /// Attached to [LevelDesigner_WorkingRoot] to guarantee it is immediately deactivated
    /// and destroyed upon entering Play Mode, preventing any editor preview blocks from
    /// interfering with playable levels.
    /// </summary>
    [DefaultExecutionOrder(-100000)]
    public sealed class LevelDesignerWorkingRoot : MonoBehaviour
    {
        private void Awake()
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}
