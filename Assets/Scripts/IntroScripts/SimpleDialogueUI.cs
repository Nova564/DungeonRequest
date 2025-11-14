using UnityEngine;
using TMPro;

public class SimpleDialogueUI : MonoBehaviour
{
    public static SimpleDialogueUI Instance { get; private set; }

    [SerializeField] private GameObject _dialoguePanel;
    [SerializeField] private TextMeshProUGUI _dialogueText;
    [SerializeField] private TextMeshProUGUI _continueHint;

    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _dialogueSound;

    private string[] _currentLines;
    private int _currentLineIndex;
    private bool _isDialogueActive;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        _dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        if (_isDialogueActive && Input.GetKeyDown(KeyCode.E))
        {
            if (_currentLineIndex < _currentLines.Length - 1)
            {
                PlayDialogueSound();
            }
            ShowNextLine();
        }
    }

    public void StartDialogue(string[] lines)
    {
        _currentLines = lines;
        _currentLineIndex = 0;
        _isDialogueActive = true;
        _dialoguePanel.SetActive(true);

        PlayDialogueSound();
        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        if (_currentLineIndex < _currentLines.Length)
        {
            _dialogueText.text = _currentLines[_currentLineIndex];
            _continueHint.gameObject.SetActive(true);
        }
    }

    private void ShowNextLine()
    {
        _currentLineIndex++;

        if (_currentLineIndex < _currentLines.Length)
        {
            ShowCurrentLine();
        }
        else
        {
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        _isDialogueActive = false;
        _dialoguePanel.SetActive(false);
        _currentLines = null;
        _currentLineIndex = 0;

        if (_audioSource != null && _audioSource.isPlaying)
            _audioSource.Stop();
    }
    public bool IsDialogueActive() => _isDialogueActive;

    private void PlayDialogueSound()
    {
        if (_audioSource != null && _dialogueSound != null)
        {
            if (_audioSource.isPlaying)
                _audioSource.Stop();

            _audioSource.clip = _dialogueSound;
            _audioSource.Play();
        }
    }
}