namespace FreeStyleApp.Application.DTOs
{
    public class UserDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; }
        public string Password { get; set; }
        public List<string> Permissions { get; set; }
    }
}
