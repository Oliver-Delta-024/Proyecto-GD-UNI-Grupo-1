using System;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[DisallowMultipleComponent]
public class CatalogoCarasSombreros : MonoBehaviour
{
    private const string ClaveCaraGuardada = "PersonalizacionJugador_Cara";
    private const string ClaveSombreroGuardado = "PersonalizacionJugador_Sombrero";
    private const string ClaveColorCaraGuardado = "PersonalizacionJugador_ColorCara";
    private const string ClaveColorSombreroGuardado = "PersonalizacionJugador_ColorSombrero";

    public const int CantidadCaras = 7;
    public const int CantidadSombreros = 7;

    private const int FilasCatalogo = 2;
    private const float AnchoCasilla = 72f;
    private const float AltoCasillaMaximo = 76f;
    private const float EspacioHorizontal = 10f;
    private const float EspacioVertical = 10f;
    private const float DesplazamientoVerticalSombrero = 0.40f;

    [SerializeField, HideInInspector] private Sprite[] spritesCaras = new Sprite[CantidadCaras - 1];
    [SerializeField, HideInInspector] private Sprite[] spritesSombreros = new Sprite[CantidadSombreros - 1];
    [SerializeField, HideInInspector] private GameObject menuCaras;
    [SerializeField, HideInInspector] private GameObject menuSombreros;

    private int indiceCaraSeleccionada;
    private int indiceSombreroSeleccionado;
    private int indiceCaraInicial;
    private int indiceSombreroInicial;
    private Color colorCaraSeleccionada = Color.white;
    private Color colorSombreroSeleccionado = Color.white;
    private Color colorCaraInicial = Color.white;
    private Color colorSombreroInicial = Color.white;

    private SpriteRenderer spriteJugadorVistaPrevia;
    private SpriteRenderer spriteCaraVistaPrevia;
    private SpriteRenderer spriteSombreroVistaPrevia;
    private EscenaCustomPlayer escenaPersonalizacion;

    private const float EscalaCara = 1f;
    private const float EscalaSombrero = 0.8f;

#if UNITY_EDITOR
    [NonSerialized] private bool sincronizacionEnCola;
#endif

    public int IndiceCaraSeleccionada => indiceCaraSeleccionada;
    public int IndiceSombreroSeleccionado => indiceSombreroSeleccionado;
    public Sprite CaraSeleccionada => ObtenerSprite(spritesCaras, indiceCaraSeleccionada);
    public Sprite SombreroSeleccionado => ObtenerSprite(spritesSombreros, indiceSombreroSeleccionado);
    public Color ColorCaraSeleccionada => colorCaraSeleccionada;
    public Color ColorSombreroSeleccionado => colorSombreroSeleccionado;

    private void OnEnable()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            ProgramarSincronizacionEnEditor();
#endif
    }

    private void OnValidate()
    {
        AjustarCantidadSprites(ref spritesCaras, CantidadCaras - 1);
        AjustarCantidadSprites(ref spritesSombreros, CantidadSombreros - 1);

#if UNITY_EDITOR
        if (!Application.isPlaying)
            ProgramarSincronizacionEnEditor();
#endif
    }

    private void OnRectTransformDimensionsChange()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            ProgramarSincronizacionEnEditor();
