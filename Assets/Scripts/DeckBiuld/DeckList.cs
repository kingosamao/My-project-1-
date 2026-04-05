using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New DeckList", menuName = "Deck List")]
public class DeckList : ScriptableObject
{
    public List<Card> cards;
    public ActionCard actionCard; // Opcional: para o deck padrão ter uma Action Card
}