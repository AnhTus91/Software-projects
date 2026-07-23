using QLThuocDAPM.Data;

namespace QLThuocDAPM.Services
{
    public interface IUserService
    {
        Task<NguoiDung> AuthenticateUserAsync(string username, string password);
        Task<bool> RegisterUserAsync(NguoiDung user);
        Task<bool> SendRecoveryCodeAsync(string email);
        Task<bool> VerifyRecoveryCodeAsync(string recoveryCode);
        Task<bool> ResetPasswordAsync(string email, string newPassword);
        Task<NguoiDung> GetUserInfoAsync(string username);
        Task<bool> UpdateUserInfoAsync(NguoiDung user);
    }
}
