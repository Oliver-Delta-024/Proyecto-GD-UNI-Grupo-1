using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
[DefaultExecutionOrder(-1000)]
public class EscaladoCanvasBuild : MonoBehaviour
{
    [SerializeField] private bool escalarCanvasEnBuild;
    [SerializeField] private RectTransform fondo16a9;
    [SerializeField] private RenderTexture texturaVideo;
    [SerializeField] private RectTransform botonBorrarDatos;
    [SerializeField] private RectTransform panelNombreActual;

    private void Awake()
    {
#if !UNITY_EDITOR
        if (texturaVideo != null && (texturaVideo.width != 1920 || texturaVideo.height != 1080))
        {
            texturaVideo.Release();
            texturaVideo.width = 1920;
            texturaVideo.height = 1080;
            texturaVideo.Create();
        }

        if (escalarCanvasEnBuild)
        {
            CanvasScaler escalador = GetComponent<CanvasScaler>();
            escalador.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            escalador.referenceResolution = new Vector2(2560f, 1440f);
            escalador.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            escalador.matchWidthOrHeight = 1f;
        }
#endif
    }
}
