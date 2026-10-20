using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Shared lifecycle for generators that spawn and own disk instances.
/// Concrete generators supply their time source, prefab selection, save prefix,
/// and any special disk-state handling.
/// </summary>
public abstract class GeneradorDiscosBase : GeneradorBase
{
    [Header("Generación común")]
    public float tiempoMinimo = 0.5f;
    public float tiempoMaximo = 1.0f;
    public float radioGeneracion = 0.5f;
    public List<GameObject> discosActivos = new List<GameObject>();

    protected float probabilidadSegundoDisco;
    protected float probabilidadTercerDisco;

    private Coroutine rutinaGeneracion;
    protected string prefijoGuardado = string.Empty;

    protected abstract float ObtenerTiempoNivel();
    protected abstract GameObject SeleccionarPrefabParaGenerar();
    protected abstract GameObject ObtenerPrefabPorTipo(Discos.TipoDisco tipo);

    protected virtual string PrefijoGuardado => prefijoGuardado;
    protected virtual bool PuedeRestaurarDiscos() => true;
    protected virtual void MarcarGuardadoDisponible() { }
    protected virtual void GuardarDatosEspecificos(Discos disco, int indice) { }
    protected virtual void RestaurarDatosEspecificos(Discos disco, int indice) { }
    protected virtual void ReanudarDatosEspecificos(Discos disco) { }
    protected virtual void AntesDeLimpiarListaParaGuardado() { }
    protected virtual void DespuesDeLimpiarListaParaGuardado() { }

    public void ConfigurarPrefijoGuardado(string nuevoPrefijo)
    {
        prefijoGuardado = nuevoPrefijo;
    }

    public void IniciarGeneracion()
    {
        if (rutinaGeneracion == null)
            rutinaGeneracion = StartCoroutine(GenerarDiscos());
    }

    public void DetenerGeneracion()
    {
        if (rutinaGeneracion == null) return;
        StopCoroutine(rutinaGeneracion);
        rutinaGeneracion = null;
    }

    protected void ActualizarDificultad()
    {
        float tiempo = ObtenerTiempoNivel();
        if (tiempo < 5f) return;
        if (tiempo < 55f)
        {
            tiempoMinimo = 2.0f; tiempoMaximo = 3.0f;
            probabilidadSegundoDisco = 0f; probabilidadTercerDisco = 0f;
        }
        else if (tiempo < 100f)
        {
            tiempoMinimo = 1.6f; tiempoMaximo = 2.3f;
            probabilidadSegundoDisco = 0.10f; probabilidadTercerDisco = 0f;
        }
        else if (tiempo < 145f)
        {
            tiempoMinimo = 1.2f; tiempoMaximo = 1.8f;
            probabilidadSegundoDisco = 0.25f; probabilidadTercerDisco = 0.03f;
        }
        else if (tiempo < 190f)
        {
            tiempoMinimo = 0.9f; tiempoMaximo = 1.5f;
            probabilidadSegundoDisco = 0.40f; probabilidadTercerDisco = 0.08f;
        }
        else
        {
            tiempoMinimo = 0.6f; tiempoMaximo = 1.1f;
            probabilidadSegundoDisco = 0.60f; probabilidadTercerDisco = 0.15f;
        }
    }

    private IEnumerator GenerarDiscos()
    {
        while (true)
        {
            ActualizarDificultad();
            yield return new WaitForSeconds(Random.Range(tiempoMinimo, tiempoMaximo));
            GenerarDisco();
            if (Random.value < probabilidadSegundoDisco) GenerarDisco();
            if (Random.value < probabilidadTercerDisco) GenerarDisco();
        }
    }

    private void GenerarDisco()
    {
        Vector2 posicion = (Vector2)transform.position
            + Random.insideUnitCircle * radioGeneracion;
        GameObject nuevoDisco = Instantiate(
            SeleccionarPrefabParaGenerar(), posicion, Quaternion.identity);
        Discos disco = nuevoDisco.GetComponent<Discos>();
        disco.InicializarDisco(true);
        AplicarEfectosActivos(disco);
        discosActivos.Add(nuevoDisco);
    }

