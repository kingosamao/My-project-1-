using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static GameManager;

public class ActionCardDisplay : MonoBehaviour, IPointerClickHandler
{
    // A variável da carta agora pode ser privada, pois só será preenchida por Setup()
    private ActionCard actionCard;

    [Header("Referências da UI")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI statsText;
    public TextMeshProUGUI descriptionText;
    public Card card;

    // A informação do dono é crucial e será definida pelo GameManager
    [HideInInspector] public DropZone.DonoDaZona owner;

    // Função de Setup PÚBLICA: O GameManager usará isso para configurar o display.
    public void Setup(ActionCard cardData, DropZone.DonoDaZona cardOwner)
    {
        this.actionCard = cardData;
        this.owner = cardOwner;
        ShowCard();
    }

    public void ShowCard()
    {
        if (actionCard == null) return;
        if (nameText != null) nameText.text = actionCard.cardName;
        if (statsText != null) statsText.text = $"VIDA: {actionCard.startingLife}\nPA/Turno: {actionCard.paPerTurn}";
        if (descriptionText != null) descriptionText.text = actionCard.description;
    }

    public ActionCard GetActionCard()
    {
        return this.actionCard;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (actionCard == null || !actionCard.HasAbility()) return;

        var gm = Object.FindFirstObjectByType<GameManager>();
        if (gm == null) return;

        if (gm.currentPhase == TurnPhase.Principal && owner == DropZone.DonoDaZona.Jogador && gm.isMyTurn())
        {
            CartaContextualUI.instancia.MostrarPainelAcao(this);
        }
    }
}