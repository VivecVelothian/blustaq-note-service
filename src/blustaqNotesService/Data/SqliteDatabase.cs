using Microsoft.Data.Sqlite;

/// <summary>
/// Database factory class.
/// Builds DB on init and returns created connection(s)
/// </summary>
public class SqliteDatabase
{
    private readonly string _connString = "Data Source=notes.db";

    /// <summary>
    /// DB init
    /// Generates DB schema if it does not already exist
    /// </summary>
    public void Initialize()
    {
        // Establish connection
        using var connection = new SqliteConnection(_connString);
        connection.Open();


        // Build DB
        var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Teams
            (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                CreateAt TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Users
            (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                TeamId INTEGER NOT NULL,
                Username TEXT NOT NULL,
                FOREIGN KEY(TeamId) REFERENCES Teams(Id) ON DELETE CASCADE 
            );

            CREATE TABLE IF NOT EXISTS Notes
            (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                TeamId INTEGER NOT NULL,
                CreatedById INTEGER NOT NULL,
                Title TEXT NOT NULL,
                Content TEXT NOT NULL,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,
                FOREIGN KEY(TeamID) REFERENCES Teams(Id) ON DELETE CASCADE,
                FOREIGN KEY(CreatedById) REFERENCES Users(Id) ON DELETE CASCADE
            );
        ";
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Creates a new connection to the DB
    /// </summary>
    /// <returns></returns>
    public SqliteConnection CreateConnection()
    {
        return new SqliteConnection(_connString);
    }
}