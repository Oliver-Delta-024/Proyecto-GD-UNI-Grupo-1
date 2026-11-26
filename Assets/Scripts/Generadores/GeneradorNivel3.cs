using UnityEngine;

public class GeneradorNivel3 : GeneradorDiscosBase
{
    [Header("Prefabs de discos")]
    public GameObject discoNormalPrefab;
    public GameObject discoRapidoPrefab;
    public GameObject discoPesadoPrefab;
    public GameObject discoVenenosoPrefab;
    public GameObject discoExplosivoPrefab;
    [SerializeField] public Nivel3 nivel;

    protected override string PrefijoGuardado => "Nivel3_";
    protected override float ObtenerTiempoNivel() => nivel.ObtenerTiempo();

    protected override GameObject SeleccionarPrefabParaGenerar()
    {
        int tipo = Random.Range(0, 5);
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
}
