using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.VisualScripting;
using System.Collections;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public abstract class NivelBase : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject Menupanel;
    public GameObject BotonMenu;

    // Tema de Tiempo
    public TextMeshProUGUI tiempoTexto;
    private float tiempo = 0f;

    //Tema de Eventos
    public SpriteRenderer panelOverdrive;
    private float SiguienteEvento = 45f;
    [Header("Overdrive")]
    public float multiplicadorOverdrive = 2f;

    //Variables de Opciones
    public Slider soundSlider;
    public AudioMixer masterMixer;
    public Toggle pantallaCompletaToggle;

    //Variables para Guardar Partida;
    protected abstract int NivelActual { get; }
    protected abstract string NombreNivel { get; }
    protected abstract string PrefijoGuardadoDiscos { get; }
    public GameObject panelContinuar;
    bool juegoPausado = false;
    public GeneradorDiscosBase generadorDiscos;
    public SistemaEventosBase sistemaEventos;
    IEnumerator RestaurarEvento()
    {
        yield return null;

        sistemaEventos.RestaurarEstadoEvento();

        if (sistemaEventos.OverdriveActivo())
        {
            generadorDiscos.MostrarOverdriveVisual();
        }
    }

    [Header("Player")]
    public Player player;

    [Header("Game Over")]
    public GameObject panelGameOver;

    public TextMeshProUGUI textoTiempoFinal;
    public TextMeshProUGUI textoNivelFinal;

    private bool finalizacionIniciada = false;

    //Metodo para Contar el Tiempo
    protected virtual void Start() 
    {
        bool continuar =
        PlayerPrefs.GetInt("ContinuarPartida", 0) == 1 &&
        PlayerPrefs.GetInt("PartidaGuardada" + NivelActual, 0) == 1;

        if (RankingOnline.Instancia != null)
        {
            RankingOnline.Instancia.PrepararRunIdV2(continuar, NombreNivel);
        }

        if (continuar)
        {
            // Recuperar el tiempo inmediatamente
            tiempo = PlayerPrefs.GetFloat("TiempoGuardado" + NivelActual);

            generadorDiscos.RestaurarDiscos();
            player.ConfigurarGuardado(NivelActual);
            player.RestaurarJugador();
            player.PausarJugador();
            StartCoroutine(RestaurarEvento());

            // Mostrar panel
            panelContinuar.SetActive(true);

            // Calcular el siguiente evento
            SiguienteEvento = Mathf.Floor(tiempo / 45f) * 45f + 45f;

            // Congelar el juego
            juegoPausado = true;
            BotonMenu.SetActive(false);
        }
        else
        {
            panelContinuar.SetActive(false);
            player.ConfigurarGuardado(NivelActual);
            player.InicializarJugador();
            // Partida nueva: comenzar a generar discos
            generadorDiscos.IniciarGeneracion();
        }

        Time.timeScale = 1f;

        //Buscamos si hay volumen guardado
        float volumenGuardado = PlayerPrefs.GetFloat("VolumenDelJuego", 1f);

        //Movemos el slider al volumen guardado
        if (soundSlider != null)
        {
            soundSlider.value = volumenGuardado;
        }

        float decibelios = Mathf.Log10(volumenGuardado) * 20;
        masterMixer.SetFloat("MasterVolume", decibelios);

        //Pantalla Completa Inicial
        int pantallaGuardada = PlayerPrefs.GetInt("PantallaGuardada", 0);

        bool esCompleta = (pantallaGuardada == 1);

        if (pantallaCompletaToggle != null)
        {
            pantallaCompletaToggle.isOn = esCompleta;
        }
        Screen.fullScreen = esCompleta;

        //==========================================
        // EVENTO DE MUERTE DEL PLAYER
        //==========================================

        player.OnPlayerMuerto += GameOver;
    }
    
    protected virtual void Update() 
    {
        if (!juegoPausado)
        {
            tiempo += Time.deltaTime;
        }

        int minutos = Mathf.FloorToInt(tiempo / 60);
        int segundos = Mathf.FloorToInt(tiempo % 60);
        tiempoTexto.text = string.Format("{0:00}:{1:00}", minutos, segundos);

        if (tiempo >= SiguienteEvento)
        {
            int evento = Random.Range(0, 2);
            Debug.Log("Evento elegido = " + evento);

            if (evento == 0)
            {
                Debug.Log("Voy a ejecutar Overdrive");
                sistemaEventos.EjecutarOverdrive();
            }
            else
            {
                Debug.Log("Voy a ejecutar Blackout");
                sistemaEventos.EjecutarBlackout();
            }

            SiguienteEvento += 45f;
        }

        //==========================================
        // PRUEBA DE HABILIDADES
        //==========================================

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            player.ActivarTeleport();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            player.ActivarRalentizacion();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            player.ActivarMuro();
        }
    }

    public void CambiarVolumen(float volumen)
    {
        float decibelios = Mathf.Log10(volumen) * 20;
        masterMixer.SetFloat("MasterVolume", decibelios);

        //Guardar Cambio
        PlayerPrefs.SetFloat("VolumenDelJuego", volumen);
        PlayerPrefs.Save();
        Debug.Log("El volumen actual en dB es: " + decibelios);
    }

    public void CambiarPantallaCompleta(bool activado)
    {
        Screen.fullScreen = activado;

        PlayerPrefs.SetInt("PantallaGuardada", activado ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log("Guardado en memoria. �Pantalla completa?: " + activado);
    }


    //Boton para abrir el panel
    public void OpenMenuPanel()
    {
        generadorDiscos.ActualizarListaDiscos();

        BotonMenu.SetActive(false);
        Time.timeScale = 0f;
        Menupanel.SetActive(true);
    }

    //Botones de Panel Menu
    public void VolverAlNivel()
    {
        Menupanel.SetActive(false);
        Time.timeScale = 1f;
        BotonMenu.SetActive(true);
    }

    public void TerminarPartida()
    {
        if (finalizacionIniciada) return;

        Menupanel.SetActive(false);
        GameOver();
    }

    public void GuardarPartida()
    {
        // Indicar que existe una partida guardada
        PlayerPrefs.SetInt("PartidaGuardada" + NivelActual, 1);

        PlayerPrefs.SetFloat("TiempoGuardado" + NivelActual, tiempo);

        generadorDiscos.GuardarDiscos();
        player.GuardarJugador();
        sistemaEventos.GuardarEstadoEvento();

        // Regresar al men� de niveles
        PlayerPrefs.SetInt("AbrirMenuNiveles", 1);

        SceneManager.LoadScene("MainMenu");
    }

    //Botones para Guardar Partida

    public void NoContinuar()
    {
        PlayerPrefs.DeleteKey(
        "PartidaGuardada" + NivelActual);

        PlayerPrefs.DeleteKey(
            "TiempoGuardado" + NivelActual);
        if (RankingOnline.Instancia != null)
        {
            RankingOnline.Instancia.PrepararRunIdV2(false, NombreNivel);
        }
        PlayerPrefs.DeleteKey(PrefijoGuardadoDiscos + "CantidadDiscos");
        for (int i = 0; i < 200; i++)
        {
            PlayerPrefs.DeleteKey(PrefijoGuardadoDiscos + "Tipo_" + i);
            PlayerPrefs.DeleteKey(PrefijoGuardadoDiscos + "PosX_" + i);
            PlayerPrefs.DeleteKey(PrefijoGuardadoDiscos + "PosY_" + i);
            PlayerPrefs.DeleteKey(PrefijoGuardadoDiscos + "DirX_" + i);
            PlayerPrefs.DeleteKey(PrefijoGuardadoDiscos + "DirY_" + i);
        }
        generadorDiscos.LimpiarDiscos();

        juegoPausado = false;
        panelContinuar.SetActive(false);

        sistemaEventos.ReiniciarEvento();
        tiempo = 0f;
        SiguienteEvento = 45f;
        player.InicializarJugador();
        player.RestaurarPosicionInicial();
        player.ReanudarJugador();
        generadorDiscos.IniciarGeneracion();
        BotonMenu.SetActive(true);
    }

    public void SeguirPartida()
    {
        if (sistemaEventos.OverdriveActivo())
        {
            generadorDiscos.OcultarOverdriveVisual();
        }

        generadorDiscos.IniciarGeneracion();
        generadorDiscos.ReanudarDiscos();

        sistemaEventos.ContinuarEvento();

        player.ReanudarJugador();

        panelContinuar.SetActive(false);
        juegoPausado = false;
        BotonMenu.SetActive(true);
    }

    public void VolveryGuardar() 
    {
        PlayerPrefs.SetInt("AbrirMenuNiveles", 1);
        SceneManager.LoadScene("MainMenu");
    }

    public async void GameOver()
    {
        if (finalizacionIniciada) return;
        finalizacionIniciada = true;

        juegoPausado = true;
        generadorDiscos.DetenerGeneracion();
        sistemaEventos.ReiniciarEvento();
        player.PausarJugador();
        Time.timeScale = 0f;

        string nombreJugador = PlayerPrefs.GetString("NombreJugador");
        int tiempoFinal = Mathf.FloorToInt(tiempo);
        PlayerPrefs.SetString("UltimoNombre", nombreJugador);
        PlayerPrefs.SetFloat("UltimoTiempo", tiempo);
        PlayerPrefs.SetInt("UltimoNivel", NivelActual);
        PlayerPrefs.DeleteKey("PartidaGuardada" + NivelActual);
        PlayerPrefs.DeleteKey("TiempoGuardado" + NivelActual);
        PlayerPrefs.DeleteKey(PrefijoGuardadoDiscos + "CantidadDiscos");
        for (int i = 0; i < 200; i++)
        {
            PlayerPrefs.DeleteKey(PrefijoGuardadoDiscos + "Tipo_" + i);
            PlayerPrefs.DeleteKey(PrefijoGuardadoDiscos + "PosX_" + i);
            PlayerPrefs.DeleteKey(PrefijoGuardadoDiscos + "PosY_" + i);
            PlayerPrefs.DeleteKey(PrefijoGuardadoDiscos + "DirX_" + i);
            PlayerPrefs.DeleteKey(PrefijoGuardadoDiscos + "DirY_" + i);
        }
        PlayerPrefs.Save();
        panelGameOver.SetActive(true);
        Menupanel.SetActive(false);
        BotonMenu.SetActive(false);
        textoTiempoFinal.text = tiempoTexto.text;
        textoNivelFinal.text = NivelActual.ToString();
        if (RankingOnline.Instancia == null)
        {
            Debug.LogWarning("Game Over -> RankingOnline no encontrado. Solo se guardo el resultado local.");
            return;
        }

        Stopwatch cronometro = Stopwatch.StartNew();
        bool registrado = await RankingOnline.Instancia.RegistrarPartida(
            nombreJugador,
            tiempoFinal,
            NombreNivel);
        cronometro.Stop();

        Debug.Log("Game Over -> Registro V2/V1 terminado en " + cronometro.ElapsedMilliseconds + " ms.");
        if (!registrado)
        {
            Debug.LogWarning("Game Over -> No se pudo registrar la partida; el resultado local se conserva.");
        }
    }

    //Pantalla Game Over

    public void ReintentarNivel()
    {
        if (!finalizacionIniciada) return;

        // Eliminar partida guardada
        //------------------------------------------

        PlayerPrefs.DeleteKey("PartidaGuardada" + NivelActual);
        PlayerPrefs.DeleteKey("TiempoGuardado" + NivelActual);

        PlayerPrefs.DeleteKey(PrefijoGuardadoDiscos + "CantidadDiscos");

        for (int i = 0; i < 200; i++)
        {
            PlayerPrefs.DeleteKey(PrefijoGuardadoDiscos + "Tipo_" + i);
            PlayerPrefs.DeleteKey(PrefijoGuardadoDiscos + "PosX_" + i);
            PlayerPrefs.DeleteKey(PrefijoGuardadoDiscos + "PosY_" + i);
            PlayerPrefs.DeleteKey(PrefijoGuardadoDiscos + "DirX_" + i);
            PlayerPrefs.DeleteKey(PrefijoGuardadoDiscos + "DirY_" + i);
        }

        PlayerPrefs.Save();

        //------------------------------------------
        // Reanudar tiempo
        //------------------------------------------

        Time.timeScale = 1f;

        //------------------------------------------
        // Reiniciar nivel
        //------------------------------------------

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void VolverMenuGameOver()
    {
        if (!finalizacionIniciada) return;

        // Eliminar partida guardada
        //------------------------------------------

        PlayerPrefs.DeleteKey("PartidaGuardada" + NivelActual);
        PlayerPrefs.DeleteKey("TiempoGuardado" + NivelActual);

        PlayerPrefs.DeleteKey(PrefijoGuardadoDiscos + "CantidadDiscos");

        for (int i = 0; i < 200; i++)
        {
            PlayerPrefs.DeleteKey(PrefijoGuardadoDiscos + "Tipo_" + i);
            PlayerPrefs.DeleteKey(PrefijoGuardadoDiscos + "PosX_" + i);
            PlayerPrefs.DeleteKey(PrefijoGuardadoDiscos + "PosY_" + i);
            PlayerPrefs.DeleteKey(PrefijoGuardadoDiscos + "DirX_" + i);
            PlayerPrefs.DeleteKey(PrefijoGuardadoDiscos + "DirY_" + i);
        }

        //------------------------------------------
        // Abrir men� de niveles
        //------------------------------------------

        PlayerPrefs.SetInt(
            "AbrirMenuNiveles",
            1);

        PlayerPrefs.Save();

        //------------------------------------------
        // Reanudar tiempo
        //------------------------------------------

        Time.timeScale = 1f;

        //------------------------------------------
        // Ir al men�
        //------------------------------------------

        SceneManager.LoadScene("MainMenu");
    }

    //-----
    public float ObtenerTiempo()
    {
        return tiempo;
    }
}
