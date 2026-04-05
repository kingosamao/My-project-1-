//CartaContextualUI.cs
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CartaContextualUI : MonoBehaviour
{
    public static CartaContextualUI instancia;

    public GameObject painel;
    public Button botaoAtivar;
    private CardDisplay cartaAtual;
    private ActionCardDisplay cartaAcaoAtual;
    private GameManager gm;
    private Transform paiOriginal; // Para guardar onde o painel estava originalmente

    void Awake()
    {
        instancia = this;
        gm = FindObjectOfType<GameManager>();

        if (painel != null)
        {
            paiOriginal = painel.transform.parent; // Guarda o pai original (o Canvas)
            painel.SetActive(false); // Esconde o painel no início
        }

        botaoAtivar.onClick.AddListener(AtivarEfeito);
    }

    public void MostrarPainel(CardDisplay carta)
    {
        // Limpa a referência antiga para evitar confusão
        cartaAcaoAtual = null;

        cartaAtual = carta;
        painel.transform.SetParent(carta.transform);
        painel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 130);
        painel.transform.SetAsLastSibling();
        painel.SetActive(true);
    }

    // NOVA FUNÇÃO para as Cartas de Ação
    public void MostrarPainelAcao(ActionCardDisplay cartaDeAcao)
    {
        // Limpa a referência antiga para evitar confusão
        cartaAtual = null;

        cartaAcaoAtual = cartaDeAcao;
        painel.transform.SetParent(cartaDeAcao.transform);
        painel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 130);
        painel.transform.SetAsLastSibling();
        painel.SetActive(true);
    }

    public void Esconder()
    {
        if (painel != null)
        {
            painel.SetActive(false);
            // Devolve o painel para seu lugar original na hierarquia (o Canvas)
            // Isso é importante para que ele não seja destruído se a carta for destruída.
            painel.transform.SetParent(paiOriginal);
        }
    }

    void AtivarEfeito()
    {
        Debug.Log("Botão 'Ativar' pressionado. Iniciando verificação de efeito.");

        // Variáveis que vamos preencher
        Ability abilityToActivate = cartaAtual.card.ability;
        DropZone.DonoDaZona owner = DropZone.DonoDaZona.Jogador; // Um valor padrão
        CardDisplay sourceCardDisplay = null;
        bool isValidSource = false;

        // --- PARTE 1: IDENTIFICAR O ATIVADOR ---
        if (cartaAcaoAtual != null)
        {
            ActionCard cardData = cartaAcaoAtual.GetActionCard();
            if (cardData != null && cardData.HasAbility())
            {
                Debug.Log($"Ativador identificado: ActionCardDisplay '{cardData.name}'.");
                abilityToActivate = cardData.ability;
                owner = cartaAcaoAtual.owner;
                isValidSource = true;
            }
        }
        else if (cartaAtual != null)
        {
            if (cartaAtual.card.HasAbility())
            {
                Debug.Log($"Ativador identificado: CardDisplay '{cartaAtual.card.name}'.");
                abilityToActivate = cartaAtual.card.ability;
                var sourceZone = cartaAtual.GetComponentInParent<DropZone>();
                owner = (sourceZone != null) ? sourceZone.dono : DropZone.DonoDaZona.Jogador;
                sourceCardDisplay = cartaAtual;
                isValidSource = true;
            }
        }

        // --- PARTE 2: VERIFICAÇÕES ---
        if (!isValidSource || abilityToActivate == null)
        {
            Debug.LogError("Falha na ativação: Fonte inválida ou sem habilidade. Escondendo painel.");
            Esconder();
            return;
        }

        Debug.Log($"Habilidade '{abilityToActivate.name}' encontrada. Verificando alvos...");
        foreach (var action in abilityToActivate.actions)
        {
            if (action.requiresTarget && !gm.CheckForValidTargets(action, owner))
            {
                Debug.LogWarning($"Ação '{action.name}' não tem alvos válidos. Cancelando.");
                Esconder();
                return;
            }
        }

        Debug.Log("Verificação de alvos concluída com sucesso. Verificando PA...");
        if (gm.SpendActionPoints(abilityToActivate.cost, owner == DropZone.DonoDaZona.Jogador))
        {
            bool needsTarget = abilityToActivate.actions.Any(a => a.requiresTarget);

            if (needsTarget)
            {
                // Se precisa de alvo, entra em modo de mira. A sincronização acontecerá DEPOIS.
                gm.EnterTargetingMode(abilityToActivate.actions[0], cartaAtual, owner);
            }
            else
            {
                // Se NÃO precisa de alvo, anuncia o evento para a rede IMEDIATAMENTE.
                // Passamos -1 para indicar que não há alvo.
                gm.AnnounceAbilityActivation(cartaAtual.matchID, -1);
            }
        }

        Esconder();
    }
}