using System;
[Serializable]
public class ScoreEntry
{
    public string playerName;
    public int score;
    public int level;
    public string date;
    public ScoreEntry(string name, int score, int level)
    {
        this.playerName = name;
        this.score = score;
        this.level = level;
        this.date = DateTime.Now.ToString("dd/MM/yyyy");
    }
}
