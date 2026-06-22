public interface IDamageable
{
    void TakeDamage(int amount);
}

public interface IInvisibilityProvider
{
    bool IsInvisible { get; }
}

public enum BossState
{
    Idle,
    Attacking,
    Summoning,
    WaitingClear,
    Dead
}

public enum BossPhase
{
    Phase1,
    Phase2
}