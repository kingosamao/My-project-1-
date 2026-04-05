using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Essencial para a função Find

[CreateAssetMenu(fileName = "CardDatabase", menuName = "Card Database")]
public class CardDatabase : ScriptableObject
{
    // Listas com TODAS as cartas possíveis no seu jogo.
    public List<Card> allNormalCards;
    public List<ActionCard> allActionCards;

    // Função pública para encontrar uma carta normal pelo nome.
    public Card FindNormalCardByName(string cardName)
    {
        if (string.IsNullOrEmpty(cardName) || allNormalCards == null)
            return null;
        string cleanedCardName = cardName.Trim();
        return allNormalCards.FirstOrDefault(c => c.cardName.Trim() == cleanedCardName);
    }

    // Função pública para encontrar uma carta de ação pelo nome.
    public ActionCard FindActionCardByName(string cardName)
    {
        if (string.IsNullOrEmpty(cardName) || allActionCards == null)
            return null;

        string cleanedCardName = cardName.Trim();
        return allActionCards.FirstOrDefault(ac => ac.cardName.Trim() == cleanedCardName);
    }
}