#endif
    }

    private void Awake()
    {
        escenaPersonalizacion = transform.root.GetComponentInChildren<EscenaCustomPlayer>(true);

        if (Application.isPlaying)
        {
            menuCaras = BuscarMenu("MenuCaras");
            menuSombreros = BuscarMenu("MenuSombreros");
            PrepararBotonesDeOpcion(menuCaras, true);
            PrepararBotonesDeOpcion(menuSombreros, false);
            MostrarCaras();
        }
    }

    private void Start()
    {
        ResolverSpritesVistaPrevia();
        indiceCaraSeleccionada = Mathf.Clamp(PlayerPrefs.GetInt(ClaveCaraGuardada, 0), 0, CantidadCaras - 1);
        indiceSombreroSeleccionado = Mathf.Clamp(PlayerPrefs.GetInt(ClaveSombreroGuardado, 0), 0, CantidadSombreros - 1);
        colorCaraSeleccionada = CargarColor(ClaveColorCaraGuardado);
        colorSombreroSeleccionado = CargarColor(ClaveColorSombreroGuardado);
        indiceCaraInicial = indiceCaraSeleccionada;
        indiceSombreroInicial = indiceSombreroSeleccionado;
        colorCaraInicial = colorCaraSeleccionada;
        colorSombreroInicial = colorSombreroSeleccionado;
        ActualizarVistaPrevia();
        ActualizarResaltado(menuCaras, indiceCaraSeleccionada);
        ActualizarResaltado(menuSombreros, indiceSombreroSeleccionado);

        if (escenaPersonalizacion != null)
            escenaPersonalizacion.ActualizarMuestrasColoresAccesorios();
    }

    private GameObject BuscarMenu(string nombre)
    {
        Transform encontrado = transform.Find(nombre);
        return encontrado != null ? encontrado.gameObject : null;
    }

    public void MostrarCaras()
    {
        if (escenaPersonalizacion != null)
            escenaPersonalizacion.CerrarMenuColores();

        if (menuCaras != null)
            menuCaras.SetActive(true);

        if (menuSombreros != null)
            menuSombreros.SetActive(false);
    }

    public void MostrarSombreros()
    {
        if (escenaPersonalizacion != null)
            escenaPersonalizacion.CerrarMenuColores();

        if (menuCaras != null)
            menuCaras.SetActive(false);

        if (menuSombreros != null)
            menuSombreros.SetActive(true);
    }

    public void SeleccionarCara(int indice)
    {
        if (indice < 0 || indice >= CantidadCaras)
            return;

        if (escenaPersonalizacion != null)
            escenaPersonalizacion.CerrarMenuColores();

        indiceCaraSeleccionada = indice;
        ActualizarResaltado(menuCaras, indiceCaraSeleccionada);
        ActualizarVistaPrevia();
    }

    public void SeleccionarSombrero(int indice)
    {
        if (indice < 0 || indice >= CantidadSombreros)
            return;

        if (escenaPersonalizacion != null)
            escenaPersonalizacion.CerrarMenuColores();

        indiceSombreroSeleccionado = indice;
        ActualizarResaltado(menuSombreros, indiceSombreroSeleccionado);
        ActualizarVistaPrevia();
    }

    public void DeshacerSeleccion()
    {
        indiceCaraSeleccionada = indiceCaraInicial;
        indiceSombreroSeleccionado = indiceSombreroInicial;
        colorCaraSeleccionada = colorCaraInicial;
        colorSombreroSeleccionado = colorSombreroInicial;
        ActualizarVistaPrevia();
        ActualizarResaltado(menuCaras, indiceCaraSeleccionada);
        ActualizarResaltado(menuSombreros, indiceSombreroSeleccionado);
        MostrarCaras();
    }

    public void RestablecerAccesoriosBase()
    {
        indiceCaraSeleccionada = 0;
        indiceSombreroSeleccionado = 0;
        indiceCaraInicial = 0;
        indiceSombreroInicial = 0;
        colorCaraSeleccionada = Color.white;
        colorSombreroSeleccionado = Color.white;
        colorCaraInicial = Color.white;
        colorSombreroInicial = Color.white;
        GuardarSeleccionActual();
        ActualizarVistaPrevia();
        ActualizarResaltado(menuCaras, indiceCaraSeleccionada);
        ActualizarResaltado(menuSombreros, indiceSombreroSeleccionado);
        MostrarCaras();
    }

    public void ConfirmarSeleccion()
    {
        GuardarSeleccionActual();
        indiceCaraInicial = indiceCaraSeleccionada;
        indiceSombreroInicial = indiceSombreroSeleccionado;
        colorCaraInicial = colorCaraSeleccionada;
        colorSombreroInicial = colorSombreroSeleccionado;
    }

    public void CambiarColorCara(Color color)
    {
        colorCaraSeleccionada = color;
        ActualizarVistaPrevia();
    }

    public void CambiarColorSombrero(Color color)
    {
        colorSombreroSeleccionado = color;
        ActualizarVistaPrevia();
    }

    private void GuardarSeleccionActual()
    {
        PlayerPrefs.SetInt(ClaveCaraGuardada, indiceCaraSeleccionada);
        PlayerPrefs.SetInt(ClaveSombreroGuardado, indiceSombreroSeleccionado);
        PlayerPrefs.SetString(ClaveColorCaraGuardado, ColorUtility.ToHtmlStringRGBA(colorCaraSeleccionada));
        PlayerPrefs.SetString(ClaveColorSombreroGuardado, ColorUtility.ToHtmlStringRGBA(colorSombreroSeleccionado));
        PlayerPrefs.Save();
    }

    private static Color CargarColor(string clave)
    {
        string valorGuardado = PlayerPrefs.GetString(clave, string.Empty);
        return ColorUtility.TryParseHtmlString("#" + valorGuardado, out Color color)
            ? color
            : Color.white;
    }

    private void ResolverSpritesVistaPrevia()
    {
        Transform canvas = transform.parent;
        Transform vistaPrevia = canvas != null ? canvas.Find("VistaPreviaPlayer") : null;
        if (vistaPrevia == null)
            return;

        Transform jugador = vistaPrevia.Find("ColorPlayer");
        Transform cara = vistaPrevia.Find("CaraPlayer");
        Transform sombrero = vistaPrevia.Find("SombreroPlayer");
        spriteJugadorVistaPrevia = jugador != null ? jugador.GetComponent<SpriteRenderer>() : null;
        spriteCaraVistaPrevia = cara != null ? cara.GetComponent<SpriteRenderer>() : null;
        spriteSombreroVistaPrevia = sombrero != null ? sombrero.GetComponent<SpriteRenderer>() : null;
    }

    private void ActualizarVistaPrevia()
    {
        if (spriteJugadorVistaPrevia == null)
            ResolverSpritesVistaPrevia();

        AplicarSpriteVistaPrevia(
            spriteCaraVistaPrevia,
            spriteJugadorVistaPrevia,
            ObtenerSprite(spritesCaras, indiceCaraSeleccionada),
            colorCaraSeleccionada,
            EscalaCara,
            false);

        AplicarSpriteVistaPrevia(
            spriteSombreroVistaPrevia,
            spriteJugadorVistaPrevia,
            ObtenerSprite(spritesSombreros, indiceSombreroSeleccionado),
            colorSombreroSeleccionado,
            EscalaSombrero,
            true);
    }

    private static void AplicarSpriteVistaPrevia(
        SpriteRenderer destino,
        SpriteRenderer jugador,
        Sprite sprite,
        Color color,
        float escalaRelativa,
        bool esSombrero)
    {
        if (destino == null)
            return;

        destino.sprite = sprite;
        destino.color = color;
        destino.enabled = sprite != null;
        if (sprite == null)
            return;

        if (jugador != null && jugador.sprite != null)
        {
            Vector3 escalaJugador = jugador.transform.localScale;
            destino.transform.localScale = new Vector3(
                escalaJugador.x * escalaRelativa,
                escalaJugador.y * escalaRelativa,
                escalaJugador.z);

            Vector3 posicion = jugador.transform.localPosition;
            if (esSombrero)
                posicion.y += jugador.sprite.bounds.size.y * escalaJugador.y * DesplazamientoVerticalSombrero;
            destino.transform.localPosition = posicion;

            destino.sortingLayerID = jugador.sortingLayerID;
            destino.sortingOrder = jugador.sortingOrder + (esSombrero ? 2 : 1);
        }
    }

    private void PrepararBotonesDeOpcion(GameObject menu, bool esMenuCaras)
    {
        if (menu == null)
            return;

        Button[] botones = menu.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < botones.Length; i++)
        {
            int indice = i;
            if (esMenuCaras)
                botones[i].onClick.AddListener(() => SeleccionarCara(indice));
            else
                botones[i].onClick.AddListener(() => SeleccionarSombrero(indice));
        }

        ActualizarResaltado(menu, esMenuCaras ? indiceCaraSeleccionada : indiceSombreroSeleccionado);
    }

    private void ActualizarResaltado(GameObject menu, int indiceSeleccionado)
    {
        if (menu == null)
            return;

        Button[] botones = menu.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < botones.Length; i++)
        {
            Graphic grafico = botones[i].targetGraphic;
            if (grafico != null)
                grafico.color = i == indiceSeleccionado
                    ? new Color32(255, 214, 40, 255)
                    : new Color32(44, 49, 61, 255);
        }
    }

    private static Sprite ObtenerSprite(Sprite[] sprites, int indice)
    {
        if (indice <= 0 || sprites == null || indice > sprites.Length)
            return null;

        return sprites[indice - 1];
    }

    private static void AjustarCantidadSprites(ref Sprite[] sprites, int cantidad)
    {
        if (sprites == null || sprites.Length != cantidad)
            Array.Resize(ref sprites, cantidad);
    }

