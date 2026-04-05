//Cards.cs
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum Classe { Nenhuma, Mammalia, Reptilia, Insecta, Aves, Actinopterygii }
public enum Ordem { Nenhuma, Carnivora, Primates, Rodentia , Perissodactyla, Squamata, Pilosa, Characiformes, Hymenoptera }
public enum CardType { Animal, Estratégia, none }
[CreateAssetMenu(fileName = "New Card", menuName = "Card")]



public class Card : ScriptableObject
{

    public string cardName;
    public CardType type;

    [TextArea] public string description;

    [Header("Custo de Invocação / Uso")]
    public int cost;

    [Header("Somente para cartas de ANIMAL")]
    public int attack;
    public int health;
    [Header("Classificação Biológica (Apenas para Animais)")]
    public Classe classe;
    public Ordem ordem;
    [Header("Visual da Carta")]
    public Sprite cardArt;

    [Header("Habilidade Ativável")]
    public Ability ability;
    public bool activateInHand;
    public bool activateInField;
    [Header("Habilidades Passivas (Keywords)")]
    [Tooltip("Inimigos devem atacar esta criatura primeiro.")]
    public bool hasTaunt; // Provocar
    [Tooltip("Habilidade que ativa ao jogar da mão.")]
    public Ability battlecryAbility; // Grito de Guerra

    [Tooltip("Habilidade que ativa ao ser destruída.")]
    public Ability deathrattleAbility;

    public bool HasAbility()
    {
        return ability != null;
    }
}
