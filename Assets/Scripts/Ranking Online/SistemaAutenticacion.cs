using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class SistemaAutenticacion : MonoBehaviour
{
    //========================================
    // ESTADO
    //========================================

    public static SistemaAutenticacion Instancia;

    public bool Autenticado { get; private set; }

    public string PlayerId
    {
        get
        {
            if (!Autenticado)
                return "";

            return AuthenticationService.Instance.PlayerId;
        }
    }

    // Tarea que representa el proceso de inicialización
    public Task Inicializacion { get; private set; }


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

        // Iniciar autenticación
        Inicializacion = IniciarAutenticacion();
    }


    //========================================
    // AUTENTICACIÓN
    //========================================

    private async Task IniciarAutenticacion()
    {
        try
        {
            // Inicializar Unity Services
            await UnityServices.InitializeAsync();

            // Comprobar si ya existe una sesión
            if (AuthenticationService.Instance.IsSignedIn)
            {
                Autenticado = true;

                Debug.Log(
                    "SistemaAutenticacion -> Sesión existente."
                );

                Debug.Log(
                    "Player ID: "
                    + AuthenticationService.Instance.PlayerId
                );

                return;
            }

            // Iniciar sesión anónima
            await AuthenticationService.Instance
                .SignInAnonymouslyAsync();

            Autenticado = true;

            Debug.Log(
                "SistemaAutenticacion -> Autenticación anónima correcta."
            );

            Debug.Log(
                "Player ID: "
                + AuthenticationService.Instance.PlayerId
            );
        }
        catch (Exception error)
        {
            Autenticado = false;

            Debug.LogError(
                "SistemaAutenticacion -> Error al autenticar: "
                + error.Message
            );
        }
    }
}
