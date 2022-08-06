using System.ComponentModel.DataAnnotations;
using SQLiteNetExtensions.Attributes;

namespace Open_MediaServer.Backend.Schema;

public class UserSchema
{
    public class User
    {
        [Required] public string Username { get; set; }
        [Required] public string Password { get; set; }
    }
    
    public class UserDelete
    {
        [Required] public User User { get; set; }
        [Required] public bool DeleteContent { get; set; }
    }
    
    public class UserUpdate
    {
        [Required] public string Username { get; set; }
        public string Name { get; set; }
    }
    
    public class UserPasswordChange
    {
        [Required] public string Username { get; set; }
        public string CurrentPassword { get; set; }
        
        public string Password { get; set; }
        public string PasswordConfirm { get; set; }
    }
}