// Ability.cs
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[CreateAssetMenu(fileName = "New Ability", menuName = "Ability")]
public class Ability : ScriptableObject
{
    public int cost; // Custo para usar esta habilidade
    public List<CardAction> actions; // A lista de "peças de LEGO"

    // O método que vai executar todas as ações em sequência
    public void Activate(GameManager gm, CardDisplay sourceCard, DropZone.DonoDaZona owner)
    {
        foreach (CardAction action in actions)
        {
            if (action != null)
            {
                if (!action.requiresTarget)
                {
                    action.ExecuteAction(gm, sourceCard, owner);
                }
            }
        }
    }
}