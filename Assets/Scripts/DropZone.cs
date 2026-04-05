using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    [Header("Identificação da Zona")]
    public int zoneID; // 0: Mão P1, 1: Suporte P1, 2: Ataque P1, 3: Mão P2, etc.
    private GameManager gameManager;

    [Header("Configuração da zona")]
    public bool isAttackZone = false;
    public bool isSupportZone = false;
    public int maxCards = 4;
    public enum DonoDaZona { Jogador, Oponente }
    public DonoDaZona dono = DonoDaZona.Jogador;

    private void Start()
    {
        gameManager = Object.FindFirstObjectByType<GameManager>();
    }

    private void ReturnToHand(GameObject card)
    {
        card.transform.SetParent(card.GetComponent<CardDragHandler>().originalParent);
    }

    // A VERSÃO REESCRITA E CORRIGIDA
    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        if (dropped == null) return;

        CardDisplay display = dropped.GetComponent<CardDisplay>();
        CardDragHandler dragHandler = dropped.GetComponent<CardDragHandler>();
        if (display == null || display.card == null || dragHandler == null) return;

        // Se não é meu turno, eu não posso fazer NADA.
        if (!gameManager.isMyTurn()) return;

        bool vindoDaMao = display.currentLocation == CardDisplay.CardLocation.InHand;
        Transform originalParent = dragHandler.originalParent;

        if (vindoDaMao)
        {
            // === ETAPA DE VALIDAÇÃO LOCAL ===

            // 1. Validação de Limite de Zona
            if (transform.childCount >= maxCards)
            {
                Debug.Log("Limite de cartas na zona.");
                ReturnToHand(dropped); // Usa a função para devolver
                return;
            }

            // 2. Validação de Alvo (se necessário)
            // ... (seu código de pré-check de alvo) ...

            // 3. Validação de Custo de PA
            int custo = display.card.cost;
            if (!gameManager.CanAfford(custo))
            {
                Debug.Log("PA insuficiente.");
                ReturnToHand(dropped); // Devolve a carta
                return;
            }

            // === ETAPA DE EXECUÇÃO ===
            // Se TODAS as validações locais passaram...

            // 1. ANUNCIA a jogada para a rede.
            gameManager.AnnounceCardPlay(display.card.cardName, this.zoneID, gameManager.isMasterClientTurn);
            gameManager.GetComponent<PhotonView>().RPC("RPC_RemoveCardFromOpponentHand", RpcTarget.All);
            // 2. CONSOME a carta da mão (destrói o GameObject que veio da mão).
            Destroy(dropped);
        }
        else // Lembre-se de adicionar um RPC para sincronizar o movimento também no futuro.
        {
            // Regra 1: Não pode mover no mesmo turno que entrou
            if (display.turnoQueEntrou == gameManager.turnoAtual)
            {
                Debug.Log($"'{display.card.cardName}' não pode ser movido no turno em que foi invocado.");
                // Não faz ReturnToHand, pois isso mandaria a carta para a mão do jogador.
                // Simplesmente não faz nada, a carta voltará para o lugar original por padrão.
                return;
            }

            // Regra 2: Não pode mover mais de uma vez
            if (display.jaMoveuNesseTurno)
            {
                Debug.Log($"'{display.card.cardName}' já foi movido neste turno.");
                return;
            }

            // Se passou, move a carta
            display.jaMoveuNesseTurno = true;
            dropped.transform.SetParent(transform);
        }

                // ATUALIZA��O FINAL DE LAYOUT
        originalParent?.GetComponent<PerspectiveZoneLayout>()?.UpdateLayout();
        GetComponent<PerspectiveZoneLayout>()?.UpdateLayout();
    }
}
