using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using TMPro;

public class MainMenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject opcionesMenu;
    public GameObject mainMenu;
    public GameObject rankingmenu;
    public GameObject Menuniveles;

    //Variables de Opciones
    public Slider soundSlider;
    public AudioMixer masterMixer;
    public Toggle pantallaCompletaToggle;

    //Variables de Nombre
    public GameObject panelSinNombre;
    public GameObject panelNombre;

    public TMP_InputField inputNombre;
    public TMP_Text textoNombreActual;

    //Variables para Ranking
    public TMP_Text puesto1;
    public TMP_Text puesto2;
    public TMP_Text puesto3;
    public TMP_Text puesto4;
    public TMP_Text puesto5;

    string[] nombres = new string[6];
    float[] tiempos = new float[6];
    int[] niveles = new int[6];
    string[] tiposNivel = new string[6];

    //Cuando empieza el juego:
    void Start()
    {
        Time.timeScale = 1f;

        //Vemos si viene de un nivel
        if (PlayerPrefs.GetInt("AbrirMenuNiveles", 0) == 1)
        {
            mainMenu.SetActive(false);
            Menuniveles.SetActive(true);

            PlayerPrefs.SetInt("AbrirMenuNiveles", 0);
        }
        else
        {
            mainMenu.SetActive(true);
            Menuniveles.SetActive(false);
        }

        //Buscamos si hay volumen guardado
        float volumenGuardado = PlayerPrefs.GetFloat("VolumenDelJuego", 1f);

        //Movemos el slider al volumen guardado
        if(soundSlider != null)
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

        //Nombre Inicial
        panelSinNombre.SetActive(false);
        panelNombre.SetActive(false);

        if (PlayerPrefs.HasKey("NombreJugador"))
        {
            textoNombreActual.text =
                "Nombre Actual: " +
                PlayerPrefs.GetString("NombreJugador");
        }
        else
        {
            textoNombreActual.text =
                "Nombre Actual: Sin Registrar";
        }

        //Actualizar Ranking
        ActualizarRanking();
    }

    //Funciones para Actualizar el Ranking
    public void ActualizarRanking()
    {
        // Obtener los datos de la última partida
        string nuevoNombre =
            PlayerPrefs.GetString("UltimoNombre", "");

        float nuevoTiempo =
            PlayerPrefs.GetFloat("UltimoTiempo", 0);

        int nuevoNivel =
            PlayerPrefs.GetInt("UltimoNivel", 0);

        string nuevoTipoNivel =
            PlayerPrefs.GetString("UltimoTipoNivel", "Nivel");

        // Si no hay partida nueva, simplemente mostrar el ranking actual
        if (nuevoNombre == "" || nuevoTiempo == 0)
        {
            MostrarRanking();
            return;
        }


        // Leer el Top 5 actual
        for (int i = 0; i < 5; i++)
        {
            nombres[i] =
                PlayerPrefs.GetString("Nombre" + (i + 1), "---");

            tiempos[i] =
                PlayerPrefs.GetFloat("Tiempo" + (i + 1), 0);
            niveles[i] =
                PlayerPrefs.GetInt("Nivel" + (i + 1), 0);
            tiposNivel[i] =
                PlayerPrefs.GetString("TipoNivel" + (i + 1), "Nivel");
        }


        // Agregar la nueva partida en la posición 6
        nombres[5] = nuevoNombre;
        tiempos[5] = nuevoTiempo;
        niveles[5] = nuevoNivel;
        tiposNivel[5] = nuevoTipoNivel;


        // Ordenar de mayor a menor tiempo
        for (int i = 0; i < 6; i++)
        {
            for (int j = i + 1; j < 6; j++)
            {
                if (tiempos[j] > tiempos[i])
                {
                    float auxTiempo = tiempos[i];
                    tiempos[i] = tiempos[j];
                    tiempos[j] = auxTiempo;

                    string auxNombre = nombres[i];
                    nombres[i] = nombres[j];
                    nombres[j] = auxNombre;

                    int auxNivel = niveles[i];
                    niveles[i] = niveles[j];
                    niveles[j] = auxNivel;

                    string auxTipoNivel = tiposNivel[i];
                    tiposNivel[i] = tiposNivel[j];
                    tiposNivel[j] = auxTipoNivel;
                }
            }
        }


        // Guardar nuevamente el Top 5
        for (int i = 0; i < 5; i++)
        {
            PlayerPrefs.SetString(
                "Nombre" + (i + 1),
                nombres[i]);

            PlayerPrefs.SetFloat(
                "Tiempo" + (i + 1),
                tiempos[i]);

            PlayerPrefs.SetInt(
                "Nivel" + (i + 1),
                niveles[i]);
            PlayerPrefs.SetString(
                "TipoNivel" + (i + 1),
                tiposNivel[i]);
        }


        // Eliminar la última partida para que no se vuelva a insertar
        PlayerPrefs.DeleteKey("UltimoNombre");
        PlayerPrefs.DeleteKey("UltimoTiempo");
        PlayerPrefs.DeleteKey("UltimoNivel");
        PlayerPrefs.DeleteKey("UltimoTipoNivel");


        // Mostrar el ranking actualizado
        MostrarRanking();
    }

    private string ObtenerTextoNivel(int puesto)
    {
        string tipo =
            PlayerPrefs.GetString(
                "TipoNivel" + puesto,
                "Nivel");

        if (tipo == "Personalizado")
        {
            return "Personalizado";
        }

        return "N" +
            PlayerPrefs.GetInt(
                "Nivel" + puesto,
                0);
    }

    public void MostrarRanking()
    {
        puesto1.text =
            "1. "
            + PlayerPrefs.GetString("Nombre1", "---")
            + "  "
            + ObtenerTextoNivel(1)
            + "  "
            + FormatoTiempo(PlayerPrefs.GetFloat("Tiempo1", 0));

        puesto2.text =
            "2. "
            + PlayerPrefs.GetString("Nombre2", "---")
            + "  "
            + ObtenerTextoNivel(2)
            + "  "
            + FormatoTiempo(PlayerPrefs.GetFloat("Tiempo2", 0));

        puesto3.text =
            "3. "
            + PlayerPrefs.GetString("Nombre3", "---")
            + "  "
            + ObtenerTextoNivel(3)
            + "  "
            + FormatoTiempo(PlayerPrefs.GetFloat("Tiempo3", 0));

        puesto4.text =
            "4. "
            + PlayerPrefs.GetString("Nombre4", "---")
            + "  "
            + ObtenerTextoNivel(4)
            + "  "
            + FormatoTiempo(PlayerPrefs.GetFloat("Tiempo4", 0));

        puesto5.text =
            "5. "
            + PlayerPrefs.GetString("Nombre5", "---")
            + "  "
            + ObtenerTextoNivel(5)
            + "  "
            + FormatoTiempo(PlayerPrefs.GetFloat("Tiempo5", 0));
    }

    public string FormatoTiempo(float tiempo)
    {
        int minutos = (int)(tiempo / 60);
        int segundos = (int)(tiempo % 60);

        return minutos.ToString("00")
               + ":"
               + segundos.ToString("00");
    }

    //Botones Pantalla Menu
    public void AbrirPanelOpciones() 
    {
        mainMenu.SetActive(false);
        opcionesMenu.SetActive(true);
    }

    public void AbrirPanelRanking()
    {
        mainMenu.SetActive(false);
        rankingmenu.SetActive(true);
    }

    public void AbrirPanelNiveles()
    {
        if (PlayerPrefs.HasKey("NombreJugador"))
        {
            mainMenu.SetActive(false);
            Menuniveles.SetActive(true);
        }
        else 
        {
            panelSinNombre.SetActive(true);
        }
    }

    public void CrearNombre()
    {
        panelSinNombre.SetActive(false);
        mainMenu.SetActive(false);
        panelNombre.SetActive(true);

        inputNombre.text = "";
    }

    public void AbrirPanelEditarNombre()
    {
        panelNombre.SetActive(true);
        mainMenu.SetActive(false);

        if (PlayerPrefs.HasKey("NombreJugador"))
        {
            inputNombre.text =
                PlayerPrefs.GetString("NombreJugador");
        }
    }

    public void QuitGame() 
    {
        Application.Quit();
    }

    public void EliminarDatos()
    {
        // Elimina absolutamente todos los PlayerPrefs
        // (ranking, configuración, partidas, eventos y discos)

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    //Botones Pantalla Nombre

    public void GuardarNombre()
    {
        if (string.IsNullOrWhiteSpace(inputNombre.text))
        {
            return;
        }

        PlayerPrefs.SetString(
            "NombreJugador",
            inputNombre.text
        );

        textoNombreActual.text =
            "Nombre Actual: " +
            inputNombre.text;

        panelNombre.SetActive(false);
        mainMenu.SetActive(true);
    }

    //Botones Pantalla Ranking

    public void VolverMenuRanking()
    {
        mainMenu.SetActive(true);
        rankingmenu.SetActive(false);
    }

    //Botones Pantalla Opciones

    public void VolverMenuOpciones()
    {
        mainMenu.SetActive(true);
        opcionesMenu.SetActive(false);
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

    //Botones Pantalla Niveles

    public void VolverMenuNiveles()
    {
        mainMenu.SetActive(true);
        Menuniveles.SetActive(false);
    }

    public void OpenNivelOne() 
    {
        PlayerPrefs.SetInt("ContinuarPartida", 1);
        SceneManager.LoadScene("Nivel1");
    }

    public void OpenNivelTwo()
    {
        PlayerPrefs.SetInt("ContinuarPartida", 1);
        SceneManager.LoadScene("Nivel2");
    }

    public void OpenNivelThree()
    {
        PlayerPrefs.SetInt("ContinuarPartida", 1);
        SceneManager.LoadScene("Nivel3");
    }

    public void AbrirNivelPersonalizado()
    {
        bool partidaGuardada =
            PlayerPrefs.GetInt("PartidaGuardada" + 4, 0) == 1;

        if (partidaGuardada)
        {
            SceneManager.LoadScene("NivelPersonalizado");
        }
        else
        {
            SceneManager.LoadScene("CreacionNivelPersonalizado");
        }
    }

    //

    public void BotónRankingOnline()
    {
        SceneManager.LoadScene("RankingMundial");
    }

}
