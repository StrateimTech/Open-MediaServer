using System.Linq;
using System.Threading.Tasks;
using Open_MediaServer.Database.Schema;

namespace Open_MediaServer.Utils;

public static class UserUtils
{
    // public static async Task<bool> IsAuthed(string username, string hashedPassword)
    // {
    //     var user = await Program.Database.UserDatabase
    //         .GetAsync<DatabaseSchema.User>(user => user.Username == username);
    //     if (user.Password.SequenceEqual(hashedPassword))
    //     {
    //         return true;
    //     }
    //     return false;
    // }
}