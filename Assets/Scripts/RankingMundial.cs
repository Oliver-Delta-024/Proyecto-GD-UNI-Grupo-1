using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RankingMundial : MonoBehaviour
{

    //==================================================
    // CONFIGURACI�N
    //==================================================

    [Header("Leaderboard")]

    // Cantidad de resultados mostrados por p�gina.
    private const int ResultadosPorPagina = 5;
    private const int MaxResultadosV1 = 50;
    private const int MaxResultadosSnapshotV2 = 50;

    [SerializeField]
    private AudioMixer masterMixer;


    //==================================================
    // FILTRO DE NIVEL
    //==================================================

    [Header("Filtro de Nivel")]

    [SerializeField]
    private TMP_Dropdown filtroNivel;


    //==================================================
    // FILTRO DE TIEMPO
    //==================================================

    [Header("Filtro de Tiempo")]

    [SerializeField]
    private TMP_InputField inputMinutos;

    [SerializeField]
    private TMP_InputField inputSegundos;

    // Aviso cuando los segundos son inv�lidos.
    [SerializeField]
    private GameObject avisoTiempoInvalido;


    //==================================================
    // BOT�N ACTUALIZAR
    //==================================================

    [Header("Botones")]

    // Bot�n que aplica los filtros sobre los datos actuales.
    [SerializeField]
    private Button botonFiltrar;

    // Bot�n que vuelve a consultar Supabase.
    [SerializeField]
    private Button botonActualizar;


    //==================================================
    // NAVEGACI�N
    //==================================================

    [Header("Navegaci�n")]

    [SerializeField]
    private Button botonAnterior;

    [SerializeField]
    private Button botonSiguiente;

    [SerializeField]
    private TextMeshProUGUI textoPagina;


    //==================================================
    // FILAS DEL RANKING
    //==================================================

    [Header("Fila 1")]

    [SerializeField]
    private TextMeshProUGUI puesto1;

    [SerializeField]
    private TextMeshProUGUI nombre1;

    [SerializeField]
    private TextMeshProUGUI tiempo1;

    [SerializeField]
    private TextMeshProUGUI nivel1;


    [Header("Fila 2")]

    [SerializeField]
    private TextMeshProUGUI puesto2;

    [SerializeField]
    private TextMeshProUGUI nombre2;

    [SerializeField]
    private TextMeshProUGUI tiempo2;

    [SerializeField]
    private TextMeshProUGUI nivel2;


    [Header("Fila 3")]

    [SerializeField]
    private TextMeshProUGUI puesto3;

    [SerializeField]
    private TextMeshProUGUI nombre3;

    [SerializeField]
    private TextMeshProUGUI tiempo3;

    [SerializeField]
    private TextMeshProUGUI nivel3;


    [Header("Fila 4")]

    [SerializeField]
    private TextMeshProUGUI puesto4;

    [SerializeField]
    private TextMeshProUGUI nombre4;

    [SerializeField]
    private TextMeshProUGUI tiempo4;

    [SerializeField]
    private TextMeshProUGUI nivel4;


    [Header("Fila 5")]

    [SerializeField]
    private TextMeshProUGUI puesto5;

    [SerializeField]
    private TextMeshProUGUI nombre5;

    [SerializeField]
    private TextMeshProUGUI tiempo5;

    [SerializeField]
    private TextMeshProUGUI nivel5;


    //==================================================
    // DATOS
    //==================================================

    // Datos recibidos desde Supabase.

    // Datos despu�s de aplicar filtros.

    // P�gina actual.
    private int paginaActual = 0;
    private int cantidadPaginas = 0;
    private List<RankingOnline.ResultadoRanking> resultados =
        new List<RankingOnline.ResultadoRanking>();
    private List<RankingOnline.ResultadoRanking> resultadosFiltrados =
        new List<RankingOnline.ResultadoRanking>();
    private List<RankingOnline.ResultadoRanking> snapshotV2 =
        new List<RankingOnline.ResultadoRanking>();

    // Tiempo m�nimo seleccionado.

    // Bloqueo de anticlicks
    private bool actualizacionEnCurso = false;

    //==================================================
    // INICIO
    //==================================================

    private void Start()
    {
        // ---------------------------------------------
        // VALORES INICIALES
        // ---------------------------------------------

        if (inputMinutos != null)
        {
            inputMinutos.text = "00";
        }

        if (inputSegundos != null)
        {
            inputSegundos.text = "00";
        }

        if (avisoTiempoInvalido != null)
        {
            avisoTiempoInvalido.SetActive(false);
        }

        // ---------------------------------------------
        // BOT�N FILTRAR
        // ---------------------------------------------

        if (botonFiltrar != null)
        {
            botonFiltrar.onClick.AddListener(
                FiltrarRanking
            );
        }


        // ---------------------------------------------
        // BOT�N ACTUALIZAR
        // ---------------------------------------------

        if (botonActualizar != null)
        {
            botonActualizar.onClick.AddListener(
                ActualizarRanking
            );
        }

        // ---------------------------------------------
        // NAVEGACI�N
        // ---------------------------------------------

        if (botonAnterior != null)
        {
            botonAnterior.onClick.AddListener(
                PaginaAnterior
            );
        }

        if (botonSiguiente != null)
        {
            botonSiguiente.onClick.AddListener(
                PaginaSiguiente
            );
        }


        // ---------------------------------------------
        // CARGAR RANKING INICIAL
        // ---------------------------------------------

        ActualizarRanking();

        float volumenGuardado = PlayerPrefs.GetFloat("VolumenDelJuego", 1f);

        float decibelios = Mathf.Log10(volumenGuardado) * 20;

        masterMixer.SetFloat(
            "MasterVolume",
            decibelios
        );
    }


    //==================================================
    // ACTUALIZAR RANKING
    //==================================================

    public async void ActualizarRanking()
    {
        if (!ValidarFiltroTiempo()) return;
        int paginaAnterior = paginaActual;
        paginaActual = 0;
        if (!await CargarPaginaActualAsync()) paginaActual = paginaAnterior;
    }

    public async void FiltrarRanking()
    {
        if (!ValidarFiltroTiempo()) return;
        if (RankingOnline.Instancia != null && !RankingOnline.Instancia.UsarMostrarRankingV2)
        {
            paginaActual = 0;
            FiltrarRankingInterno();
            MostrarPaginaV1();
            return;
        }

        int paginaAnterior = paginaActual;
        paginaActual = 0;
        if (!await CargarPaginaActualAsync()) paginaActual = paginaAnterior;
    }

    // FILTRAR RANKING
    //==================================================

    // OBTENER NIVEL SELECCIONADO
    //==================================================

    private string ObtenerNivelSeleccionado()
    {
        if (filtroNivel == null)
        {
            return "Todos";
        }

        if (filtroNivel.options == null)
        {
            return "Todos";
        }

        if (filtroNivel.options.Count == 0)
        {
            return "Todos";
        }


        int indice =
            filtroNivel.value;


        if (
            indice < 0 ||
            indice >= filtroNivel.options.Count
        )
        {
            return "Todos";
        }


        return filtroNivel.options[
            indice
        ].text.Trim();
    }


    //==================================================
    // PAGINACI�N
    //==================================================

    private async Task<bool> CargarPaginaActualAsync()
    {
        if (actualizacionEnCurso) return false;

        RankingOnline rankingOnline = RankingOnline.Instancia;
        if (rankingOnline == null)
        {
            Debug.LogWarning("RankingMundial -> RankingOnline no encontrado.");
            return false;
        }

        actualizacionEnCurso = true;
        EstablecerControlesDuranteCarga(true);

        try
        {
            int paginaSolicitada = Mathf.Max(1, paginaActual + 1);
            if (!rankingOnline.UsarMostrarRankingV2)
            {
                RankingOnline.RespuestaRanking respuestaV1 =
                    await rankingOnline.ObtenerRanking("", 0, 1, MaxResultadosV1, false);

                if (respuestaV1 == null)
                {
                    Debug.LogWarning("RankingMundial -> No se pudo cargar el ranking V1.");
                    return false;
                }

                resultados = new List<RankingOnline.ResultadoRanking>(respuestaV1.resultados ?? Array.Empty<RankingOnline.ResultadoRanking>());
                FiltrarRankingInterno();
                cantidadPaginas = Mathf.CeilToInt((float)resultadosFiltrados.Count / ResultadosPorPagina);
                paginaActual = Mathf.Clamp(paginaSolicitada - 1, 0, Mathf.Max(0, cantidadPaginas - 1));
                MostrarPaginaV1();
                return true;
            }

            string nivel = ObtenerNivelSeleccionado();
            if (nivel == "Todos") nivel = "";
            int tiempoMinimo = ObtenerTiempoMinimo();

            RankingOnline.RespuestaRanking respuesta =
                await rankingOnline.ObtenerRanking(
                    nivel,
                    tiempoMinimo,
                    1,
                    MaxResultadosSnapshotV2);

            if (respuesta == null)
            {
                Debug.LogWarning("RankingMundial -> No se pudo cargar el ranking.");
                return false;
            }

            snapshotV2 = new List<RankingOnline.ResultadoRanking>(
                respuesta.resultados ?? Array.Empty<RankingOnline.ResultadoRanking>());
            paginaActual = 0;
            MostrarPaginaSnapshotV2();
            return true;
        }
        catch (Exception error)
        {
            Debug.LogError("RankingMundial -> Error al actualizar ranking: " + error.Message);
            return false;
        }
        finally
        {
            actualizacionEnCurso = false;
            EstablecerControlesDuranteCarga(false);
        }
    }

    private void EstablecerControlesDuranteCarga(bool cargando)
    {
        if (botonFiltrar != null) botonFiltrar.interactable = !cargando;
        if (botonActualizar != null) botonActualizar.interactable = !cargando;
        if (botonAnterior != null) botonAnterior.interactable = !cargando;
        if (botonSiguiente != null) botonSiguiente.interactable = !cargando;
    }

    private void MostrarPaginaSnapshotV2()
    {
        cantidadPaginas = Mathf.CeilToInt(
            (float)snapshotV2.Count / ResultadosPorPagina);
        LimpiarRanking();

        if (cantidadPaginas == 0)
        {
            paginaActual = 0;
            if (textoPagina != null) textoPagina.text = "0/0";
            if (botonAnterior != null) botonAnterior.gameObject.SetActive(false);
            if (botonSiguiente != null) botonSiguiente.gameObject.SetActive(false);
            return;
        }

        paginaActual = Mathf.Clamp(paginaActual, 0, cantidadPaginas - 1);
        if (textoPagina != null)
            textoPagina.text = (paginaActual + 1) + "/" + cantidadPaginas;

        if (botonAnterior != null)
            botonAnterior.gameObject.SetActive(paginaActual > 0);
        if (botonSiguiente != null)
            botonSiguiente.gameObject.SetActive(paginaActual < cantidadPaginas - 1);

        int inicio = paginaActual * ResultadosPorPagina;
        int cantidadVisible = Mathf.Min(
            ResultadosPorPagina,
            snapshotV2.Count - inicio);
        for (int i = 0; i < cantidadVisible; i++)
        {
            RankingOnline.ResultadoRanking resultado = snapshotV2[inicio + i];
            MostrarEntrada(i, resultado, resultado.posicion);
        }
    }

    private void FiltrarRankingInterno()
    {
        resultadosFiltrados.Clear();
        string nivelSeleccionado = ObtenerNivelSeleccionado();
        int tiempoMinimo = ObtenerTiempoMinimo();

        foreach (RankingOnline.ResultadoRanking resultado in resultados)
        {
            if (resultado == null || resultado.tiempo < tiempoMinimo) continue;
            if (nivelSeleccionado != "Todos" &&
                !string.Equals(resultado.nivel?.Trim(), nivelSeleccionado, StringComparison.OrdinalIgnoreCase)) continue;
            resultadosFiltrados.Add(resultado);
        }
    }

    private void MostrarPaginaV1()
    {
        cantidadPaginas = Mathf.CeilToInt((float)resultadosFiltrados.Count / ResultadosPorPagina);
        LimpiarRanking();

        if (cantidadPaginas == 0)
        {
            paginaActual = 0;
            if (textoPagina != null) textoPagina.text = "0/0";
            if (botonAnterior != null) botonAnterior.gameObject.SetActive(false);
            if (botonSiguiente != null) botonSiguiente.gameObject.SetActive(false);
            return;
        }

        paginaActual = Mathf.Clamp(paginaActual, 0, cantidadPaginas - 1);
        if (textoPagina != null) textoPagina.text = (paginaActual + 1) + "/" + cantidadPaginas;
        if (botonAnterior != null) botonAnterior.gameObject.SetActive(paginaActual > 0);
        if (botonSiguiente != null) botonSiguiente.gameObject.SetActive(paginaActual < cantidadPaginas - 1);

        int inicio = paginaActual * ResultadosPorPagina;
        int cantidadVisible = Mathf.Min(ResultadosPorPagina, resultadosFiltrados.Count - inicio);
        for (int fila = 0; fila < cantidadVisible; fila++)
        {
            MostrarEntrada(fila, resultadosFiltrados[inicio + fila], inicio + fila + 1);
        }
    }
    // MOSTRAR ENTRADA
    //==================================================

    private void MostrarEntrada(
        int fila,
        RankingOnline.ResultadoRanking entrada,
        int puesto)
    {
        string nombre =
            entrada.nombre;


        if (string.IsNullOrEmpty(nombre))
        {
            nombre = "Jugador";
        }


        string tiempo =
            FormatearTiempo(
                entrada.tiempo
            );


        string nivel =
            entrada.nivel;


        switch (fila)
        {
            case 0:

                if (puesto1 != null)
                    puesto1.text =
                        puesto.ToString();

                if (nombre1 != null)
                    nombre1.text =
                        nombre;

                if (tiempo1 != null)
                    tiempo1.text =
                        tiempo;

                if (nivel1 != null)
                    nivel1.text =
                        nivel;

                break;


            case 1:

                if (puesto2 != null)
                    puesto2.text =
                        puesto.ToString();

                if (nombre2 != null)
                    nombre2.text =
                        nombre;

                if (tiempo2 != null)
                    tiempo2.text =
                        tiempo;

                if (nivel2 != null)
                    nivel2.text =
                        nivel;

                break;


            case 2:

                if (puesto3 != null)
                    puesto3.text =
                        puesto.ToString();

                if (nombre3 != null)
                    nombre3.text =
                        nombre;

                if (tiempo3 != null)
                    tiempo3.text =
                        tiempo;

                if (nivel3 != null)
                    nivel3.text =
                        nivel;

                break;


            case 3:

                if (puesto4 != null)
                    puesto4.text =
                        puesto.ToString();

                if (nombre4 != null)
                    nombre4.text =
                        nombre;

                if (tiempo4 != null)
                    tiempo4.text =
                        tiempo;

                if (nivel4 != null)
                    nivel4.text =
                        nivel;

                break;


            case 4:

                if (puesto5 != null)
                    puesto5.text =
                        puesto.ToString();

                if (nombre5 != null)
                    nombre5.text =
                        nombre;

                if (tiempo5 != null)
                    tiempo5.text =
                        tiempo;

                if (nivel5 != null)
                    nivel5.text =
                        nivel;

                break;
        }
    }


    //==================================================
    // P�GINA ANTERIOR
    //==================================================

    public void PaginaAnterior()
    {
        if (actualizacionEnCurso || paginaActual <= 0) return;
        if (RankingOnline.Instancia != null && !RankingOnline.Instancia.UsarMostrarRankingV2)
        {
            paginaActual--;
            MostrarPaginaV1();
            return;
        }
        paginaActual--;
        MostrarPaginaSnapshotV2();
    }

    public void PaginaSiguiente()
    {
        if (actualizacionEnCurso || paginaActual >= cantidadPaginas - 1) return;
        if (RankingOnline.Instancia != null && !RankingOnline.Instancia.UsarMostrarRankingV2)
        {
            paginaActual++;
            MostrarPaginaV1();
            return;
        }
        paginaActual++;
        MostrarPaginaSnapshotV2();
    }
    // VALIDAR TIEMPO
    //==================================================

    private bool ValidarFiltroTiempo()
    {
        if (
            inputMinutos == null ||
            inputSegundos == null
        )
        {
            return true;
        }


        int minutos = 0;
        int segundos = 0;


        // ---------------------------------------------
        // MINUTOS
        // ---------------------------------------------

        if (
            !string.IsNullOrEmpty(
                inputMinutos.text
            )
        )
        {
            if (
                !int.TryParse(
                    inputMinutos.text,
                    out minutos
                )
            )
            {
                MostrarErrorTiempo();
                return false;
            }
        }


        // ---------------------------------------------
        // SEGUNDOS
        // ---------------------------------------------

        if (
            !string.IsNullOrEmpty(
                inputSegundos.text
            )
        )
        {
            if (
                !int.TryParse(
                    inputSegundos.text,
                    out segundos
                )
            )
            {
                MostrarErrorTiempo();
                return false;
            }
        }


        // ---------------------------------------------
        // VALORES NEGATIVOS
        // ---------------------------------------------

        if (
            minutos < 0 ||
            segundos < 0
        )
        {
            MostrarErrorTiempo();
            return false;
        }


        // ---------------------------------------------
        // SEGUNDOS 00 - 59
        // ---------------------------------------------

        if (segundos > 59)
        {
            MostrarErrorTiempo();
            return false;
        }


        // ---------------------------------------------
        // OCULTAR AVISO
        // ---------------------------------------------

        if (avisoTiempoInvalido != null)
        {
            avisoTiempoInvalido.SetActive(false);
        }


        return true;
    }


    private void MostrarErrorTiempo()
    {
        if (avisoTiempoInvalido != null)
        {
            avisoTiempoInvalido.SetActive(true);
        }


        Debug.LogWarning(
            "RankingMundial -> Los segundos deben estar entre 00 y 59."
        );
    }


    //==================================================
    // OBTENER TIEMPO M�NIMO
    //==================================================

    private int ObtenerTiempoMinimo()
    {
        int minutos = 0;
        int segundos = 0;


        if (inputMinutos != null)
        {
            int.TryParse(
                inputMinutos.text,
                out minutos
            );
        }


        if (inputSegundos != null)
        {
            int.TryParse(
                inputSegundos.text,
                out segundos
            );
        }


        return
            minutos * 60 +
            segundos;
    }


    //==================================================
    // FORMATEAR TIEMPO
    //==================================================

    private string FormatearTiempo(
        int segundosTotales)
    {
        int segundos =
            Mathf.Max(
                0,
                segundosTotales
            );


        int minutos =
            segundos / 60;


        int segundosRestantes =
            segundos % 60;


        return string.Format(
            "{0:00}:{1:00}",
            minutos,
            segundosRestantes
        );
    }


    //==================================================
    // LIMPIAR RANKING
    //==================================================

    private void LimpiarRanking()
    {
        LimpiarFila(
            puesto1,
            nombre1,
            tiempo1,
            nivel1
        );


        LimpiarFila(
            puesto2,
            nombre2,
            tiempo2,
            nivel2
        );


        LimpiarFila(
            puesto3,
            nombre3,
            tiempo3,
            nivel3
        );


        LimpiarFila(
            puesto4,
            nombre4,
            tiempo4,
            nivel4
        );


        LimpiarFila(
            puesto5,
            nombre5,
            tiempo5,
            nivel5
        );
    }


    private void LimpiarFila(
        TextMeshProUGUI puesto,
        TextMeshProUGUI nombre,
        TextMeshProUGUI tiempo,
        TextMeshProUGUI nivel)
    {
        if (puesto != null)
            puesto.text = "";

        if (nombre != null)
            nombre.text = "";

        if (tiempo != null)
            tiempo.text = "";

        if (nivel != null)
            nivel.text = "";
    }

    public void VolverMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

}