    public virtual void GuardarDiscos()
    {
        AntesDeLimpiarListaParaGuardado();
        ActualizarListaDiscos();
        DespuesDeLimpiarListaParaGuardado();
        MarcarGuardadoDisponible();
        PlayerPrefs.SetInt(PrefijoGuardado + "CantidadDiscos", discosActivos.Count);
        for (int i = 0; i < discosActivos.Count; i++)
        {
            Discos disco = discosActivos[i].GetComponent<Discos>();
            PlayerPrefs.SetInt(PrefijoGuardado + "Tipo_" + i, (int)disco.ObtenerTipo());
            PlayerPrefs.SetFloat(PrefijoGuardado + "PosX_" + i, disco.ObtenerPosicion().x);
            PlayerPrefs.SetFloat(PrefijoGuardado + "PosY_" + i, disco.ObtenerPosicion().y);
            PlayerPrefs.SetFloat(PrefijoGuardado + "DirX_" + i, disco.ObtenerDireccion().x);
            PlayerPrefs.SetFloat(PrefijoGuardado + "DirY_" + i, disco.ObtenerDireccion().y);
            GuardarDatosEspecificos(disco, i);
        }
        PlayerPrefs.Save();
    }

    public virtual void RestaurarDiscos()
    {
        if (!PuedeRestaurarDiscos()) return;
        int cantidad = PlayerPrefs.GetInt(PrefijoGuardado + "CantidadDiscos", 0);
        for (int i = 0; i < cantidad; i++)
        {
            Discos.TipoDisco tipo = (Discos.TipoDisco)PlayerPrefs.GetInt(
                PrefijoGuardado + "Tipo_" + i);
            GameObject prefab = ObtenerPrefabPorTipo(tipo);
            Vector2 posicion = new Vector2(
                PlayerPrefs.GetFloat(PrefijoGuardado + "PosX_" + i),
                PlayerPrefs.GetFloat(PrefijoGuardado + "PosY_" + i));
            Vector2 direccion = new Vector2(
                PlayerPrefs.GetFloat(PrefijoGuardado + "DirX_" + i),
                PlayerPrefs.GetFloat(PrefijoGuardado + "DirY_" + i));
            GameObject nuevoDisco = Instantiate(prefab, posicion, Quaternion.identity);
            Discos disco = nuevoDisco.GetComponent<Discos>();
            disco.RestaurarDisco(posicion, direccion, false);
            RestaurarDatosEspecificos(disco, i);
            discosActivos.Add(nuevoDisco);
        }
    }

    public virtual void ReanudarDiscos()
    {
        foreach (GameObject objeto in discosActivos)
        {
            if (objeto == null) continue;
            Discos disco = objeto.GetComponent<Discos>();
            if (disco == null) continue;
            disco.ReanudarMovimiento();
            ReanudarDatosEspecificos(disco);
        }
    }

    public virtual void LimpiarDiscos()
    {
        foreach (GameObject disco in discosActivos)
            if (disco != null) Destroy(disco);
        discosActivos.Clear();
    }

    public virtual void ActualizarListaDiscos()
    {
        for (int i = discosActivos.Count - 1; i >= 0; i--)
            if (discosActivos[i] == null) discosActivos.RemoveAt(i);
    }

    public virtual void MostrarOverdriveVisual()
    {
        foreach (GameObject objeto in discosActivos)
        {
            if (objeto == null) continue;
            Discos disco = objeto.GetComponent<Discos>();
            if (disco != null) disco.MostrarOverdriveVisual();
        }
    }

    public virtual void OcultarOverdriveVisual()
    {
        foreach (GameObject objeto in discosActivos)
        {
            if (objeto == null) continue;
            Discos disco = objeto.GetComponent<Discos>();
            if (disco != null) disco.OcultarOverdriveVisual();
        }
    }

    protected override IEnumerable<GameObject> ObtenerDiscosAfectados()
    {
        foreach (GameObject disco in discosActivos) yield return disco;
    }
}