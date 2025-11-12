using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Référence au joueur")]
    public GameObject player;

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 newPosition = new Vector3(
            player.transform.position.x,
            player.transform.position.y,
            transform.position.z 
        );

        transform.position = newPosition;
    }
}
