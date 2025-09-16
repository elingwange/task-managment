public class RegisterRequest
{
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; } // 这个属性用来接收原始密码
}