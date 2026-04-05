using UnityEngine;


[CreateAssetMenu(fileName = "BuffTargetClassAction", menuName = "CardActions/Buff Target")]
public class BuffTargetClassAction : CardAction
{
    public Classe classToBuff;
    public int attackBonus = 10;

    // A versão sem alvo agora fica vazia ou com um erro.
    public override void ExecuteAction(GameManager gm, CardDisplay sourceCard, DropZone.DonoDaZona owner)
    {
        Debug.LogError("Esta ação requer um alvo, mas foi chamada sem um!");
    }

    // --- TODA A LÓGICA VEM PARA CÁ ---
    public override void ExecuteAction(GameManager gm, CardDisplay sourceCard, DropZone.DonoDaZona owner, CardDisplay target)
    {
        // A verificação principal agora é feita no alvo selecionado.
        if (target.card.classe == classToBuff)
        {
            Debug.Log($"Buffando o alvo selecionado: {target.card.cardName}!");
            target.card.attack += attackBonus;
            target.ShowCard(); // Atualiza a UI da carta buffada
        }
        else
        {
            Debug.LogWarning($"Alvo inválido! '{target.card.cardName}' não é da classe '{classToBuff}'.");
        }
    }
}