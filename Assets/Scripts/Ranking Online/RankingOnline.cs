using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.Networking;
using Stopwatch = System.Diagnostics.Stopwatch;

public class RankingOnline : MonoBehaviour
{
    public static RankingOnline Instancia;

    [Header("Supabase V2")]
    [SerializeField]
    private string urlRegistrarPartida =
        "https://rtngwptcrecttvbluhgv.supabase.co/functions/v1/registrar-partida-v2";

    [SerializeField]
    private string urlMostrarRanking =
        "https://rtngwptcrecttvbluhgv.supabase.co/functions/v1/mostrar-ranking-v2";

    [SerializeField]
    private string supabasePublishableKey = "";

    private const bool FuncionesRegistroV2Activas = true;
    private const bool FuncionLecturaV2Activa = true;

    public bool UsarFuncionesV2 => FuncionesRegistroV2Activas;
    public bool UsarMostrarRankingV2 => FuncionLecturaV2Activa;

    public bool Disponible { get; private set; }

    private Task<bool> tareaPreparacion;

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;
        DontDestroyOnLoad(gameObject);
    }

    private async void Start()
    {
        await ObtenerTareaPreparacion();
    }

    private Task<bool> ObtenerTareaPreparacion()
    {
        if (tareaPreparacion == null)
        {
            tareaPreparacion = PrepararSistemaAsync();
        }

        return tareaPreparacion;
    }

    private async Task<bool> PrepararSistemaAsync()
    {
        SistemaAutenticacion autenticacion = SistemaAutenticacion.Instancia;
        if (autenticacion == null)
        {
            Debug.LogWarning("RankingOnline V2 -> SistemaAutenticacion no encontrado.");
            Disponible = false;
            return false;
        }

        try
        {
            if (autenticacion.Inicializacion != null)
            {
                await autenticacion.Inicializacion;
            }

            Disponible = autenticacion.Autenticado &&
                !string.IsNullOrEmpty(autenticacion.PlayerId);

            if (!Disponible)
            {
                Debug.LogWarning("RankingOnline V2 -> Unity Authentication no está disponible.");
            }

            return Disponible;
        }
        catch (Exception error)
        {
            Disponible = false;
            Debug.LogError("RankingOnline V2 -> Falló la preparación de autenticación: " + error.Message);
            return false;
        }
    }

    public async Task<bool> RegistrarPartida(
        string nombre,
        int tiempo,
        string nivel,
        bool? usarV2 = null)
    {
        bool usarVersionV2 = usarV2 ?? FuncionesRegistroV2Activas;
        string version = usarVersionV2 ? "V2" : "V1";
        string endpoint = usarVersionV2
            ? urlRegistrarPartida
            : urlRegistrarPartida.Replace("registrar-partida-v2", "registrar-partida");
        long inicioTotal = Stopwatch.GetTimestamp();
        long inicioAuth = Stopwatch.GetTimestamp();
        bool disponible = await ObtenerTareaPreparacion();
        double authMs = MilisegundosDesde(inicioAuth);

        if (!disponible || SistemaAutenticacion.Instancia == null)
        {
            if (usarVersionV2)
            {
                Debug.LogWarning(
                    "RankingOnline V2 -> Registro cancelado: Unity Authentication no terminó correctamente o no hay una sesión disponible.");
            }

            RegistrarMetricas(version + " POST", authMs, 0, 0, 0, inicioTotal, 0, false, "");
            return false;
        }

        string accessToken = null;
        if (usarVersionV2)
        {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                Debug.LogWarning(
                    "RankingOnline V2 -> Registro cancelado: Unity Authentication no tiene una sesión iniciada.");
                RegistrarMetricas(version + " POST", authMs, 0, 0, 0,
                    inicioTotal, 0, false, "");
                return false;
            }

            accessToken = AuthenticationService.Instance.AccessToken;
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                Debug.LogWarning(
                    "RankingOnline V2 -> Registro cancelado: la sesión no tiene un AccessToken disponible.");
                RegistrarMetricas(version + " POST", authMs, 0, 0, 0,
                    inicioTotal, 0, false, "");
                return false;
            }
        }

        long inicioPreparacionPeticion = Stopwatch.GetTimestamp();
        string json;
        if (usarVersionV2)
        {
            json = JsonUtility.ToJson(new DatosPartidaV2
            {
                player_id = SistemaAutenticacion.Instancia.PlayerId,
                nombre = nombre,
                tiempo = tiempo,
                nivel = nivel,
                run_id = ObtenerRunIdV2(nivel)
            });
        }
        else
        {
            json = JsonUtility.ToJson(new DatosPartidaV1
            {
                player_id = SistemaAutenticacion.Instancia.PlayerId,
                nombre = nombre,
                tiempo = tiempo,
                nivel = nivel
            });
        }

        byte[] body = Encoding.UTF8.GetBytes(json);
        long inicioHttp = Stopwatch.GetTimestamp();
        double preRequestMs = MilisegundosDesde(inicioPreparacionPeticion);

        try
        {
            using (UnityWebRequest request = CrearSolicitudRegistro(
                endpoint,
                body,
                usarVersionV2,
                accessToken))
            {
                await request.SendWebRequest();
                double httpMs = MilisegundosDesde(inicioHttp);
                bool success = request.result == UnityWebRequest.Result.Success;

                if (!success)
                {
                    Debug.LogWarning(
                        "RankingOnline " + version + " -> POST falló. HTTP " +
                        request.responseCode + " | " + request.error);
                }

                string edgeTiming = request.GetResponseHeader("Server-Timing") ?? "";
                RegistrarMetricas(version + " POST", authMs, preRequestMs, httpMs, 0,
                    inicioTotal, body.Length, success, edgeTiming);
                return success;
            }
        }
        catch (Exception error)
        {
            RegistrarMetricas(version + " POST", authMs, preRequestMs,
                MilisegundosDesde(inicioHttp), 0, inicioTotal, body.Length, false, "");
            Debug.LogError("RankingOnline " + version + " -> Error de conexión POST: " + error.Message);
            return false;
        }
    }

    /// <summary>
    /// Mantiene el identificador al continuar una partida guardada y crea uno
    /// nuevo cuando el controlador confirma que comienza una partida nueva.
    /// </summary>
    public void PrepararRunIdV2(bool continuarPartida, string identificadorModo)
    {
        string claveRunId = RunIdV2Lifecycle.PreferenceKeyForMode(identificadorModo);
        string runId = PlayerPrefs.GetString(claveRunId, "");

        runId = RunIdV2Lifecycle.Resolve(
            continuarPartida,
            runId,
            () => Guid.NewGuid().ToString("D"));

        PlayerPrefs.SetString(claveRunId, runId);
        PlayerPrefs.Save();
    }

    private static string ObtenerRunIdV2(string nivel)
    {
        string claveRunId = RunIdV2Lifecycle.PreferenceKeyForMode(nivel);
        string runId = PlayerPrefs.GetString(claveRunId, "");
        runId = RunIdV2Lifecycle.Resolve(
            true,
            runId,
            () => Guid.NewGuid().ToString("D"));
        PlayerPrefs.SetString(claveRunId, runId);
        PlayerPrefs.Save();
        return runId;
    }

    private UnityWebRequest CrearSolicitudRegistro(
        string endpoint,
        byte[] body,
        bool usarVersionV2,
        string accessToken)
    {
        UnityWebRequest request = new UnityWebRequest(
            endpoint,
            UnityWebRequest.kHttpVerbPOST);
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("apikey", supabasePublishableKey);

        if (usarVersionV2)
        {
            request.SetRequestHeader(
                "Authorization",
                "Bearer " + accessToken);
        }

        return request;
    }

    public async Task<RespuestaRanking> ObtenerRanking(
        string nivel = "",
        int tiempoMinimo = 0,
        int pagina = 1,
        int cantidad = 5,
        bool? usarV2 = null)
    {
        bool usarVersionV2 = usarV2 ?? FuncionLecturaV2Activa;
        string version = usarVersionV2 ? "V2" : "V1";
        string endpoint = usarVersionV2
            ? urlMostrarRanking
            : urlMostrarRanking.Replace("mostrar-ranking-v2", "mostrar-ranking");
        long inicioTotal = Stopwatch.GetTimestamp();
        long inicioAuth = Stopwatch.GetTimestamp();
        bool disponible = await ObtenerTareaPreparacion();
        double authMs = MilisegundosDesde(inicioAuth);

        if (!disponible)
        {
            RegistrarMetricas(version + " GET", authMs, 0, 0, 0, inicioTotal, 0, false, "");
            return null;
        }

        long inicioPreparacionPeticion = Stopwatch.GetTimestamp();
        List<string> parametros = new List<string>();
        if (!string.IsNullOrWhiteSpace(nivel) && nivel != "Todos")
        {
            parametros.Add("nivel=" + UnityWebRequest.EscapeURL(nivel.Trim()));
        }

        if (tiempoMinimo > 0)
        {
            parametros.Add("tiempo_min=" + tiempoMinimo);
        }

        parametros.Add("pagina=" + Mathf.Max(1, pagina));
        parametros.Add("cantidad=" + Mathf.Clamp(cantidad, 1, 200));

        string url = endpoint + "?" + string.Join("&", parametros);
        long inicioHttp = Stopwatch.GetTimestamp();
        double preRequestMs = MilisegundosDesde(inicioPreparacionPeticion);

        try
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("apikey", supabasePublishableKey);

                await request.SendWebRequest();
                double httpMs = MilisegundosDesde(inicioHttp);

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning(
                        "RankingOnline " + version + " -> GET falló. HTTP " +
                        request.responseCode + " | " + request.error);
                    RegistrarMetricas(version + " GET", authMs, preRequestMs, httpMs, 0,
                        inicioTotal, request.downloadHandler?.data?.Length ?? 0,
                        false, request.GetResponseHeader("Server-Timing") ?? "");
                    return null;
                }

                long inicioParseo = Stopwatch.GetTimestamp();
                string respuesta = request.downloadHandler.text;
                RespuestaRanking ranking =
                    JsonUtility.FromJson<RespuestaRanking>(respuesta);
                double parseoMs = MilisegundosDesde(inicioParseo);

                RegistrarMetricas(version + " GET", authMs, preRequestMs, httpMs, parseoMs,
                    inicioTotal, request.downloadHandler.data?.Length ?? 0,
                    ranking != null, request.GetResponseHeader("Server-Timing") ?? "");
                return ranking;
            }
        }
        catch (Exception error)
        {
            RegistrarMetricas(version + " GET", authMs, preRequestMs,
                MilisegundosDesde(inicioHttp), 0, inicioTotal, 0, false, "");
            Debug.LogError("RankingOnline " + version + " -> Error de conexión GET: " + error.Message);
            return null;
        }
    }

    private static double MilisegundosDesde(long inicio)
    {
        return (Stopwatch.GetTimestamp() - inicio) * 1000d / Stopwatch.Frequency;
    }

    private static void RegistrarMetricas(
        string metodo,
        double authMs,
        double preRequestMs,
        double httpMs,
        double parseoMs,
        long inicioTotal,
        int bytes,
        bool success,
        string edgeTiming)
    {
        Debug.Log(
            "RankingOnline métricas -> " + metodo +
            " | auth_wait_ms=" + authMs.ToString("F2") +
            " | pre_request_ms=" + preRequestMs.ToString("F2") +
            " | http_ms=" + httpMs.ToString("F2") +
            " | parse_ms=" + parseoMs.ToString("F2") +
            " | total_ms=" + MilisegundosDesde(inicioTotal).ToString("F2") +
            " | payload_bytes=" + bytes +
            " | ok=" + success +
            " | edge_timing=" + edgeTiming);
    }

    [Serializable]
    private class DatosPartidaV1
    {
        public string player_id;
        public string nombre;
        public int tiempo;
        public string nivel;
    }

    [Serializable]
    private class DatosPartidaV2
    {
        public string player_id;
        public string nombre;
        public int tiempo;
        public string nivel;
        public string run_id;
    }

    [Serializable]
    public class ResultadoRanking
    {
        public int posicion;
        public string nombre;
        public string nivel;
        public int tiempo;
        public string created_at;
    }

    [Serializable]
    public class RespuestaRanking
    {
        public ResultadoRanking[] resultados;
        public int pagina;
        public int cantidad;
        public int total;
        public int total_paginas;
        public bool has_more;
    }

    public async void ProbarRegistroDePartida()
    {
        bool resultado = await RegistrarPartida("PruebaUnity", 523, "Nivel 1");
        Debug.Log(resultado
            ? "RankingOnline V2 -> Prueba de registro exitosa."
            : "RankingOnline V2 -> Prueba de registro fallida.");
    }

    public async void ProbarMostrarRanking()
    {
        RespuestaRanking ranking = await ObtenerRanking("", 0, 1, 5, true);
        if (ranking == null)
        {
            Debug.LogWarning("RankingOnline V2 -> Prueba de ranking fallida.");
            return;
        }

        Debug.Log(
            "RankingOnline V2 -> Prueba de ranking exitosa. Resultados: " +
            (ranking.resultados?.Length ?? 0) +
            " | página " + ranking.pagina + "/" + ranking.total_paginas);
    }

    public async void ProbarComparacionRankingV1V2()
    {
        RespuestaRanking v1 = await ObtenerRanking("", 0, 1, 5, false);
        RespuestaRanking v2 = await ObtenerRanking("", 0, 1, 5, true);

        if (v1 == null || v2 == null)
        {
            Debug.LogWarning("RankingOnline -> Comparación V1/V2 incompleta; revisar métricas HTTP.");
            return;
        }

        bool iguales = v1.total == v2.total &&
            v1.resultados != null &&
            v2.resultados != null &&
            v1.resultados.Length == v2.resultados.Length;

        if (iguales)
        {
            for (int i = 0; i < v1.resultados.Length; i++)
            {
                RankingOnline.ResultadoRanking a = v1.resultados[i];
                RankingOnline.ResultadoRanking b = v2.resultados[i];
                if (a.posicion != b.posicion || a.nombre != b.nombre ||
                    a.nivel != b.nivel || a.tiempo != b.tiempo ||
                    a.created_at != b.created_at)
                {
                    iguales = false;
                    break;
                }
            }
        }

        Debug.Log("RankingOnline -> Comparación V1/V2, página 1: " +
            (iguales ? "coincide" : "hay diferencias") +
            " | V1 filas=" + (v1.resultados?.Length ?? 0) +
            " | V2 filas=" + (v2.resultados?.Length ?? 0));
    }
}
