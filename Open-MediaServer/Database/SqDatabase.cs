using System;
using System.IO;
using Open_MediaServer.Database.Schema;
using SQLite;

namespace Open_MediaServer.Database;

public class SqDatabase
{
    public readonly SQLiteAsyncConnection MediaDatabase;
    public readonly SQLiteAsyncConnection UserDatabase;

    public SqDatabase(string databasePath)
    {
        var mediaDatabase = Path.Combine(databasePath, "media.db");
        var userDatabase = Path.Combine(databasePath, "users.db");

        MediaDatabase = new SQLiteAsyncConnection(mediaDatabase);
        MediaDatabase.CreateTableAsync<DatabaseSchema.Media>();

        UserDatabase = new SQLiteAsyncConnection(userDatabase);
        UserDatabase.CreateTableAsync<DatabaseSchema.User>();
    }
}