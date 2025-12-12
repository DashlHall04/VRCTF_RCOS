using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Mono.Data.Sqlite;

public class ActionLoggerDB : MonoBehaviour
{
    public static ActionLoggerDB Instance { get; private set; }

    [Header("DB Settings")]
    public string dbFileName = "user_actions.sqlite";

    private string _connString;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        string dbPath = Path.Combine(Application.persistentDataPath, dbFileName);
        _connString = "URI=file:" + dbPath;

        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using (var conn = new SqliteConnection(_connString))
        {
            conn.Open();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText =
                    @"CREATE TABLE IF NOT EXISTS action_logs (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        timestamp_utc TEXT NOT NULL,
                        session_id TEXT NOT NULL,
                        user_id TEXT,
                        action TEXT NOT NULL,
                        target TEXT,
                        scene TEXT,
                        color_index INTEGER,
                        meta_json TEXT
                    );";
                cmd.ExecuteNonQuery();
            }
        }
    }

    public void LogAction(
        string action,
        string target = null,
        string userId = null,
        int? colorIndex = null,
        Dictionary<string, string> meta = null)
    {
        string ts = DateTime.UtcNow.ToString("o");
        string sid = GetOrCreateSessionId();
        string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        string metaJson = ToFlatJson(meta);

        using (var conn = new SqliteConnection(_connString))
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText =
                    @"INSERT INTO action_logs
                      (timestamp_utc, session_id, user_id, action, target, scene, color_index, meta_json)
                      VALUES
                      (@ts, @sid, @uid, @action, @target, @scene, @cidx, @meta);";

                cmd.Parameters.AddWithValue("@ts", ts);
                cmd.Parameters.AddWithValue("@sid", sid);
                cmd.Parameters.AddWithValue("@uid", (object)userId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@action", action);
                cmd.Parameters.AddWithValue("@target", (object)target ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@scene", scene);
                cmd.Parameters.AddWithValue("@cidx", colorIndex.HasValue ? (object)colorIndex.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@meta", (object)metaJson ?? DBNull.Value);

                cmd.ExecuteNonQuery();
            }
        }
    }

    /// <summary>
    /// Fetch the most recent color indexes logged for a given target + action (newest first).
    /// </summary>
    public List<int> GetRecentColorPattern(string target, string action = "light_color", int limit = 12)
    {
        var result = new List<int>();
        if (string.IsNullOrEmpty(target)) return result;

        using (var conn = new SqliteConnection(_connString))
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText =
                    @"SELECT color_index
                      FROM action_logs
                      WHERE target = @target AND action = @action AND color_index IS NOT NULL
                      ORDER BY id DESC
                      LIMIT @limit;";

                cmd.Parameters.AddWithValue("@target", target);
                cmd.Parameters.AddWithValue("@action", action);
                cmd.Parameters.AddWithValue("@limit", limit);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // newest-first
                        int idx = reader.GetInt32(0);
                        if (idx >= 0 && idx <= 2) result.Add(idx);
                    }
                }
            }
        }

        return result;
    }

    private string GetOrCreateSessionId()
    {
        const string key = "logger_session_id";
        if (!PlayerPrefs.HasKey(key))
        {
            PlayerPrefs.SetString(key, Guid.NewGuid().ToString("N"));
            PlayerPrefs.Save();
        }
        return PlayerPrefs.GetString(key);
    }

    private string ToFlatJson(Dictionary<string, string> dict)
    {
        if (dict == null || dict.Count == 0) return null;
        var parts = new List<string>();
        foreach (var kv in dict)
            parts.Add($"\"{Escape(kv.Key)}\":\"{Escape(kv.Value)}\"");
        return "{" + string.Join(",", parts) + "}";
    }

    private string Escape(string s)
    {
        if (s == null) return "";
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}