using System.ComponentModel.DataAnnotations;

namespace Open_MediaServer.Backend.Schema;

public class UserSchema
{
    public class UserLogin
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
    
    public class UserRegister
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
    
    public class UserDelete
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public bool DeleteMedia { get; set; }
    }
}