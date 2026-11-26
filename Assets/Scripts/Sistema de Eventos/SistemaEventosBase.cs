using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;

public abstract class SistemaEventosBase : MonoBehaviour
{
    protected abstract string PrefijoEvento { get; }

    [Header("Referencias")]
    public SpriteRenderer panelOverdrive;
    public GeneradorBase generadorDiscos;

    public Light2D globalLight;
    public Light2D blackoutLight;

    private Color colorOriginal;

    private float intensidadInicialGlobal = 1f;
    private float intensidadFinalGlobal = 0f;

    private float intensidadInicialFreeform = 1f;

    [Header("Overdrive")]
    public float multiplicadorOverdrive = 2f;

    [Header("Tiempos")]
    public float tiempoEntrada = 1f;
    public float tiempoActivo = 8f;
    public float tiempoSalida = 1f;

    private Coroutine rutinaEvento;

    private bool eventoActivo = false;

    private bool overdriveActivo = false;
    private bool blackoutActivo = false;

    public Transform player;

    // 0 = Ninguna
    // 1 = Entrada
    // 2 = Activo
    // 3 = Salida
    private int faseEvento = 0;

    // Tiempo transcurrido dentro de la fase actual
    private float tiempoFase = 0f;

    public bool HayEventoActivo()
    {
        return eventoActivo;
    }

    public bool OverdriveActivo()
    {
        return overdriveActivo;
    }

    public bool BlackoutActivo()
    {
        return blackoutActivo;
    }

    public int ObtenerFase()
    {
        return faseEvento;
    }

    public float ObtenerTiempoFase()
    {
        return tiempoFase;
    }

    private void ActualizarBlackout()
    {
        if (!blackoutActivo)
            return;

        if (player == null)
            return;

        blackoutLight.transform.position =
            new Vector3(
                player.position.x,
                player.position.y,
                blackoutLight.transform.position.z);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        colorOriginal = panelOverdrive.color;

        if (blackoutLight != null)
        {
            blackoutLight.intensity = 0f;
            blackoutLight.gameObject.SetActive(false);
        }

        Debug.Log(
        "START -> intensidadInicialGlobal = "
        + intensidadInicialGlobal);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        ActualizarBlackout();
    }

    //==========================================
    // CONTROL GENERAL
    //==========================================

    public IEnumerator Overdrive()
    {
        eventoActivo = true;
        overdriveActivo = true;
        blackoutActivo = false;

        faseEvento = 1;
        tiempoFase = 0f;

        Color colorOriginal = panelOverdrive.color;
        Color colorEvento = new Color(1f, 0f, 0f, 1f);

        float t = 0f;

        // Entrada
        while (t < tiempoEntrada)
        {
            t += Time.deltaTime;
            tiempoFase = t;

            panelOverdrive.color =
                Color.Lerp(
                    colorOriginal,
                    colorEvento,
                    t / tiempoEntrada);

            yield return null;
        }
        faseEvento = 2;
        tiempoFase = 0f;
        panelOverdrive.color = colorEvento;
        Debug.Log("SistemaEventos -> Activando Overdrive");
        generadorDiscos.ActivarOverdrive(multiplicadorOverdrive);

        // Activo
        t = 0f;

        while (t < tiempoActivo)
        {
            t += Time.deltaTime;
            tiempoFase = t;

            yield return null;
        }

        generadorDiscos.DesactivarOverdrive();

        // Salida
        faseEvento = 3;
        tiempoFase = 0f;
        t = 0f;

        while (t < tiempoSalida)
        {
            t += Time.deltaTime;
            tiempoFase = t;

            panelOverdrive.color =
                Color.Lerp(
                    colorEvento,
                    colorOriginal,
                    t / tiempoSalida);

            yield return null;
        }

        panelOverdrive.color = colorOriginal;

        eventoActivo = false;
        overdriveActivo = false;

        faseEvento = 0;
        tiempoFase = 0f;
        rutinaEvento = null;
    }

