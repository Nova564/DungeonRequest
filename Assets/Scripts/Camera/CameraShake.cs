using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeStrength = 0.2f;
    [SerializeField] private AnimationCurve shakeCurve;

    [Header("Follow Settings")]
    [SerializeField] private Transform target; // Le joueur
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);

    private bool isShaking = false;
    private Vector3 shakeOffset = Vector3.zero;

    void LateUpdate()
    {
        if (target == null)
            return;

        // Position de suivi du joueur
        Vector3 desiredPosition = target.position + offset;

        // Ajouter l'offset de shake
        Vector3 finalPosition = desiredPosition + shakeOffset;

        // Smooth suivie
        transform.position = Vector3.Lerp(transform.position, finalPosition, smoothSpeed * Time.deltaTime);
    }

    void Update()
    {
        // Test manuel
        if (Input.GetKeyDown(KeyCode.H))
        {
            TriggerShake();
        }
    }

    public void TriggerShake()
    {
        StartCoroutine(ShakeCoroutine());
    }

    public void TriggerShake(float duration, float strength)
    {
        shakeDuration = duration;
        shakeStrength = strength;
        StartCoroutine(ShakeCoroutine());
    }

    private IEnumerator ShakeCoroutine()
    {
        if (isShaking)
            isShaking = false;
            shakeOffset = Vector3.zero;
    
        isShaking = true;
        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            // Calculer l'intensité du shake selon la courbe
            float strength = shakeStrength * shakeCurve.Evaluate(elapsedTime / shakeDuration);

            // Générer un offset aléatoire
            shakeOffset = Random.insideUnitSphere * strength;
            shakeOffset.z = 0; // Garder la profondeur fixe pour une caméra 2D

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Réinitialiser l'offset
        shakeOffset = Vector3.zero;
        isShaking = false;
    }
}