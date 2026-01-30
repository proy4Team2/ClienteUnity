using System.Collections.Generic;
using UnityEngine;

public class DetailsData : MonoBehaviour
{
    private static DetailsData _instance;
    public static DetailsData Instance
    {
        get { return _instance; }
    }
    private void Start()
    {
        _instance = this;
    }

    private List<string> dataEntrevista = new List<string> {
        "Entrevista", 
        "1 persona", 
        "Formal", 
        "Realiza una entrevista laboral desde el punto de vista de un aplicante junior, buscando empezar a entrar al mercado laboral."
    };

    public List<string> Entrevista()
    {
        return dataEntrevista;
    }

    private List<string> dataReunion = new List<string> {
        "Reunión", 
        "Varias personas", 
        "Formal",
        "Participa en una reunión de trabajo con compañeros."
    };

    public List<string> Reunion()
    {
        return dataReunion;
    }
}
