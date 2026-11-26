using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(CatalogoCarasSombreros))]
public class CatalogoCarasSombrerosEditor : Editor
{
    private SerializedProperty spritesCaras;
    private SerializedProperty spritesSombreros;

    private void OnEnable()
    {
        spritesCaras = serializedObject.FindProperty("spritesCaras");
        spritesSombreros = serializedObject.FindProperty("spritesSombreros");
    }

    public override void OnInspectorGUI()
    {
        CatalogoCarasSombreros catalogo = (CatalogoCarasSombreros)target;
        if (!Application.isPlaying)
            catalogo.AsegurarMenusEnEscena();

        serializedObject.Update();
        EditorGUI.BeginChangeCheck();

        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.IntField("Cuadros de caras", CatalogoCarasSombreros.CantidadCaras);
            EditorGUILayout.IntField("Cuadros de sombreros", CatalogoCarasSombreros.CantidadSombreros);
        }

        EditorGUILayout.Space(6f);
        DibujarSprites(spritesCaras, "Sprites de caras");
        EditorGUILayout.Space(4f);
        DibujarSprites(spritesSombreros, "Sprites de sombreros");

        bool cambiaronSprites = EditorGUI.EndChangeCheck();
        serializedObject.ApplyModifiedProperties();

        if (cambiaronSprites && !Application.isPlaying)
        {
            catalogo.ActualizarSpritesEnEscena();
            EditorSceneManager.MarkSceneDirty(catalogo.gameObject.scene);
        }
    }

    private static void DibujarSprites(SerializedProperty propiedad, string titulo)
    {
        EditorGUILayout.LabelField(titulo, EditorStyles.boldLabel);
        for (int i = 0; i < propiedad.arraySize; i++)
        {
            EditorGUILayout.PropertyField(
                propiedad.GetArrayElementAtIndex(i),
                new GUIContent("Sprite " + (i + 1) + " (cuadro " + (i + 2) + ")"));
        }
    }
}
