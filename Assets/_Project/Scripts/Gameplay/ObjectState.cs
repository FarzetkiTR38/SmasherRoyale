namespace RoyalSmash.Gameplay
{
    /// <summary>
    /// Life-cycle states for target physics puzzle objects on the stand.
    /// Follows the state flow: IDLE -> HIT -> MOVING -> DESTROY_CONDITION -> DESTROYING -> DESTROYED
    /// </summary>
    public enum ObjectState
    {
        Idle,
        Hit,
        Moving,
        DestroyCondition,
        Destroying,
        Destroyed
    }

    /// <summary>
    /// Types of physics objects available in the MVP.
    /// </summary>
    public enum ObjectType
    {
        Box,
        Stone,
        Column,
        Cone,
        PaintBottle
    }
}
