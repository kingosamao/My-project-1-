using UnityEngine;
using static CardDisplay;

[CreateAssetMenu(fileName = "BuffByClassAction", menuName = "CardActions/Buff By Class")]
public class BuffByClassAction : CardAction
{
    public Classe classToBuff; // Você seleciona "Mammalia" no Inspector
    public int attackBonus = 10;

    public override void ExecuteAction(GameManager gm, CardDisplay sourceCard, DropZone.DonoDaZona owner)
    {
        Debug.Log($"Ativando efeito para a classe: {classToBuff}");

        // Encontra todas as cartas em campo
        var allCardsOnField = FindObjectsOfType<CardDisplay>();

        foreach (var cardDisplay in allCardsOnField)
        {
            // Pula cartas que estão na mão ou no deck
            if (cardDisplay.currentLocation != CardLocation.InField) continue;

            // Pega o dono da carta atual
            var targetZone = cardDisplay.GetComponentInParent<DropZone>();
            if (targetZone.dono == owner) // É uma carta aliada?
            {
                // A VERIFICAÇÃO PRINCIPAL:
                // A classe da carta é a mesma que queremos buffar?
                if (cardDisplay.card.classe == classToBuff)
                {
                    Debug.Log($"Buffando {cardDisplay.card.cardName}!");
                    cardDisplay.card.attack += attackBonus;
                    cardDisplay.ShowCard(); // Atualiza a UI da carta buffada
                }
            }
        }
    }
}