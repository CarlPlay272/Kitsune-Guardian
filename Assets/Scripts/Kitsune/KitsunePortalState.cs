using System.Collections;
using UnityEngine;

public class KitsunePortalState : MonoBehaviour
{
    private bool puedeUsarPortal = true;

    public bool PuedeUsarPortal => puedeUsarPortal;

    public void BloquearPortales(float duracion)
    {
        StartCoroutine(BloquearPortalesRutina(duracion));
    }

    private IEnumerator BloquearPortalesRutina(float duracion)
    {
        puedeUsarPortal = false;
        yield return new WaitForSeconds(duracion);
        puedeUsarPortal = true;
    }
}