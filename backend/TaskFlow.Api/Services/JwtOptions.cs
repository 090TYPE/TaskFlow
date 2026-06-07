namespace TaskFlow.Api.Services;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "TaskFlow";
    public string Audience { get; set; } = "TaskFlowClient";
    public string Key { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 1440; // 24h
}
