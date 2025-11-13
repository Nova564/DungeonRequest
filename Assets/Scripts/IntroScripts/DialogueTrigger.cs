using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueData _dialogueData;
    [SerializeField] private bool _triggerOnce = true;

    private bool _hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && (!_triggerOnce || !_hasTriggered))
        {
            if (_dialogueData != null && _dialogueData.lines.Length > 0)
            {
                SimpleDialogueUI.Instance.StartDialogue(_dialogueData.lines);
                _hasTriggered = true;
            }
        }
    }
}