//TargetOponente.cs
using UnityEngine;
using UnityEngine.EventSystems;
using static GameManager;

public class TargetOponente : MonoBehaviour, IPointerClickHandler
{
    public GameManager gameManager;
    public Transform zonaDeAtaqueOponente;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (gameManager.currentPhase != TurnPhase.Batalha) return;
        if (gameManager.attacker == null) return;

        // Só pode atacar se não houver defensores
        if (zonaDeAtaqueOponente.childCount == 0)
        {
            // Apenas PEDE ao GameManager para processar o ataque.
            // Enviamos o ID da nossa carta atacante.
            gameManager.RequestDirectAttack(gameManager.attacker.matchID);
        }
    }
}