    IEnumerator Blackout()
    {
        eventoActivo = true;
        overdriveActivo = false;
        blackoutActivo = true;

        faseEvento = 1;
        tiempoFase = 0f;

        float t = 0f;

        //-----------------------
        // Preparaci�n
        //-----------------------

        blackoutLight.gameObject.SetActive(true);
        blackoutLight.intensity = 0f;

        //-----------------------
        // Entrada
        //-----------------------

        while (t < tiempoEntrada)
        {
            t += Time.deltaTime;
            tiempoFase = t;

            // Global Light
            globalLight.intensity =
                Mathf.Lerp(
                    intensidadInicialGlobal,
                    intensidadFinalGlobal,
                    t / tiempoEntrada);

            // Freeform Light
            blackoutLight.intensity =
                Mathf.Lerp(
                    0f,
                    intensidadInicialFreeform,
                    t / tiempoEntrada);

            yield return null;
        }

        globalLight.intensity = intensidadFinalGlobal;
        blackoutLight.intensity = intensidadInicialFreeform;

        faseEvento = 2;
        tiempoFase = 0f;

        //-----------------------
        // Evento activo
        //-----------------------

        t = 0f;

        while (t < tiempoActivo)
        {
            t += Time.deltaTime;
            tiempoFase = t;

            yield return null;
        }

        //-----------------------
        // Salida
        //-----------------------

        faseEvento = 3;
        tiempoFase = 0f;

        t = 0f;

        while (t < tiempoSalida)
        {
            t += Time.deltaTime;
            tiempoFase = t;

            // Global Light
            globalLight.intensity =
                Mathf.Lerp(
                    intensidadFinalGlobal,
                    intensidadInicialGlobal,
                    t / tiempoSalida);

            // Freeform Light
            blackoutLight.intensity =
                Mathf.Lerp(
                    intensidadInicialFreeform,
                    0f,
                    t / tiempoSalida);

            yield return null;
        }

        //-----------------------
        // Restaurar estado
        //-----------------------

        globalLight.intensity = intensidadInicialGlobal;

        blackoutLight.intensity = 0f;
        blackoutLight.gameObject.SetActive(false);

        eventoActivo = false;
        blackoutActivo = false;

        faseEvento = 0;
        tiempoFase = 0f;

        rutinaEvento = null;
    }

    public void EjecutarOverdrive()
    {
        if (eventoActivo)
            return;

        rutinaEvento = StartCoroutine(Overdrive());
    }

    public void EjecutarBlackout()
    {
        if (eventoActivo)
            return;

        rutinaEvento = StartCoroutine(Blackout());
    }

    public void GuardarEstadoEvento()
    {
        //--------------------------
        // Estado com�n
        //--------------------------

        PlayerPrefs.SetInt(
            PrefijoEvento + "EventoActivo",
            eventoActivo ? 1 : 0);

        PlayerPrefs.SetInt(
            PrefijoEvento + "Overdrive",
            overdriveActivo ? 1 : 0);

        PlayerPrefs.SetInt(
            PrefijoEvento + "Blackout",
            blackoutActivo ? 1 : 0);

        PlayerPrefs.SetInt(
            PrefijoEvento + "Fase",
            faseEvento);

        PlayerPrefs.SetFloat(
            PrefijoEvento + "TiempoFase",
            tiempoFase);

        //--------------------------
        // Estado espec�fico
        //--------------------------

        if (overdriveActivo)
        {
            GuardarOverdrive();
        }

        if (blackoutActivo)
        {
            GuardarBlackout();
        }

        PlayerPrefs.Save();

        Debug.Log("===== GUARDANDO EVENTO =====");
        Debug.Log("EventoActivo: " + eventoActivo);
        Debug.Log("Overdrive: " + overdriveActivo);
        Debug.Log("Blackout: " + blackoutActivo);
        Debug.Log("Fase: " + faseEvento);
        Debug.Log("TiempoFase: " + tiempoFase);
    }

    //==========================================
    // GUARDADO ESPEC�FICO
    //==========================================

    private void GuardarOverdrive()
    {
        PlayerPrefs.SetFloat(
            PrefijoEvento + "ColorR",
            panelOverdrive.color.r);

        PlayerPrefs.SetFloat(
            PrefijoEvento + "ColorG",
            panelOverdrive.color.g);

        PlayerPrefs.SetFloat(
            PrefijoEvento + "ColorB",
            panelOverdrive.color.b);

        PlayerPrefs.SetFloat(
            PrefijoEvento + "ColorA",
            panelOverdrive.color.a);
    }

    private void GuardarBlackout()
    {
        PlayerPrefs.SetFloat(
            PrefijoEvento + "GlobalIntensity",
            globalLight.intensity);

        PlayerPrefs.SetFloat(
            PrefijoEvento + "FreeformIntensity",
            blackoutLight.intensity);

        PlayerPrefs.SetInt(
            PrefijoEvento + "BlackoutGO",
            blackoutLight.gameObject.activeSelf ? 1 : 0);
    }

