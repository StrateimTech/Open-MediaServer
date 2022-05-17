using System;
using System.Data;
using System.IO;
using System.Threading;
using Open_MediaServer.Database.Schema;
using SQLite;

namespace Open_MediaServer.Database;

public class SqLite
{
    public readonly SQLiteAsyncConnection MediaDatabase;
    public readonly SQLiteAsyncConnection UserDatabase;
    
    public SqLite(string databasePath)
    {
        var mediaDatabase = Path.Combine(databasePath, "media.db");
        var userDatabase = Path.Combine(databasePath, "users.db");
        
        MediaDatabase = new SQLiteAsyncConnection(mediaDatabase);
        MediaDatabase.CreateTableAsync<DatabaseSchema.Media>();

        UserDatabase = new SQLiteAsyncConnection(userDatabase);
        UserDatabase.CreateTableAsync<DatabaseSchema.User>();
    }
}