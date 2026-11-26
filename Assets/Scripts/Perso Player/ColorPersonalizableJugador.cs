using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class ColorPersonalizableJugador : MonoBehaviour
{
    private const string ClaveColorGuardado = "PersonalizacionJugador_Color";
    private static readonly int ColorElegidoId = Shader.PropertyToID("_ColorElegido");
    private static readonly int RecolorActivoId = Shader.PropertyToID("_RecolorActivo");
    private static readonly int TonoMinimoId = Shader.PropertyToID("_TonoMinimo");
    private static readonly int TonoMaximoId = Shader.PropertyToID("_TonoMaximo");
    private static readonly int SaturacionMinimaId = Shader.PropertyToID("_SaturacionMinima");

    [Header("Color del personaje")]
    [SerializeField] private Shader shaderReemplazo;
    [SerializeField] private bool usarColorPersonalizado;
    [SerializeField] private Color colorElegido = new Color(246f / 255f, 1f, 1f / 255f, 1f);
    [SerializeField] private Color colorReferencia = new Color(246f / 255f, 1f, 1f / 255f, 1f);

    [Header("Tonos del dibujo que se reemplazan")]
    [SerializeField, Range(0f, 60f)] private float toleranciaHaciaNaranja = 52f;
    [SerializeField, Range(0f, 60f)] private float toleranciaHaciaVerde = 3f;
    [SerializeField, Range(0f, 1f)] private float saturacionMinima = 0.25f;

    private SpriteRenderer spriteRenderer;
    private Material materialInstanciado;
    private MaterialPropertyBlock bloquePropiedades;

    public Color ColorElegido => colorElegido;
    public bool UsaColorPersonalizado => usarColorPersonalizado;

    private void OnEnable()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        bloquePropiedades = new MaterialPropertyBlock();

        if (Application.isPlaying)
        {
            CargarColorGuardado();
            CrearMaterialDePrueba();
        }

        AplicarParametrosShader();
    }

    private void OnDisable()
    {
        if (spriteRenderer != null)
            spriteRenderer.SetPropertyBlock(null);

        if (materialInstanciado != null)
        {
            if (Application.isPlaying)
                Destroy(materialInstanciado);
            else
                DestroyImmediate(materialInstanciado);

            materialInstanciado = null;
        }
    }

    private void OnValidate()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (bloquePropiedades == null)
            bloquePropiedades = new MaterialPropertyBlock();

        AplicarParametrosShader();
    }

    public void CambiarColor(Color nuevoColor, bool guardar = false)
    {
        colorElegido = nuevoColor;
        usarColorPersonalizado = true;
        AplicarParametrosShader();

        if (guardar && Application.isPlaying)
            GuardarColor();
    }

    public void RestaurarColorBase(bool guardar = false)
    {
        colorElegido = colorReferencia;
        usarColorPersonalizado = false;
        AplicarParametrosShader();

        if (guardar && Application.isPlaying)
            GuardarColor();
    }

    public void GuardarColor()
    {
        if (!usarColorPersonalizado)
        {
            PlayerPrefs.DeleteKey(ClaveColorGuardado);
            PlayerPrefs.Save();
            return;
        }

        PlayerPrefs.SetString(ClaveColorGuardado, ColorUtility.ToHtmlStringRGBA(colorElegido));
        PlayerPrefs.Save();
    }

    public void CargarColorGuardado()
    {
        if (!PlayerPrefs.HasKey(ClaveColorGuardado))
            return;

        if (ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString(ClaveColorGuardado), out Color guardado))
        {
            colorElegido = guardado;
            usarColorPersonalizado = true;
        }
    }

    private void CrearMaterialDePrueba()
    {
        if (shaderReemplazo == null || spriteRenderer == null)
            return;

        if (materialInstanciado != null)
            Destroy(materialInstanciado);

        materialInstanciado = new Material(shaderReemplazo)
        {
            name = "Material temporal de color del jugador"
        };

        spriteRenderer.material = materialInstanciado;
    }

    private void AplicarParametrosShader()
    {
        if (spriteRenderer == null || bloquePropiedades == null)
            return;

        spriteRenderer.GetPropertyBlock(bloquePropiedades);
        bloquePropiedades.SetFloat(RecolorActivoId, usarColorPersonalizado ? 1f : 0f);
        bloquePropiedades.SetColor(ColorElegidoId, colorElegido);

        Color.RGBToHSV(colorReferencia.linear, out float tonoReferencia, out _, out _);
        float tonoMinimo = Mathf.Repeat(tonoReferencia - toleranciaHaciaNaranja / 360f, 1f);
        float tonoMaximo = Mathf.Repeat(tonoReferencia + toleranciaHaciaVerde / 360f, 1f);

        bloquePropiedades.SetFloat(TonoMinimoId, tonoMinimo);
        bloquePropiedades.SetFloat(TonoMaximoId, tonoMaximo);
        bloquePropiedades.SetFloat(SaturacionMinimaId, saturacionMinima);
        spriteRenderer.SetPropertyBlock(bloquePropiedades);
    }
}
