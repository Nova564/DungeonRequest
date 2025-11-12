using UnityEngine;

public class PlayerEquipement : MonoBehaviour
{
    [Header("Références visuelles du joueur")]
    public GameObject epee;
    public GameObject hache;
    public GameObject bouclier;
    public GameObject bouclierPaladin;
    public GameObject lance;
    public GameObject marteau;

    void Awake()
    {
        DesactiverTout();
    }

    private void DesactiverTout()
    {
        // Désactive toutes les armes et armures au début
        if (epee != null) epee.SetActive(false);
        if (hache != null) hache.SetActive(false);
        if (bouclier != null) bouclier.SetActive(false);
        if (bouclierPaladin != null) bouclierPaladin.SetActive(false);
        if (lance != null) lance.SetActive(false);
        if (marteau != null) marteau.SetActive(false);
    }

    public void ActiverObjet(GameObject objetRamasse)
    {
        // Si l'objet ramassé est dans le layer "Arme" ou "Armure"
        if (objetRamasse.layer == LayerMask.NameToLayer("Arme"))
        {
            // Désactive toutes les armes
            DesactiverArmes();

            // Vérifie le tag pour activer le bon sprite d'arme
            switch (objetRamasse.tag)
            {
                case "epee":
                    if (epee != null) epee.SetActive(true);  // Active l'épée
                    break;
                case "hache":
                    if (hache != null) hache.SetActive(true); // Active la hache
                    break;
                case "lance":
                    if (lance != null) lance.SetActive(true); // Active la lance
                    break;
                case "marteau":
                    if (marteau != null) marteau.SetActive(true); // Active la lance
                    break;
            }
        }
        else if (objetRamasse.layer == LayerMask.NameToLayer("Armure"))
        {
            // Désactive toutes les armures
            DesactiverArmures();

            // Vérifie le tag pour activer le bon sprite de bouclier
            switch (objetRamasse.tag)
            {
                case "bouclier":
                    if (bouclier != null) bouclier.SetActive(true);  // Active le bouclier
                    break;
                case "bouclier paladin":
                    bouclierPaladin.SetActive(true);  // Active le bouclier paladin
                    break;
            }
        }
    }

    private void DesactiverArmes()
    {
        if (epee != null) epee.SetActive(false);
        if (hache != null) hache.SetActive(false);
        if (lance != null) lance.SetActive(false);
        if (marteau != null) marteau.SetActive(false);
    }

    private void DesactiverArmures()
    {
        if (bouclier != null) bouclier.SetActive(false);
        if (bouclierPaladin != null) bouclierPaladin.SetActive(false);
    }
}
