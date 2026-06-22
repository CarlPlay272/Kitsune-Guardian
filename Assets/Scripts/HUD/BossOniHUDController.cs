using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossOniHUDController : MonoBehaviour
{
    public static BossOniHUDController Instance { get; private set; }

    [Header("Referencias Barra Principal (Oni)")]
    [SerializeField] private GameObject oniContainer;
    [SerializeField] private Image oniFillImage;
    [SerializeField] private CanvasGroup oniCanvasGroup;
    [SerializeField] private float radioDeVisionOni = 60f;

    [Header("Referencias Mini-Barras (Tengus)")]
    [SerializeField] private CanvasGroup[] tenguCanvasGroups;
    [SerializeField] private Image[] tenguFillImages;

    [Header("Configuración Visual")]
    [SerializeField] private float speedFade = 4f;

    private Transform playerTransform;
    private OniBoss activeOni;
    private bool kitsuneHaVistoAlOni = false;
    private bool jugadorEnArena = false;

    private List<TenguState> tengusActivos = new List<TenguState>();

    private void Awake()
    {
        Instance = this;
        InitializeUI();
    }

    private void InitializeUI()
    {
        if (oniContainer != null)
        {
            if (oniContainer == gameObject)
            {
                Debug.LogError("❌ [ERROR] Asignaste el HUD completo en 'Oni Container'.");
            }
            else
            {
                oniContainer.SetActive(false);
            }
        }

        if (oniCanvasGroup != null) oniCanvasGroup.alpha = 0f;

        foreach (var cg in tenguCanvasGroups)
        {
            if (cg != null) cg.alpha = 0f;
        }
    }

    public void ConfigurarArena(OniBoss boss)
    {
        activeOni = boss;
        kitsuneHaVistoAlOni = false;
        tengusActivos.Clear();
    }

    public void CambiarEstadoPresenciaJugador(bool estaEnArena)
    {
        jugadorEnArena = estaEnArena;
        if (!estaEnArena)
        {
            if (oniCanvasGroup != null) oniCanvasGroup.alpha = 0f;
            if (oniContainer != null && oniContainer != gameObject) oniContainer.SetActive(false);
            foreach (var cg in tenguCanvasGroups) if (cg != null) cg.alpha = 0f;
        }
    }

    public void RegistrarTengu(TenguState nuevoTengu)
    {
        tengusActivos.RemoveAll(t => t == null || t.IsDead);

        if (tengusActivos.Count < 4 && !tengusActivos.Contains(nuevoTengu))
        {
            tengusActivos.Add(nuevoTengu);
        }
    }

    void Update()
    {
        BuscarJugador();
        ControlarLogicaBarras();
    }

    private void BuscarJugador()
    {
        if (playerTransform == null && GameController.Instance != null && GameController.Instance.Player != null)
        {
            playerTransform = GameController.Instance.Player.transform;
        }
    }

    private void ControlarLogicaBarras()
    {
        if (activeOni == null || playerTransform == null || !jugadorEnArena) return;

        float distanciaAlOni = Vector2.Distance(activeOni.transform.position, playerTransform.position);

        if (!kitsuneHaVistoAlOni && distanciaAlOni <= radioDeVisionOni)
        {
            kitsuneHaVistoAlOni = true;
            if (oniContainer != null && oniContainer != gameObject) oniContainer.SetActive(true);
        }

        tengusActivos.RemoveAll(t => t == null || t.IsDead);

        for (int i = 0; i < 4; i++)
        {
            if (i < tengusActivos.Count)
            {
                if (tenguCanvasGroups[i] != null)
                    tenguCanvasGroups[i].alpha = Mathf.MoveTowards(tenguCanvasGroups[i].alpha, 1f, Time.deltaTime * speedFade);

                if (tenguFillImages[i] != null)
                    tenguFillImages[i].fillAmount = tengusActivos[i].ObtenerPorcentajeVida();
            }
            else
            {
                if (tenguCanvasGroups[i] != null)
                    tenguCanvasGroups[i].alpha = Mathf.MoveTowards(tenguCanvasGroups[i].alpha, 0f, Time.deltaTime * speedFade);
            }
        }

        if (kitsuneHaVistoAlOni && oniCanvasGroup != null)
        {
            float targetAlphaOni = (tengusActivos.Count > 0) ? 0.3f : 1f;
            oniCanvasGroup.alpha = Mathf.MoveTowards(oniCanvasGroup.alpha, targetAlphaOni, Time.deltaTime * speedFade);

            if (oniContainer != null && !oniContainer.activeSelf)
            {
                oniContainer.SetActive(true);
            }
        }
    }

    public void ActualizarVidaOni(float porcentaje)
    {
        if (oniFillImage != null) oniFillImage.fillAmount = porcentaje;
    }
}