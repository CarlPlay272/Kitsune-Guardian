using UnityEngine;

public class CheckpointSpiritual : MonoBehaviour
{
    [Header("Configuración de Secuencia")]
    [Tooltip("El número de este checkpoint en el orden del mapa (1 al 5)")]
    [SerializeField] private int checkpointID;

    [Header("Referencias Visuales")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    [Header("Configuración de Filtros (Colores)")]
    [SerializeField] private Color colorPlomo = new Color(0.3f, 0.3f, 0.3f, 1f); // Gris oscuro para inactivo/superado
    [SerializeField] private Color colorVanilla = new Color(1f, 1f, 1f, 1f);     // Blanco/Amarillo original

    private bool yaFueActivado = false;

    void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (animator == null)
            animator = GetComponent<Animator>();

        // Al iniciar la escena, aplicamos el filtro plomo de forma directa
        AplicarFiltro(colorPlomo);
    }

    void Update()
    {
        // Monitoreo constante del estado global en el GameController
        if (GameController.Instance == null) return;

        int idActualGlobal = GameController.Instance.CheckpointActualID;

        // Caso A: Si este es el checkpoint activo actual del jugador
        if (idActualGlobal == checkpointID)
        {
            if (!yaFueActivado)
            {
                yaFueActivado = true;
                ActivarVisualmente();
            }
        }
        // Caso B: Si el jugador ya avanzó a un checkpoint superior, este se apaga y vuelve a plomo
        else if (idActualGlobal > checkpointID)
        {
            if (yaFueActivado || (spriteRenderer != null && spriteRenderer.color != colorPlomo))
            {
                yaFueActivado = false;
                ApagarYVolverPlomo();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Evita lecturas si el personaje está muerto
        KitsuneHealth kitsune = other.GetComponentInParent<KitsuneHealth>();
        if (kitsune == null || kitsune.IsDead) return;

        if (GameController.Instance != null)
        {
            // Valida el orden secuencial estricto en el GameController
            bool exito = GameController.Instance.IntentarActivarCheckpoint(checkpointID, transform.position);

            if (exito)
            {
                Debug.Log("Criterio de secuencia aprobado para Checkpoint ID: " + checkpointID);
            }
        }
    }

    private void ActivarVisualmente()
    {
        AplicarFiltro(colorVanilla); // Quita el filtro plomo y lo devuelve a su color base original

        if (animator != null)
        {
            // Forzamos al Animator a reproducir el estado desde cero y lanzamos el trigger de destello
            animator.Play("Idle_Inactivo", 0, 0f);
            animator.SetTrigger("Activar");
        }
    }

    private void ApagarYVolverPlomo()
    {
        AplicarFiltro(colorPlomo); // Aplica el filtro gris oscuro definitivo

        if (animator != null)
        {
            // Forzamos al Animator a regresar a la animación quieta estática
            animator.Play("Idle_Inactivo", 0, 0f);
        }
    }

    private void AplicarFiltro(Color colorFiltro)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = colorFiltro;
        }
    }

    private void OnDrawGizmos()
    {
        // Muestra de manera visual el ID asignado encima del objeto en la pestaña Scene para facilitar el ordenamiento
        Gizmos.color = Color.yellow;
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1f, "CHK " + checkpointID);
#endif
    }
}