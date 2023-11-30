using WebApp.Models;

namespace WebApp.Repository.Interfaces
{
    public interface ILogin
    {
        Task<IEnumerable<UserDTO>> getuser();
        Task<UserDTO> AuthenticateUser(string username, string passcode);
    }
}
