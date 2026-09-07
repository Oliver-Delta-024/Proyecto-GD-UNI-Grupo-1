using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class RankingOnline : MonoBehaviour
{
    //========================================
    // INSTANCIA
    //========================================

    public static RankingOnline Instancia;


    //========================================
    // CONFIGURACIÓN
    //========================================

    [Header("Supabase")]
    [SerializeField]
    private string urlRegistrarPartida =
        "https://rtngwptcrecttvbluhgv.supabase.co/functions/v1/registrar-partida";

    [SerializeField]
    private string urlMostrarRanking =
        "https://rtngwptcrecttvbluhgv.supabase.co/functions/v1/mostrar-ranking";

    [SerializeField]
    private string supabasePublishableKey = "";


    //========================================
    // ESTADO
    //========================================

    public bool Disponible { get; private set; }


    //========================================
    // UNITY
    //========================================

    private void Awake()
    {
        // Evitar duplicados
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;

        // Mantener entre escenas
        DontDestroyOnLoad(gameObject);
    }


    //========================================
    // INICIALIZAR
    //========================================

    private async void Start()
    {
        await EsperarAutenticacion();
    }


    private async Task EsperarAutenticacion()
    {
        if (SistemaAutenticacion.Instancia == null)
        {
            Debug.LogWarning(
                "RankingOnline -> SistemaAutenticacion no encontrado."
            );

            Disponible = false;
            return;
        }

        // Esperar a que Unity Authentication termine
        await SistemaAutenticacion.Instancia.Inicializacion;

        ComprobarDisponibilidad();
    }


    private void ComprobarDisponibilidad()
    {
        if (SistemaAutenticacion.Instancia == null)
        {
            Debug.LogWarning(
                "RankingOnline -> SistemaAutenticacion no encontrado."
            );

            Disponible = false;
            return;
        }

        if (!SistemaAutenticacion.Instancia.Autenticado)
        {
            Debug.LogWarning(
                "RankingOnline -> La autenticación falló."
            );

            Disponible = false;
            return;
        }

        Disponible = true;

        Debug.Log(
            "RankingOnline -> Sistema preparado."
        );

        Debug.Log(
            "RankingOnline -> Player ID: "
            + SistemaAutenticacion.Instancia.PlayerId
        );
    }


    //========================================
    // REGISTRAR PARTIDA
    //========================================

    public async Task<bool> RegistrarPartida(
        string nombre,
        int tiempo,
        string nivel)
    {
        if (!Disponible)
        {
            Debug.LogWarning(
                "RankingOnline -> El sistema no está disponible."
            );

            return false;
        }

        if (SistemaAutenticacion.Instancia == null)
        {
            Debug.LogWarning(
                "RankingOnline -> SistemaAutenticacion no encontrado."
            );

            return false;
        }

        // Obtener Player ID desde Unity Authentication
        string playerId =
            SistemaAutenticacion.Instancia.PlayerId;

        // Crear datos de la partida
        DatosPartida datos = new DatosPartida
        {
            player_id = playerId,
            nombre = nombre,
            tiempo = tiempo,
            nivel = nivel
        };

        string json = JsonUtility.ToJson(datos);

        try
        {
            using (UnityWebRequest request =
                new UnityWebRequest(
                    urlRegistrarPartida,
                    UnityWebRequest.kHttpVerbPOST))
            {
                byte[] body =
                    Encoding.UTF8.GetBytes(json);

                request.uploadHandler =
                    new UploadHandlerRaw(body);

                request.downloadHandler =
                    new DownloadHandlerBuffer();

                request.SetRequestHeader(
                    "Content-Type",
                    "application/json"
                );

                request.SetRequestHeader(
                    "apikey",
                    supabasePublishableKey
                );

                await request.SendWebRequest();

                if (request.result !=
                    UnityWebRequest.Result.Success)
                {
                    Debug.LogError(
                        "RankingOnline -> Error al registrar partida: "
                        + request.error
                    );

                    Debug.LogError(
                        "RankingOnline -> Respuesta: "
                        + request.downloadHandler.text
                    );

                    return false;
                }

                Debug.Log(
                    "RankingOnline -> Partida registrada correctamente."
                );

                Debug.Log(
                    "RankingOnline -> Respuesta: "
                    + request.downloadHandler.text
                );

                return true;
            }
        }
        catch (Exception error)
        {
            Debug.LogError(
                "RankingOnline -> Error de conexión: "
                + error.Message
            );

            return false;
        }
    }


    //========================================
    // OBTENER RANKING
    //========================================

    public async Task<RespuestaRanking> ObtenerRanking(
        string nivel = "",
        int tiempoMinimo = 0,
        int pagina = 1,
        int cantidad = 5)
    {
        if (!Disponible)
        {
            Debug.LogWarning(
                "RankingOnline -> El sistema no está disponible."
            );

            return null;
        }

        try
        {
            // Construir URL
            string url = urlMostrarRanking;

            List<string> parametros =
                new List<string>();

            if (!string.IsNullOrEmpty(nivel))
            {
                parametros.Add(
                    "nivel="
                    + UnityWebRequest.EscapeURL(nivel)
                );
            }

            if (tiempoMinimo > 0)
            {
                parametros.Add(
                    "tiempo_min="
                    + tiempoMinimo
                );
            }

            parametros.Add(
                "pagina="
                + pagina
            );

            parametros.Add(
                "cantidad="
                + cantidad
            );

            if (parametros.Count > 0)
            {
                url += "?"
                    + string.Join("&", parametros);
            }

            using (UnityWebRequest request =
                UnityWebRequest.Get(url))
            {
                request.downloadHandler =
                    new DownloadHandlerBuffer();

                request.SetRequestHeader(
                    "apikey",
                    supabasePublishableKey
                );

                await request.SendWebRequest();

                if (request.result !=
                    UnityWebRequest.Result.Success)
                {
                    Debug.LogError(
                        "RankingOnline -> Error al obtener ranking: "
                        + request.error
                    );

                    Debug.LogError(
                        "RankingOnline -> Respuesta: "
                        + request.downloadHandler.text
                    );

                    return null;
                }

                string respuesta =
                    request.downloadHandler.text;

                Debug.Log(
                    "RankingOnline -> Ranking recibido."
                );

                Debug.Log(
                    "RankingOnline -> Respuesta: "
                    + respuesta
                );

                RespuestaRanking ranking =
                    JsonUtility.FromJson<RespuestaRanking>(
                        respuesta
                    );

                return ranking;
            }
        }
        catch (Exception error)
        {
            Debug.LogError(
                "RankingOnline -> Error de conexión: "
                + error.Message
            );

            return null;
        }
    }


    //========================================
    // CLASES DE DATOS
    //========================================

    [Serializable]
    private class DatosPartida
    {
        public string player_id;
        public string nombre;
        public int tiempo;
        public string nivel;
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
    }

    //========================================
    // PRUEBA DE CONEXIÓN
    //========================================

    //==================================================
    // PRUEBA 1: REGISTRAR PARTIDA
    //==================================================

    public async void ProbarRegistroDePartida()
    {
        Debug.Log(
            "========================================"
        );

        Debug.Log(
            "RankingOnline -> INICIANDO PRUEBA DE REGISTRO"
        );

        bool resultado = await RegistrarPartida(
            "PruebaUnity",
            523,
            "Nivel 1"
        );

        if (resultado)
        {
            Debug.Log(
                "RankingOnline -> PRUEBA DE REGISTRO EXITOSA"
            );
        }
        else
        {
            Debug.LogError(
                "RankingOnline -> PRUEBA DE REGISTRO FALLIDA"
            );
        }

        Debug.Log(
            "========================================"
        );
    }

    //==================================================
    // PRUEBA 2: MOSTRAR RANKING
    //==================================================

    public async void ProbarMostrarRanking()
    {
        Debug.Log(
            "========================================"
        );

        Debug.Log(
            "RankingOnline -> INICIANDO PRUEBA DE RANKING"
        );

        RespuestaRanking ranking =
            await ObtenerRanking(
                "",
                0,
                1,
                5
            );

        if (ranking == null)
        {
            Debug.LogError(
                "RankingOnline -> PRUEBA DE RANKING FALLIDA"
            );

            Debug.Log(
                "========================================"
            );

            return;
        }

        Debug.Log(
            "RankingOnline -> PRUEBA DE RANKING EXITOSA"
        );

        Debug.Log(
            "RankingOnline -> Total: "
            + ranking.total
        );

        Debug.Log(
            "RankingOnline -> Página: "
            + ranking.pagina
            + " / "
            + ranking.total_paginas
        );

        if (ranking.resultados == null ||
            ranking.resultados.Length == 0)
        {
            Debug.Log(
                "RankingOnline -> No hay resultados."
            );
        }
        else
        {
            foreach (
                ResultadoRanking resultado
                in ranking.resultados)
            {
                Debug.Log(
                    resultado.posicion
                    + ". "
                    + resultado.nombre
                    + " | "
                    + resultado.nivel
                    + " | "
                    + resultado.tiempo
                    + " segundos"
                );
            }
        }

        Debug.Log(
            "========================================"
        );
    }
}