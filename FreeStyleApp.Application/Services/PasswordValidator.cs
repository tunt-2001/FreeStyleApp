namespace FreeStyleApp.Application.Services
{
    public class PasswordValidator
    {
        public Task<(bool IsValid, string ErrorMessage)> ValidatePasswordAsync(string password)
        {
            // Cấu hình cố định
            var minLengthValue = 6;
            var requireStrongValue = true;

            // Kiểm tra độ dài tối thiểu
            if (password.Length < minLengthValue)
            {
                return Task.FromResult((false, $"Mật khẩu phải có ít nhất {minLengthValue} ký tự."));
            }

            // Kiểm tra mật khẩu mạnh nếu được yêu cầu
            if (requireStrongValue)
            {
                if (!password.Any(char.IsDigit))
                {
                    return Task.FromResult((false, "Mật khẩu phải chứa ít nhất một chữ số."));
                }

                if (!password.Any(char.IsLetter))
                {
                    return Task.FromResult((false, "Mật khẩu phải chứa ít nhất một chữ cái."));
                }

                if (!password.Any(c => char.IsSymbol(c) || char.IsPunctuation(c)))
                {
                    return Task.FromResult((false, "Mật khẩu phải chứa ít nhất một ký tự đặc biệt."));
                }
            }

            return Task.FromResult((true, string.Empty));
        }
    }
}

