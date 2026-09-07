using System;
using System.Threading.Tasks;
using Unity.Services.Leaderboards;
using UnityEngine;

public class RankingOnline : MonoBehaviour
{
    //========================================
    // INSTANCIA
    //========================================

    public static RankingOnline Instancia;


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

        // Esperar a que termine todo el proceso
        // de inicialización y autenticación
        await SistemaAutenticacion.Instancia.Inicializacion;

        // Ahora sí comprobamos
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
    // ENVIAR PUNTUACIÓN
    //========================================

    public async Task<bool> EnviarPuntuacion(
        string leaderboardId,
        double puntuacion)
    {
        if (!Disponible)
        {
            Debug.LogWarning(
                "RankingOnline -> El sistema no está disponible."
            );

            return false;
        }

        try
        {
            await LeaderboardsService.Instance
                .AddPlayerScoreAsync(
                    leaderboardId,
                    puntuacion
                );

            Debug.Log(
                "RankingOnline -> Puntuación enviada correctamente."
            );

            Debug.Log(
                "RankingOnline -> Leaderboard: "
                + leaderboardId
            );

            Debug.Log(
                "RankingOnline -> Puntuación: "
                + puntuacion
            );

            return true;
        }
        catch (Exception error)
        {
            Debug.LogError(
                "RankingOnline -> Error al enviar puntuación: "
                + error.Message
            );

            return false;
        }
    }
}