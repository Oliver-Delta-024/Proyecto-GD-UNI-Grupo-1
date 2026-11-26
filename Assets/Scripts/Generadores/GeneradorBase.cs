using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Common contract and event-effect handling for generator components.
/// Coordinators may override these methods to delegate effects to child generators.
/// </summary>
public abstract class GeneradorBase : MonoBehaviour
{
    private bool overdriveActivo;
    private float multiplicadorOverdrive = 1f;
    private bool ralentizacionActiva;
    private float multiplicadorRalentizacion = 1f;

    protected virtual IEnumerable<GameObject> ObtenerDiscosAfectados()
    {
        yield break;
    }

    public virtual void ActivarOverdrive(float multiplicador)
    {
        overdriveActivo = true;
        multiplicadorOverdrive = multiplicador;
        foreach (GameObject objeto in ObtenerDiscosAfectados())
        {
            if (objeto == null) continue;
            Discos disco = objeto.GetComponent<Discos>();
            if (disco != null) disco.ActivarOverdrive(multiplicador);
        }
    }

    public virtual void DesactivarOverdrive()
    {
        overdriveActivo = false;
        multiplicadorOverdrive = 1f;
        foreach (GameObject objeto in ObtenerDiscosAfectados())
        {
            if (objeto == null) continue;
            Discos disco = objeto.GetComponent<Discos>();
            if (disco != null) disco.DesactivarOverdrive();
        }
    }

    public virtual void ActivarRalentizacion(float multiplicador)
    {
        ralentizacionActiva = true;
        multiplicadorRalentizacion = multiplicador;
        foreach (GameObject objeto in ObtenerDiscosAfectados())
        {
            if (objeto == null) continue;
            Discos disco = objeto.GetComponent<Discos>();
            if (disco != null) disco.ActivarRalentizacion(multiplicador);
        }
    }

    public virtual void DesactivarRalentizacion()
    {
        ralentizacionActiva = false;
        multiplicadorRalentizacion = 1f;
        foreach (GameObject objeto in ObtenerDiscosAfectados())
        {
            if (objeto == null) continue;
            Discos disco = objeto.GetComponent<Discos>();
            if (disco != null) disco.DesactivarRalentizacion();
        }
    }

    protected void AplicarEfectosActivos(Discos disco)
    {
        if (disco == null) return;
        if (overdriveActivo) disco.ActivarOverdrive(multiplicadorOverdrive);
        if (ralentizacionActiva) disco.ActivarRalentizacion(multiplicadorRalentizacion);
    }
}