using UnityEngine;

public class PlayerEquipement : MonoBehaviour
{
    [Header("Références visuelles du joueur")]
    public GameObject epee;
    public GameObject hache;
    public GameObject bouclier;
    public GameObject bouclierPaladin;

    void Start()
    {
        // 🔹 On s’assure que tout est invisible au lancement du jeu
        DesactiverTout();
    }

    private void DesactiverTout()
    {
        if (epee != null) epee.SetActive(false);
        if (hache != null) hache.SetActive(false);
        if (bouclier != null) bouclier.SetActive(false);
        if (bouclierPaladin != null) bouclierPaladin.SetActive(false);
    }

    public void ActiverObjet(string nom)
    {
        string type = ObtenirType(nom);

        // 🔹 Désactiver uniquement les objets du même type
        switch (type)
        {
            case "Arme":
                if (epee != null) epee.SetActive(false);
                if (hache != null) hache.SetActive(false);
                break;

            case "Armure":
                if (bouclier != null) bouclier.SetActive(false);
                if (bouclierPaladin != null) bouclierPaladin.SetActive(false);
                break;
        }

        // 🔹 Activer l’objet correspondant
        switch (nom)
        {
            case "epee":
                if (epee != null) epee.SetActive(true);
                break;
            case "hache":
                if (hache != null) hache.SetActive(true);
                break;
            case "bouclier":
                if (bouclier != null) bouclier.SetActive(true);
                break;
            case "bouclier paladin":
                if (bouclierPaladin != null) bouclierPaladin.SetActive(true);
                break;
        }
    }

    private string ObtenirType(string nom)
    {
        switch (nom)
        {
            case "epee":
            case "hache":
                return "Arme";
            case "bouclier":
            case "bouclier paladin":
                return "Armure";
            default:
                return "Autre";
        }
    }
}
