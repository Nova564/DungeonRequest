using System.Collections.Generic;
using UnityEngine;

public enum TypeObjet
{
    Arme,
    Armure,
    ObjetDeQuete
}

[System.Serializable]
public class InfosObjet
{
    public TypeObjet type;
    public string name;
    public bool ramasse;
}

public class DicoItem : MonoBehaviour
{
    private Dictionary<string, InfosObjet> objets;

    void Start()
    {
        objets = new Dictionary<string, InfosObjet>()
        {
            { "epee", new InfosObjet { type = TypeObjet.Arme, name = "epee", ramasse = false } },
            { "hache", new InfosObjet { type = TypeObjet.Arme, name = "hache", ramasse = false } },
            { "bouclier", new InfosObjet { type = TypeObjet.Armure, name = "bouclier", ramasse = false } }
        };
    }


    public void ActiverObjet(string elem)
    {
        TypeObjet type = objets[elem].type;

        foreach (var kvp in objets)
        {
            if (kvp.Value.type == type)
                kvp.Value.ramasse = false;
        }


        objets[elem].ramasse = true;

    }
}