#if UNITY_EDITOR
    public void AsegurarMenusEnEscena()
    {
        if (Application.isPlaying || gameObject == null || !gameObject.scene.IsValid())
            return;

        if (EstructuraCompleta())
        {
            menuCaras = BuscarMenu("MenuCaras");
            menuSombreros = BuscarMenu("MenuSombreros");
            ActualizarTamanoMenusEnEscena();
            return;
        }

        UnityEditor.Undo.RegisterFullObjectHierarchyUndo(gameObject, "Preparar catálogo de personalización");
        menuCaras = CrearOBuscarMenu("MenuCaras", true);
        menuSombreros = CrearOBuscarMenu("MenuSombreros", false);
        ActualizarSpritesEnEscena();
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
    }

    public void ActualizarSpritesEnEscena()
    {
        ActualizarSpritesMenu(menuCaras, spritesCaras, CantidadCaras);
        ActualizarSpritesMenu(menuSombreros, spritesSombreros, CantidadSombreros);
    }

    private void ProgramarSincronizacionEnEditor()
    {
        if (sincronizacionEnCola)
            return;

        sincronizacionEnCola = true;
        UnityEditor.EditorApplication.delayCall += () =>
        {
            sincronizacionEnCola = false;
            if (this != null && !Application.isPlaying && gameObject.scene.IsValid())
                AsegurarMenusEnEscena();
        };
    }

    private GameObject CrearOBuscarMenu(string nombre, bool activo)
    {
        Transform existente = transform.Find(nombre);
        GameObject menu = existente != null
            ? existente.gameObject
            : new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D), typeof(ScrollRect));

        if (existente == null)
        {
            UnityEditor.Undo.RegisterCreatedObjectUndo(menu, "Crear menú de accesorios");
            menu.layer = gameObject.layer;
            menu.transform.SetParent(transform, false);
        }

        RectTransform menuRect = menu.GetComponent<RectTransform>();
        menuRect.anchorMin = Vector2.zero;
        menuRect.anchorMax = Vector2.one;
        menuRect.offsetMin = Vector2.zero;
        menuRect.offsetMax = Vector2.zero;

        Image fondo = menu.GetComponent<Image>();
        fondo.sprite = GetComponent<Image>() != null ? GetComponent<Image>().sprite : null;
        fondo.color = new Color(1f, 1f, 1f, 0.01f);
        fondo.raycastTarget = true;

        Transform contenidoExistente = menu.transform.Find("Contenido");
        GameObject contenidoObjeto = contenidoExistente != null
            ? contenidoExistente.gameObject
            : new GameObject("Contenido", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
        if (contenidoExistente == null)
        {
            UnityEditor.Undo.RegisterCreatedObjectUndo(contenidoObjeto, "Crear contenido del catálogo");
            contenidoObjeto.layer = gameObject.layer;
            contenidoObjeto.transform.SetParent(menu.transform, false);
        }

        RectTransform contenidoRect = contenidoObjeto.GetComponent<RectTransform>();
        contenidoRect.anchorMin = new Vector2(0f, 1f);
        contenidoRect.anchorMax = new Vector2(0f, 1f);
        contenidoRect.pivot = new Vector2(0f, 1f);
        contenidoRect.anchoredPosition = Vector2.zero;

        GridLayoutGroup grilla = contenidoObjeto.GetComponent<GridLayoutGroup>();
        grilla.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grilla.startAxis = GridLayoutGroup.Axis.Horizontal;
        grilla.childAlignment = TextAnchor.UpperLeft;
        grilla.constraint = GridLayoutGroup.Constraint.FixedRowCount;
        grilla.constraintCount = FilasCatalogo;
        grilla.cellSize = CalcularTamanoCasilla(menuRect);
        grilla.spacing = new Vector2(EspacioHorizontal, EspacioVertical);
        grilla.padding = new RectOffset(8, 8, 8, 8);

        ContentSizeFitter ajustador = contenidoObjeto.GetComponent<ContentSizeFitter>();
        ajustador.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        ajustador.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scroll = menu.GetComponent<ScrollRect>();
        scroll.content = contenidoRect;
        scroll.viewport = menuRect;
        scroll.horizontal = true;
        scroll.vertical = false;
        scroll.movementType = ScrollRect.MovementType.Elastic;
        scroll.inertia = true;
        scroll.scrollSensitivity = 35f;

        int cantidad = nombre == "MenuCaras" ? CantidadCaras : CantidadSombreros;
        string prefijo = nombre == "MenuCaras" ? "OpcionCara_" : "OpcionSombrero_";
        SincronizarCasillas(contenidoObjeto.transform, prefijo, cantidad);
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contenidoRect);

        menu.SetActive(activo);
        return menu;
    }

    private void SincronizarCasillas(Transform contenido, string prefijo, int cantidad)
    {
        for (int indice = 0; indice < cantidad; indice++)
        {
            string nombre = prefijo + indice;
            Transform existente = contenido.Find(nombre);
            GameObject casilla = existente != null
                ? existente.gameObject
                : new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));

            if (existente == null)
            {
                UnityEditor.Undo.RegisterCreatedObjectUndo(casilla, "Crear casilla del catálogo");
                casilla.layer = gameObject.layer;
                casilla.transform.SetParent(contenido, false);
            }

            Image fondo = casilla.GetComponent<Image>();
            fondo.sprite = GetComponent<Image>() != null ? GetComponent<Image>().sprite : null;
            fondo.color = indice == 0
                ? new Color32(255, 214, 40, 255)
                : new Color32(44, 49, 61, 255);
            fondo.raycastTarget = true;

            Button boton = casilla.GetComponent<Button>();
            boton.transition = Selectable.Transition.None;
            boton.targetGraphic = fondo;

            Transform iconoExistente = casilla.transform.Find("Icono");
            GameObject iconoObjeto = iconoExistente != null
                ? iconoExistente.gameObject
                : new GameObject("Icono", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            if (iconoExistente == null)
            {
                UnityEditor.Undo.RegisterCreatedObjectUndo(iconoObjeto, "Crear imagen de casilla");
                iconoObjeto.layer = gameObject.layer;
                iconoObjeto.transform.SetParent(casilla.transform, false);
            }

            RectTransform iconoRect = iconoObjeto.GetComponent<RectTransform>();
            iconoRect.anchorMin = Vector2.zero;
            iconoRect.anchorMax = Vector2.one;
            iconoRect.offsetMin = new Vector2(7f, 7f);
            iconoRect.offsetMax = new Vector2(-7f, -7f);
            Image icono = iconoObjeto.GetComponent<Image>();
            icono.preserveAspect = true;
            icono.raycastTarget = false;
        }
    }

    private void ActualizarSpritesMenu(GameObject menu, Sprite[] sprites, int cantidad)
    {
        if (menu == null)
            return;

        string prefijo = menu.name == "MenuCaras" ? "OpcionCara_" : "OpcionSombrero_";
        for (int indice = 0; indice < cantidad; indice++)
        {
            Transform iconoTransform = menu.transform.Find("Contenido/" + prefijo + indice + "/Icono");
            if (iconoTransform == null)
                continue;

            Image icono = iconoTransform.GetComponent<Image>();
            Sprite sprite = indice == 0 ? null : sprites[indice - 1];
            bool visible = sprite != null;
            if (icono.sprite == sprite && icono.enabled == visible && icono.preserveAspect)
                continue;

            UnityEditor.Undo.RecordObject(icono, "Actualizar sprite del catálogo");
            icono.sprite = sprite;
            icono.enabled = visible;
            icono.preserveAspect = true;
            UnityEditor.EditorUtility.SetDirty(icono);
        }
    }

    private static Vector2 CalcularTamanoCasilla(RectTransform menuRect)
    {
        float altoDisponible = menuRect.rect.height - 16f - 16f - EspacioVertical * (FilasCatalogo - 1);
        float alto = Mathf.Min(AltoCasillaMaximo, Mathf.Max(20f, altoDisponible / FilasCatalogo));
        return new Vector2(AnchoCasilla, alto);
    }

    private void ActualizarTamanoMenusEnEscena()
    {
        ActualizarTamanoMenu(menuCaras);
        ActualizarTamanoMenu(menuSombreros);
    }

    private static void ActualizarTamanoMenu(GameObject menu)
    {
        if (menu == null)
            return;

        Transform contenido = menu.transform.Find("Contenido");
        if (contenido == null)
            return;

        GridLayoutGroup grilla = contenido.GetComponent<GridLayoutGroup>();
        if (grilla == null)
            return;

        Vector2 nuevoTamano = CalcularTamanoCasilla(menu.GetComponent<RectTransform>());
        if (grilla.cellSize != nuevoTamano)
        {
            grilla.cellSize = nuevoTamano;
            UnityEditor.EditorUtility.SetDirty(grilla);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(menu.scene);
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contenido.GetComponent<RectTransform>());
    }

    private bool EstructuraCompleta()
    {
        return MenuCompleto(BuscarMenu("MenuCaras"), "OpcionCara_", CantidadCaras)
            && MenuCompleto(BuscarMenu("MenuSombreros"), "OpcionSombrero_", CantidadSombreros);
    }

    private static bool MenuCompleto(GameObject menu, string prefijo, int cantidad)
    {
        if (menu == null)
            return false;

        Transform contenido = menu.transform.Find("Contenido");
        if (contenido == null || contenido.childCount != cantidad)
            return false;

        for (int i = 0; i < cantidad; i++)
        {
            Transform casilla = contenido.Find(prefijo + i);
            if (casilla == null || casilla.GetComponent<Button>() == null || casilla.Find("Icono") == null)
                return false;
        }

        return true;
    }
#endif
}
