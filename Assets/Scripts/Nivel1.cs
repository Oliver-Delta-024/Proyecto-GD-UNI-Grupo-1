using UnityEngine;

public class Nivel1 : NivelBase
{
    protected override int NivelActual => 1;
    protected override string NombreNivel => "Nivel 1";
    protected override string PrefijoGuardadoDiscos => "Nivel1_";
}