using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// If you're using Unity's built-in SQLite support, this is typically available.
// If you get missing reference errors, see note at bottom.
using Mono.Data.Sqlite;

public class ActionLoggerDB : MonoBehaviour
{
    public static ActionLoggerDB Instance { get; private set; }

    [Header("DB Settings")]
    [Tooltip("Database file name created under Application.persistentDataPath")]
    public string dbFileName = "user_actions.sqlite";

    private string _dbPath;
    private string _connString;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _dbPath = Path.Combine(Application.persistentDataPath, dbFileName);
        _connString = "URI=file:" + _dbPath;

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
                        position_x REAL,
                        position_y REAL,
                        position_z REAL,
                        meta_json TEXT
                    );";
                cmd.ExecuteNonQuery();
            }
        }
    }

    /// <summary>
    /// Logs an action to SQLite.
    /// meta can be any key/value pairs; stored as a simple JSON string.
    /// </summary>
    public void LogAction(
        string action,
        string target = null,
        string userId = null,
        Vector3? worldPos = null,
        Dictionary<string, string> meta = null)
    {
        string timestampUtc = DateTime.UtcNow.ToString("o");
        string sessionId = GetOrCreateSessionId();
        string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        Vector3 pos = worldPos ?? new Vector3(float.NaN, float.NaN, float.NaN);
        string metaJson = ToFlatJson(meta);

        using (var conn = new SqliteConnection(_connString))
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText =
                    @"INSERT INTO action_logs
                      (timestamp_utc, session_id, user_id, action, target, scene, position_x, position_y, position_z, meta_json)
                      VALUES
                      (@ts, @sid, @uid, @action, @target, @scene, @x, @y, @z, @meta);";

                cmd.Parameters.AddWithValue("@ts", timestampUtc);
                cmd.Parameters.AddWithValue("@sid", sessionId);
                cmd.Parameters.AddWithValue("@uid", (object)userId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@action", action);
                cmd.Parameters.AddWithValue("@target", (object)target ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@scene", scene);
                cmd.Parameters.AddWithValue("@x", pos.x);
                cmd.Parameters.AddWithValue("@y", pos.y);
                cmd.Parameters.AddWithValue("@z", pos.z);
                cmd.Parameters.AddWithValue("@meta", (object)metaJson ?? DBNull.Value);

                cmd.ExecuteNonQuery();
            }
        }
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

    // Minimal “flat json” builder to avoid extra packages.
    private string ToFlatJson(Dictionary<string, string> dict)
    {
        if (dict == null || dict.Count == 0) return null;

        // {"k":"v","k2":"v2"}
        List<string> parts = new List<string>();
        foreach (var kv in dict)
        {
            parts.Add($"\"{Escape(kv.Key)}\":\"{Escape(kv.Value)}\"");
        }
        return "{" + string.Join(",", parts) + "}";
    }

    private string Escape(string s)
    {
        if (s == null) return "";
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
