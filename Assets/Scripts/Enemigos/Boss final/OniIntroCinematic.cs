using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OniIntroCinematicController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform player;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private OniBoss oniBoss;

    [Header("Cámara")]
    [SerializeField] private float cameraMoveTime = 1.5f;

    [Header("UI Diálogo")]
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Tiempos")]
    [SerializeField] private float kitsuneDialogueTime = 3f;
    [SerializeField] private float oniDialogueTime = 3f;
    [SerializeField] private float pauseBetweenLines = 0.5f;

    [SerializeField] private float kitsuneLookTime = 1.0f;
    [SerializeField] private float oniLookTime = 2.0f;

    private KitsuneController kitsune;

    // FIX CAMERA: referencia al follow — arrastrarlo en el Inspector
    [SerializeField] private MainCameraFollow cameraFollow;

    // FIX 1: guard contra doble ejecución
    private bool _isPlaying = false;

    void Start()
    {
        if (player != null)
            kitsune = player.GetComponent<KitsuneController>();

        if (dialogueText != null)
            dialogueText.gameObject.SetActive(false);
        else
            Debug.LogError("❌ dialogueText NO asignado en el Inspector");
    }

    public void PlayCinematic()
    {
        // FIX 1: si la cinemática ya está corriendo, ignorar llamadas duplicadas
        if (_isPlaying) return;
        _isPlaying = true;

        StartCoroutine(CinematicRoutine());
    }

    private IEnumerator CinematicRoutine()
    {
        if (kitsune == null || mainCamera == null || oniBoss == null)
        {
            Debug.LogError("❌ Faltan referencias en OniIntroCinematicController");
            _isPlaying = false;
            yield break;
        }

        // 🔒 bloquear jugador
        kitsune.BloquearControles();

        // FIX CAMERA: desactivar el follow ANTES de mover la camara
        // Si esta activo, LateUpdate() pisa MoveCameraTo() cada frame
        if (cameraFollow != null) cameraFollow.enabled = false;

        // =========================
        // 1. KITSUNE
        // =========================
        yield return StartCoroutine(MoveCameraTo(player.position));

        // 👁️ tiempo para "respirar" la escena
        yield return new WaitForSeconds(1.2f);

        yield return StartCoroutine(ShowDialogue(
            "Kitsune: Esto no me gusta nada...",
            kitsuneDialogueTime
        ));

        yield return new WaitForSeconds(pauseBetweenLines);

        // =========================
        // 2. ONI
        // =========================
        yield return StartCoroutine(MoveCameraTo(oniBoss.transform.position));

        // 👁️ IMPORTANTE: tiempo de presencia del Oni
        yield return new WaitForSeconds(1.8f);

        yield return StartCoroutine(ShowDialogue(
            "Oni: Has llegado demasiado lejos...",
            oniDialogueTime
        ));

        yield return new WaitForSeconds(pauseBetweenLines);

        yield return StartCoroutine(ShowDialogue(
            "Oni: ¡Mis Tengus te detendrán!",
            oniDialogueTime
        ));

        yield return new WaitForSeconds(pauseBetweenLines);

        // 🔓 desbloquear jugador
        kitsune.DesbloquearControles();

        // FIX CAMERA: reactivar el follow al volver al combate normal
        if (cameraFollow != null) cameraFollow.enabled = true;

        // FIX: el orden correcto es:
        // 1. StartCombat() arranca el CombatLoop (que queda bloqueado esperando la señal)
        // 2. NotifyCinematicDone() da la señal → el combate arranca
        // Así NUNCA hay combate ni cámara duplicada durante la cinemática.
        oniBoss.StartCombat();
        oniBoss.NotifyCinematicDone();

        // FIX 1: liberar el guard al terminar
        _isPlaying = false;
    }

    private IEnumerator ShowDialogue(string text, float duration)
    {
        if (dialogueText == null) yield break;

        // FIX 2: activar toda la cadena de padres si alguno está inactivo
        Transform parent = dialogueText.transform.parent;
        while (parent != null)
        {
            if (!parent.gameObject.activeSelf)
                parent.gameObject.SetActive(true);
            parent = parent.parent;
        }

        dialogueText.gameObject.SetActive(true);
        dialogueText.transform.SetAsLastSibling();
        dialogueText.text = text;

        // FIX 3: forzar rebuild del Canvas en el mismo frame
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(dialogueText.rectTransform);

        yield return new WaitForSeconds(duration);

        dialogueText.gameObject.SetActive(false);
    }

    private IEnumerator MoveCameraTo(Vector3 target)
    {
        if (mainCamera == null) yield break;

        Vector3 startPos = mainCamera.transform.position;
        Vector3 endPos = new Vector3(target.x, target.y, startPos.z);

        float distance = Vector3.Distance(startPos, endPos);

        // 🔥 velocidad real de cámara (ajustable)
        float speed = 3.5f;

        float duration = Mathf.Clamp(distance / speed, 1.2f, 4.5f);

        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;

            mainCamera.transform.position = Vector3.Lerp(
                startPos,
                endPos,
                t / duration
            );

            yield return null;
        }

        mainCamera.transform.position = endPos;
    }
}