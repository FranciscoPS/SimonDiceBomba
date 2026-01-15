using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }
    private const string LEADERBOARD_KEY = "SimonDiceBomba_Leaderboard";
    private const int MAX_SCORES = 20;
    private List<ScoreEntry> scores = new List<ScoreEntry>();
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadScores();
    }
    public void AddScore(string playerName, int score, int level)
    {
        ScoreEntry newEntry = new ScoreEntry(playerName, score, level);
        scores.Add(newEntry);
        scores = scores.OrderByDescending(s => s.score).ToList();
        if (scores.Count > MAX_SCORES)
        {
            scores = scores.Take(MAX_SCORES).ToList();
        }
        SaveScores();
    }
    public List<ScoreEntry> GetTopScores()
    {
        return new List<ScoreEntry>(scores);
    }
    public bool IsHighScore(int score)
    {
        if (scores.Count < MAX_SCORES) return true;
        return score > scores[scores.Count - 1].score;
    }
    private void SaveScores()
    {
        string json = JsonUtility.ToJson(new ScoreListWrapper { scores = scores }, true);
        PlayerPrefs.SetString(LEADERBOARD_KEY, json);
        PlayerPrefs.Save();
    }
    private void LoadScores()
    {
        if (PlayerPrefs.HasKey(LEADERBOARD_KEY))
        {
            string json = PlayerPrefs.GetString(LEADERBOARD_KEY);
            ScoreListWrapper wrapper = JsonUtility.FromJson<ScoreListWrapper>(json);
            scores = wrapper.scores ?? new List<ScoreEntry>();
        }
        else
        {
            scores = new List<ScoreEntry>();
        }
    }
    [System.Serializable]
    private class ScoreListWrapper
    {
        public List<ScoreEntry> scores;
    }
}
