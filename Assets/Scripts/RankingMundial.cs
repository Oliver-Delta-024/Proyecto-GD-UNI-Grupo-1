using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using UnityEngine.UI;

public class RankingMundial : MonoBehaviour
{

    //==================================================
    // CONFIGURACIÓN
    //==================================================

    [Header("Leaderboard")]

    [SerializeField]
    private string leaderboardId = "ranking-mundial";

    // Máximo de resultados que consultaremos.
    [SerializeField]
    private int maxResultados = 50;

    // Cantidad de resultados mostrados por página.
    private const int ResultadosPorPagina = 5;


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

    // Aviso cuando los segundos son inválidos.
    [SerializeField]
    private GameObject avisoTiempoInvalido;


    //==================================================
    // BOTÓN ACTUALIZAR
    //==================================================

    [Header("Actualizar")]

    [SerializeField]
    private Button botonActualizar;


    //==================================================
    // NAVEGACIÓN
    //==================================================

    [Header("Navegación")]

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

    private List<LeaderboardEntry> resultados = new List<LeaderboardEntry>();

    private int paginaActual = 0;

    private int tiempoMinimoSegundos = 0;

    //==================================================
    // INICIO
    //==================================================

    private void Start()
    {
        // Valor inicial del filtro.
        inputMinutos.text = "00";
        inputSegundos.text = "00";

        // Ocultar aviso.
        if (avisoTiempoInvalido != null)
        {
            avisoTiempoInvalido.SetActive(false);
        }

        // Eventos de UI.
        if (botonActualizar != null)
        {
            botonActualizar.onClick.AddListener(ActualizarRanking);
        }

        if (botonAnterior != null)
        {
            botonAnterior.onClick.AddListener(PaginaAnterior);
        }

        if (botonSiguiente != null)
        {
            botonSiguiente.onClick.AddListener(PaginaSiguiente);
        }

        // Cargar ranking inicialmente.
        ActualizarRanking();
    }


    //==================================================
    // ACTUALIZAR RANKING
    //==================================================

    public async void ActualizarRanking()
    {
        if (!ValidarFiltroTiempo())
        {
            return;
        }

        paginaActual = 0;

        tiempoMinimoSegundos = ObtenerTiempoMinimo();

        LimpiarRanking();


        try
        {
            if (RankingOnline.Instancia == null)
            {
                Debug.LogWarning(
                    "RankingMundial -> RankingOnline no encontrado."
                );

                return;
            }

            if (!RankingOnline.Instancia.Disponible)
            {
                Debug.LogWarning(
                    "RankingMundial -> RankingOnline todavía no está disponible."
                );

                return;
            }

            // Pedimos hasta 50 resultados.
            var respuesta =
                await LeaderboardsService.Instance.GetScoresAsync(
                    leaderboardId,
                    new GetScoresOptions
                    {
                        Offset = 0,
                        Limit = maxResultados,
                        IncludeMetadata = true
                    }
                );

            resultados = new List<LeaderboardEntry>(
                respuesta.Results
            );

            Debug.Log(
                "RankingMundial -> Resultados recibidos: "
                + resultados.Count
            );

            AplicarFiltros();

            MostrarPagina();

        }
        catch (Exception error)
        {
            Debug.LogError(
                "RankingMundial -> Error al obtener ranking: "
                + error.Message
            );

        }
    }


    //==================================================
    // FILTROS
    //==================================================

    private void AplicarFiltros()
    {
        // Primero filtramos por nivel.
        string nivelSeleccionado =
            ObtenerNivelSeleccionado();

        List<LeaderboardEntry> filtrados =
            new List<LeaderboardEntry>();

        foreach (LeaderboardEntry entrada in resultados)
        {
            // -----------------------------------------
            // FILTRO DE TIEMPO
            // -----------------------------------------

            if (entrada.Score < tiempoMinimoSegundos)
            {
                continue;
            }


            // -----------------------------------------
            // FILTRO DE NIVEL
            // -----------------------------------------

            string nivelEntrada =
                ObtenerNivelDeMetadata(entrada);

            if (
                nivelSeleccionado != "Todos" &&
                nivelEntrada != nivelSeleccionado
            )
            {
                continue;
            }

            filtrados.Add(entrada);
        }

        resultados = filtrados;

        // Después de filtrar, empezamos nuevamente
        // desde la primera página.
        paginaActual = 0;
    }


    //==================================================
    // FILTRO DE NIVEL
    //==================================================

    private string ObtenerNivelSeleccionado()
    {
        if (filtroNivel == null)
        {
            return "Todos";
        }

        if (filtroNivel.options.Count == 0)
        {
            return "Todos";
        }

        return filtroNivel.options[
            filtroNivel.value
        ].text;
    }


    //==================================================
    // METADATA DEL NIVEL
    //==================================================

    private string ObtenerNivelDeMetadata(
        LeaderboardEntry entrada)
    {
        /*
         * Aquí leeremos el nivel guardado dentro
         * de la metadata de la puntuación.
         *
         * La estructura exacta de Metadata depende
         * de cómo creemos ScoreMetadata en
         * RankingOnline.
         *
         * Por ahora dejamos este método aislado
         * para no mezclarlo con la interfaz.
         */

        if (entrada.Metadata == null)
        {
            return "";
        }

        try
        {
            string metadata =
                entrada.Metadata.ToString();

            if (metadata.Contains("Personalizado"))
                return "Personalizado";

            if (metadata.Contains("Nivel 1"))
                return "Nivel 1";

            if (metadata.Contains("Nivel 2"))
                return "Nivel 2";

            if (metadata.Contains("Nivel 3"))
                return "Nivel 3";
        }
        catch
        {
            // Ignorar metadata inválida.
        }

        return "";
    }


