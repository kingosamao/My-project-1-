using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static GameManager;
// This script displays the card information in the UI.



public class CardDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI Elements")]
    public Image backgroundArtImage;
    public GameObject cardFrontElements; // Um objeto pai que contém TODOS os textos e imagens da frente
    public Image cardBackImage; // A referência para a imagem do VERSO
    public Card card;
    public enum CardLocation { InDeck, InHand, InField, InGraveyard }
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI statsText;
    public TextMeshProUGUI cost;
    public TextMeshProUGUI classificationText;
    // --- MUDANÇA 1: Referência ao GameObject dos Stats ---
    [HideInInspector] public int turnoQueEntrou = -1;
    [HideInInspector] public bool jaAtacouNesseTurno = false;
    [HideInInspector] public bool jaMoveuNesseTurno = false;
    [Header("Debug State")]
    public CardLocation currentLocation;
    public int matchID = -1;


    public void Flip(bool faceUp, Sprite cardBackSprite = null)
    {
        if (cardFrontElements == null || cardBackImage == null)
        {
            Debug.LogError($"ERRO no prefab da carta '{this.name}': As referências 'CardFrontElements' ou 'CardBackImage' não foram atribuídas no Inspector!");
            return;
        }

        cardFrontElements.SetActive(faceUp);
        cardBackImage.gameObject.SetActive(!faceUp);

        if (!faceUp && cardBackSprite != null)
        {
            cardBackImage.sprite = cardBackSprite;
        }
    }
    public void ShowCard()
    {
        if (card == null) return;
        Flip(true);
        // Arte de fundo
        if (backgroundArtImage != null && card.cardArt != null)
        {
            backgroundArtImage.sprite = card.cardArt;
        }

        // Textos básicos
        if (nameText != null) nameText.text = card.cardName;
        if (descriptionText != null) descriptionText.text = card.description;
        if (cost != null) cost.text = $"{card.cost}";

        // Lógica para campos de Animal
        if (card.type == CardType.Animal)
        {
            if (statsText != null)
            {
                statsText.gameObject.SetActive(true);
                statsText.text = $"ATK: {card.attack} / HP: {card.health}";
            }
            if (classificationText != null)
            {
                classificationText.gameObject.SetActive(true);
                classificationText.text = $"Classe:{card.classe}" +
                    $" Ordem: {card.ordem}";
            }
        }
        else
        {
            // Esconde os campos se não for um Animal
            if (statsText != null) statsText.gameObject.SetActive(false);
            if (classificationText != null) classificationText.gameObject.SetActive(false);

        }

        void Update()
        {
            // Opcional: pode abrir só se passar o mouse
            if (Input.GetKeyDown(KeyCode.Space)) // usar botão ou evento
            {
                CartaZoomUI.instancia.MostrarCarta(card);
            }
        }
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        var gm = Object.FindFirstObjectByType<GameManager>();
        if (gm == null) return;

        if (gm.isTargetingModeActive)
        {
            gm.SelectTarget(this);
            return;
        }

        if (gm.currentPhase == TurnPhase.Principal && gm.isMyTurn())
        {
            if (card.HasAbility() && card.type == CardType.Animal)
            {
                bool isZoneValid = false;

                // LÓGICA DE VERIFICAÇÃO FINAL E ROBUSTA
                // Usamos a variável de estado que a própria carta carrega.
                if (currentLocation == CardLocation.InHand)
                {
                    if (card.activateInHand) isZoneValid = true;
                }
                else if (currentLocation == CardLocation.InField)
                {
                    var dropZone = GetComponentInParent<DropZone>();
                    if (dropZone != null) // Apenas uma checagem de segurança
                    {
                        if (dropZone.isSupportZone || dropZone.isAttackZone && card.activateInField) isZoneValid = true;

                    }
                    if (currentLocation == CardLocation.InField)
                    {
                        if (dropZone != null) // Apenas uma checagem de segurança
                        {
                            if (dropZone.isAttackZone || dropZone.isSupportZone && card.activateInField) isZoneValid = true;

                        }
                    }
                }

                if (isZoneValid)
                {
                    Debug.Log($"CONDIÇÃO VÁLIDA: A carta sabe que está em uma zona válida ('{currentLocation}'). Chamando MostrarPainel.");
                    CartaContextualUI.instancia.MostrarPainel(this);
                }
                else
                {
                    Debug.Log($"CONDIÇÃO INVÁLIDA: A carta está em '{currentLocation}', mas a habilidade não pode ser ativada desta zona.");
                }
            }
        }

        // Lógica de Batalha (Inalterada)
        if (gm.currentPhase == TurnPhase.Batalha)
        {
            var zona = GetComponentInParent<DropZone>(); // Aqui GetComponentInParent é seguro
            if (gm.attacker == null)
            {
                if (zona != null && zona.isAttackZone && zona.dono == DropZone.DonoDaZona.Jogador && !jaAtacouNesseTurno)
                {
                    gm.SelectAttacker(this);
                }
            }
            else
            {
                if (zona != null && zona.dono == DropZone.DonoDaZona.Oponente && card.type == CardType.Animal)
                {
                    gm.SelectDefender(this);
                }
            }
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CartaZoomUI.instancia != null)
            CartaZoomUI.instancia.MostrarCarta(card);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (CartaZoomUI.instancia != null)
            CartaZoomUI.instancia.Esconder();
    }
}
