//DeckData.cs

using System.Collections.Generic;

[System.Serializable] // Essencial para que a Unity possa converter para JSON
public class DeckData
{
    public string deckName;
    public string actionCardName; // O nome da Action Card escolhida
    public List<string> mainDeckCardNames; // Uma lista com os nomes das cartas
}