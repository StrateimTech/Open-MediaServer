using System;
using System.Data;
using System.IO;
using Open_MediaServer.Database.Schema;
using SQLite;

namespace Open_MediaServer.Database;

public class SqLite
{
    public readonly SQLiteAsyncConnection DatabaseConnection;
    
    public SqLite(string databasePath)
    {
        var database = Path.Combine(databasePath, "media.db");
        DatabaseConnection = new SQLiteAsyncConnection(database);
        
        DatabaseConnection.CreateTableAsync<DatabaseSchema.Media>();
    }
}