using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class NivelPersonalizado : MonoBehaviour
{
    //Tema de Menu de Nivel
    public GameObject Menupanel;
    public GameObject MenuEditar;
    public GameObject BotonMenu;
    public GameObject BotonEditar;

    //==========================================
    // RESUMEN DE CONFIGURACIÓN - MENÚ
    //==========================================

    [Header("Resumen Configuración - Menú")]

    public TextMeshProUGUI textoResumenDiscosMenu;
    public TextMeshProUGUI textoResumenMusicaMenu;
    public TextMeshProUGUI textoResumenEventosMenu;
    public TextMeshProUGUI textoResumenGeneradoresMenu;


    //==========================================
    // RESUMEN DE CONFIGURACIÓN - GAME OVER
    //==========================================

    [Header("Resumen Configuración - Game Over")]

    public TextMeshProUGUI textoResumenDiscosGameOver;
    public TextMeshProUGUI textoResumenMusicaGameOver;
    public TextMeshProUGUI textoResumenEventosGameOver;
    public TextMeshProUGUI textoResumenGeneradoresGameOver;

    // Tema de Tiempo
    public TextMeshProUGUI tiempoTexto;
    private float tiempo = 0f;

    //Variables de Opciones
    public Slider soundSlider;
    public AudioMixer masterMixer;
    public Toggle pantallaCompletaToggle;

    [Header("Player")]
    public Player player;

    //Música
    public AudioSource audioSource;

    [Header("Música")]
    public AudioClip musicaMenuPrincipal;
    public AudioClip musicaNivel1;
    public AudioClip musicaNivel2;
    public AudioClip musicaNivel3;


    //Eventos
    private bool ovActivado;
    private bool blActivado;
    public SistemaEventosNivelPersonalizado sistemaEventos;
    public SpriteRenderer panelOverdrive;
    public float multiplicadorOverdrive = 2f;

    private int ConfigEventos;
    // Tiempo del próximo evento
    private float SiguienteEvento = 45f;

    //Generador
    public GeneradorControlNivelPersonalizado generadorDiscos;

    //Variables para Guardar Partida;
    private int nivelActual = 4;
    private string nivelPers = "Personalizado";
    public GameObject panelContinuar;
    private bool juegoPausado = false;

    [Header("Game Over")]
    public GameObject panelGameOver;

    public TextMeshProUGUI textoTiempoFinal;
    public TextMeshProUGUI textoNivelFinal;

    IEnumerator IniciarNivel()
    {
        yield return null;

        player.ConfigurarGuardado(nivelActual);
        player.InicializarJugador();
        EstablecerConfiguracionEventos();
        generadorDiscos.IniciarGeneracion();
    }

    IEnumerator RestaurarEvento()
    {
        yield return null;

        sistemaEventos.RestaurarEstadoEvento();

        if (sistemaEventos.OverdriveActivo())
        {
            generadorDiscos.MostrarOverdriveVisual();
        }

        //EstablecerConfiguracionEventos();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //------------------------------------------
        // COMPROBAR PARTIDA GUARDADA
        //------------------------------------------

        bool continuar =
            PlayerPrefs.GetInt("ContinuarPartida", 0) == 1 &&
            PlayerPrefs.GetInt("PartidaGuardada" + nivelActual, 0) == 1;

        //------------------------------------------
        // PARTIDA GUARDADA
        //------------------------------------------

        if (continuar)
        {
            // Recuperar tiempo
            tiempo = PlayerPrefs.GetFloat("TiempoGuardado" + nivelActual);

            // RESTAURAR GENERADORES

            generadorDiscos.RestaurarDiscos();

            // RESTAURAR PLAYER

            player.ConfigurarGuardado(nivelActual);
            player.RestaurarJugador();
            player.PausarJugador();

            // RESTAURAR EVENTO

            StartCoroutine(RestaurarEvento());

            // MOSTRAR PANEL CONTINUAR

            panelContinuar.SetActive(true);

            // CALCULAR SIGUIENTE EVENTO

            SiguienteEvento = Mathf.Floor(tiempo / 45f) * 45f + 45f;

            // PAUSAR PARTIDA

            juegoPausado = true;
            BotonMenu.SetActive(false);
            BotonEditar.SetActive(false);

        }


        //------------------------------------------
        // PARTIDA NUEVA
        //------------------------------------------

        else
        {
            panelContinuar.SetActive(false);

            StartCoroutine(IniciarNivel());
        }

        Time.timeScale = 1f;

        //Buscamos si hay volumen guardado
        float volumenGuardado = PlayerPrefs.GetFloat("VolumenDelJuego", 1f);

        //Movemos el slider al volumen guardado
        if (soundSlider != null)
        {
            soundSlider.value = volumenGuardado;
        }

        int musicaGuardada = PlayerPrefs.GetInt("MusicaElegida", 1);
        PonerMusica(musicaGuardada);

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

    // Update is called once per frame
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
            GenerarEvento();

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

    public void PonerMusica(int indiceMusica)
    {
        // Elige la canción según el valor que le mandó el nivel personalizado
        switch (indiceMusica)
        {
            case 0:
                audioSource.clip = musicaMenuPrincipal;
                break;
            case 1:
                audioSource.clip = musicaNivel1;
                break;
            case 2:
                audioSource.clip = musicaNivel2;
                break;
            case 3:
                audioSource.clip = musicaNivel3;
                break;
        }

        // La reproduce
        audioSource.Play();
    }

    public void CambiarPantallaCompleta(bool activado)
    {
        Screen.fullScreen = activado;

        PlayerPrefs.SetInt("PantallaGuardada", activado ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log("Guardado en memoria. ¿Pantalla completa?: " + activado);
    }

    public void EstablecerConfiguracionEventos()
    {
        ovActivado = PlayerPrefs.GetInt("EvOverdrive", 0) == 1;
        blActivado = PlayerPrefs.GetInt("EvBlackout", 0) == 1;

        // 1. Ambos eventos
        if (ovActivado && blActivado)
        {
            ConfigEventos = 3;
            Debug.Log("Configuración: Ambos eventos activados");
        }
        // 2. Solo Overdrive
        else if (ovActivado && !blActivado)
        {
            ConfigEventos = 1;
            Debug.Log("Configuración: Solo Overdrive");
        }
        // 3. Solo Blackout
        else if (!ovActivado && blActivado)
        {
            ConfigEventos = 2;
            Debug.Log("Configuración: Solo Blackout");
        }
        // 4. Ningún evento
        else
        {
            ConfigEventos = 0;
            Debug.Log("Configuración: Ningún evento");
        }
    }

    public void GenerarEvento()
    {
        // Ningún evento
        if (ConfigEventos == 0)
        {
            Debug.Log("No hay eventos configurados.");
            return;
        }

        // Solo Overdrive
        if (ConfigEventos == 1)
        {
            Debug.Log("Generando Overdrive.");
            sistemaEventos.EjecutarOverdrive();
            return;
        }

        // Solo Blackout
        if (ConfigEventos == 2)
        {
            Debug.Log("Generando Blackout.");
            sistemaEventos.EjecutarBlackout();
            return;
        }

        // Ambos eventos
        if (ConfigEventos == 3)
        {
            int evento = UnityEngine.Random.Range(0, 2);

            if (evento == 0)
            {
                Debug.Log("Generando Overdrive.");
                sistemaEventos.EjecutarOverdrive();
            }
            else
            {
                Debug.Log("Generando Blackout.");
                sistemaEventos.EjecutarBlackout();
            }
        }
    }

    public void MenuEditarNivel()
    {
        generadorDiscos.ActualizarListaDiscos();


        BotonMenu.SetActive(false);
        BotonEditar.SetActive(false);
        Time.timeScale = 0f;
        // Actualizar resumen
        MostrarResumenConfiguracion();

        // Abrir menú de edición
        MenuEditar.SetActive(true);
    }

    public void VolverAlNivelDesdeEditar()
    {
        // Cerrar menú de edición
        MenuEditar.SetActive(false);

        // Reabrir botón de menú
        BotonMenu.SetActive(true);

        // Reabrir botón editar
        BotonEditar.SetActive(true);

        // Continuar juego
        Time.timeScale = 1f;
    }

    //Boton para abrir el panel
    public void OpenMenuPanel()
    {
        generadorDiscos.ActualizarListaDiscos();

        BotonMenu.SetActive(false);
        BotonEditar.SetActive(false);
        Time.timeScale = 0f;
        Menupanel.SetActive(true);
    }

    public void EditarNivel()
    {
        // Guardar estado actual
        PlayerPrefs.SetInt("PartidaGuardada" + nivelActual, 1);
        PlayerPrefs.SetFloat("TiempoGuardado" + nivelActual, tiempo);

        generadorDiscos.GuardarDiscos();
        player.GuardarJugador();
        sistemaEventos.GuardarEstadoEvento();

        // Avisar que vamos a editar
        PlayerPrefs.SetInt("ModoEditarNivelPersonalizado", 1);
        PlayerPrefs.Save();

        // Volver a la escena de configuración
        SceneManager.LoadScene("CreacionNivelPersonalizado");
    }

    //Botones de Panel Menu
    public void VolverAlNivel()
    {
        Menupanel.SetActive(false);
        Time.timeScale = 1f;
        BotonMenu.SetActive(true);
        BotonEditar.SetActive(true);
    }

    public async void TerminarPartida()
    {
        // CERRAR MENU

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

        PlayerPrefs.SetString(
            "UltimoTipoNivel",
            "Personalizado");

        //========================================== 
        // REGISTRAR EN RANKING MUNDIAL 
        //==========================================
        if (RankingOnline.Instancia != null)
        {
            Debug.Log("DevLog Ranking -> Iniciando registro mundial. " +
                "Nivel: Nivel " + nivelPers +
                " | Tiempo: " + tiempoFinal +
                " segundos");

            Stopwatch cronometro = Stopwatch.StartNew();

            bool registrado =
                await RankingOnline.Instancia.RegistrarPartida(
                    nombreJugador,
                    tiempoFinal,
                    nivelPers);

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

        // ABRIR MENU DE NIVELES

        PlayerPrefs.SetInt(
            "AbrirMenuNiveles",
            1);

        // ELIMINAR PARTIDA GUARDADA

        PlayerPrefs.DeleteKey(
            "PartidaGuardada" + nivelActual);

        PlayerPrefs.DeleteKey(
            "TiempoGuardado" + nivelActual);

        generadorDiscos.LimpiarDatosGuardado();

        //------------------------------------------
        // GUARDAR CAMBIOS
        //------------------------------------------

        PlayerPrefs.Save();


        //------------------------------------------
        // IR AL MENU
        //------------------------------------------

        SceneManager.LoadScene("MainMenu");
    }

    // GUARDAR PARTIDA

    public void GuardarPartida()
    {
        // INDICAR PARTIDA GUARDADA

        PlayerPrefs.SetInt("PartidaGuardada" + nivelActual, 1);

        PlayerPrefs.SetFloat("TiempoGuardado" + nivelActual, tiempo);

        // GUARDAR GENERADORES

        generadorDiscos.GuardarDiscos();

        // GUARDAR PLAYER

        player.GuardarJugador();

        // GUARDAR EVENTO

        sistemaEventos.GuardarEstadoEvento();

        // ABRIR MENU DE NIVELES

        PlayerPrefs.SetInt("AbrirMenuNiveles",1);
        PlayerPrefs.Save();

        // IR AL MENU
        SceneManager.LoadScene(
            "MainMenu");
    }

    // NO CONTINUAR PARTIDA

    public void NoContinuar()
    {
        // ELIMINAR PARTIDA GUARDADA

        PlayerPrefs.DeleteKey(
            "PartidaGuardada" + nivelActual);

        PlayerPrefs.DeleteKey(
            "TiempoGuardado" + nivelActual);

        generadorDiscos.LimpiarDatosGuardado();

        // LIMPIAR DISCOS ACTUALES

        generadorDiscos.LimpiarDiscos();

        // REINICIAR EVENTO

        sistemaEventos.ReiniciarEvento();

        // REINICIAR TIEMPO

        tiempo = 0f;

        SiguienteEvento = 45f;

        // REINICIAR PLAYER

        player.InicializarJugador();

        player.RestaurarPosicionInicial();

        player.ReanudarJugador();

        // NUEVA PARTIDA

        juegoPausado = false;

        panelContinuar.SetActive(false);

        generadorDiscos.IniciarGeneracion();

        BotonMenu.SetActive(true);

        PlayerPrefs.Save();

        // Ir a creación de nivel personalizado
        SceneManager.LoadScene("CreacionNivelPersonalizado");
    }


    // SEGUIR PARTIDA

    public void SeguirPartida()
    {
        // OVERDRIVE VISUAL

        if (sistemaEventos.OverdriveActivo())
        {
            generadorDiscos.OcultarOverdriveVisual();
        }

        // REANUDAR GENERADORES

        EstablecerConfiguracionEventos();

        generadorDiscos.IniciarGeneracion();

        generadorDiscos.ReanudarDiscos();

        // CONTINUAR EVENTO

        sistemaEventos.ContinuarEvento();

        // REANUDAR PLAYER

        player.ReanudarJugador();

        // QUITAR PANEL

        panelContinuar.SetActive(false);

        juegoPausado = false;

        BotonMenu.SetActive(true);
        BotonEditar.SetActive(true);
    }

    // VOLVER Y GUARDAR / MENU

    public void VolveryGuardar()
    {
        PlayerPrefs.SetInt("AbrirMenuNiveles",1);

        //PlayerPrefs.Save();

        SceneManager.LoadScene(
            "MainMenu");
    }


    //==========================================
    // GAME OVER
    //==========================================

    public void GameOver()
    {
        juegoPausado = true;


        //--------------------------------------
        // DETENER GENERADORES
        //--------------------------------------

        generadorDiscos.DetenerGeneracion();


        //--------------------------------------
        // DETENER EVENTO
        //--------------------------------------

        sistemaEventos.ReiniciarEvento();


        //--------------------------------------
        // PAUSAR PLAYER
        //--------------------------------------

        player.PausarJugador();


        //--------------------------------------
        // CONGELAR JUEGO
        //--------------------------------------

        Time.timeScale = 0f;


        //--------------------------------------
        // MOSTRAR PANEL
        //--------------------------------------

        panelGameOver.SetActive(true);

        BotonMenu.SetActive(false);
        BotonEditar.SetActive(false);


        //--------------------------------------
        // MOSTRAR ESTADISTICAS
        //--------------------------------------

        textoTiempoFinal.text =tiempoTexto.text;

        textoNivelFinal.text = "Personalizado";

        // MOSTRAR RESUMEN DE CONFIGURACIÓN

        MostrarResumenConfiguracion();
    }


    // REINTENTAR NIVEL

    public async void ReintentarNivel()
    {
        //------------------------------------------
        // Enviar resultado al Ranking
        //------------------------------------------

        string nombreJugador = PlayerPrefs.GetString("NombreJugador");
        int tiempoFinal = Mathf.FloorToInt(tiempo);
        PlayerPrefs.SetString("UltimoNombre", nombreJugador);
        PlayerPrefs.SetFloat("UltimoTiempo", tiempo);
        PlayerPrefs.SetInt("UltimoNivel", nivelActual);

        PlayerPrefs.SetString(
            "UltimoTipoNivel",
            "Personalizado");

        //========================================== 
        // Registrar en ranking mundial
        //==========================================

        if (RankingOnline.Instancia != null)
        {
            Debug.Log("DevLog Ranking -> Iniciando registro mundial. " +
                "Nivel: Nivel " + nivelPers +
                " | Tiempo: " + tiempoFinal +
                " segundos");

            Stopwatch cronometro = Stopwatch.StartNew();

            bool registrado =
                await RankingOnline.Instancia.RegistrarPartida(
                    nombreJugador,
                    tiempoFinal,
                    nivelPers);

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

        //--------------------------------------
        // ELIMINAR PARTIDA GUARDADA
        //--------------------------------------

        PlayerPrefs.DeleteKey(
            "PartidaGuardada" + nivelActual);

        PlayerPrefs.DeleteKey(
            "TiempoGuardado" + nivelActual);

        generadorDiscos.LimpiarDatosGuardado();

        PlayerPrefs.Save();


        //--------------------------------------
        // REANUDAR TIEMPO
        //--------------------------------------

        Time.timeScale = 1f;


        //--------------------------------------
        // REINICIAR ESCENA
        //--------------------------------------

        SceneManager.LoadScene("CreacionNivelPersonalizado");
    }


    // VOLVER AL MENU DESDE GAME OVER

    public async void VolverMenuGameOver()
    {
        //------------------------------------------
        // Enviar resultado al Ranking
        //------------------------------------------

        string nombreJugador = PlayerPrefs.GetString("NombreJugador");
        int tiempoFinal = Mathf.FloorToInt(tiempo);
        PlayerPrefs.SetString("UltimoNombre", nombreJugador);
        PlayerPrefs.SetFloat("UltimoTiempo", tiempo);
        PlayerPrefs.SetInt("UltimoNivel", nivelActual);

        PlayerPrefs.SetString(
            "UltimoTipoNivel",
            "Personalizado");

        //========================================== 
        // Registrar en ranking mundial
        //==========================================

        if (RankingOnline.Instancia != null)
        {
            Debug.Log("DevLog Ranking -> Iniciando registro mundial. " +
                "Nivel: Nivel " + nivelPers +
                " | Tiempo: " + tiempoFinal +
                " segundos");

            Stopwatch cronometro = Stopwatch.StartNew();

            bool registrado =
                await RankingOnline.Instancia.RegistrarPartida(
                    nombreJugador,
                    tiempoFinal,
                    nivelPers);

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

        //--------------------------------------
        // ELIMINAR PARTIDA GUARDADA
        //--------------------------------------

        PlayerPrefs.DeleteKey(
            "PartidaGuardada" + nivelActual);

        PlayerPrefs.DeleteKey(
            "TiempoGuardado" + nivelActual);

        generadorDiscos.LimpiarDatosGuardado();


        //--------------------------------------
        // ABRIR MENU DE NIVELES
        //--------------------------------------

        PlayerPrefs.SetInt(
            "AbrirMenuNiveles",
            1);


        PlayerPrefs.Save();


        //--------------------------------------
        // REANUDAR
        //--------------------------------------

        Time.timeScale = 1f;


        //--------------------------------------
        // IR AL MENU
        //--------------------------------------

        SceneManager.LoadScene(
            "MainMenu");
    }

    public float ObtenerTiempo()
    {
        return tiempo;
    }

    //==========================================
    // RESUMEN DE CONFIGURACIÓN
    //==========================================

    public void MostrarResumenConfiguracion()
    {
        //========================================
        // DISCOS
        //========================================

        string discos = "";

        if (PlayerPrefs.GetInt("Disco1", 0) == 1)
            discos += "Normal, ";

        if (PlayerPrefs.GetInt("Disco2", 0) == 1)
            discos += "Rápido, ";

        if (PlayerPrefs.GetInt("Disco3", 0) == 1)
            discos += "Pesado, ";

        if (PlayerPrefs.GetInt("Disco4", 0) == 1)
            discos += "Venenoso, ";

        if (PlayerPrefs.GetInt("Disco5", 0) == 1)
            discos += "Explosivo, ";

        // Quitar última coma
        if (discos.EndsWith(", "))
            discos = discos.Substring(0, discos.Length - 2);


        //========================================
        // MÚSICA
        //========================================

        string musica = "";

        int musicaElegida =
            PlayerPrefs.GetInt("MusicaElegida", 0);

        switch (musicaElegida)
        {
            case 0:
                musica = "Menú Principal";
                break;

            case 1:
                musica = "Nivel 1";
                break;

            case 2:
                musica = "Nivel 2";
                break;

            case 3:
                musica = "Nivel 3";
                break;

            default:
                musica = "Desconocida";
                break;
        }


        //========================================
        // EVENTOS
        //========================================

        string eventos = "";

        bool overdrive =
            PlayerPrefs.GetInt("EvOverdrive", 0) == 1;

        bool blackout =
            PlayerPrefs.GetInt("EvBlackout", 0) == 1;


        if (overdrive && blackout)
        {
            eventos = "Overdrive y Blackout";
        }
        else if (overdrive)
        {
            eventos = "Overdrive";
        }
        else if (blackout)
        {
            eventos = "Blackout";
        }
        else
        {
            eventos = "Ninguno";
        }


        //========================================
        // GENERADORES
        //========================================

        string generadores = "";

        if (PlayerPrefs.GetInt("GenCentro", 0) == 1)
            generadores += "Centro, ";

        if (PlayerPrefs.GetInt("GenIzqArr", 0) == 1)
            generadores += "Izquierda Arriba, ";

        if (PlayerPrefs.GetInt("GenIzqAba", 0) == 1)
            generadores += "Izquierda Abajo, ";

        if (PlayerPrefs.GetInt("GenDerArr", 0) == 1)
            generadores += "Derecha Arriba, ";

        if (PlayerPrefs.GetInt("GenDerAba", 0) == 1)
            generadores += "Derecha Abajo, ";

        // Quitar última coma
        if (generadores.EndsWith(", "))
            generadores =
                generadores.Substring(
                    0,
                    generadores.Length - 2
                );


        //========================================
        // ACTUALIZAR RESUMEN DEL MENÚ
        //========================================

        if (textoResumenDiscosMenu != null)
        {
            textoResumenDiscosMenu.text =
                "Discos: " + discos;
        }

        if (textoResumenMusicaMenu != null)
        {
            textoResumenMusicaMenu.text =
                "Música: " + musica;
        }

        if (textoResumenEventosMenu != null)
        {
            textoResumenEventosMenu.text =
                "Eventos: " + eventos;
        }

        if (textoResumenGeneradoresMenu != null)
        {
            textoResumenGeneradoresMenu.text =
                "Generadores: " + generadores;
        }


        //========================================
        // ACTUALIZAR RESUMEN DEL GAME OVER
        //========================================

        if (textoResumenDiscosGameOver != null)
        {
            textoResumenDiscosGameOver.text =
                "Discos: " + discos;
        }

        if (textoResumenMusicaGameOver != null)
        {
            textoResumenMusicaGameOver.text =
                "Música: " + musica;
        }

        if (textoResumenEventosGameOver != null)
        {
            textoResumenEventosGameOver.text =
                "Eventos: " + eventos;
        }

        if (textoResumenGeneradoresGameOver != null)
        {
            textoResumenGeneradoresGameOver.text =
                "Generadores: " + generadores;
        }
    }
}
