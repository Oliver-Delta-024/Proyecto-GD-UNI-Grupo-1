using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EscenaCustomPlayer : MonoBehaviour
{
    private enum ObjetivoColor
    {
        Jugador,
        Cara,
        Sombrero
    }

    [Header("Referencias de la escena")]
    [SerializeField] private ColorPersonalizableJugador jugadorVistaPrevia;
    [SerializeField] private Image imagenColorActual;
    [SerializeField] private GameObject menuColores;
    [SerializeField] private Camera camaraVistaPrevia;
    [SerializeField] private RawImage imagenVistaPrevia;
    [SerializeField] private CatalogoCarasSombreros catalogoAccesorios;

    private RenderTexture texturaVistaPrevia;
    private Color colorOriginal;
    private bool colorOriginalPersonalizado;
    private Transform grupoMenuColores;
    private ObjetivoColor objetivoColorActual;
    private Image imagenColorCara;
    private Image imagenColorSombrero;

    private static readonly Color[] ColoresDisponibles =
    {
        new Color32(246, 255, 1, 255),
        new Color32(255, 48, 48, 255),
        new Color32(40, 123, 255, 255),
        new Color32(25, 200, 90, 255),
        new Color32(154, 65, 255, 255),
        new Color32(24, 221, 224, 255),
        new Color32(255, 38, 201, 255),
        new Color32(255, 122, 22, 255),
        new Color32(255, 161, 200, 255),
        Color.white,
        new Color32(37, 37, 37, 255),
        new Color32(0, 168, 150, 255)
    };

    private void Start()
    {
        Transform filaCatalogos = transform.Find("BotonesMenuSeleccion");
        Transform botonColorCara = filaCatalogos != null ? filaCatalogos.Find("BotonColorCara") : null;
        Transform botonColorSombrero = filaCatalogos != null ? filaCatalogos.Find("BotonColorSombrero") : null;
        imagenColorCara = botonColorCara != null ? botonColorCara.GetComponent<Image>() : null;
        imagenColorSombrero = botonColorSombrero != null ? botonColorSombrero.GetComponent<Image>() : null;

        if (menuColores != null)
        {
            menuColores.SetActive(false);
            grupoMenuColores = menuColores.transform.parent;

            Image fondoMenu = menuColores.GetComponent<Image>();
            if (fondoMenu != null)
                fondoMenu.raycastTarget = false;
        }

        GuardarEstadoOriginal();
        PrepararVistaPrevia();
        ActualizarColorMostrado();
        ActualizarMuestrasColoresAccesorios();
    }

    private void OnDestroy()
    {
        if (camaraVistaPrevia != null)
            camaraVistaPrevia.targetTexture = null;

        if (texturaVistaPrevia != null)
        {
            texturaVistaPrevia.Release();
            Destroy(texturaVistaPrevia);
        }
    }

    private void PrepararVistaPrevia()
    {
        if (camaraVistaPrevia == null || imagenVistaPrevia == null)
            return;

        texturaVistaPrevia = new RenderTexture(512, 512, 16, RenderTextureFormat.ARGB32);
        texturaVistaPrevia.name = "RenderTexture PlayerPreview";
        texturaVistaPrevia.Create();
        camaraVistaPrevia.targetTexture = texturaVistaPrevia;
        imagenVistaPrevia.texture = texturaVistaPrevia;
    }

    public void AlternarMenuColores()
    {
        AlternarMenuColores(ObjetivoColor.Jugador);
    }

    public void AlternarMenuColorCara()
    {
        AlternarMenuColores(ObjetivoColor.Cara);
    }

    public void AlternarMenuColorSombrero()
    {
        AlternarMenuColores(ObjetivoColor.Sombrero);
    }

    private void AlternarMenuColores(ObjetivoColor objetivo)
    {
        if (menuColores == null)
            return;

        bool yaAbiertoParaEsteObjetivo = menuColores.activeSelf && objetivoColorActual == objetivo;
        if (yaAbiertoParaEsteObjetivo)
        {
            CerrarMenuColores();
            return;
        }

        objetivoColorActual = objetivo;
        menuColores.SetActive(true);

        if (grupoMenuColores != null)
            grupoMenuColores.SetAsLastSibling();

        menuColores.transform.SetAsLastSibling();
    }

    public void CerrarMenuColores()
    {
        if (menuColores != null)
            menuColores.SetActive(false);
    }

    public void SeleccionarColor(int indice)
    {
        if (indice < 0 || indice >= ColoresDisponibles.Length)
            return;

        Color color = ColoresDisponibles[indice];
        switch (objetivoColorActual)
        {
            case ObjetivoColor.Jugador:
                if (jugadorVistaPrevia != null)
                    jugadorVistaPrevia.CambiarColor(color);
                ActualizarColorMostrado();
                break;
            case ObjetivoColor.Cara:
                if (catalogoAccesorios != null)
                    catalogoAccesorios.CambiarColorCara(color);
                if (imagenColorCara != null)
                    imagenColorCara.color = color;
                break;
            case ObjetivoColor.Sombrero:
                if (catalogoAccesorios != null)
                    catalogoAccesorios.CambiarColorSombrero(color);
                if (imagenColorSombrero != null)
                    imagenColorSombrero.color = color;
                break;
        }

        CerrarMenuColores();
    }

    public void AplicarCambios()
    {
        CerrarMenuColores();

        if (jugadorVistaPrevia == null)
            return;

        jugadorVistaPrevia.GuardarColor();
        if (catalogoAccesorios != null)
            catalogoAccesorios.ConfirmarSeleccion();

        GuardarEstadoOriginal();
        ActualizarColorMostrado();
    }

    public void DeshacerCambios()
    {
        if (jugadorVistaPrevia != null)
        {
            if (colorOriginalPersonalizado)
                jugadorVistaPrevia.CambiarColor(colorOriginal);
            else
                jugadorVistaPrevia.RestaurarColorBase();
        }

        if (catalogoAccesorios != null)
            catalogoAccesorios.DeshacerSeleccion();

        CerrarMenuColores();

        ActualizarColorMostrado();
        ActualizarMuestrasColoresAccesorios();
    }

    public void ReiniciarColorBase()
    {
        if (jugadorVistaPrevia != null)
        {
            jugadorVistaPrevia.RestaurarColorBase(guardar: true);
            GuardarEstadoOriginal();
        }

        if (catalogoAccesorios != null)
            catalogoAccesorios.RestablecerAccesoriosBase();

        CerrarMenuColores();

        ActualizarColorMostrado();
        ActualizarMuestrasColoresAccesorios();
    }

    private void GuardarEstadoOriginal()
    {
        if (jugadorVistaPrevia == null)
            return;

        colorOriginal = jugadorVistaPrevia.ColorElegido;
        colorOriginalPersonalizado = jugadorVistaPrevia.UsaColorPersonalizado;
    }

    private void ActualizarColorMostrado()
    {
        if (jugadorVistaPrevia != null && imagenColorActual != null)
            imagenColorActual.color = jugadorVistaPrevia.ColorElegido;
    }

    public void ActualizarMuestrasColoresAccesorios()
    {
        if (catalogoAccesorios == null)
            return;

        if (imagenColorCara != null)
            imagenColorCara.color = catalogoAccesorios.ColorCaraSeleccionada;

        if (imagenColorSombrero != null)
            imagenColorSombrero.color = catalogoAccesorios.ColorSombreroSeleccionado;
    }

    public void VolverMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
