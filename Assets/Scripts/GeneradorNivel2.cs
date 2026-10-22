using UnityEngine;

public class GeneradorNivel2 : GeneradorDiscosBase
{
    [Header("Prefabs de discos")]
    public GameObject discoNormalPrefab;
    public GameObject discoRapidoPrefab;
    public GameObject discoPesadoPrefab;
    [SerializeField] public Nivel2 nivel;

    protected override string PrefijoGuardado => "Nivel2_";
    protected override float ObtenerTiempoNivel() => nivel.ObtenerTiempo();

    protected override GameObject SeleccionarPrefabParaGenerar()
    {
        int tipo = Random.Range(0, 3);
        switch (tipo)
        {
            case 0: return discoNormalPrefab;
            case 1: return discoRapidoPrefab;
            case 2: return discoPesadoPrefab;
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
            default: return discoNormalPrefab;
        }
    }
}