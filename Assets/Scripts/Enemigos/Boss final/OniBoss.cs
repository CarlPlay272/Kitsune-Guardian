using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class OniBoss : MonoBehaviour, IDamageable
{
    [Header("Vida")]
    [SerializeField] private int maxHealth = 50;
    private int currentHealth;

    [Header("Referencias")]
    [SerializeField] private Transform player;
    [SerializeField] private GameEndScreen gameEndScreen;
    [SerializeField] private Animator animator;
    [SerializeField] private Collider2D damageCollider;
    [SerializeField] private BossHealthBar healthBar;

    [Header("Invocación")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameObject tenguPrefab;
    [SerializeField] private float delayBetweenTengus = 1.2f;

    [Header("Ataques Básicos")]
    [SerializeField] private GameObject corruptProjectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private int burstProjectileCount = 3;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float projectileSpacing = 0.3f;

    [Header("Ataques Especiales (Fase 2)")]
    [SerializeField] private GameObject homingOrbPrefab;
    [SerializeField] private GameObject waveAttackPrefab;

    [Header("Cooldowns")]
    [SerializeField] private float attackCooldownPhase1 = 3f;
    [SerializeField] private float attackCooldownPhase2 = 1.8f;
    [SerializeField] private float summonCooldownPhase1 = 12f;
    [SerializeField] private float summonCooldownPhase2 = 18f;

    [Header("Eventos")]
    [SerializeField] private UnityEvent onBossDeath;
    [SerializeField] private UnityEvent onCombatStart;

    // FIX: mainCamera y camSpeed eliminados — la cámara la maneja
    // exclusivamente OniIntroCinematicController. OniBoss no toca la cámara.

    private BossState currentState = BossState.Idle;
    private BossPhase currentPhase = BossPhase.Phase1;

    private int enemiesAlive = 0;
    private bool combatActive;
    private Coroutine combatLoopRoutine;

    private List<GameObject> misTengusInvocados = new List<GameObject>();
    private bool yaInvoqueEnEsteCiclo = false;

    // FIX: flag que indica si la cinemática externa ya terminó
    private bool cinematicaDone = false;

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
        yaInvoqueEnEsteCiclo = false;
        cinematicaDone = false;
        misTengusInvocados.Clear();
        currentState = BossState.Idle;

        if (BossOniHUDController.Instance != null)
        {
            BossOniHUDController.Instance.ConfigurarArena(this);
            BossOniHUDController.Instance.ActualizarVidaOni(1f);
            BossOniHUDController.Instance.CambiarEstadoPresenciaJugador(true);
        }

        onCombatStart?.Invoke();

        // FIX: NO lanzar OniIntroCinematic() desde aquí.
        // La cinemática la maneja OniIntroCinematicController.
        // Cuando termina, llama NotifyCinematicDone() y recién ahí arranca el combate.

        if (combatLoopRoutine != null)
            StopCoroutine(combatLoopRoutine);

        combatLoopRoutine = StartCoroutine(CombatLoop());
    }

    // FIX: OniIntroCinematicController llama esto al final de su secuencia,
    // en lugar de llamar StartCombat(). Así el CombatLoop espera la señal.
    public void NotifyCinematicDone()
    {
        cinematicaDone = true;
    }

    private IEnumerator CombatLoop()
    {
        // FIX: esperar a que OniIntroCinematicController termine su cinemática
        // antes de hacer cualquier cosa de combate
        yield return new WaitUntil(() => cinematicaDone);

        yield return StartCoroutine(SummonTengus());
        float lastSummonTime = Time.time;

        while (currentState != BossState.Dead)
        {
            currentPhase = (currentHealth <= maxHealth * 0.5f) ? BossPhase.Phase2 : BossPhase.Phase1;

            float summonCooldown = currentPhase == BossPhase.Phase1 ? summonCooldownPhase1 : summonCooldownPhase2;
            float attackCooldown = currentPhase == BossPhase.Phase1 ? attackCooldownPhase1 : attackCooldownPhase2;

            if (!QuedanMisTengusVivos())
            {
                enemiesAlive = 0;
            }

            if (enemiesAlive > 0)
            {
                currentState = BossState.WaitingClear;
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            currentState = BossState.Idle;

            bool canSummon = (Time.time - lastSummonTime >= summonCooldown) && !yaInvoqueEnEsteCiclo;

            if (canSummon && Random.value < 0.15f)
            {
                lastSummonTime = Time.time;
                yaInvoqueEnEsteCiclo = true;
                yield return StartCoroutine(SummonTengus());
            }
            else
            {
                yaInvoqueEnEsteCiclo = false;
                yield return StartCoroutine(ExecuteRandomAttack());
            }

            yield return new WaitForSeconds(attackCooldown);
        }
    }

    private IEnumerator SummonTengus()
    {
        currentState = BossState.Summoning;

        int amount = Random.Range(2, 4);
        enemiesAlive = 0;

        for (int i = 0; i < amount; i++)
        {
            if (spawnPoints.Length == 0) break;
            Transform spawn = spawnPoints[i % spawnPoints.Length];

            GameObject enemy = Instantiate(tenguPrefab, spawn.position, spawn.rotation);
            enemiesAlive++;
            misTengusInvocados.Add(enemy);

            TenguState tState = enemy.GetComponent<TenguState>();
            if (tState != null && BossOniHUDController.Instance != null)
            {
                BossOniHUDController.Instance.RegistrarTengu(tState);
            }

            TenguDeathHook hook = enemy.GetComponent<TenguDeathHook>();
            if (hook != null) hook.Init(this);

            yield return new WaitForSeconds(delayBetweenTengus);
        }
    }

    public void NotifyEnemyDeath()
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
    }

    private bool QuedanMisTengusVivos()
    {
        misTengusInvocados.RemoveAll(t => t == null);
        foreach (var go in misTengusInvocados)
        {
            TenguState state = go.GetComponent<TenguState>();
            if (state != null && !state.IsDead) return true;
        }
        return false;
    }

    private IEnumerator ExecuteRandomAttack()
    {
        currentState = BossState.Attacking;

        int attackRangeMax = (currentPhase == BossPhase.Phase2) ? 3 : 2;
        int attack = Random.Range(0, attackRangeMax);

        if (attack == 0)
            yield return StartCoroutine(AttackCorruptBurst());
        else if (attack == 1)
            yield return StartCoroutine(AttackVoidMark());
        else
            yield return StartCoroutine(AttackBossWave());

        currentState = BossState.Idle;
    }

    private IEnumerator AttackCorruptBurst()
    {
        Transform origin = projectileSpawnPoint != null ? projectileSpawnPoint : transform;

        for (int i = 0; i < burstProjectileCount; i++)
        {
            if (corruptProjectilePrefab == null) break;
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
        Transform origin = projectileSpawnPoint != null ? projectileSpawnPoint : transform;
        if (homingOrbPrefab != null && player != null)
        {
            GameObject orb = Instantiate(homingOrbPrefab, origin.position, Quaternion.identity);
            var script = orb.GetComponent<BossHomingOrb>();
            var invProvider = player.GetComponent<IInvisibilityProvider>();
            if (script != null)
                script.Initialize(player, invProvider);
        }
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator AttackBossWave()
    {
        Transform origin = projectileSpawnPoint != null ? projectileSpawnPoint : transform;
        if (waveAttackPrefab != null)
        {
            Instantiate(
                waveAttackPrefab,
                new Vector3(origin.position.x, origin.position.y - 1.8f, origin.position.z),
                Quaternion.identity
            );
        }
        yield return new WaitForSeconds(0.8f);
    }

    public void TakeDamage(int amount)
    {
        if (currentState == BossState.Dead) return;

        if (enemiesAlive > 0 || QuedanMisTengusVivos())
        {
            Debug.Log("🛡️ ¡El Oni está protegido por el escudo místico de los Tengus!");
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - amount);

        if (BossOniHUDController.Instance != null)
            BossOniHUDController.Instance.ActualizarVidaOni((float)currentHealth / maxHealth);

        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        currentState = BossState.Dead;
        combatActive = false;
        StopAllCoroutines(); // para el combate

        if (BossOniHUDController.Instance != null)
            BossOniHUDController.Instance.CambiarEstadoPresenciaJugador(false);

        if (damageCollider != null) damageCollider.enabled = false;
        animator?.SetTrigger("Dead");
        onBossDeath?.Invoke();

        // FIX: GameEndScreen maneja su propio delay con WaitForSecondsRealtime.
        // No usar coroutine aquí porque StopAllCoroutines() ya fue llamado arriba.
        if (gameEndScreen != null)
            gameEndScreen.ShowEndScreen();
        else
            Debug.LogWarning("⚠️ GameEndScreen no asignado en OniBoss");

        Destroy(gameObject, 1.5f);
    }

    public bool TieneTengusActivos()
    {
        return enemiesAlive > 0 || QuedanMisTengusVivos();
    }
    // FIX: OniIntroCinematic y MoveCameraTo eliminados de OniBoss.
    // La cámara la controla exclusivamente OniIntroCinematicController.
    // Tener dos scripts moviendo mainCamera.transform.position en paralelo
    // era la causa del "rebote" de cámara.
}