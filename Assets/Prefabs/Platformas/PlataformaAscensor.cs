using System.Collections;
using UnityEngine;

public class PlataformaAscensor : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float distanciaMovimiento = 5f;
    [SerializeField] private float velocidad = 2f;

    [Header("Dirección Inicial")]
    [SerializeField] private bool iniciarSubiendo = true;

    [Header("Pausas")]
    [SerializeField] private float esperaArriba = 1f;
    [SerializeField] private float esperaAbajo = 1f;

    private Vector3 posicionInicial;
    private Vector3 posicionSuperior;
    private Vector3 posicionInferior;

    private void Start()
    {
        posicionInicial = transform.position;

        if (iniciarSubiendo)
        {
            posicionInferior = posicionInicial;
            posicionSuperior =
                posicionInicial + Vector3.up * distanciaMovimiento;

            StartCoroutine(MoverAscensor(
                posicionInferior,
                posicionSuperior
            ));
        }
        else
        {
            posicionSuperior = posicionInicial;
            posicionInferior =
                posicionInicial + Vector3.down * distanciaMovimiento;

            StartCoroutine(MoverAscensor(
                posicionSuperior,
                posicionInferior
            ));
        }
    }

    private IEnumerator MoverAscensor(
        Vector3 puntoA,
        Vector3 puntoB
    )
    {
        while (true)
        {
            yield return MoverHacia(
                puntoA,
                puntoB
            );

            yield return new WaitForSeconds(
                esperaArriba
            );

            yield return MoverHacia(
                puntoB,
                puntoA
            );

            yield return new WaitForSeconds(
                esperaAbajo
            );
        }
    }

    private IEnumerator MoverHacia(
        Vector3 origen,
        Vector3 destino
    )
    {
        while (
            Vector3.Distance(
                transform.position,
                destino
            ) > 0.01f
        )
        {
            transform.position =
                Vector3.MoveTowards(
                    transform.position,
                    destino,
                    velocidad * Time.deltaTime
                );

            yield return null;
        }

        transform.position = destino;
    }
}