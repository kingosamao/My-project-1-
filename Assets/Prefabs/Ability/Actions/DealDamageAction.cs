using UnityEngine;

[CreateAssetMenu(fileName = "New DealDamageAction", menuName = "CardActions/Deal Damage")]
public class DealDamageAction : CardAction
{
    [Header("Configuração de Dano")]
    public int damageAmount;

    
    public override void ExecuteAction(GameManager gm, CardDisplay sourceCard, DropZone.DonoDaZona owner)
    {
        Debug.LogError("DealDamageAction requer um alvo, mas foi chamada sem um!");
    }

    public override void ExecuteAction(GameManager gm, CardDisplay sourceCard, DropZone.DonoDaZona owner, CardDisplay target)
    {
        Debug.Log($"Causando {damageAmount} de dano em {target.card.cardName}.");

        // Aplica o dano
        target.card.health -= damageAmount;

        // Feedback visual
        target.GetComponent<CardVisualFeedback>()?.Flash(Color.red);

        // Atualiza a UI da carta alvo
        target.ShowCard();

        // Verifica se a criatura alvo morreu
        if (target.card.health <= 0)
        {
            Debug.Log($"{target.card.cardName} foi destruído pelo dano da habilidade!");
            var targetZone = target.GetComponentInParent<DropZone>();
            if (targetZone != null)
            {
                bool wasOwnedByMaster = (targetZone.dono == DropZone.DonoDaZona.Jogador);
                gm.SendToGraveyard(target, wasOwnedByMaster);
            }
        }
    }
}