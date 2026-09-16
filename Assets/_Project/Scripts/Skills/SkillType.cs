namespace RoyalSmash.Skills
{
    /// <summary>
    /// Supported active player skills in Royal Smash.
    /// </summary>
    public enum SkillType
    {
        None = 0,
        MegaBall = 1,       // 2.0x diameter, increased mass, 1-shot impact booster (unlocks Level 5)
        InfinityBall = 2    // 5.0s cooldown override for rapid stream fire (unlocks Level 10)
    }
}
