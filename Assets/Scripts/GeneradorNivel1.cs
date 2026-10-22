using UnityEngine;

public class GeneradorNivel1 : GeneradorDiscosBase
{
    [Header("Prefab del disco")]
    public GameObject discoNormalPrefab;
    [SerializeField] public Nivel1 nivel;

    protected override string PrefijoGuardado => "Nivel1_";
    protected override float ObtenerTiempoNivel() => nivel.ObtenerTiempo();
    protected override GameObject SeleccionarPrefabParaGenerar() => discoNormalPrefab;

    protected override GameObject ObtenerPrefabPorTipo(Discos.TipoDisco tipo)
    {
        switch (tipo)
        {
            case Discos.TipoDisco.Normal:
                return discoNormalPrefab;
            default:
                return discoNormalPrefab;
        }
    }
}