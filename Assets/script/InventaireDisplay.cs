using UnityEngine;

public class InventaireDisplay : MonoBehaviour
{
    public DicoItem dico;

    void Update()
    {
        // Pour tester : appuie sur "I" pour afficher les objets ramassés
        if (Input.GetKeyDown(KeyCode.I))
        {
            var objetsRamasses = dico.ObtenirObjetsRamasses();

            Debug.Log("Objets ramassés par le joueur :");

            foreach (var obj in objetsRamasses)
            {
                Debug.Log($"- {obj.name} ({obj.type})");
            }

            if (objetsRamasses.Count == 0)
                Debug.Log("Aucun objet ramassé pour le moment !");
        }
    }
}
