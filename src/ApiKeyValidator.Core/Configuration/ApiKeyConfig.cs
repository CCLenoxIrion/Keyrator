namespace ApiKeyValidator.Core.Configuration;

public class ApiKeyConfig
{
    public string Prefix { get; set; } = string.Empty;
    public int? Length { get; set; }
    public string AllowedCharsRegex { get; set; } = @"^[a-zA-Z0-9-_]+$";
    public string GenerationChars { get; set; } = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";
    public DateTime? ExpirationDate { get; set; }
    public bool RequirePrefix { get; set; } = true;
}