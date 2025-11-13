using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTeleporter : MonoBehaviour
{
    [SerializeField] private string _targetSceneName;
    [SerializeField] private float _teleportDelay = 0.5f;
    [SerializeField] private bool _showDialogueBeforeTeleport = false;
    [SerializeField] private DialogueData _teleportDialogue;

    private bool _isTeleporting = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !_isTeleporting)
        {
            _isTeleporting = true;

            if (_showDialogueBeforeTeleport && _teleportDialogue != null)
            {
                SimpleDialogueUI.Instance.StartDialogue(_teleportDialogue.lines);
                StartCoroutine(WaitForDialogueAndTeleport());
            }
            else
            {
                StartCoroutine(TeleportAfterDelay());
            }
        }
    }

    private System.Collections.IEnumerator WaitForDialogueAndTeleport()
    {
        while (SimpleDialogueUI.Instance.IsDialogueActive())
        {
            yield return null;
        }

        yield return new WaitForSeconds(_teleportDelay);
        Teleport();
    }

    private System.Collections.IEnumerator TeleportAfterDelay()
    {
        yield return new WaitForSeconds(_teleportDelay);
        Teleport();
    }

    private void Teleport()
    {
        if (!string.IsNullOrEmpty(_targetSceneName))
        {
            SceneManager.LoadScene(_targetSceneName);
        }
        else
        {
            Debug.LogWarning("[SceneTeleporter] pas de nom de scène mis");
        }
    }
}