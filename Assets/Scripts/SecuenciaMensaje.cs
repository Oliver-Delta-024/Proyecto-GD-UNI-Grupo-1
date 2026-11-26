using UnityEngine;
using System.Collections;
using TMPro;

public class SecuenciaMensaje : MonoBehaviour
{
    private const string ClaveRutaZeroAnterior = "PartidaRutaZero";
    private string claveRutaZero = ClaveRutaZeroAnterior;

    [SerializeField] private TMP_Text textoMensaje;
    [SerializeField] private GameObject[] botonesOcultosAlInicio;
    [SerializeField] private float duracionInicio = 5f;
    [SerializeField] private float segundosPorLetra = 0.05f;

    private readonly string[] frasesInicioNormales =
    {
        "Que comience el experimento",
        "Iniciando prueba n.5827",
        "Es hora del espectaculo"
    };

    private const string FraseInicioZero = "Impresioname, Zero";

    private readonly string[,] finalesNormales =
    {
        {
            "Muestra insuficiente",
            "El experimento termino antes de tiempo",
            "Se requieren mas datos"
        },
        {
            "Datos recopilados",
            "Resultado dentro de los parametros",
            "El analisis continua"
        },
        {
            "Resultado prometedor",
            "La muestra supero las expectativas",
            "Avance significativo"
        },
        {
            "Resultado excepcional",
            "Experimento sobresaliente",
            "Se alcanzo un nuevo hito"
        }
    };

    private readonly string[] finalesZero =
    {
        "Decepcionante como siempre, Zero",
        "Muy mediocre para ti, Zero",
        "Mejoraste algo, Zero",
        "Esto es lo minimo que esperamos de ti, Zero"
    };

    private bool rutaZero;

    private void Awake()
    {
        rutaZero = PlayerPrefs.GetInt(claveRutaZero, 0) == 1;

        if (textoMensaje != null)
            textoMensaje.gameObject.SetActive(false);
    }

    public void ConfigurarPrefijoRutaZero(string prefijoNivel)
    {
        claveRutaZero = string.IsNullOrEmpty(prefijoNivel)
            ? ClaveRutaZeroAnterior
            : prefijoNivel + "RutaZero";

        int valorGuardado = PlayerPrefs.HasKey(claveRutaZero)
            ? PlayerPrefs.GetInt(claveRutaZero, 0)
            : PlayerPrefs.GetInt(ClaveRutaZeroAnterior, 0);
        rutaZero = valorGuardado == 1;
    }

    public void IniciarIntroduccion()
    {
        StartCoroutine(EscribirIntroduccion());
    }

    private IEnumerator EscribirIntroduccion()
    {
        int tirada = Random.Range(0, 100);
        rutaZero = tirada == 0;

        PlayerPrefs.SetInt(claveRutaZero, rutaZero ? 1 : 0);
        PlayerPrefs.Save();

        foreach (GameObject boton in botonesOcultosAlInicio)
        {
            if (boton != null)
                boton.SetActive(false);
        }

        string frase = rutaZero
            ? FraseInicioZero
            : frasesInicioNormales[Random.Range(0, frasesInicioNormales.Length)];

        float instanteInicio = Time.realtimeSinceStartup;

        if (textoMensaje != null)
        {
            textoMensaje.transform.SetAsLastSibling();
            textoMensaje.gameObject.SetActive(true);
            yield return Escribir(frase);
        }

        while (Time.realtimeSinceStartup - instanteInicio < duracionInicio)
            yield return null;

        if (textoMensaje != null)
            textoMensaje.gameObject.SetActive(false);

        foreach (GameObject boton in botonesOcultosAlInicio)
        {
            if (boton != null)
                boton.SetActive(true);
        }
    }

    public void MostrarMensajeFinal(float tiempoSobrevivido)
    {
        if (textoMensaje == null)
            return;

        int intervalo = ObtenerIntervalo(tiempoSobrevivido);
        string frase;

        if (rutaZero)
        {
            frase = finalesZero[intervalo];
        }
        else
        {
            int fraseAleatoria = Random.Range(0, 3);
            frase = finalesNormales[intervalo, fraseAleatoria];
        }

        textoMensaje.transform.SetAsLastSibling();
        textoMensaje.gameObject.SetActive(true);
        StartCoroutine(Escribir(frase));
    }

    public void GuardarRutaZero()
    {
        PlayerPrefs.SetInt(claveRutaZero, rutaZero ? 1 : 0);
        PlayerPrefs.Save();
    }

    private static int ObtenerIntervalo(float tiempo)
    {
        if (tiempo < 55f) return 0;
        if (tiempo < 100f) return 1;
        if (tiempo < 145f) return 2;
        return 3;
    }

    private IEnumerator Escribir(string frase)
    {
        textoMensaje.text = "";

        for (int i = 0; i < frase.Length; i++)
        {
            textoMensaje.text = frase.Substring(0, i + 1);
            yield return new WaitForSecondsRealtime(segundosPorLetra);
        }
    }
}