    public void RestaurarEstadoEvento()
    {
        //--------------------------
        // Estado com�n
        //--------------------------

        eventoActivo =
            PlayerPrefs.GetInt(
                PrefijoEvento + "EventoActivo",
                0) == 1;

        overdriveActivo =
            PlayerPrefs.GetInt(
                PrefijoEvento + "Overdrive",
                0) == 1;

        blackoutActivo =
            PlayerPrefs.GetInt(
                PrefijoEvento + "Blackout",
                0) == 1;

        faseEvento =
            PlayerPrefs.GetInt(
                PrefijoEvento + "Fase",
                0);

        tiempoFase =
            PlayerPrefs.GetFloat(
                PrefijoEvento + "TiempoFase",
                0f);

        //--------------------------
        // Restaurar estado espec�fico
        //--------------------------

        if (overdriveActivo)
        {
            RestaurarOverdrive();
        }
        else
        {
            panelOverdrive.color = Color.white;
        }

        if (blackoutActivo)
        {
            RestaurarBlackout();
        }
        else
        {
            globalLight.intensity = intensidadInicialGlobal;

            blackoutLight.intensity = 0f;
            blackoutLight.gameObject.SetActive(false);
        }

        Debug.Log("===== RESTAURANDO EVENTO =====");
        Debug.Log(
            "Evento=" + eventoActivo +
            " | Overdrive=" + overdriveActivo +
            " | Blackout=" + blackoutActivo +
            " | Fase=" + faseEvento +
            " | Global=" + globalLight.intensity +
            " | BlackoutLight=" + blackoutLight.intensity +
            " | BlackoutGO=" + blackoutLight.gameObject.activeSelf);
    }

    //==========================================
    // RESTAURACI�N ESPEC�FICA
    //==========================================

    private void RestaurarOverdrive()
    {
        Color colorGuardado = new Color(
            PlayerPrefs.GetFloat(PrefijoEvento + "ColorR", 1f),
            PlayerPrefs.GetFloat(PrefijoEvento + "ColorG", 1f),
            PlayerPrefs.GetFloat(PrefijoEvento + "ColorB", 1f),
            PlayerPrefs.GetFloat(PrefijoEvento + "ColorA", 0f)
        );

        panelOverdrive.color = colorGuardado;
    }

    private void RestaurarBlackout()
    {
        globalLight.intensity =
            PlayerPrefs.GetFloat(
                PrefijoEvento + "GlobalIntensity",
                intensidadInicialGlobal);

        blackoutLight.gameObject.SetActive(true);

        blackoutLight.intensity =
            PlayerPrefs.GetFloat(
                PrefijoEvento + "FreeformIntensity",
                0f);

        ActualizarBlackout();
    }

    //==========================================
    // RESTAURAR VISUAL
    //==========================================

    private void RestaurarVisualOverdrive()
    {
        RestaurarOverdrive();
    }

    private void RestaurarVisualBlackout()
    {
        RestaurarBlackout();
    }

    public void ContinuarEvento()
    {
        if (!eventoActivo)
            return;

        if (rutinaEvento != null)
            StopCoroutine(rutinaEvento);

        //--------------------------
        // Overdrive
        //--------------------------

        if (overdriveActivo)
        {
            RestaurarVisualOverdrive();

            rutinaEvento =
                StartCoroutine(
                    ContinuarOverdrive());
        }

        //--------------------------
        // Blackout
        //--------------------------

        else if (blackoutActivo)
        {
            RestaurarVisualBlackout();

            rutinaEvento =
                StartCoroutine(
                    ContinuarBlackout());
        }
    }

    IEnumerator ContinuarOverdrive()
    {
        Debug.Log(">>> Continuando Overdrive");

        // El panel YA fue restaurado con su color exacto
        Color colorActual = panelOverdrive.color;
        Color colorEvento = new Color(1f, 0f, 0f, 1f);

        //---------------------------------------
        // FASE 1 : ENTRADA
        //---------------------------------------

        if (faseEvento == 1)
        {
            float tiempoRestante = tiempoEntrada - tiempoFase;

            float t = 0f;

            while (t < tiempoRestante)
            {
                t += Time.deltaTime;
                tiempoFase += Time.deltaTime;

                panelOverdrive.color =
                    Color.Lerp(
                        colorActual,
                        colorEvento,
                        t / tiempoRestante);

                yield return null;
            }

            panelOverdrive.color = colorEvento;

            faseEvento = 2;
            tiempoFase = 0f;
        }

        //---------------------------------------
        // FASE 2 : ACTIVO
        //---------------------------------------

        if (faseEvento == 2)
        {
            generadorDiscos.ActivarOverdrive(multiplicadorOverdrive);

            float tiempoRestante = tiempoActivo - tiempoFase;

            float t = 0f;

            while (t < tiempoRestante)
            {
                t += Time.deltaTime;
                tiempoFase += Time.deltaTime;

                yield return null;
            }

            generadorDiscos.DesactivarOverdrive();

            faseEvento = 3;
            tiempoFase = 0f;
            panelOverdrive.color = colorEvento;

            yield return null;
        }

        //---------------------------------------
        // FASE 3 : SALIDA
        //---------------------------------------
        Debug.Log("Entr� a FASE 3");
        if (faseEvento == 3)
        {
            float tiempoRestante = tiempoSalida - tiempoFase;
            Debug.Log("Salida " + tiempoFase);
            float t = 0f;

            Color colorActualSalida = panelOverdrive.color;
            Color colorFinal = Color.white;

            while (t < tiempoRestante)
            {
                t += Time.deltaTime;
                tiempoFase += Time.deltaTime;

                panelOverdrive.color =
                    Color.Lerp(
                        colorActualSalida,
                        colorFinal,
                        t / tiempoRestante);

                yield return null;
            }

            panelOverdrive.color = Color.white;
        }

        //---------------------------------------
        // LIMPIEZA
        //---------------------------------------

        eventoActivo = false;
        overdriveActivo = false;

        faseEvento = 0;
        tiempoFase = 0f;

        rutinaEvento = null;

        Debug.Log(">>> Overdrive finalizado");
    }

