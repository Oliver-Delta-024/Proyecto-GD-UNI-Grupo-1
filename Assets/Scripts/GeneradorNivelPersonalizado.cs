using UnityEngine;

public class GeneradorNivelPersonalizado : GeneradorDiscosBase
{
    [Header("Prefabs de discos")]
    public GameObject discoNormalPrefab;
    public GameObject discoRapidoPrefab;
    public GameObject discoPesadoPrefab;
    public GameObject discoVenenosoPrefab;
    public GameObject discoExplosivoPrefab;

    private bool[] tiposPermitidos = new bool[5];
    [SerializeField] public NivelPersonalizado nivel;

    protected override string PrefijoGuardado =>
        string.IsNullOrEmpty(prefijoGuardado) ? "NivelPersonalizado_" : prefijoGuardado;

    protected override float ObtenerTiempoNivel() => nivel.ObtenerTiempo();
    protected override bool PuedeRestaurarDiscos() =>
        PlayerPrefs.GetInt(PrefijoGuardado + "Activo", 0) == 1;

    protected override void MarcarGuardadoDisponible()
    {
        PlayerPrefs.SetInt(PrefijoGuardado + "Activo", 1);
    }

    void Awake()
    {
        EstablecerConfiguracionDiscos();
    }

    public void ConfigurarProteccion(bool activa)
    {
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (boxCollider != null) boxCollider.enabled = activa;
        if (circleCollider != null) circleCollider.enabled = activa;
        if (sprite != null) sprite.enabled = activa;
    }

    public void EstablecerConfiguracionDiscos()
    {
        tiposPermitidos[0] = PlayerPrefs.GetInt("Disco1", 0) == 1;
        tiposPermitidos[1] = PlayerPrefs.GetInt("Disco2", 0) == 1;
        tiposPermitidos[2] = PlayerPrefs.GetInt("Disco3", 0) == 1;
        tiposPermitidos[3] = PlayerPrefs.GetInt("Disco4", 0) == 1;
        tiposPermitidos[4] = PlayerPrefs.GetInt("Disco5", 0) == 1;
    }

    protected override GameObject SeleccionarPrefabParaGenerar()
    {
        int tipo;
        do
        {
            tipo = Random.Range(0, 5);
        }
        while (!tiposPermitidos[tipo]);
        switch (tipo)
        {
            case 0: return discoNormalPrefab;
            case 1: return discoRapidoPrefab;
            case 2: return discoPesadoPrefab;
            case 3: return discoVenenosoPrefab;
            case 4: return discoExplosivoPrefab;
            default: return discoNormalPrefab;
        }
    }

    protected override GameObject ObtenerPrefabPorTipo(Discos.TipoDisco tipo)
    {
        switch (tipo)
        {
            case Discos.TipoDisco.Normal: return discoNormalPrefab;
            case Discos.TipoDisco.Rapido: return discoRapidoPrefab;
            case Discos.TipoDisco.Pesado: return discoPesadoPrefab;
            case Discos.TipoDisco.Venenoso: return discoVenenosoPrefab;
            case Discos.TipoDisco.Explosivo: return discoExplosivoPrefab;
            default: return discoNormalPrefab;
        }
    }

    protected override void AntesDeLimpiarListaParaGuardado()
    {
        Debug.Log(discosActivos.Count);
    }

    protected override void DespuesDeLimpiarListaParaGuardado()
    {
        Debug.Log(discosActivos.Count);
    }

    protected override void GuardarDatosEspecificos(Discos disco, int indice)
    {
        if (disco.ObtenerTipo() != Discos.TipoDisco.Explosivo) return;
        PlayerPrefs.SetInt(PrefijoGuardado + "EstadoExplosion_" + indice,
            (int)disco.ObtenerEstadoExplosivo());
        PlayerPrefs.SetFloat(PrefijoGuardado + "TemporizadorExplosion_" + indice,
            disco.ObtenerTemporizadorExplosion());
        PlayerPrefs.SetFloat(PrefijoGuardado + "TiempoEstado_" + indice,
            disco.ObtenerTiempoEstado());
        PlayerPrefs.SetInt(PrefijoGuardado + "DanioExplosion_" + indice,
            disco.ObtenerDanioExplosionAplicado() ? 1 : 0);
    }

    protected override void RestaurarDatosEspecificos(Discos disco, int indice)
    {
        if (disco.ObtenerTipo() == Discos.TipoDisco.Explosivo)
        {
            Discos.EstadoExplosivo estado = (Discos.EstadoExplosivo)PlayerPrefs.GetInt(
                PrefijoGuardado + "EstadoExplosion_" + indice);
            float temporizador = PlayerPrefs.GetFloat(
                PrefijoGuardado + "TemporizadorExplosion_" + indice);
            float tiempoEstado = PlayerPrefs.GetFloat(
                PrefijoGuardado + "TiempoEstado_" + indice);
            bool danioAplicado = PlayerPrefs.GetInt(
                PrefijoGuardado + "DanioExplosion_" + indice) == 1;
            disco.RestaurarEstadoExplosivo(estado, temporizador, tiempoEstado, danioAplicado);
        }

        disco.PausarExplosivo();
    }

    protected override void ReanudarDatosEspecificos(Discos disco)
    {
        disco.ReanudarExplosivo();
    }

    public void LimpiarDatosGuardado()
    {
        PlayerPrefs.DeleteKey(PrefijoGuardado + "Activo");
        PlayerPrefs.DeleteKey(PrefijoGuardado + "CantidadDiscos");
        for (int i = 0; i < 200; i++)
        {
            PlayerPrefs.DeleteKey(PrefijoGuardado + "Tipo_" + i);
            PlayerPrefs.DeleteKey(PrefijoGuardado + "PosX_" + i);
            PlayerPrefs.DeleteKey(PrefijoGuardado + "PosY_" + i);
            PlayerPrefs.DeleteKey(PrefijoGuardado + "DirX_" + i);
            PlayerPrefs.DeleteKey(PrefijoGuardado + "DirY_" + i);
            PlayerPrefs.DeleteKey(PrefijoGuardado + "EstadoExplosion_" + i);
            PlayerPrefs.DeleteKey(PrefijoGuardado + "TemporizadorExplosion_" + i);
            PlayerPrefs.DeleteKey(PrefijoGuardado + "TiempoEstado_" + i);
            PlayerPrefs.DeleteKey(PrefijoGuardado + "DanioExplosion_" + i);
        }
    }
}
