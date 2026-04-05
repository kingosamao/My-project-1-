//ConnectionManager.cs
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using TMPro; // Para o TextMeshPro
using UnityEngine;
using UnityEngine.UI;

// Usamos a interface IMatchmakingCallbacks para receber eventos do Photon
public class ConnectionManager : MonoBehaviourPunCallbacks
{
    [Header("UI de Conexão")]
    public TMP_InputField playerNameInput;
    public TextMeshProUGUI statusText;
    

    void Start()
    {
        // Define um nome padrão se nada for salvo
        string defaultName = "Jogador" + Random.Range(1000, 9999);
        playerNameInput.text = PlayerPrefs.GetString("PlayerName", defaultName);
        statusText.text = "Desconectado";
    }

    public void ConnectToServer()
    {
        if (string.IsNullOrEmpty(playerNameInput.text))
        {
            statusText.text = "Por favor, insira um nome.";
            return;
        }

        // Salva o nome do jogador para a próxima vez
        PlayerPrefs.SetString("PlayerName", playerNameInput.text);
        PhotonNetwork.NickName = playerNameInput.text;

        // --- AQUI ESTÁ A CORREÇÃO CRUCIAL ---
        // Ativamos a sincronização de cena automática para esta sessão.
        PhotonNetwork.AutomaticallySyncScene = true;

        statusText.text = "Conectando...";
        PhotonNetwork.ConnectUsingSettings();
    }

    // --- Callbacks do Photon (funções chamadas automaticamente pelo Photon) ---

    public override void OnConnectedToMaster()
    {
        statusText.text = "Conectado! Procurando por uma sala...";
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        statusText.text = "Nenhuma sala encontrada. Criando uma nova...";
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 2 });
    }

    public override void OnJoinedRoom()
    {
        statusText.text = $"Entrou na sala! Jogadores: {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}";
        Debug.Log($"Entrou na sala '{PhotonNetwork.CurrentRoom.Name}'. Nickname: {PhotonNetwork.NickName}");
        
        ActionCard chosenActionCard = GameData.instance?.selectedActionCard;
        List<Card> chosenDeck = GameData.instance?.selectedDeck;
       
        if (chosenActionCard == null)
        {
            // Adicione referências ao DeckManager e ao CardDatabase se precisar de fallback
            Debug.LogWarning("Nenhuma ActionCard encontrada no GameData. Usando fallback (se configurado).");
        }

        // Converte o deck em uma lista de nomes e cria as propriedades.
        Hashtable playerProps = new Hashtable();
        if (chosenActionCard != null)
        {
            playerProps.Add("ActionCardName", chosenActionCard.cardName);
        }
        if (chosenDeck != null && chosenDeck.Count > 0)
        {
            playerProps.Add("DeckCardNames", chosenDeck.Select(c => c.cardName).ToArray());
        }
        else
        {
            playerProps.Add("DeckCardNames", new string[0]);
        }
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);

        // --- ETAPA DE CARREGAMENTO DE CENA ---
        // Apenas o Master Client tem a autoridade para carregar a cena,
        // e ele só faz isso quando a sala estiver cheia.
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2 && PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Sala cheia! Carregando a cena de jogo para todos...");
            PhotonNetwork.LoadLevel("Game");
        }

    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        statusText.text = $"Um oponente entrou! Jogadores: {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}";
        Debug.Log($"Jogador '{newPlayer.NickName}' entrou na sala.");

        // Se a sala ficou cheia, o jogador mestre (o primeiro que entrou) carrega a cena.
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2 && PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Sala cheia! Master Client está carregando a cena 'Game' para todos.");

            // ...eu carrego a cena do jogo. Como AutomaticallySyncScene é true,
            // o Jogador 2 também carregará a cena.
            PhotonNetwork.LoadLevel("Game");
        }
    }
}