    IEnumerator ContinuarBlackout()
    {
        Debug.Log(">>> Continuando Blackout");

        Debug.Log("CONTINUAR -> intensidadInicialGlobal = "
            + intensidadInicialGlobal);

        // El panel YA fue restaurado con su color exacto
        Color colorActual = panelOverdrive.color;
        Color colorEvento = new Color(1f, 0f, 0f, 0.8f);

        //---------------------------------------
        // FASE 1 : ENTRADA
        //---------------------------------------

        if (faseEvento == 1)
        {
            float tiempoRestante =
                tiempoEntrada - tiempoFase;

            float t = 0f;

            blackoutLight.gameObject.SetActive(true);

            while (t < tiempoRestante)
            {
                t += Time.deltaTime;
                tiempoFase += Time.deltaTime;

                float progreso =
                    (tiempoFase / tiempoEntrada);

                globalLight.intensity =
                    Mathf.Lerp(
                        intensidadInicialGlobal,
                        intensidadFinalGlobal,
                        progreso);

                blackoutLight.intensity =
                    Mathf.Lerp(
                        0f,
                        intensidadInicialFreeform,
                        progreso);

                yield return null;
            }

            globalLight.intensity =
                intensidadFinalGlobal;

            blackoutLight.intensity =
                intensidadInicialFreeform;

            faseEvento = 2;
            tiempoFase = 0f;
        }

        //---------------------------------------
        // FASE 2 : ACTIVO
        //---------------------------------------

        if (faseEvento == 2)
        {
            float tiempoRestante =
                tiempoActivo - tiempoFase;

            float t = 0f;

            while (t < tiempoRestante)
            {
                t += Time.deltaTime;
                tiempoFase += Time.deltaTime;

                yield return null;
            }

            faseEvento = 3;
            tiempoFase = 0f;

            yield return null;
        }

        //---------------------------------------
        // FASE 3 : SALIDA
        //---------------------------------------
        if (faseEvento == 3)
        {
            float tiempoRestante =
                tiempoSalida - tiempoFase;

            float t = 0f;

            while (t < tiempoRestante)
            {
                t += Time.deltaTime;
                tiempoFase += Time.deltaTime;

                float progreso =
                    tiempoFase / tiempoSalida;

                globalLight.intensity =
                    Mathf.Lerp(
                        intensidadFinalGlobal,
                        intensidadInicialGlobal,
                        progreso);

                blackoutLight.intensity =
                    Mathf.Lerp(
                        intensidadInicialFreeform,
                        0f,
                        progreso);

                yield return null;
            }
            Debug.Log("ANTES DE RESTAURAR GLOBAL = "
                + intensidadInicialGlobal);
            globalLight.intensity =
                intensidadInicialGlobal;

            blackoutLight.intensity = 0f;
            blackoutLight.gameObject.SetActive(false);
        }

        //---------------------------------------
        // LIMPIEZA
        //---------------------------------------

        eventoActivo = false;
        blackoutActivo = false;

        faseEvento = 0;
        tiempoFase = 0f;

        rutinaEvento = null;

        Debug.Log(">>> Blackout finalizado");
    }

    public void ReiniciarEvento()
    {
        //---------------------------------------
        // Detener corrutina si existe
        //---------------------------------------

        if (rutinaEvento != null)
        {
            StopCoroutine(rutinaEvento);
            rutinaEvento = null;
        }

        //---------------------------------------
        // Restaurar panel
        //---------------------------------------

        panelOverdrive.color = Color.white;

        //---------------------------------------
        // Restaurar discos
        //---------------------------------------

        generadorDiscos.DesactivarOverdrive();

        // Restaurar luces del Blackout

        globalLight.intensity =
            intensidadInicialGlobal;

        blackoutLight.intensity = 0f;
        blackoutLight.gameObject.SetActive(false);

        //---------------------------------------
        // Limpiar estados
        //---------------------------------------

        eventoActivo = false;
        overdriveActivo = false;
        blackoutActivo = false;

        faseEvento = 0;
        tiempoFase = 0f;
    }
}
