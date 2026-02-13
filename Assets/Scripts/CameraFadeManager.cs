using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;

public class CameraFadeManager : MonoBehaviour
{
    [SerializeField] float fadeTime;
    [SerializeField] Material fadeMat;

    //Siempre que una escena se carga, hay un fade in para ayudar a la inmpersión del jugador (mejor que un corte directamente a la escena)
    void Start()
    {
        fadeMat.color = Color.black;
        StartCoroutine(fadeIn());
    }

    public IEnumerator fadeIn()
    {
        yield return new WaitForSeconds(1f);
        float timePassed = 0;
        while (timePassed < fadeTime)
        {
            timePassed += Time.deltaTime;
            Color c = fadeMat.color;
            c.a = 1f - timePassed/fadeTime;
            fadeMat.color = c;
            yield return null;
        }
    }

    public IEnumerator fadeOut()
    {
        yield return new WaitForSeconds(1f);
        float timePassed = 0;
        while (timePassed < fadeTime)
        {
            timePassed += Time.deltaTime;
            Color c = fadeMat.color;
            c.a = timePassed / fadeTime;
            fadeMat.color = c;
            yield return null;
        }
    }
}
