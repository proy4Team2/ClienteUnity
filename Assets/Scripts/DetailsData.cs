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

    /*
     INFO:
    Cada List<string> contiene una serie de datos de cada experiencia, para ser usados con el menú de detalles. Para cada nueva experiencia se
    debe crear una nueva List<string> según se especifica aquí.
    Los campos/datos siguen un orden concreto, siendo:

    private List<string> data[NOMBRE] = new List<string> {
        "[TITULO_EXPERIENCIA]", 
        "[Nº_PERSONAS]",    //"1 persona" (1-a-1) o "Varias personas" (Parte de un grupo)
        "[TIPO]",           //Tipo de conversación: "Formal" (trabajo, etc.) o "Informal" (quedar con amigos, conocidos...)
        "[DESCRIPCIÓN]"
        "[NOMBRE_ESCENA]"   // IMPORTANTE que coincida con el nombre de la escena correspondiente, se usará para el teletransporte

    };
     
     */

    private List<string> dataEntrevista = new List<string> {
        "Entrevista", 
        "1 persona", 
        "Formal", 
        "Realiza una entrevista laboral desde el punto de vista de un aplicante junior, buscando empezar a entrar al mercado laboral.",
        "INTERVIEW"
    };

    public List<string> Entrevista()
    {
        return dataEntrevista;
    }

    private List<string> dataReunion = new List<string> {
        "Reunión", 
        "Varias personas", 
        "Formal",
        "Participa en una reunión de trabajo con compañeros.",
        "MEETING"
    };

    public List<string> Reunion()
    {
        return dataReunion;
    }
}
