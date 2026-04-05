//card Action.cs
using UnityEngine;
using Photon.Pun;
public enum ValidTargetType
{
    Qualquer,
    CriaturaNoCampo,
    CriaturaInimigaNoCampo,
    CriaturaAliadaNoCampo
}
public abstract class CardAction : ScriptableObject
{
    [Header("Configuração de Alvo")]
    public bool requiresTarget = false;
    public ValidTargetType validTargets = ValidTargetType.Qualquer;

    // Método para ações SEM alvo
    public virtual void ExecuteAction(GameManager gm, CardDisplay sourceCard, DropZone.DonoDaZona owner) { }

    // NOVO método para ações COM alvo
    public virtual void ExecuteAction(GameManager gm, CardDisplay sourceCard, DropZone.DonoDaZona owner, CardDisplay target) { }
    public bool IsValidTarget(CardDisplay target, DropZone.DonoDaZona actionOwner)
    {
        // Pega informações cruciais sobre o alvo
        var targetZone = target.GetComponentInParent<DropZone>();
        bool isOnField = targetZone != null;
        bool isCreature = target.card.type == CardType.Animal;

        switch (validTargets)
        {
            case ValidTargetType.Qualquer:
                return true; // Se for "Qualquer", sempre é válido.

            case ValidTargetType.CriaturaNoCampo:
                return isOnField && isCreature; // Precisa estar no campo E ser uma criatura.

            case ValidTargetType.CriaturaInimigaNoCampo:
                // Precisa estar no campo, ser criatura E o dono da zona do alvo
                // deve ser DIFERENTE do dono da ação.
                return isOnField && isCreature && targetZone.dono != actionOwner;

            case ValidTargetType.CriaturaAliadaNoCampo:
                // Precisa estar no campo, ser criatura E o dono da zona do alvo
                // deve ser IGUAL ao dono da ação.
                return isOnField && isCreature && targetZone.dono == actionOwner;

            default:
                return false;
        }
    }
}