    //==================================================
    // PAGINACIÓN
    //==================================================

    private int ObtenerCantidadPaginas()
    {
        if (resultados.Count == 0)
        {
            return 0;
        }

        return Mathf.CeilToInt(
            resultados.Count /
            (float)ResultadosPorPagina
        );
    }


    private void MostrarPagina()
    {
        int cantidadPaginas =
            ObtenerCantidadPaginas();

        if (cantidadPaginas == 0)
        {
            LimpiarRanking();

            if (textoPagina != null)
            {
                textoPagina.text = "0/0";
            }

            if (botonAnterior != null)
            {
                botonAnterior.gameObject.SetActive(false);
            }

            if (botonSiguiente != null)
            {
                botonSiguiente.gameObject.SetActive(false);
            }

            return;
        }


        // ---------------------------------------------
        // TEXTO DE PÁGINA
        // ---------------------------------------------

        if (textoPagina != null)
        {
            textoPagina.text =
                (paginaActual + 1)
                + "/"
                + cantidadPaginas;
        }


        // ---------------------------------------------
        // BOTÓN ANTERIOR
        // ---------------------------------------------

        if (botonAnterior != null)
        {
            botonAnterior.gameObject.SetActive(
                paginaActual > 0
            );
        }


        // ---------------------------------------------
        // BOTÓN SIGUIENTE
        // ---------------------------------------------

        if (botonSiguiente != null)
        {
            botonSiguiente.gameObject.SetActive(
                paginaActual < cantidadPaginas - 1
            );
        }


        // ---------------------------------------------
        // LIMPIAR FILAS
        // ---------------------------------------------

        LimpiarRanking();


        // ---------------------------------------------
        // DETERMINAR RANGO
        // ---------------------------------------------

        int inicio =
            paginaActual *
            ResultadosPorPagina;

        int fin =
            Mathf.Min(
                inicio + ResultadosPorPagina,
                resultados.Count
            );


        // ---------------------------------------------
        // MOSTRAR RESULTADOS
        // ---------------------------------------------

        for (int i = inicio; i < fin; i++)
        {
            int fila =
                i - inicio;

            LeaderboardEntry entrada =
                resultados[i];

            MostrarEntrada(
                fila,
                entrada,
                i + 1
            );
        }
    }


    //==================================================
    // MOSTRAR UNA ENTRADA
    //==================================================

    private void MostrarEntrada(
        int fila,
        LeaderboardEntry entrada,
        int puesto)
    {
        string nombre =
            entrada.PlayerName;

        if (string.IsNullOrEmpty(nombre))
        {
            nombre = "Jugador";
        }

        string tiempo =
            FormatearTiempo(
                entrada.Score
            );

        string nivel =
            ObtenerNivelDeMetadata(
                entrada
            );


        switch (fila)
        {
            case 0:
                puesto1.text = puesto.ToString();
                nombre1.text = nombre;
                tiempo1.text = tiempo;
                nivel1.text = nivel;
                break;

            case 1:
                puesto2.text = puesto.ToString();
                nombre2.text = nombre;
                tiempo2.text = tiempo;
                nivel2.text = nivel;
                break;

            case 2:
                puesto3.text = puesto.ToString();
                nombre3.text = nombre;
                tiempo3.text = tiempo;
                nivel3.text = nivel;
                break;

            case 3:
                puesto4.text = puesto.ToString();
                nombre4.text = nombre;
                tiempo4.text = tiempo;
                nivel4.text = nivel;
                break;

            case 4:
                puesto5.text = puesto.ToString();
                nombre5.text = nombre;
                tiempo5.text = tiempo;
                nivel5.text = nivel;
                break;
        }
    }


    //==================================================
    // PÁGINA ANTERIOR
    //==================================================

    public void PaginaAnterior()
    {
        if (paginaActual <= 0)
        {
            return;
        }

        paginaActual--;

        MostrarPagina();
    }


    //==================================================
    // PÁGINA SIGUIENTE
    //==================================================

    public void PaginaSiguiente()
    {
        int cantidadPaginas =
            ObtenerCantidadPaginas();

        if (
            paginaActual >=
            cantidadPaginas - 1
        )
        {
            return;
        }

        paginaActual++;

        MostrarPagina();
    }


    //==================================================
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
        // SEGUNDOS 00 - 59
        // ---------------------------------------------

        if (
            segundos < 0 ||
            segundos > 59
        )
        {
            MostrarErrorTiempo();
            return false;
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
    // OBTENER TIEMPO MÍNIMO
    //==================================================

    private int ObtenerTiempoMinimo()
    {
        int minutos = 0;
        int segundos = 0;

        int.TryParse(
            inputMinutos.text,
            out minutos
        );

        int.TryParse(
            inputSegundos.text,
            out segundos
        );

        return
            minutos * 60 +
            segundos;
    }


    //==================================================
    // FORMATEAR TIEMPO
    //==================================================

    private string FormatearTiempo(
        double segundosTotales)
    {
        int segundos =
            Mathf.Max(
                0,
                Mathf.FloorToInt(
                    (float)segundosTotales
                )
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
    // LIMPIAR FILAS
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

}
