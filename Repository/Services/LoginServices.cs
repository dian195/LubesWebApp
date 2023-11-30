using Microsoft.EntityFrameworkCore;
using WebApp.Models;
using WebApp.Repository.Interfaces;

namespace WebApp.Repository.Services
{
    public class LoginServices : ILogin
    {
        private readonly AppDB _context;
        public LoginServices(AppDB context)
        {
            _context = context;
        }

        public async Task<UserDTO> AuthenticateUser(string username, string passcode)
        {
            string encPassword = EncryptionHelper.Encrypt(passcode);
            string decPassword = EncryptionHelper.Decrypt(encPassword);

            if (username.Contains("@"))
            {
                var succeeded = await _context.account.FirstOrDefaultAsync(authUser => authUser.email == username && authUser.password == encPassword && authUser.isActive == 1);
                return succeeded;
            }
            else
            {
                var succeeded = await _context.account.FirstOrDefaultAsync(authUser => authUser.username == username && authUser.password == encPassword && authUser.isActive == 1);
                return succeeded;
            }
            
        }

        public async Task<IEnumerable<UserDTO>> getuser()
        {
            return await _context.account.ToListAsync();
        }
    }
}
