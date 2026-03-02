using KlinikRandevuPlatformu.Shared.Enums;

namespace KlinikRandevuPlatformu.MAUI.Models;

//public record RegisterRequest(string Username, string Password, UserRole Role);

public record LoginRequest(string Username, string Password);

public record LoginResponse(string token, UserInfo user);

public record UserInfo(int id, string username, string role);

public record HealthResponse(string status);