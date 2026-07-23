using Microsoft.EntityFrameworkCore;
using QLThuocDAPM.Data;

namespace QLThuocDAPM.Services
{
    public class UserService : IUserService
    {
        private readonly QlthuocDapm6Context _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(QlthuocDapm6Context context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<NguoiDung> AuthenticateUserAsync(string username, string password)
        {
            return await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Username == username && u.Matkhau == password);
        }

        public async Task<bool> RegisterUserAsync(NguoiDung user)
        {
            var existingUser = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Username == user.Username);
            if (existingUser != null)
                return false;

            user.TrangThai = "Chưa mua hàng";
            _context.NguoiDungs.Add(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SendRecoveryCodeAsync(string email)
        {
            var account = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == email);
            if (account == null)
            {
                Console.WriteLine("[ERROR] Không tìm thấy email trong database!");
                return false;
            }

            // Lấy thông tin user (không cần thiết vì account đã chứa đủ dữ liệu)
            var user = account;

            // Tạo mã khôi phục ngẫu nhiên
            Random random = new Random();
            string recoveryCode = random.Next(100000, 999999).ToString();

            // Lưu mã vào session
            _httpContextAccessor.HttpContext.Session.SetString("RecoveryCode", recoveryCode);
            _httpContextAccessor.HttpContext.Session.SetString("email", account.Email);
            _httpContextAccessor.HttpContext.Session.SetString("RecoveryCodeCreationTime", DateTime.Now.ToString());

            // Gửi email
            string subject = "Khôi phục mật khẩu";
            string content = $"Mã khôi phục mật khẩu của bạn là: {recoveryCode}. <br> Lưu ý: mã sẽ hết hạn trong 1 phút!";

            bool emailSent = Common.Common.SendMail(user.Username, subject, content, account.Email);
            Console.WriteLine($"[DEBUG] Trạng thái gửi mail: {emailSent}");

            return emailSent;
        }


        public async Task<bool> VerifyRecoveryCodeAsync(string recoveryCode)
        {
            string sessionRecoveryCode = _httpContextAccessor.HttpContext.Session.GetString("RecoveryCode");
            string recoveryCodeCreationTime = _httpContextAccessor.HttpContext.Session.GetString("RecoveryCodeCreationTime");

            if (sessionRecoveryCode == null || recoveryCodeCreationTime == null)
                return false;

            DateTime creationTime = DateTime.Parse(recoveryCodeCreationTime);
            if ((DateTime.Now - creationTime).TotalMinutes > 1)
                return false;

            return sessionRecoveryCode == recoveryCode;
        }

        public async Task<bool> ResetPasswordAsync(string email, string newPassword)
        {
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return false;

            user.Matkhau = newPassword;
            _context.NguoiDungs.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<NguoiDung> GetUserInfoAsync(string username)
        {
            return await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<bool> UpdateUserInfoAsync(NguoiDung user)
        {
            _context.NguoiDungs.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
