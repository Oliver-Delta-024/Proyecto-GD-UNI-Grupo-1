using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RankingMundial : MonoBehaviour
{

    //==================================================
    // CONFIGURACIÓN
    //==================================================

    [Header("Leaderboard")]

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

    [Header("Botones")]

    // Botón que aplica los filtros sobre los datos actuales.
    [SerializeField]
    private Button botonFiltrar;

    // Botón que vuelve a consultar Supabase.
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

    // Datos recibidos desde Supabase.
    private List<RankingOnline.ResultadoRanking> resultados =
        new List<RankingOnline.ResultadoRanking>();

    // Datos después de aplicar filtros.
    private List<RankingOnline.ResultadoRanking> resultadosFiltrados =
        new List<RankingOnline.ResultadoRanking>();

    // Página actual.
    private int paginaActual = 0;

    // Tiempo mínimo seleccionado.
    private int tiempoMinimoSegundos = 0;

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
        // BOTÓN FILTRAR
        // ---------------------------------------------

        if (botonFiltrar != null)
        {
            botonFiltrar.onClick.AddListener(
                FiltrarRanking
            );
        }


        // ---------------------------------------------
        // BOTÓN ACTUALIZAR
        // ---------------------------------------------

        if (botonActualizar != null)
        {
            botonActualizar.onClick.AddListener(
                ActualizarRanking
            );
        }

        // ---------------------------------------------
        // NAVEGACIÓN
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
    }


    //==================================================
    // ACTUALIZAR RANKING
    //==================================================

    public async void ActualizarRanking()
    {
        if (actualizacionEnCurso)
        {
            Debug.LogWarning(
                "RankingMundial -> Actualización ya en curso. Click ignorado."
            );
            return;
        }

        actualizacionEnCurso = true;

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


        // ---------------------------------------------
        // VALIDAR FILTRO DE TIEMPO
        // ---------------------------------------------

        if (!ValidarFiltroTiempo())
        {
            return;
        }


        // ---------------------------------------------
        // GUARDAR FILTRO ACTUAL
        // ---------------------------------------------

        tiempoMinimoSegundos =
            ObtenerTiempoMinimo();


        Debug.Log(
            "RankingMundial -> Actualizando datos desde Supabase..."
        );


        try
        {
            RankingOnline.RespuestaRanking respuesta =
                await RankingOnline.Instancia.ObtenerRanking(
                    "",
                    0,
                    1,
                    maxResultados
                );


            if (respuesta == null)
            {
                Debug.LogError(
                    "RankingMundial -> No se pudo obtener el ranking."
                );

                return;
            }


            // -----------------------------------------
            // GUARDAR RESULTADOS
            // -----------------------------------------

            resultados =
                new List<RankingOnline.ResultadoRanking>(
                    respuesta.resultados ??
                    new RankingOnline.ResultadoRanking[0]
                );


            Debug.Log(
                "RankingMundial -> Datos recibidos: "
                + resultados.Count
            );


            // -----------------------------------------
            // APLICAR FILTROS ACTIVOS
            // -----------------------------------------

            FiltrarRankingInterno();


            // -----------------------------------------
            // VOLVER A LA PRIMERA PÁGINA
            // -----------------------------------------

            paginaActual = 0;


            // -----------------------------------------
            // MOSTRAR
            // -----------------------------------------

            MostrarPagina();


            Debug.Log(
                "RankingMundial -> Ranking actualizado correctamente."
            );
        }
        catch (Exception error)
        {
            Debug.LogError(
                "RankingMundial -> Error al actualizar ranking: "
                + error.Message
            );
        }

        actualizacionEnCurso = false;
    }


    //==================================================
    // FILTRAR RANKING
    //==================================================

    public void FiltrarRanking()
    {
        if (!ValidarFiltroTiempo())
        {
            return;
        }


        // ---------------------------------------------
        // OBTENER NUEVO TIEMPO MÍNIMO
        // ---------------------------------------------

        tiempoMinimoSegundos =
            ObtenerTiempoMinimo();


        Debug.Log(
            "RankingMundial -> Aplicando filtros..."
        );


        // ---------------------------------------------
        // APLICAR FILTROS
        // ---------------------------------------------

        FiltrarRankingInterno();


        // ---------------------------------------------
        // VOLVER A PRIMERA PÁGINA
        // ---------------------------------------------

        paginaActual = 0;


        // ---------------------------------------------
        // MOSTRAR
        // ---------------------------------------------

        MostrarPagina();


        Debug.Log(
            "RankingMundial -> Filtros aplicados."
        );
    }


    //==================================================
    // FILTRADO INTERNO
    //==================================================

    private void FiltrarRankingInterno()
    {
        resultadosFiltrados =
            new List<RankingOnline.ResultadoRanking>();


        string nivelSeleccionado =
            ObtenerNivelSeleccionado();


        foreach (
            RankingOnline.ResultadoRanking entrada
            in resultados)
        {
            // -----------------------------------------
            // FILTRO DE TIEMPO
            // -----------------------------------------

            if (
                entrada.tiempo <
                tiempoMinimoSegundos
            )
            {
                continue;
            }


            // -----------------------------------------
            // FILTRO DE NIVEL
            // -----------------------------------------

            if (
                nivelSeleccionado != "Todos" &&
                !string.Equals(
                    entrada.nivel?.Trim(),
                    nivelSeleccionado,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                continue;
            }


            resultadosFiltrados.Add(
                entrada
            );
        }
    }

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
    // PAGINACIÓN
    //==================================================

    private int ObtenerCantidadPaginas()
    {
        if (
            resultadosFiltrados == null ||
            resultadosFiltrados.Count == 0
        )
        {
            return 0;
        }


        return Mathf.CeilToInt(
            resultadosFiltrados.Count /
            (float)ResultadosPorPagina
        );
    }


    private void MostrarPagina()
    {
        int cantidadPaginas =
            ObtenerCantidadPaginas();


        // ---------------------------------------------
        // NO HAY RESULTADOS
        // ---------------------------------------------

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
        // ASEGURAR PÁGINA VÁLIDA
        // ---------------------------------------------

        if (paginaActual < 0)
        {
            paginaActual = 0;
        }


        if (
            paginaActual >=
            cantidadPaginas
        )
        {
            paginaActual =
                cantidadPaginas - 1;
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
                paginaActual <
                cantidadPaginas - 1
            );
        }


        // ---------------------------------------------
        // LIMPIAR FILAS
        // ---------------------------------------------

        LimpiarRanking();


        // ---------------------------------------------
        // RANGO
        // ---------------------------------------------

        int inicio =
            paginaActual *
            ResultadosPorPagina;


        int fin =
            Mathf.Min(
                inicio +
                ResultadosPorPagina,
                resultadosFiltrados.Count
            );


        // ---------------------------------------------
        // MOSTRAR
        // ---------------------------------------------

        for (
            int i = inicio;
            i < fin;
            i++
        )
        {
            int fila =
                i - inicio;


            RankingOnline.ResultadoRanking entrada =
                resultadosFiltrados[i];


            MostrarEntrada(
                fila,
                entrada,
                i + 1
            );
        }
    }

    //==================================================
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
    // OBTENER TIEMPO MÍNIMO
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
