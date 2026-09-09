using TMPro;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using System.Collections;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class Nivel2 : MonoBehaviour
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
    int nivelActual = 2;
    public GameObject panelContinuar;
    bool juegoPausado = false;
    public GeneradorNivel2 generadorDiscos;
    public SistemaEventosNivel2 sistemaEventos;
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

    private bool registroEnCurso = false;

    //Metodo para Contar el Tiempo
    void Start()
    {
        bool continuar =
        PlayerPrefs.GetInt("ContinuarPartida", 0) == 1 &&
        PlayerPrefs.GetInt("PartidaGuardada" + nivelActual, 0) == 1;

        if (continuar)
        {
            // Recuperar el tiempo inmediatamente
            tiempo = PlayerPrefs.GetFloat("TiempoGuardado" + nivelActual);

            generadorDiscos.RestaurarDiscos();
            player.ConfigurarGuardado(nivelActual);
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
            player.ConfigurarGuardado(nivelActual);
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

    void Update()
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

        Debug.Log("Guardado en memoria. ¿Pantalla completa?: " + activado);
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

    public async void TerminarPartida()
    {
        // ========================================== 
        // PREVENIR MULTI-CLICK 
        // ==========================================

        if (registroEnCurso)
        {
            Debug.LogWarning("Nivel1 -> Operación de ranking ya en curso. Click ignorado.");
            return;
        }

        registroEnCurso = true;

        // ========================================== 
        // BLOQUEAR FLUJO DEL MENÚ 
        // ==========================================

        BotonMenu.SetActive(false);
        Menupanel.SetActive(true);

        //========================================== 
        // GUARDAR RESULTADO PARA EL RANKING LOCAL 
        //==========================================

        string nombreJugador = PlayerPrefs.GetString("NombreJugador");
        int tiempoFinal = Mathf.FloorToInt(tiempo);
        PlayerPrefs.SetString("UltimoNombre", nombreJugador);
        PlayerPrefs.SetFloat("UltimoTiempo", tiempo);
        PlayerPrefs.SetInt("UltimoNivel", nivelActual);

        //========================================== 
        // REGISTRAR EN RANKING MUNDIAL 
        //==========================================
        if (RankingOnline.Instancia != null)
        {
            Debug.Log("DevLog Ranking -> Iniciando registro mundial. " +
                "Nivel: Nivel " + nivelActual +
                " | Tiempo: " + tiempoFinal +
                " segundos");

            Stopwatch cronometro = Stopwatch.StartNew();

            bool registrado =
                await RankingOnline.Instancia.RegistrarPartida(
                    nombreJugador,
                    tiempoFinal,
                    "Nivel " + nivelActual);

            cronometro.Stop();

            Debug.Log("DevLog Ranking -> Registro mundial terminado. " +
                "Tiempo: " +
                cronometro.ElapsedMilliseconds +
                " ms");

            if (registrado)
            {
                Debug.Log("DevLog Ranking -> Registro mundial EXITOSO.");
            }
            else
            {
                Debug.LogWarning("DevLog Ranking -> Registro mundial FALLIDO. " + "El ranking local se mantiene.");
            }
        }
        else
        {
            Debug.LogWarning("DevLog Ranking -> RankingOnline no encontrado. " + "Solo se guardará el ranking local.");
        }

        //========================================== 
        // CONTINUAR CON EL FLUJO ORIGINAL 
        //==========================================

        PlayerPrefs.SetInt("AbrirMenuNiveles", 1);

        PlayerPrefs.DeleteKey("PartidaGuardada" + nivelActual);
        PlayerPrefs.DeleteKey("TiempoGuardado" + nivelActual);

        PlayerPrefs.DeleteKey("Nivel2_CantidadDiscos");

        for (int i = 0; i < 200; i++)
        {
            PlayerPrefs.DeleteKey("Nivel2_Tipo_" + i);
            PlayerPrefs.DeleteKey("Nivel2_PosX_" + i);
            PlayerPrefs.DeleteKey("Nivel2_PosY_" + i);
            PlayerPrefs.DeleteKey("Nivel2_DirX_" + i);
            PlayerPrefs.DeleteKey("Nivel2_DirY_" + i);
        }
        SceneManager.LoadScene("MainMenu");
    }

    public void GuardarPartida()
    {
        // Indicar que existe una partida guardada
        PlayerPrefs.SetInt("PartidaGuardada" + nivelActual, 1);

        PlayerPrefs.SetFloat("TiempoGuardado" + nivelActual, tiempo);

        generadorDiscos.GuardarDiscos();
        player.GuardarJugador();
        sistemaEventos.GuardarEstadoEvento();

        // Regresar al menú de niveles
        PlayerPrefs.SetInt("AbrirMenuNiveles", 1);

        SceneManager.LoadScene("MainMenu");
    }

    //Botones para Guardar Partida

    public void NoContinuar()
    {
        PlayerPrefs.DeleteKey(
        "PartidaGuardada" + nivelActual);

        PlayerPrefs.DeleteKey(
            "TiempoGuardado" + nivelActual);
        PlayerPrefs.DeleteKey("Nivel2_CantidadDiscos");
        for (int i = 0; i < 200; i++)
        {
            PlayerPrefs.DeleteKey("Nivel2_Tipo_" + i);
            PlayerPrefs.DeleteKey("Nivel2_PosX_" + i);
            PlayerPrefs.DeleteKey("Nivel2_PosY_" + i);
            PlayerPrefs.DeleteKey("Nivel2_DirX_" + i);
            PlayerPrefs.DeleteKey("Nivel2_DirY_" + i);
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

    public void GameOver()
    {
        juegoPausado = true;

        //------------------------------------------------
        // Detener generación
        //------------------------------------------------

        generadorDiscos.DetenerGeneracion();

        //------------------------------------------------
        // Detener eventos
        //------------------------------------------------

        sistemaEventos.ReiniciarEvento();

        //------------------------------------------------
        // Pausar jugador
        //------------------------------------------------

        player.PausarJugador();

        //------------------------------------------------
        // Congelar juego
        //------------------------------------------------

        Time.timeScale = 0f;

        //------------------------------------------------
        // Música
        //------------------------------------------------

        // AudioSource.Stop();

        //------------------------------------------------
        // Mostrar panel
        //------------------------------------------------

        panelGameOver.SetActive(true);

        BotonMenu.SetActive(false);

        //------------------------------------------------
        // Mostrar estadísticas
        //------------------------------------------------

        textoTiempoFinal.text = tiempoTexto.text;

        textoNivelFinal.text = nivelActual.ToString();
    }

    //Pantalla Game Over

    public async void ReintentarNivel()
    {
        // ========================================== 
        // Prevenir multi-click
        // ==========================================

        if (registroEnCurso)
        {
            Debug.LogWarning("Nivel1 -> Operación de ranking ya en curso. Click ignorado.");
            return;
        }

        registroEnCurso = true;

        //------------------------------------------
        // Enviar resultado al Ranking
        //------------------------------------------

        string nombreJugador = PlayerPrefs.GetString("NombreJugador");
        int tiempoFinal = Mathf.FloorToInt(tiempo);
        PlayerPrefs.SetString("UltimoNombre", nombreJugador);
        PlayerPrefs.SetFloat("UltimoTiempo", tiempo);
        PlayerPrefs.SetInt("UltimoNivel", nivelActual);

        //========================================== 
        // Registrar en ranking mundial
        //==========================================

        if (RankingOnline.Instancia != null)
        {
            Debug.Log("DevLog Ranking -> Iniciando registro mundial. " +
                "Nivel: Nivel " + nivelActual +
                " | Tiempo: " + tiempoFinal +
                " segundos");

            Stopwatch cronometro = Stopwatch.StartNew();

            bool registrado =
                await RankingOnline.Instancia.RegistrarPartida(
                    nombreJugador,
                    tiempoFinal,
                    "Nivel " + nivelActual);

            cronometro.Stop();

            Debug.Log("DevLog Ranking -> Registro mundial terminado. " +
                "Tiempo: " +
                cronometro.ElapsedMilliseconds +
                " ms");

            if (registrado)
            {
                Debug.Log("DevLog Ranking -> Registro mundial EXITOSO.");
            }
            else
            {
                Debug.LogWarning("DevLog Ranking -> Registro mundial FALLIDO. " + "El ranking local se mantiene.");
            }
        }
        else
        {
            Debug.LogWarning("DevLog Ranking -> RankingOnline no encontrado. " + "Solo se guardará el ranking local.");
        }

        //------------------------------------------
        // Eliminar partida guardada
        //------------------------------------------

        PlayerPrefs.DeleteKey("PartidaGuardada" + nivelActual);
        PlayerPrefs.DeleteKey("TiempoGuardado" + nivelActual);

        PlayerPrefs.DeleteKey("Nivel2_CantidadDiscos");

        for (int i = 0; i < 200; i++)
        {
            PlayerPrefs.DeleteKey("Nivel2_Tipo_" + i);
            PlayerPrefs.DeleteKey("Nivel2_PosX_" + i);
            PlayerPrefs.DeleteKey("Nivel2_PosY_" + i);
            PlayerPrefs.DeleteKey("Nivel2_DirX_" + i);
            PlayerPrefs.DeleteKey("Nivel2_DirY_" + i);
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

    public async void VolverMenuGameOver()
    {
        // ========================================== 
        // Prevenir multi-click
        // ==========================================

        if (registroEnCurso)
        {
            Debug.LogWarning("Nivel1 -> Operación de ranking ya en curso. Click ignorado.");
            return;
        }

        registroEnCurso = true;

        //------------------------------------------
        // Enviar resultado al Ranking
        //------------------------------------------

        string nombreJugador = PlayerPrefs.GetString("NombreJugador");
        int tiempoFinal = Mathf.FloorToInt(tiempo);
        PlayerPrefs.SetString("UltimoNombre", nombreJugador);
        PlayerPrefs.SetFloat("UltimoTiempo", tiempo);
        PlayerPrefs.SetInt("UltimoNivel", nivelActual);

        //========================================== 
        // Registrar en ranking mundial
        //==========================================

        if (RankingOnline.Instancia != null)
        {
            Debug.Log("DevLog Ranking -> Iniciando registro mundial. " +
                "Nivel: Nivel " + nivelActual +
                " | Tiempo: " + tiempoFinal +
                " segundos");

            Stopwatch cronometro = Stopwatch.StartNew();

            bool registrado =
                await RankingOnline.Instancia.RegistrarPartida(
                    nombreJugador,
                    tiempoFinal,
                    "Nivel " + nivelActual);

            cronometro.Stop();

            Debug.Log("DevLog Ranking -> Registro mundial terminado. " +
                "Tiempo: " +
                cronometro.ElapsedMilliseconds +
                " ms");

            if (registrado)
            {
                Debug.Log("DevLog Ranking -> Registro mundial EXITOSO.");
            }
            else
            {
                Debug.LogWarning("DevLog Ranking -> Registro mundial FALLIDO. " + "El ranking local se mantiene.");
            }
        }
        else
        {
            Debug.LogWarning("DevLog Ranking -> RankingOnline no encontrado. " + "Solo se guardará el ranking local.");
        }

        //------------------------------------------
        // Eliminar partida guardada
        //------------------------------------------

        PlayerPrefs.DeleteKey("PartidaGuardada" + nivelActual);
        PlayerPrefs.DeleteKey("TiempoGuardado" + nivelActual);

        PlayerPrefs.DeleteKey("Nivel2_CantidadDiscos");

        for (int i = 0; i < 200; i++)
        {
            PlayerPrefs.DeleteKey("Nivel2_Tipo_" + i);
            PlayerPrefs.DeleteKey("Nivel2_PosX_" + i);
            PlayerPrefs.DeleteKey("Nivel2_PosY_" + i);
            PlayerPrefs.DeleteKey("Nivel2_DirX_" + i);
            PlayerPrefs.DeleteKey("Nivel2_DirY_" + i);
        }

        //------------------------------------------
        // Abrir menú de niveles
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
        // Ir al menú
        //------------------------------------------

        SceneManager.LoadScene("MainMenu");
    }

    //-----
    public float ObtenerTiempo()
    {
        return tiempo;
    }
}
