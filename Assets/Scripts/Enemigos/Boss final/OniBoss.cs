using System.Collections;
using UnityEngine;
using UnityEngine.Events;

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

[RequireComponent(typeof(Collider2D))]
public class OniBoss : MonoBehaviour, IDamageable
{
    [Header("Vida")]
    [SerializeField] private int maxHealth = 50;
    private int currentHealth;

    [Header("Referencias")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator animator;
    [SerializeField] private Collider2D damageCollider;
    [SerializeField] private BossHealthBar healthBar;

    [Header("Invocación")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameObject tenguPrefab;
    [SerializeField] private float delayBetweenTengus = 0.2f;

    [Header("Ataques")]
    [SerializeField] private GameObject corruptProjectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private int burstProjectileCount = 3;
    [SerializeField] private float projectileSpeed = 6f;
    [SerializeField] private float projectileSpacing = 0.3f;

    [Header("Cooldowns")]
    [SerializeField] private float attackCooldownPhase1 = 3f;
    [SerializeField] private float attackCooldownPhase2 = 1.5f;
    [SerializeField] private float summonCooldownPhase1 = 8f;
    [SerializeField] private float summonCooldownPhase2 = 4f;

    [Header("Eventos")]
    [SerializeField] private UnityEvent onBossDeath;
    [SerializeField] private UnityEvent onCombatStart;

    private BossState currentState = BossState.Idle;
    private BossPhase currentPhase = BossPhase.Phase1;

    private int enemiesAlive = 0;
    private bool combatActive;
    private Coroutine combatLoopRoutine;

    private void Awake()
    {
        currentHealth = maxHealth;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        Debug.Log("👹 ONI READY");
    }

    public void StartCombat()
    {
        if (combatActive) return;

        combatActive = true;
        currentHealth = maxHealth;

        enemiesAlive = 0;
        currentState = BossState.Idle;

        onCombatStart?.Invoke();

        if (combatLoopRoutine != null)
            StopCoroutine(combatLoopRoutine);

        combatLoopRoutine = StartCoroutine(CombatLoop());
    }

    private IEnumerator CombatLoop()
    {
        float lastSummonTime = -999f;

        while (currentState != BossState.Dead)
        {
            currentPhase = (currentHealth <= maxHealth * 0.5f)
                ? BossPhase.Phase2
                : BossPhase.Phase1;

            float summonCooldown = currentPhase == BossPhase.Phase1
                ? summonCooldownPhase1
                : summonCooldownPhase2;

            float attackCooldown = currentPhase == BossPhase.Phase1
                ? attackCooldownPhase1
                : attackCooldownPhase2;

            // 🔥 PRIORIDAD: si hay enemigos vivos, esperar
            if (enemiesAlive > 0)
            {
                currentState = BossState.WaitingClear;
                yield return new WaitForSeconds(1f);
                continue;
            }

            currentState = BossState.Idle;

            bool canSummon = Time.time - lastSummonTime >= summonCooldown;

            if (canSummon && Random.value < 0.5f)
            {
                lastSummonTime = Time.time;
                yield return StartCoroutine(SummonTengus());
            }
            else
            {
                yield return StartCoroutine(ExecuteRandomAttack());
            }

            yield return new WaitForSeconds(attackCooldown);
        }
    }

    private IEnumerator SummonTengus()
    {
        currentState = BossState.Summoning;

        int amount = Random.Range(2, 5);

        enemiesAlive += amount;

        for (int i = 0; i < amount; i++)
        {
            Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];

            GameObject enemy = Instantiate(tenguPrefab, spawn.position, spawn.rotation);

            // 🔥 IMPORTANTE: conectar muerte desde Tengu
            TenguDeathHook hook = enemy.GetComponent<TenguDeathHook>();
            if (hook != null)
                hook.Init(this);

            yield return new WaitForSeconds(delayBetweenTengus);
        }
    }

    public void NotifyEnemyDeath()
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
    }

    private IEnumerator ExecuteRandomAttack()
    {
        currentState = BossState.Attacking;

        int attack = Random.Range(0, 2);

        if (attack == 0)
            yield return AttackCorruptBurst();
        else
            yield return AttackVoidMark();

        currentState = BossState.Idle;
    }

    private IEnumerator AttackCorruptBurst()
    {
        Transform origin = projectileSpawnPoint != null ? projectileSpawnPoint : transform;

        for (int i = 0; i < burstProjectileCount; i++)
        {
            GameObject proj = Instantiate(corruptProjectilePrefab, origin.position, Quaternion.identity);

            var script = proj.GetComponent<BossProjectile>();
            if (script != null && player != null)
            {
                Vector2 dir = (player.position - origin.position).normalized;
                script.Initialize(dir, projectileSpeed);
            }

            yield return new WaitForSeconds(projectileSpacing);
        }
    }

    private IEnumerator AttackVoidMark()
    {
        yield return null;
    }

    public void TakeDamage(int amount)
    {
        if (currentState == BossState.Dead) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        currentState = BossState.Dead;
        combatActive = false;

        StopAllCoroutines();

        damageCollider.enabled = false;

        animator?.SetTrigger("Dead");

        onBossDeath?.Invoke();
    }
}