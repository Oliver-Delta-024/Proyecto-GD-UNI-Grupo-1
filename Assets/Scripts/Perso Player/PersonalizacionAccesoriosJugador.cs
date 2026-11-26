using UnityEngine;

[DisallowMultipleComponent]
public class PersonalizacionAccesoriosJugador : MonoBehaviour
{
    private const string ClaveCaraGuardada = "PersonalizacionJugador_Cara";
    private const string ClaveSombreroGuardado = "PersonalizacionJugador_Sombrero";
    private const string ClaveColorCaraGuardado = "PersonalizacionJugador_ColorCara";
    private const string ClaveColorSombreroGuardado = "PersonalizacionJugador_ColorSombrero";

    [SerializeField] private Sprite[] spritesCaras;
    [SerializeField] private Sprite[] spritesSombreros;

    private SpriteRenderer spriteCara;
    private SpriteRenderer spriteSombrero;

    private void Awake()
    {
        Transform cara = transform.Find("Cara");
        Transform sombrero = transform.Find("Sombrero");
        spriteCara = cara != null ? cara.GetComponent<SpriteRenderer>() : null;
        spriteSombrero = sombrero != null ? sombrero.GetComponent<SpriteRenderer>() : null;
    }

    private void Start()
    {
        AplicarSeleccionGuardada();
    }

    private void AplicarSeleccionGuardada()
    {
        int indiceCara = PlayerPrefs.GetInt(ClaveCaraGuardada, 0);
        int indiceSombrero = PlayerPrefs.GetInt(ClaveSombreroGuardado, 0);
        AplicarSprite(spriteCara, ObtenerSprite(spritesCaras, indiceCara));
        AplicarSprite(spriteSombrero, ObtenerSprite(spritesSombreros, indiceSombrero));
        AplicarColor(spriteCara, ObtenerColor(ClaveColorCaraGuardado));
        AplicarColor(spriteSombrero, ObtenerColor(ClaveColorSombreroGuardado));
    }

    private static Sprite ObtenerSprite(Sprite[] sprites, int indice)
    {
        if (sprites == null || indice <= 0 || indice > sprites.Length)
            return null;

        return sprites[indice - 1];
    }

    private static void AplicarSprite(SpriteRenderer destino, Sprite sprite)
    {
        if (destino == null)
            return;

        destino.sprite = sprite;
        destino.enabled = sprite != null;
    }

    private static Color ObtenerColor(string clave)
    {
        string valorGuardado = PlayerPrefs.GetString(clave, string.Empty);
        return ColorUtility.TryParseHtmlString("#" + valorGuardado, out Color color)
            ? color
            : Color.white;
    }

    private static void AplicarColor(SpriteRenderer destino, Color color)
    {
        if (destino != null)
            destino.color = color;
    }
}
