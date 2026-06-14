using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI")]
    [SerializeField] private GameObject dialogueBubble;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Configuración")]
    [SerializeField] private float typingSpeed = 0.03f;
    [SerializeField] private KeyCode advanceKey = KeyCode.Space;

    private readonly Queue<string> lines = new();

    private bool isTyping;
    private bool dialogueActive;

    public bool IsDialogueActive => dialogueActive;

    private Coroutine typingCoroutine;

    private string currentLine;

    private KitsuneController kitsuneController;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        dialogueBubble.SetActive(false);
    }

    private void Update()
    {
        if (!dialogueActive)
            return;

        if (Input.GetKeyDown(advanceKey))
        {
            if (isTyping)
            {
                FinishCurrentLine();
            }
            else
            {
                DisplayNextLine();
            }
        }
    }
    public void StartDialogue(DialogueData data)
    {
        Debug.Log("StartDialogue llamado");
        Debug.Log("DialogueData recibido: " + data);

        if (data == null)
        {
            Debug.LogWarning("Se intentó iniciar un diálogo sin DialogueData.");
            return;
        }

        lines.Clear();

        foreach (var line in data.lines)
        {
            if (!string.IsNullOrWhiteSpace(line.text))
            {
                lines.Enqueue(line.text);
            }
        }

        if (lines.Count == 0)
        {
            Debug.LogWarning("El diálogo no contiene líneas.");
            return;
        }

        if (kitsuneController == null)
        {
            kitsuneController = FindFirstObjectByType<KitsuneController>();
        }

        if (kitsuneController != null)
        {
            kitsuneController.BloquearControles();
        }

        dialogueActive = true;

        dialogueBubble.SetActive(true);

        DisplayNextLine();
    }

    private void DisplayNextLine()
    {
        if (lines.Count == 0)
        {
            EndDialogue();
            return;
        }

        currentLine = lines.Dequeue();

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeLine(currentLine));
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;

        dialogueText.text = "";

        foreach (char letter in line)
        {
            dialogueText.text += letter;

            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        typingCoroutine = null;
    }

    private void FinishCurrentLine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        dialogueText.text = currentLine;

        isTyping = false;
    }

    private void EndDialogue()
    {
        dialogueActive = false;

        dialogueBubble.SetActive(false);

        if (kitsuneController != null)
        {
            kitsuneController.DesbloquearControles();
        }
    }
}