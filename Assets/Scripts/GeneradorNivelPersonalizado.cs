using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class GeneradorNivelPersonalizado : GeneradorBase
{
    [Header("Prefabs de discos")]
    public GameObject discoNormalPrefab;
    public GameObject discoRapidoPrefab;
    public GameObject discoPesadoPrefab;
    public GameObject discoVenenosoPrefab;
    public GameObject discoExplosivoPrefab;

    [Header("Tiempo de generación")]
    public float tiempoMinimo = 0.5f;
    public float tiempoMaximo = 1.0f;

    [Header("Radio de aparición")]
    public float radioGeneracion = 0.5f;

    // Lista de todos los discos activos (servirá para guardado y eventos)
    public List<GameObject> discosActivos = new List<GameObject>();
    private bool[] tiposPermitidos = new bool[5];

    // Prefijo para diferenciar datos entre niveles
    private string prefijoGuardado = "NivelPersonalizado_";
    public void ConfigurarPrefijoGuardado(string nuevoPrefijo)
    {
        prefijoGuardado = nuevoPrefijo;
    }
    private Coroutine rutinaGeneracion;

    private bool overdriveActivo = false;
    private float multiplicadorActual = 1f;

    private bool slowActivo = false;
    private float multiplicadorSlow = 1f;

    private float probabilidadSegundoDisco = 0f;
    private float probabilidadTercerDisco = 0f;
    [SerializeField] public NivelPersonalizado nivel;

    void Awake()
    {
        EstablecerConfiguracionDiscos();
    }

    public void IniciarGeneracion()
    {
        if (rutinaGeneracion == null)
        {
            rutinaGeneracion = StartCoroutine(GenerarDiscos());
        }
    }

    public void ConfigurarProteccion(bool activa)
    {
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();

        if (boxCollider != null)
            boxCollider.enabled = activa;

        if (circleCollider != null)
            circleCollider.enabled = activa;

        if (sprite != null)
            sprite.enabled = activa;
    }

    void ActualizarDificultad()
    {
        float tiempo = nivel.ObtenerTiempo();

        //-------------------------
        // 0 - 5 segundos
        // (Por ahora no generar discos)
        //-------------------------
        if (tiempo < 5f)
        {
            return;
        }

        //-------------------------
        // 5 - 55 segundos
        //-------------------------
        else if (tiempo < 55f)
        {
            tiempoMinimo = 2.0f;
            tiempoMaximo = 3.0f;

            probabilidadSegundoDisco = 0f;
            probabilidadTercerDisco = 0f;
        }

        //-------------------------
        // 55 - 100 segundos
        //-------------------------
        else if (tiempo < 100f)
        {
            tiempoMinimo = 1.6f;
            tiempoMaximo = 2.3f;

            probabilidadSegundoDisco = 0.10f;
            probabilidadTercerDisco = 0f;
        }

        //-------------------------
        // 100 - 145 segundos
        //-------------------------
        else if (tiempo < 145f)
        {
            tiempoMinimo = 1.2f;
            tiempoMaximo = 1.8f;

            probabilidadSegundoDisco = 0.25f;
            probabilidadTercerDisco = 0.03f;
        }

        //-------------------------
        // 145 - 190 segundos
        //-------------------------
        else if (tiempo < 190f)
        {
            tiempoMinimo = 0.9f;
            tiempoMaximo = 1.5f;

            probabilidadSegundoDisco = 0.40f;
            probabilidadTercerDisco = 0.08f;
        }

        //-------------------------
        // 190 segundos en adelante
        //-------------------------
        else
        {
            tiempoMinimo = 0.6f;
            tiempoMaximo = 1.1f;

            probabilidadSegundoDisco = 0.60f;
            probabilidadTercerDisco = 0.15f;
        }
    }

    public void EstablecerConfiguracionDiscos()
    {
        tiposPermitidos[0] = PlayerPrefs.GetInt("Disco1", 0) == 1;
        tiposPermitidos[1] = PlayerPrefs.GetInt("Disco2", 0) == 1;
        tiposPermitidos[2] = PlayerPrefs.GetInt("Disco3", 0) == 1;
        tiposPermitidos[3] = PlayerPrefs.GetInt("Disco4", 0) == 1;
        tiposPermitidos[4] = PlayerPrefs.GetInt("Disco5", 0) == 1;
    }

    public void DetenerGeneracion()
    {
        if (rutinaGeneracion != null)
        {
            StopCoroutine(rutinaGeneracion);
            rutinaGeneracion = null;
        }
    }

    IEnumerator GenerarDiscos()
    {
        while (true)
        {
            // Ajustar dificultad según el tiempo de partida
            ActualizarDificultad();

            float espera = Random.Range(tiempoMinimo, tiempoMaximo);

            yield return new WaitForSeconds(espera);

            GenerarDisco();

            // Posibilidad de un segundo disco
            if (Random.value < probabilidadSegundoDisco)
            {
                GenerarDisco();
            }

            // Posibilidad de un tercer disco
            if (Random.value < probabilidadTercerDisco)
            {
                GenerarDisco();
            }
        }
    }

    void GenerarDisco()
    {
        Vector2 posicionAleatoria =
            (Vector2)transform.position +
            Random.insideUnitCircle * radioGeneracion;

        int tipo;

        // Elegimos aleatoriamente el tipo de disco
        do
        {
            tipo = Random.Range(0, 5);
        }
        while (!tiposPermitidos[tipo]);

        GameObject prefabSeleccionado = null;

        switch (tipo)
        {
            case 0:
                prefabSeleccionado = discoNormalPrefab;
                break;

            case 1:
                prefabSeleccionado = discoRapidoPrefab;
                break;

            case 2:
                prefabSeleccionado = discoPesadoPrefab;
                break;

            case 3:
                prefabSeleccionado = discoVenenosoPrefab;
                break;

            case 4:
                prefabSeleccionado = discoExplosivoPrefab;
                break;
        }

        GameObject nuevoDisco =
            Instantiate(prefabSeleccionado,
                        posicionAleatoria,
                        Quaternion.identity);

        // Obtener el script del disco
        Discos script = nuevoDisco.GetComponent<Discos>();

        // Inicializar el disco recién generado
        script.InicializarDisco(true);

        // Si el evento está activo, aplicar también al disco nuevo
        if (overdriveActivo)
        {
            script.ActivarOverdrive(multiplicadorActual);
        }

        //Si la habilidad Ralentizar está activa, aplicar también al disco nuevo
        if (slowActivo)
        {
            script.ActivarRalentizacion(multiplicadorSlow);
        }

        discosActivos.Add(nuevoDisco);
    }

    public void GuardarDiscos()
    {
        Debug.Log(discosActivos.Count);

        ActualizarListaDiscos();

        Debug.Log(discosActivos.Count);

        PlayerPrefs.SetInt(prefijoGuardado + "Activo", 1);

        PlayerPrefs.SetInt(prefijoGuardado + "CantidadDiscos", discosActivos.Count);

        for (int i = 0; i < discosActivos.Count; i++)
        {
            Discos disco = discosActivos[i].GetComponent<Discos>();

            PlayerPrefs.SetInt(
                prefijoGuardado + "Tipo_" + i,
                (int)disco.ObtenerTipo());

            PlayerPrefs.SetFloat(
                prefijoGuardado + "PosX_" + i,
                disco.ObtenerPosicion().x);

            PlayerPrefs.SetFloat(
                prefijoGuardado + "PosY_" + i,
                disco.ObtenerPosicion().y);

            PlayerPrefs.SetFloat(
                prefijoGuardado + "DirX_" + i,
                disco.ObtenerDireccion().x);

            PlayerPrefs.SetFloat(
                prefijoGuardado + "DirY_" + i,
                disco.ObtenerDireccion().y);

            GuardarDatosExplosivo(disco, i);
        }

        PlayerPrefs.Save();
    }

    void GuardarDatosExplosivo(Discos disco, int indice)
    {
        if (disco.ObtenerTipo() != Discos.TipoDisco.Explosivo)
            return;

        PlayerPrefs.SetInt(
            prefijoGuardado + "EstadoExplosion_" + indice,
            (int)disco.ObtenerEstadoExplosivo());

        PlayerPrefs.SetFloat(
            prefijoGuardado + "TemporizadorExplosion_" + indice,
            disco.ObtenerTemporizadorExplosion());

        PlayerPrefs.SetFloat(
            prefijoGuardado + "TiempoEstado_" + indice,
            disco.ObtenerTiempoEstado());

        PlayerPrefs.SetInt(
            prefijoGuardado + "DanioExplosion_" + indice,
            disco.ObtenerDanioExplosionAplicado() ? 1 : 0);
    }

    public void RestaurarDiscos()
    {
        int activo =
        PlayerPrefs.GetInt(
            prefijoGuardado + "Activo",
            0
        );

        // Este generador no pertenecía
        // a la partida que se está restaurando.
        if (activo == 0)
        {
            return;
        }

        int cantidad =
            PlayerPrefs.GetInt(prefijoGuardado + "CantidadDiscos", 0);

        for (int i = 0; i < cantidad; i++)
        {
            int tipo =
                PlayerPrefs.GetInt(prefijoGuardado + "Tipo_" + i);

            GameObject prefab = discoNormalPrefab;

            switch ((Discos.TipoDisco)tipo)
            {
                case Discos.TipoDisco.Normal:
                    prefab = discoNormalPrefab;
                    break;

                case Discos.TipoDisco.Rapido:
                    prefab = discoRapidoPrefab;
                    break;

                case Discos.TipoDisco.Pesado:
                    prefab = discoPesadoPrefab;
                    break;

                case Discos.TipoDisco.Venenoso:
                    prefab = discoVenenosoPrefab;
                    break;

                case Discos.TipoDisco.Explosivo:
                    prefab = discoExplosivoPrefab;
                    break;
            }

            Vector2 posicion = new Vector2(
                PlayerPrefs.GetFloat(prefijoGuardado + "PosX_" + i),
                PlayerPrefs.GetFloat(prefijoGuardado + "PosY_" + i));

            Vector2 direccion = new Vector2(
                PlayerPrefs.GetFloat(prefijoGuardado + "DirX_" + i),
                PlayerPrefs.GetFloat(prefijoGuardado + "DirY_" + i));

            GameObject nuevoDisco =
                Instantiate(prefab, posicion, Quaternion.identity);

            Discos script = nuevoDisco.GetComponent<Discos>();

            script.RestaurarDisco(posicion, direccion, false);
            RestaurarDatosExplosivo(script, i);
            script.PausarExplosivo();

            discosActivos.Add(nuevoDisco);
        }
    }

    void RestaurarDatosExplosivo(Discos disco, int indice)
    {
        if (disco.ObtenerTipo() != Discos.TipoDisco.Explosivo)
            return;

        Discos.EstadoExplosivo estado =
            (Discos.EstadoExplosivo)PlayerPrefs.GetInt(
                prefijoGuardado + "EstadoExplosion_" + indice);

        float temporizador =
            PlayerPrefs.GetFloat(
                prefijoGuardado + "TemporizadorExplosion_" + indice);

        float tiempoEstado =
            PlayerPrefs.GetFloat(
                prefijoGuardado + "TiempoEstado_" + indice);

        bool danioAplicado =
            PlayerPrefs.GetInt(
                prefijoGuardado + "DanioExplosion_" + indice) == 1;

        disco.RestaurarEstadoExplosivo(
            estado,
            temporizador,
            tiempoEstado,
            danioAplicado);
    }

    public void ReanudarDiscos()
    {
        foreach (GameObject disco in discosActivos)
        {
            if (disco != null)
            {
                Discos script = disco.GetComponent<Discos>();
                script.ReanudarMovimiento();
                script.ReanudarExplosivo();
            }
        }
    }

    public void LimpiarDiscos()
    {
        foreach (GameObject disco in discosActivos)
        {
            Destroy(disco);
        }

        discosActivos.Clear();
    }

    public void ActualizarListaDiscos()
    {
        for (int i = discosActivos.Count - 1; i >= 0; i--)
        {
            if (discosActivos[i] == null)
            {
                discosActivos.RemoveAt(i);
            }
        }
    }

    public override void ActivarOverdrive(float multiplicador)
    {
        overdriveActivo = true;
        multiplicadorActual = multiplicador;
        foreach (GameObject disco in discosActivos)
        {
            if (disco != null)
            {
                disco.GetComponent<Discos>().ActivarOverdrive(multiplicador);
            }
        }
    }

    public override void DesactivarOverdrive()
    {
        overdriveActivo = false;
        multiplicadorActual = 1f;

        foreach (GameObject disco in discosActivos)
        {
            if (disco != null)
            {
                disco.GetComponent<Discos>().DesactivarOverdrive();
            }
        }
    }

    public void MostrarOverdriveVisual()
    {
        foreach (GameObject disco in discosActivos)
        {
            if (disco != null)
            {
                disco.GetComponent<Discos>().MostrarOverdriveVisual();
            }
        }
    }

    public void OcultarOverdriveVisual()
    {
        foreach (GameObject disco in discosActivos)
        {
            if (disco != null)
            {
                disco.GetComponent<Discos>().OcultarOverdriveVisual();
            }
        }
    }

    //Efecto de Ralentización a Discos

    public override void ActivarRalentizacion(float multiplicador)
    {
        slowActivo = true;
        multiplicadorSlow = multiplicador;

        foreach (GameObject disco in discosActivos)
        {
            if (disco != null)
            {
                disco.GetComponent<Discos>().ActivarRalentizacion(multiplicador);
            }
        }
    }

    public override void DesactivarRalentizacion()
    {
        slowActivo = false;
        multiplicadorSlow = 1f;

        foreach (GameObject disco in discosActivos)
        {
            if (disco != null)
            {
                disco.GetComponent<Discos>().DesactivarRalentizacion();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LimpiarDatosGuardado()
    {
        PlayerPrefs.DeleteKey(prefijoGuardado + "Activo");

        PlayerPrefs.DeleteKey(prefijoGuardado + "CantidadDiscos");

        for (int i = 0; i < 200; i++)
        {
            PlayerPrefs.DeleteKey(prefijoGuardado + "Tipo_" + i);
            PlayerPrefs.DeleteKey(prefijoGuardado + "PosX_" + i);
            PlayerPrefs.DeleteKey(prefijoGuardado + "PosY_" + i);
            PlayerPrefs.DeleteKey(prefijoGuardado + "DirX_" + i);
            PlayerPrefs.DeleteKey(prefijoGuardado + "DirY_" + i);

            PlayerPrefs.DeleteKey(
                prefijoGuardado + "EstadoExplosion_" + i);

            PlayerPrefs.DeleteKey(
                prefijoGuardado + "TemporizadorExplosion_" + i);

            PlayerPrefs.DeleteKey(
                prefijoGuardado + "TiempoEstado_" + i);

            PlayerPrefs.DeleteKey(
                prefijoGuardado + "DanioExplosion_" + i);
        }
    }
}
