
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TMPStyleController : MonoBehaviour
{
    [Header("Estilo compartido")]
    public TMP_FontAsset fontAsset;
    public float fontSize = 32f;
    public float horizontalScale = 1f;
    public float characterSpacing = 0f;

    [Header("Aplicar a los hijos")]
    public bool includeInactive = true;

    [System.Serializable]
    public class AnchoPorNombre
    {
        public string nombreObjeto;
        public float ancho = 100f;
    }

    [Header("Anchos de los nietos")]
    public AnchoPorNombre[] anchos;

    [ContextMenu("Aplicar estilo a todos los textos")]
    public void ApplyStyle()
    {
        TMP_Text[] texts =
            GetComponentsInChildren<TMP_Text>(includeInactive);

        foreach (TMP_Text text in texts)
        {
            if (fontAsset != null)
                text.font = fontAsset;

            text.fontSize = fontSize;
            text.characterSpacing = characterSpacing;

            RectTransform rect = text.rectTransform;
            Vector3 scale = rect.localScale;
            scale.x = horizontalScale;
            rect.localScale = scale;
        }
    }
    [ContextMenu("Aplicar anchos de los nietos")]
    public void ApplyIndividualWidths()
    {
        int modificados = 0;

        // Recorremos los hijos directos del padre.
        foreach (Transform hijo in transform)
        {
            // Recorremos los hijos de cada hijo: los nietos.
            foreach (Transform nieto in hijo)
            {
                RectTransform rect =
                    nieto.GetComponent<RectTransform>();

                if (rect == null)
                    continue;

                foreach (AnchoPorNombre config in anchos)
                {
                    if (nieto.name != config.nombreObjeto)
                        continue;

                    // Modificar directamente el ancho del RectTransform.
                    rect.SetSizeWithCurrentAnchors(
                        RectTransform.Axis.Horizontal,
                        config.ancho
                    );

                    Debug.Log(
                        nieto.name + " | Ancho solicitado: "
                        + config.ancho
                        + " | sizeDelta.x: " + rect.sizeDelta.x
                    );

                    modificados++;
                    break;
                }
            }
        }

        Debug.Log("Nietos modificados: " + modificados);
    
}

    private void Start()
    {
        ApplyStyle();
        ApplyIndividualWidths();
    }
}
