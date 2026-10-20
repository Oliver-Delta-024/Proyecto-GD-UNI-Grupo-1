using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class GeneradorControlNivelPersonalizado : GeneradorBase
{
    //==================================================
    // PREFABS DE DISCOS
    //==================================================

    [Header("Prefabs de discos")]

    public GameObject discoNormalPrefab;
    public GameObject discoRapidoPrefab;
    public GameObject discoPesadoPrefab;
    public GameObject discoVenenosoPrefab;
    public GameObject discoExplosivoPrefab;

    //==================================================
    // GENERADORES
    //==================================================

    [Header("Generadores")]

    public GeneradorNivelPersonalizado generadorCentro;

    public GeneradorNivelPersonalizado generadorIzquierdaArriba;

    public GeneradorNivelPersonalizado generadorIzquierdaAbajo;

    public GeneradorNivelPersonalizado generadorDerechaArriba;

    public GeneradorNivelPersonalizado generadorDerechaAbajo;


    //==================================================
    // GENERADORES ACTIVOS
    //==================================================

    // Aquí estarán solamente los generadores que
    // fueron seleccionados en la configuración.
    private List<GeneradorNivelPersonalizado> generadoresActivos =
        new List<GeneradorNivelPersonalizado>();


    //==================================================
    // DISCOS
    //==================================================

    // Lista general de todos los discos que existen
    // en los 5 generadores.
    public List<GameObject> discosActivos =
        new List<GameObject>();


    //==================================================
    // GUARDADO
    //==================================================

    // Por ahora solamente dejamos preparado el prefijo.
    // La lógica de guardado se hará posteriormente.
    private string prefijoGuardado =
        "NivelPersonalizado_";


    //==================================================
    // REFERENCIA AL NIVEL
    //==================================================

    [Header("Nivel")]

    public NivelPersonalizado nivel;


    //==================================================
    // ESTADO DE GENERADORES
    //==================================================

    private bool generacionActiva = false;


    //==================================================
    // ESTADO DE EVENTOS
    //==================================================

    private bool slowActivo = false;

    private float multiplicadorSlow = 1f;


    //==================================================
    // UNITY
    //==================================================

    void Awake()
    {
        ConfigurarGeneradores();
        ConfigurarPrefijosGuardado();
    }

    //==================================================
    // CONFIGURAR GENERADORES
    //==================================================

    public void ConfigurarGeneradores()
    {
        // Limpiar la lista por seguridad
        generadoresActivos.Clear();


        //==============================================
        // CENTRO
        //==============================================

        if (PlayerPrefs.GetInt("GenCentro", 0) == 1)
        {
            if (generadorCentro != null)
            {
                generadoresActivos.Add(generadorCentro);
            }
        }


        //==============================================
        // IZQUIERDA ARRIBA
        //==============================================

        if (PlayerPrefs.GetInt("GenIzqArr", 0) == 1)
        {
            if (generadorIzquierdaArriba != null)
            {
                generadoresActivos.Add(generadorIzquierdaArriba);
            }
        }


        //==============================================
        // IZQUIERDA ABAJO
        //==============================================

        if (PlayerPrefs.GetInt("GenIzqAba", 0) == 1)
        {
            if (generadorIzquierdaAbajo != null)
            {
                generadoresActivos.Add(generadorIzquierdaAbajo);
            }
        }


        //==============================================
        // DERECHA ARRIBA
        //==============================================

        if (PlayerPrefs.GetInt("GenDerArr", 0) == 1)
        {
            if (generadorDerechaArriba != null)
            {
                generadoresActivos.Add(generadorDerechaArriba);
            }
        }


        //==============================================
        // DERECHA ABAJO
        //==============================================

        if (PlayerPrefs.GetInt("GenDerAba", 0) == 1)
        {
            if (generadorDerechaAbajo != null)
            {
                generadoresActivos.Add(generadorDerechaAbajo);
            }
        }

        //==============================================
        // CONFIGURAR PROTECCIÓN
        //==============================================

        // Primero desactivar la protección de todos
        if (generadorCentro != null)
            generadorCentro.ConfigurarProteccion(false);

        if (generadorIzquierdaArriba != null)
            generadorIzquierdaArriba.ConfigurarProteccion(false);

        if (generadorIzquierdaAbajo != null)
            generadorIzquierdaAbajo.ConfigurarProteccion(false);

        if (generadorDerechaArriba != null)
            generadorDerechaArriba.ConfigurarProteccion(false);

        if (generadorDerechaAbajo != null)
            generadorDerechaAbajo.ConfigurarProteccion(false);


        // Activar protección únicamente
        // en los generadores activos
        foreach (GeneradorNivelPersonalizado generador
                 in generadoresActivos)
        {
            if (generador != null)
            {
                generador.ConfigurarProteccion(true);
            }
        }

        //==============================================
        // INFORMACIÓN
        //==============================================

        Debug.Log(
            "GeneradorControl -> Generadores activos: "
            + generadoresActivos.Count
        );

        foreach (GeneradorNivelPersonalizado generador
                 in generadoresActivos)
        {
            Debug.Log(
                "Generador activo: "
                + generador.gameObject.name
            );
        }
    }

    //==================================================
    // CONFIGURAR PREFIJOS DE GUARDADO
    //==================================================

    public void ConfigurarPrefijosGuardado()
    {
        if (generadorCentro != null)
        {
            generadorCentro.ConfigurarPrefijoGuardado(
                prefijoGuardado + "GenCentro_"
            );
        }

        if (generadorIzquierdaArriba != null)
        {
            generadorIzquierdaArriba.ConfigurarPrefijoGuardado(
                prefijoGuardado + "GenIzqArr_"
            );
        }

        if (generadorIzquierdaAbajo != null)
        {
            generadorIzquierdaAbajo.ConfigurarPrefijoGuardado(
                prefijoGuardado + "GenIzqAba_"
            );
        }

        if (generadorDerechaArriba != null)
        {
            generadorDerechaArriba.ConfigurarPrefijoGuardado(
                prefijoGuardado + "GenDerArr_"
            );
        }

        if (generadorDerechaAbajo != null)
        {
            generadorDerechaAbajo.ConfigurarPrefijoGuardado(
                prefijoGuardado + "GenDerAba_"
            );
        }
    }

    //==================================================
    // GENERACIÓN
    //==================================================

    public void IniciarGeneracion()
    {
        if (generacionActiva)
            return;

        generacionActiva = true;

        foreach (GeneradorNivelPersonalizado generador
                 in generadoresActivos)
        {
            if (generador != null)
            {
                generador.IniciarGeneracion();
            }
        }

        Debug.Log(
            "GeneradorControl -> Generación iniciada en "
            + generadoresActivos.Count
            + " generadores."
        );
    }


    public void DetenerGeneracion()
    {
        if (!generacionActiva)
            return;

        generacionActiva = false;

        foreach (GeneradorNivelPersonalizado generador
                 in generadoresActivos)
        {
            if (generador != null)
            {
                generador.DetenerGeneracion();
            }
        }

        Debug.Log(
            "GeneradorControl -> Generación detenida."
        );
    }


    //==================================================
    // LISTA DE DISCOS
    //==================================================

    public void ActualizarListaDiscos()
    {
        // Primero eliminar únicamente referencias nulas
        // de la lista que ya controla este script.
        for (int i = discosActivos.Count - 1; i >= 0; i--)
        {
            if (discosActivos[i] == null)
            {
                discosActivos.RemoveAt(i);
            }
        }


        //==================================================
        // RECOPILAR DISCOS NUEVOS DE LOS GENERADORES
        //==================================================

        foreach (GeneradorNivelPersonalizado generador
                 in generadoresActivos)
        {
            if (generador == null)
                continue;

            // El generador limpia sus propias referencias nulas.
            generador.ActualizarListaDiscos();

            foreach (GameObject disco
                     in generador.discosActivos)
            {
                if (disco == null)
                    continue;

                // Evitar duplicados.
                if (!discosActivos.Contains(disco))
                {
                    discosActivos.Add(disco);
                }
            }
        }


        Debug.Log(
            "GeneradorControl -> Lista general actualizada. "
            + "Discos activos: "
            + discosActivos.Count
        );
    }


    //==================================================
    // EVENTO OVERDRIVE
    //==================================================

    public override void ActivarOverdrive(float multiplicador)
    {

        // Asegurarnos de tener todos los discos actuales
        ActualizarListaDiscos();

        foreach (GameObject objetoDisco in discosActivos)
        {
            if (objetoDisco == null)
                continue;

            Discos disco = objetoDisco.GetComponent<Discos>();

            if (disco != null)
            {
                disco.ActivarOverdrive(multiplicador);
            }
        }

        // Mantener informado a los generadores activos.
        foreach (GeneradorNivelPersonalizado generador
                 in generadoresActivos)
        {
            if (generador != null)
            {
                generador.ActivarOverdrive(multiplicador);
            }
        }

        Debug.Log(
            "GeneradorControl -> Overdrive activado."
        );
    }


    public override void DesactivarOverdrive()
    {

        foreach (GameObject objetoDisco in discosActivos)
        {
            if (objetoDisco == null)
                continue;

            Discos disco = objetoDisco.GetComponent<Discos>();

            if (disco != null)
            {
                disco.DesactivarOverdrive();
            }
        }

        foreach (GeneradorNivelPersonalizado generador
             in generadoresActivos)
        {
            if (generador != null)
            {
                generador.DesactivarOverdrive();
            }
        }

        Debug.Log(
            "GeneradorControl -> Overdrive desactivado."
        );
    }

    public void MostrarOverdriveVisual()
    {
        foreach (GameObject objetoDisco in discosActivos)
        {
            if (objetoDisco == null)
                continue;

            Discos disco = objetoDisco.GetComponent<Discos>();

            if (disco != null)
            {
                disco.MostrarOverdriveVisual();
            }
        }
    }


    public void OcultarOverdriveVisual()
    {
        foreach (GameObject objetoDisco in discosActivos)
        {
            if (objetoDisco == null)
                continue;

            Discos disco = objetoDisco.GetComponent<Discos>();

            if (disco != null)
            {
                disco.OcultarOverdriveVisual();
            }
        }
    }


    //==================================================
    // RALENTIZACIÓN
    //==================================================

    public override void ActivarRalentizacion(float multiplicador)
    {
        slowActivo = true;
        multiplicadorSlow = multiplicador;

        // Asegurarnos de tener todos los discos actuales
        ActualizarListaDiscos();

        foreach (GameObject objetoDisco in discosActivos)
        {
            if (objetoDisco == null)
                continue;

            Discos disco = objetoDisco.GetComponent<Discos>();

            if (disco != null)
            {
                disco.ActivarRalentizacion(multiplicador);
            }
        }

        foreach (GeneradorNivelPersonalizado generador
             in generadoresActivos)
        {
            if (generador != null)
            {
                generador.ActivarRalentizacion(multiplicador);
            }
        }

        Debug.Log(
            "GeneradorControl -> Ralentización activada."
        );
    }


    public override void DesactivarRalentizacion()
    {
        slowActivo = false;
        multiplicadorSlow = 1f;

        foreach (GameObject objetoDisco in discosActivos)
        {
            if (objetoDisco == null)
                continue;

            Discos disco = objetoDisco.GetComponent<Discos>();

            if (disco != null)
            {
                disco.DesactivarRalentizacion();
            }
        }

        foreach (GeneradorNivelPersonalizado generador
             in generadoresActivos)
        {
            if (generador != null)
            {
                generador.DesactivarRalentizacion();
            }
        }

        Debug.Log(
            "GeneradorControl -> Ralentización desactivada."
        );
    }

    //==================================================
    // GUARDADO Y RESTAURACION
    //==================================================

    public void GuardarDiscos()
    {
        Debug.Log(
        "===== GUARDANDO DISCOS NIVEL PERSONALIZADO ====="
    );

        // Recopilar todos los discos existentes
        ActualizarListaDiscos();

        // Marcar que existe un guardado
        PlayerPrefs.SetInt(
            prefijoGuardado + "DiscosActivos",
            1
        );

        // Cantidad total de discos
        PlayerPrefs.SetInt(
            prefijoGuardado + "CantidadDiscos",
            discosActivos.Count
        );

        // Guardar cada disco
        for (int i = 0; i < discosActivos.Count; i++)
        {
            GameObject objetoDisco = discosActivos[i];

            if (objetoDisco == null)
                continue;

            Discos disco = objetoDisco.GetComponent<Discos>();

            if (disco == null)
                continue;

            // Tipo
            PlayerPrefs.SetInt(
                prefijoGuardado + "Tipo_" + i,
                (int)disco.ObtenerTipo()
            );

            // Posición
            PlayerPrefs.SetFloat(
                prefijoGuardado + "PosX_" + i,
                disco.ObtenerPosicion().x
            );

            PlayerPrefs.SetFloat(
                prefijoGuardado + "PosY_" + i,
                disco.ObtenerPosicion().y
            );

            // Dirección
            PlayerPrefs.SetFloat(
                prefijoGuardado + "DirX_" + i,
                disco.ObtenerDireccion().x
            );

            PlayerPrefs.SetFloat(
                prefijoGuardado + "DirY_" + i,
                disco.ObtenerDireccion().y
            );

            // Datos especiales del explosivo
            GuardarDatosExplosivo(disco, i);
        }

        PlayerPrefs.Save();

        Debug.Log(
            "GeneradorControl -> "
            + discosActivos.Count
            + " discos guardados correctamente."
        );
    }

    void GuardarDatosExplosivo(Discos disco, int indice)
    {
        if (disco.ObtenerTipo() != Discos.TipoDisco.Explosivo)
            return;

        PlayerPrefs.SetInt(
            prefijoGuardado + "EstadoExplosion_" + indice,
            (int)disco.ObtenerEstadoExplosivo()
        );

        PlayerPrefs.SetFloat(
            prefijoGuardado + "TemporizadorExplosion_" + indice,
            disco.ObtenerTemporizadorExplosion()
        );

        PlayerPrefs.SetFloat(
            prefijoGuardado + "TiempoEstado_" + indice,
            disco.ObtenerTiempoEstado()
        );

        PlayerPrefs.SetInt(
            prefijoGuardado + "DanioExplosion_" + indice,
            disco.ObtenerDanioExplosionAplicado() ? 1 : 0
        );
    }

    public void RestaurarDiscos()
    {
        Debug.Log(
        "===== RESTAURANDO DISCOS NIVEL PERSONALIZADO ====="
    );

        // Limpiar la lista general
        discosActivos.Clear();

        // Comprobar si existe guardado de discos
        int activo = PlayerPrefs.GetInt(
            prefijoGuardado + "DiscosActivos",
            0
        );

        if (activo == 0)
        {
            Debug.Log(
                "GeneradorControl -> No existen discos guardados."
            );

            return;
        }

        int cantidad = PlayerPrefs.GetInt(
            prefijoGuardado + "CantidadDiscos",
            0
        );

        for (int i = 0; i < cantidad; i++)
        {
            //========================================
            // TIPO
            //========================================

            int tipo = PlayerPrefs.GetInt(
                prefijoGuardado + "Tipo_" + i
            );

            GameObject prefab = ObtenerPrefabDisco(tipo);

            if (prefab == null)
            {
                Debug.LogWarning(
                    "GeneradorControl -> No se encontró prefab para el disco "
                    + i
                );

                continue;
            }

            //========================================
            // POSICIÓN
            //========================================

            Vector2 posicion = new Vector2(
                PlayerPrefs.GetFloat(
                    prefijoGuardado + "PosX_" + i
                ),
                PlayerPrefs.GetFloat(
                    prefijoGuardado + "PosY_" + i
                )
            );

            //========================================
            // DIRECCIÓN
            //========================================

            Vector2 direccion = new Vector2(
                PlayerPrefs.GetFloat(
                    prefijoGuardado + "DirX_" + i
                ),
                PlayerPrefs.GetFloat(
                    prefijoGuardado + "DirY_" + i
                )
            );

            //========================================
            // CREAR DISCO
            //========================================

            GameObject nuevoDisco = Instantiate(
                prefab,
                posicion,
                Quaternion.identity
            );

            Discos script = nuevoDisco.GetComponent<Discos>();

            if (script == null)
            {
                Destroy(nuevoDisco);
                continue;
            }

            // Restaurar movimiento
            script.RestaurarDisco(
                posicion,
                direccion,
                false
            );

            // Restaurar datos del explosivo
            RestaurarDatosExplosivo(
                script,
                i
            );

            // Mantenerlo pausado hasta que la partida continúe
            script.PausarExplosivo();

            // Agregar a la lista global
            discosActivos.Add(nuevoDisco);
        }

        Debug.Log(
            "GeneradorControl -> "
            + discosActivos.Count
            + " discos restaurados correctamente."
        );
    }

    GameObject ObtenerPrefabDisco(int tipo)
    {
        switch ((Discos.TipoDisco)tipo)
        {
            case Discos.TipoDisco.Normal:
                return discoNormalPrefab;

            case Discos.TipoDisco.Rapido:
                return discoRapidoPrefab;

            case Discos.TipoDisco.Pesado:
                return discoPesadoPrefab;

            case Discos.TipoDisco.Venenoso:
                return discoVenenosoPrefab;

            case Discos.TipoDisco.Explosivo:
                return discoExplosivoPrefab;

            default:
                return null;
        }
    }

    void RestaurarDatosExplosivo(Discos disco, int indice)
    {
        if (disco.ObtenerTipo() != Discos.TipoDisco.Explosivo)
            return;

        Discos.EstadoExplosivo estado =
            (Discos.EstadoExplosivo)PlayerPrefs.GetInt(
                prefijoGuardado + "EstadoExplosion_" + indice
            );

        float temporizador =
            PlayerPrefs.GetFloat(
                prefijoGuardado + "TemporizadorExplosion_" + indice
            );

        float tiempoEstado =
            PlayerPrefs.GetFloat(
                prefijoGuardado + "TiempoEstado_" + indice
            );

        bool danioAplicado =
            PlayerPrefs.GetInt(
                prefijoGuardado + "DanioExplosion_" + indice,
                0
            ) == 1;

        disco.RestaurarEstadoExplosivo(
            estado,
            temporizador,
            tiempoEstado,
            danioAplicado
        );
    }

    public void ReanudarDiscos()
    {
        // Actualizar la lista para asegurarnos de trabajar
        // con todos los discos existentes.
        ActualizarListaDiscos();

        foreach (GameObject objetoDisco in discosActivos)
        {
            if (objetoDisco == null)
                continue;

            Discos disco = objetoDisco.GetComponent<Discos>();

            if (disco != null)
            {
                disco.ReanudarMovimiento();
                disco.ReanudarExplosivo();
            }
        }

        Debug.Log(
            "GeneradorControl -> Discos reanudados: "
            + discosActivos.Count
        );
    }

    public void LimpiarDiscos()
    {
        foreach (GameObject objetoDisco in discosActivos)
        {
            if (objetoDisco != null)
            {
                Destroy(objetoDisco);
            }
        }

        discosActivos.Clear();

        Debug.Log(
            "GeneradorControl -> Todos los discos eliminados."
        );
    }

    public void LimpiarDatosGuardado()
    {
        generadorCentro.LimpiarDatosGuardado();
        generadorIzquierdaArriba.LimpiarDatosGuardado();
        generadorIzquierdaAbajo.LimpiarDatosGuardado();
        generadorDerechaArriba.LimpiarDatosGuardado();
        generadorDerechaAbajo.LimpiarDatosGuardado();

        PlayerPrefs.Save();

        Debug.Log("Datos guardados de los 5 generadores eliminados.");
    }
}
