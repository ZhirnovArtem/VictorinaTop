using Microsoft.Data.Sqlite;
using VictorinaTop.Mobile.Models;

namespace VictorinaTop.Mobile.Services;

public class LocalDatabaseService
{
    private readonly string _dbPath;

    public LocalDatabaseService()
    {
        _dbPath = Path.Combine(FileSystem.AppDataDirectory, "VictorinaTopCache.db");
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();

        string createThemes = @"
            CREATE TABLE IF NOT EXISTS Themes (
                Id INTEGER PRIMARY KEY,
                Name TEXT NOT NULL,
                Author TEXT NOT NULL,
                CachedAt TEXT NOT NULL
            )";
        using var cmd1 = new SqliteCommand(createThemes, connection);
        cmd1.ExecuteNonQuery();

        string createQuestions = @"
            CREATE TABLE IF NOT EXISTS Questions (
                Id INTEGER PRIMARY KEY,
                ThemeId INTEGER NOT NULL,
                Text TEXT NOT NULL,
                OptionA TEXT NOT NULL,
                OptionB TEXT NOT NULL,
                OptionC TEXT NOT NULL,
                OptionD TEXT NOT NULL,
                CorrectAnswer TEXT NOT NULL,
                Status TEXT NOT NULL
            )";
        using var cmd2 = new SqliteCommand(createQuestions, connection);
        cmd2.ExecuteNonQuery();
    }

    public void CacheThemes(List<Theme> themes)
    {
        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();

        using var deleteCmd = new SqliteCommand("DELETE FROM Themes", connection);
        deleteCmd.ExecuteNonQuery();

        foreach (var theme in themes)
        {
            using var cmd = new SqliteCommand(
                "INSERT INTO Themes (Id, Name, Author, CachedAt) VALUES (@id, @name, @author, @cached)",
                connection);
            cmd.Parameters.AddWithValue("@id", theme.Id);
            cmd.Parameters.AddWithValue("@name", theme.Name);
            cmd.Parameters.AddWithValue("@author", theme.Author);
            cmd.Parameters.AddWithValue("@cached", DateTime.Now.ToString("o"));
            cmd.ExecuteNonQuery();
        }
    }

    public List<Theme> GetCachedThemes()
    {
        var themes = new List<Theme>();

        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();

        using var cmd = new SqliteCommand("SELECT Id, Name, Author FROM Themes", connection);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            themes.Add(new Theme
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Author = reader.GetString(2)
            });
        }
        return themes;
    }

    // Метод для кэширования вопросов
    public void CacheQuestions(int themeId, List<Question> questions)
    {
        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();

        using var deleteCmd = new SqliteCommand("DELETE FROM Questions WHERE ThemeId = @themeId", connection);
        deleteCmd.Parameters.AddWithValue("@themeId", themeId);
        deleteCmd.ExecuteNonQuery();

        foreach (var q in questions)
        {
            using var cmd = new SqliteCommand(
                @"INSERT INTO Questions (Id, ThemeId, Text, OptionA, OptionB, OptionC, OptionD, CorrectAnswer, Status) 
                  VALUES (@id, @themeId, @text, @a, @b, @c, @d, @correct, @status)",
                connection);
            cmd.Parameters.AddWithValue("@id", q.Id);
            cmd.Parameters.AddWithValue("@themeId", themeId);
            cmd.Parameters.AddWithValue("@text", q.Text);
            cmd.Parameters.AddWithValue("@a", q.OptionA);
            cmd.Parameters.AddWithValue("@b", q.OptionB);
            cmd.Parameters.AddWithValue("@c", q.OptionC);
            cmd.Parameters.AddWithValue("@d", q.OptionD);
            cmd.Parameters.AddWithValue("@correct", q.CorrectAnswer);
            cmd.Parameters.AddWithValue("@status", q.Status);
            cmd.ExecuteNonQuery();
        }
    }

    public List<Question> GetCachedQuestions(int themeId)
    {
        var questions = new List<Question>();

        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();

        using var cmd = new SqliteCommand("SELECT * FROM Questions WHERE ThemeId = @themeId", connection);
        cmd.Parameters.AddWithValue("@themeId", themeId);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            questions.Add(new Question
            {
                Id = reader.GetInt32(0),
                Text = reader.GetString(2),
                OptionA = reader.GetString(3),
                OptionB = reader.GetString(4),
                OptionC = reader.GetString(5),
                OptionD = reader.GetString(6),
                CorrectAnswer = reader.GetString(7),
                Status = reader.GetString(8)
            });
        }
        return questions;
    }
}