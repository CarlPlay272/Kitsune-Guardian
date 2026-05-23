using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHover : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [SerializeField] private GameObject hoverFrame;

    private Vector3 originalScale;

    private AudioSource audioSource;

    void Start()
    {
        hoverFrame.SetActive(false);

        originalScale = transform.localScale;

        audioSource = GetComponent<AudioSource>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hoverFrame.SetActive(true);

        transform.localScale =
            originalScale * 1.05f;

        if (audioSource != null)
            audioSource.Play();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hoverFrame.SetActive(false);

        transform.localScale = originalScale;
    }
}