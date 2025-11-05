namespace FreeStyleApp.DTOs
{
    public class UserDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public List<string> Permissions { get; set; }
    }
}
