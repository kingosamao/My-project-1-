using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Card))]
public class CardEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Pega o objeto que estamos editando
        Card card = (Card)target;

        // Desenha os campos que são comuns a todas as cartas
        EditorGUILayout.PropertyField(serializedObject.FindProperty("cardName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("type"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("description"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("cardArt"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("cost")); 
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Habilidade Ativável", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ability"));


        // A MÁGICA: Só desenha os campos de Animal se o tipo for Animal
        if (card.type == CardType.Animal)
        {
            EditorGUILayout.Space(); // Adiciona um espaço
            EditorGUILayout.LabelField("Atributos de Animal", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("attack"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("health"));
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Onde Ativa a habilidade", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("activateInHand"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("activateInField"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Classificação Biológica", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("classe"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ordem"));
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Habilidades Passivas (Keywords)", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("hasTaunt"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("battlecryAbility"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("deathrattleAbility"));
        }
        serializedObject.ApplyModifiedProperties();
    }
}