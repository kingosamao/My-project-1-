using UnityEngine;

[CreateAssetMenu(fileName = "New Action Card", menuName = "Action Card")]
public class ActionCard : ScriptableObject
{
    [Header("Visual da Carta")]
    public Sprite cardArt;
    [Header("Informações Básicas")]
    public string cardName;
    [TextArea] public string description;

    [Header("Atributos de Jogo")]
    public int startingLife;
    public int paPerTurn;

    [Header("Habilidade Ativável")]
    public Ability ability;

    public bool HasAbility()
    {
        return ability != null;
    }
}