// DrawCardsAction.cs
using UnityEngine;
using Photon.Pun;

[CreateAssetMenu(fileName = "New Draw Cards Action", menuName = "Card Actions/Draw Cards")]
public class DrawCardsAction : CardAction
{
    public int amountToDraw;

    public override void ExecuteAction(GameManager gm, CardDisplay sourceCard, DropZone.DonoDaZona owner)
    {
        // A única informação que importa é o 'owner' que recebemos.
        // 'Jogador' sempre significa "meu lado", 'Oponente' sempre significa "lado do oponente".
        bool isForMyDeck = (owner == DropZone.DonoDaZona.Jogador);

        gm.deckManager.DrawCards(amountToDraw, isForMyDeck);
    }
}