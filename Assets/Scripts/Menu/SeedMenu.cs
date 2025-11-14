using Components.ProceduralGeneration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SeedMenu : MonoBehaviour
{
    [SerializeField] private TMP_InputField seedInputField;
    [SerializeField] private Button playButton;
    [SerializeField] private ProceduralGridGenerator gridGenerator;
    [SerializeField] private GameObject menuPanel;

    [Header("UI Gameplay")]
    [SerializeField] private GameObject healthBar;

    private Vector3 originalScale;

    void Start()
    {
        originalScale = playButton.transform.localScale;
        playButton.onClick.AddListener(OnPlayClicked);

        if (healthBar != null)
            healthBar.SetActive(false);

        var eventTrigger = playButton.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

        var pointerEnter = new UnityEngine.EventSystems.EventTrigger.Entry
        {
            eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter
        };
        pointerEnter.callback.AddListener((_) => playButton.transform.localScale = originalScale * 1.2f);

        var pointerExit = new UnityEngine.EventSystems.EventTrigger.Entry
        {
            eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit
        };
        pointerExit.callback.AddListener((_) => playButton.transform.localScale = originalScale);

        eventTrigger.triggers.Add(pointerEnter);
        eventTrigger.triggers.Add(pointerExit);
    }

    private void OnPlayClicked()
    {
        int seed = gridGenerator.GetDefaultSeed();
        if (!string.IsNullOrEmpty(seedInputField.text) && int.TryParse(seedInputField.text, out int inputSeed))
        {
            seed = inputSeed;
        }

        gridGenerator.SetSeedAndGenerate(seed);

        if (healthBar != null)
            healthBar.SetActive(true);

        if (menuPanel != null)
            menuPanel.SetActive(false);
        else
            gameObject.SetActive(false);
    }
}