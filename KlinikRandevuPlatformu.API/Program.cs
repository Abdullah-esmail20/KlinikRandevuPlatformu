using KlinikRandevuPlatformu.API.Data;
using KlinikRandevuPlatformu.API.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using KlinikRandevuPlatformu.Shared.Enums;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// =====================
// Services
// =====================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// =====================
// Simple Sessions (in-memory)
// =====================
var sessions = new ConcurrentDictionary<string, SessionUser>();

static string HashPassword(string password)
{
    using var sha256 = SHA256.Create();
    var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
    return Convert.ToBase64String(bytes);
}

static IResult Fail(string msg, int code = 400) =>
    Results.Json(new { error = msg }, statusCode: code);

bool TryGetSession(HttpRequest req, out string token, out SessionUser user)
{
    token = "";
    user = default!;

    if (!req.Headers.TryGetValue("X-Auth-Token", out var v)) return false;

    token = v.ToString().Trim();
    if (string.IsNullOrWhiteSpace(token)) return false;

    return sessions.TryGetValue(token, out user);
}

IResult RequireAuth(HttpRequest req, out string token, out SessionUser user)
{
    if (!TryGetSession(req, out token, out user))
        return Fail("Unauthorized. Missing/invalid X-Auth-Token.", 401);

    return Results.Ok();
}

IResult RequireRole(SessionUser user, UserRole role)
{
    if (user.Role != role)
        return Fail("Forbidden. Role not allowed.", 403);

    return Results.Ok();
}

static void SetCreatedAudit(object entity, SessionUser user)
{
    var t = entity.GetType();
    t.GetProperty("CreatedAt")?.SetValue(entity, DateTime.UtcNow);
    t.GetProperty("CreatedBy")?.SetValue(entity, user.Username);
}

static void SetUpdatedAudit(object entity, SessionUser user)
{
    var t = entity.GetType();
    t.GetProperty("UpdatedAt")?.SetValue(entity, DateTime.UtcNow);
    t.GetProperty("UpdatedBy")?.SetValue(entity, user.Username);
}

// =====================
// /api
// =====================
var api = app.MapGroup("/api");

// =====================
// AUTH (Register/Login/Logout/ChangePassword)
// =====================
var auth = api.MapGroup("/auth");

auth.MapPost("/register", async (RegisterRequest req, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(req.Username)) return Fail("Username is required");
    if (string.IsNullOrWhiteSpace(req.Password)) return Fail("Password is required");

    if (req.Role is not (UserRole.Owner or UserRole.Patient))
        return Fail("Role must be Owner or Patient");

    var exists = await db.Users.AnyAsync(u => u.Username == req.Username.Trim());
    if (exists) return Fail("Username already exists");

    var user = new AppUser
    {
        Username = req.Username.Trim(),
        PasswordHash = HashPassword(req.Password),
        Role = req.Role
    };

    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Ok(new { user.Id, user.Username, role = user.Role.ToString() });
});

auth.MapPost("/login", async (LoginRequest req, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
        return Fail("Username and Password are required");

    var hashed = HashPassword(req.Password);

    var user = await db.Users.FirstOrDefaultAsync(u =>
        u.Username == req.Username.Trim() && u.PasswordHash == hashed);

    if (user is null) return Fail("Invalid username or password", 401);

    var token = Guid.NewGuid().ToString("N");
    sessions[token] = new SessionUser(user.Id, user.Username, user.Role);

    return Results.Ok(new
    {
        token,
        user = new { user.Id, user.Username, role = user.Role.ToString() }
    });
});

auth.MapPost("/logout", (HttpRequest req) =>
{
    var authRes = RequireAuth(req, out var token, out _);
    if (authRes is not Ok) return authRes;

    sessions.TryRemove(token, out _);
    return Results.Ok(new { message = "Logged out" });
});

auth.MapPost("/change-password", async (HttpRequest req, ChangePasswordRequest body, AppDbContext db) =>
{
    var authRes = RequireAuth(req, out _, out var sess);
    if (authRes is not Ok) return authRes;

    if (string.IsNullOrWhiteSpace(body.OldPassword) || string.IsNullOrWhiteSpace(body.NewPassword))
        return Fail("OldPassword and NewPassword are required");

    var user = await db.Users.FindAsync(sess.UserId);
    if (user is null) return Fail("User not found", 404);

    if (user.PasswordHash != HashPassword(body.OldPassword))
        return Fail("Old password is incorrect", 401);

    user.PasswordHash = HashPassword(body.NewPassword);
    await db.SaveChangesAsync();

    // remove all sessions for that user
    foreach (var kv in sessions.Where(x => x.Value.UserId == sess.UserId).ToList())
        sessions.TryRemove(kv.Key, out _);

    return Results.Ok(new { message = "Password changed. Please login again." });
});

// =====================
// DISEASE TYPES (GET)
// =====================
var disease = api.MapGroup("/disease-types");

disease.MapGet("/", async (AppDbContext db) =>
{
    return await db.DiseaseTypes
        .OrderBy(x => x.Name)
        .Select(x => new { x.Id, x.Name })
        .ToListAsync();
});

// =====================
// CLINICS (GET + CRUD owner)
// =====================
var clinics = api.MapGroup("/clinics");

// GET /api/clinics?city=Istanbul  (LINQ filtering)
clinics.MapGet("/", async (string? city, AppDbContext db) =>
{
    var q = db.Clinics.AsQueryable();

    if (!string.IsNullOrWhiteSpace(city))
        q = q.Where(c => c.City == city.Trim());

    var list = await q
        .Select(c => new
        {
            c.Id,
            c.ClinicName,
            c.City,
            c.Description,
            AppointmentCount = db.Appointments.Count(a => a.ClinicId == c.Id)
        })
        .OrderBy(x => x.City).ThenBy(x => x.ClinicName)
        .ToListAsync();

    return Results.Ok(list);
});

// GET clinic details
clinics.MapGet("/{id:int}", async (int id, AppDbContext db) =>
{
    var c = await db.Clinics
        .Where(x => x.Id == id)
        .Select(x => new { x.Id, x.ClinicName, x.City, x.Description, x.OwnerUserId })
        .FirstOrDefaultAsync();

    return c is null ? Results.NotFound() : Results.Ok(c);
});

// GET services by clinic ✅
clinics.MapGet("/{id:int}/services", async (int id, AppDbContext db) =>
{
    var exists = await db.Clinics.AnyAsync(c => c.Id == id);
    if (!exists) return Results.NotFound();

    var list = await db.Services
        .Where(s => s.ClinicId == id)
        .Select(s => new { s.Id, s.ServiceName, s.Price, s.DurationMinutes })
        .OrderBy(s => s.ServiceName)
        .ToListAsync();

    return Results.Ok(list);
});

// POST create clinic (Owner)
clinics.MapPost("/", async (HttpRequest req, ClinicCreateRequest body, AppDbContext db) =>
{
    var authRes = RequireAuth(req, out _, out var sess);
    if (authRes is not Ok) return authRes;

    var roleRes = RequireRole(sess, UserRole.Owner);
    if (roleRes is not Ok) return roleRes;

    if (string.IsNullOrWhiteSpace(body.ClinicName)) return Fail("ClinicName is required");
    if (string.IsNullOrWhiteSpace(body.City)) return Fail("City is required");

    var clinic = new Clinic
    {
        OwnerUserId = sess.UserId,
        ClinicName = body.ClinicName.Trim(),
        City = body.City.Trim(),
        Description = body.Description?.Trim()
    };

    SetCreatedAudit(clinic, sess);

    db.Clinics.Add(clinic);
    await db.SaveChangesAsync();

    return Results.Created($"/api/clinics/{clinic.Id}", new { clinic.Id, clinic.ClinicName, clinic.City });
});

// PUT update clinic (Owner)
clinics.MapPut("/{id:int}", async (HttpRequest req, int id, ClinicUpdateRequest body, AppDbContext db) =>
{
    var authRes = RequireAuth(req, out _, out var sess);
    if (authRes is not Ok) return authRes;

    var roleRes = RequireRole(sess, UserRole.Owner);
    if (roleRes is not Ok) return roleRes;

    var clinic = await db.Clinics.FindAsync(id);
    if (clinic is null) return Results.NotFound();

    if (clinic.OwnerUserId != sess.UserId) return Fail("Forbidden. Not your clinic.", 403);

    if (!string.IsNullOrWhiteSpace(body.ClinicName)) clinic.ClinicName = body.ClinicName.Trim();
    if (!string.IsNullOrWhiteSpace(body.City)) clinic.City = body.City.Trim();
    if (body.Description is not null) clinic.Description = body.Description.Trim();

    SetUpdatedAudit(clinic, sess);

    await db.SaveChangesAsync();
    return Results.Ok(new { clinic.Id, clinic.ClinicName, clinic.City, clinic.Description });
});

// DELETE clinic (Owner)
clinics.MapDelete("/{id:int}", async (HttpRequest req, int id, AppDbContext db) =>
{
    var authRes = RequireAuth(req, out _, out var sess);
    if (authRes is not Ok) return authRes;

    var roleRes = RequireRole(sess, UserRole.Owner);
    if (roleRes is not Ok) return roleRes;

    var clinic = await db.Clinics.FindAsync(id);
    if (clinic is null) return Results.NotFound();

    if (clinic.OwnerUserId != sess.UserId) return Fail("Forbidden. Not your clinic.", 403);

    db.Clinics.Remove(clinic);
    await db.SaveChangesAsync();

    return Results.Ok(new { message = "Clinic deleted" });
});

// =====================
// SERVICES (CRUD owner) + schedules view
// =====================
var services = api.MapGroup("/services");

// GET schedules by service ✅
services.MapGet("/{id:int}/schedules", async (int id, AppDbContext db) =>
{
    var exists = await db.Services.AnyAsync(s => s.Id == id);
    if (!exists) return Results.NotFound();

    var list = await db.ServiceSchedules
        .Where(x => x.ServiceId == id)
        .OrderBy(x => x.DayOfWeek)
        .ThenBy(x => x.StartTime)
        .Select(x => new { x.Id, x.ServiceId, x.DayOfWeek, x.StartTime, x.EndTime, x.DoctorName })
        .ToListAsync();

    return Results.Ok(list);
});

// POST create service (Owner)
services.MapPost("/", async (HttpRequest req, ServiceCreateRequest body, AppDbContext db) =>
{
    var authRes = RequireAuth(req, out _, out var sess);
    if (authRes is not Ok) return authRes;

    var roleRes = RequireRole(sess, UserRole.Owner);
    if (roleRes is not Ok) return roleRes;

    if (body.ClinicId <= 0) return Fail("ClinicId is required");
    if (string.IsNullOrWhiteSpace(body.ServiceName)) return Fail("ServiceName is required");
    if (body.Price < 0) return Fail("Price must be >= 0");

    var clinic = await db.Clinics.FindAsync(body.ClinicId);
    if (clinic is null) return Fail("Clinic not found", 404);
    if (clinic.OwnerUserId != sess.UserId) return Fail("Forbidden. Not your clinic.", 403);

    var service = new Service
    {
        ClinicId = body.ClinicId,
        ServiceName = body.ServiceName.Trim(),
        Price = body.Price,
        DurationMinutes = body.DurationMinutes
    };

    SetCreatedAudit(service, sess);

    db.Services.Add(service);
    await db.SaveChangesAsync();

    return Results.Created($"/api/services/{service.Id}", new { service.Id, service.ClinicId, service.ServiceName, service.Price });
});

// PUT update service (Owner)
services.MapPut("/{id:int}", async (HttpRequest req, int id, ServiceUpdateRequest body, AppDbContext db) =>
{
    var authRes = RequireAuth(req, out _, out var sess);
    if (authRes is not Ok) return authRes;

    var roleRes = RequireRole(sess, UserRole.Owner);
    if (roleRes is not Ok) return roleRes;

    var service = await db.Services.Include(s => s.Clinic).FirstOrDefaultAsync(s => s.Id == id);
    if (service is null) return Results.NotFound();
    if (service.Clinic is null) return Fail("Clinic not found for service", 500);

    if (service.Clinic.OwnerUserId != sess.UserId) return Fail("Forbidden. Not your clinic.", 403);

    if (!string.IsNullOrWhiteSpace(body.ServiceName)) service.ServiceName = body.ServiceName.Trim();
    if (body.Price.HasValue)
    {
        if (body.Price.Value < 0) return Fail("Price must be >= 0");
        service.Price = body.Price.Value;
    }
    if (body.DurationMinutes.HasValue) service.DurationMinutes = body.DurationMinutes.Value;

    SetUpdatedAudit(service, sess);

    await db.SaveChangesAsync();
    return Results.Ok(new { service.Id, service.ServiceName, service.Price, service.DurationMinutes });
});

// DELETE service (Owner)
services.MapDelete("/{id:int}", async (HttpRequest req, int id, AppDbContext db) =>
{
    var authRes = RequireAuth(req, out _, out var sess);
    if (authRes is not Ok) return authRes;

    var roleRes = RequireRole(sess, UserRole.Owner);
    if (roleRes is not Ok) return roleRes;

    var service = await db.Services.Include(s => s.Clinic).FirstOrDefaultAsync(s => s.Id == id);
    if (service is null) return Results.NotFound();
    if (service.Clinic is null) return Fail("Clinic not found for service", 500);

    if (service.Clinic.OwnerUserId != sess.UserId) return Fail("Forbidden. Not your clinic.", 403);

    db.Services.Remove(service);
    await db.SaveChangesAsync();
    return Results.Ok(new { message = "Service deleted" });
});

// =====================
// SCHEDULES (CRUD owner)
// =====================
var schedules = api.MapGroup("/schedules");

// POST create schedule (Owner)
schedules.MapPost("/", async (HttpRequest req, ScheduleCreateRequest body, AppDbContext db) =>
{
    var authRes = RequireAuth(req, out _, out var sess);
    if (authRes is not Ok) return authRes;

    var roleRes = RequireRole(sess, UserRole.Owner);
    if (roleRes is not Ok) return roleRes;

    if (body.ServiceId <= 0) return Fail("ServiceId is required");
    if (body.DayOfWeek is < 0 or > 6) return Fail("DayOfWeek must be 0..6");
    if (body.StartTime >= body.EndTime) return Fail("StartTime must be before EndTime");
    if (string.IsNullOrWhiteSpace(body.DoctorName)) return Fail("DoctorName is required");

    var service = await db.Services.Include(s => s.Clinic).FirstOrDefaultAsync(s => s.Id == body.ServiceId);
    if (service is null) return Fail("Service not found", 404);
    if (service.Clinic is null) return Fail("Clinic not found", 500);
    if (service.Clinic.OwnerUserId != sess.UserId) return Fail("Forbidden. Not your clinic.", 403);

    var sc = new ServiceSchedule
    {
        ServiceId = body.ServiceId,
        DayOfWeek = body.DayOfWeek,
        StartTime = body.StartTime,
        EndTime = body.EndTime,
        DoctorName = body.DoctorName.Trim()
    };

    SetCreatedAudit(sc, sess);

    db.ServiceSchedules.Add(sc);
    await db.SaveChangesAsync();

    return Results.Created($"/api/schedules/{sc.Id}", new { sc.Id, sc.ServiceId, sc.DayOfWeek, sc.StartTime, sc.EndTime, sc.DoctorName });
});

// PUT update schedule (Owner)
schedules.MapPut("/{id:int}", async (HttpRequest req, int id, ScheduleUpdateRequest body, AppDbContext db) =>
{
    var authRes = RequireAuth(req, out _, out var sess);
    if (authRes is not Ok) return authRes;

    var roleRes = RequireRole(sess, UserRole.Owner);
    if (roleRes is not Ok) return roleRes;

    var sc = await db.ServiceSchedules
        .Include(x => x.Service)
        .ThenInclude(s => s!.Clinic)
        .FirstOrDefaultAsync(x => x.Id == id);

    if (sc is null) return Results.NotFound();
    if (sc.Service?.Clinic is null) return Fail("Clinic not found", 500);

    if (sc.Service.Clinic.OwnerUserId != sess.UserId) return Fail("Forbidden. Not your clinic.", 403);

    if (body.DayOfWeek.HasValue)
    {
        if (body.DayOfWeek.Value is < 0 or > 6) return Fail("DayOfWeek must be 0..6");
        sc.DayOfWeek = body.DayOfWeek.Value;
    }

    if (body.StartTime.HasValue) sc.StartTime = body.StartTime.Value;
    if (body.EndTime.HasValue) sc.EndTime = body.EndTime.Value;

    if (sc.StartTime >= sc.EndTime) return Fail("StartTime must be before EndTime");

    if (!string.IsNullOrWhiteSpace(body.DoctorName)) sc.DoctorName = body.DoctorName.Trim();

    SetUpdatedAudit(sc, sess);

    await db.SaveChangesAsync();
    return Results.Ok(new { sc.Id, sc.ServiceId, sc.DayOfWeek, sc.StartTime, sc.EndTime, sc.DoctorName });
});

// DELETE schedule (Owner)
schedules.MapDelete("/{id:int}", async (HttpRequest req, int id, AppDbContext db) =>
{
    var authRes = RequireAuth(req, out _, out var sess);
    if (authRes is not Ok) return authRes;

    var roleRes = RequireRole(sess, UserRole.Owner);
    if (roleRes is not Ok) return roleRes;

    var sc = await db.ServiceSchedules
        .Include(x => x.Service)
        .ThenInclude(s => s!.Clinic)
        .FirstOrDefaultAsync(x => x.Id == id);

    if (sc is null) return Results.NotFound();
    if (sc.Service?.Clinic is null) return Fail("Clinic not found", 500);

    if (sc.Service.Clinic.OwnerUserId != sess.UserId) return Fail("Forbidden. Not your clinic.", 403);

    db.ServiceSchedules.Remove(sc);
    await db.SaveChangesAsync();
    return Results.Ok(new { message = "Schedule deleted" });
});

// =====================
// APPOINTMENTS
// =====================
var appts = api.MapGroup("/appointments");

// POST create appointment (Patient)
appts.MapPost("/", async (HttpRequest req, AppointmentCreateRequest body, AppDbContext db) =>
{
    var authRes = RequireAuth(req, out _, out var sess);
    if (authRes is not Ok) return authRes;

    var roleRes = RequireRole(sess, UserRole.Patient);
    if (roleRes is not Ok) return roleRes;

    if (body.ClinicId <= 0) return Fail("ClinicId is required");
    if (body.ServiceId <= 0) return Fail("ServiceId is required");
    if (body.DiseaseTypeId <= 0) return Fail("DiseaseTypeId is required");
    if (string.IsNullOrWhiteSpace(body.PatientName)) return Fail("PatientName is required");
    if (string.IsNullOrWhiteSpace(body.Kimlik)) return Fail("Kimlik is required");
    if (string.IsNullOrWhiteSpace(body.Phone)) return Fail("Phone is required");
    if (body.AppointmentDateTime <= DateTime.Now) return Fail("AppointmentDateTime must be in the future");

    var clinic = await db.Clinics.FindAsync(body.ClinicId);
    if (clinic is null) return Fail("Clinic not found", 404);

    var service = await db.Services.FirstOrDefaultAsync(s => s.Id == body.ServiceId && s.ClinicId == body.ClinicId);
    if (service is null) return Fail("Service not found for this clinic", 404);

    var diseaseExists = await db.DiseaseTypes.AnyAsync(d => d.Id == body.DiseaseTypeId);
    if (!diseaseExists) return Fail("DiseaseType not found", 404);

    // validate schedule
    var dow = (int)body.AppointmentDateTime.DayOfWeek; // 0..6
    var time = body.AppointmentDateTime.TimeOfDay;

    var scheduleOk = await db.ServiceSchedules.AnyAsync(s =>
        s.ServiceId == body.ServiceId &&
        s.DayOfWeek == dow &&
        s.StartTime <= time &&
        time < s.EndTime);

    if (!scheduleOk) return Fail("Selected time is خارج وقت الدوام لهذه الخدمة");

    // prevent duplicates
    var duplicate = await db.Appointments.AnyAsync(a =>
        a.ServiceId == body.ServiceId && a.AppointmentDateTime == body.AppointmentDateTime);

    if (duplicate) return Fail("This time is already booked for this service");

    var appt = new Appointment
    {
        ClinicId = body.ClinicId,
        ServiceId = body.ServiceId,
        PatientUserId = sess.UserId,
        PatientName = body.PatientName.Trim(),
        Kimlik = body.Kimlik.Trim(),
        Phone = body.Phone.Trim(),
        DiseaseTypeId = body.DiseaseTypeId,
        AppointmentDateTime = body.AppointmentDateTime,
        Status = AppointmentStatus.Pending
    };

    SetCreatedAudit(appt, sess);

    db.Appointments.Add(appt);
    await db.SaveChangesAsync();

    return Results.Created($"/api/appointments/{appt.Id}", new { appt.Id, status = appt.Status.ToString() });
});

// GET patient own appointments
appts.MapGet("/mine", async (HttpRequest req, AppDbContext db) =>
{
    var authRes = RequireAuth(req, out _, out var sess);
    if (authRes is not Ok) return authRes;

    var roleRes = RequireRole(sess, UserRole.Patient);
    if (roleRes is not Ok) return roleRes;

    var list = await db.Appointments
        .Where(a => a.PatientUserId == sess.UserId)
        .OrderByDescending(a => a.AppointmentDateTime)
        .Select(a => new
        {
            a.Id,
            a.ClinicId,
            a.ServiceId,
            a.PatientName,
            a.Kimlik,
            a.Phone,
            a.DiseaseTypeId,
            a.AppointmentDateTime,
            status = a.Status.ToString()
        })
        .ToListAsync();

    return Results.Ok(list);
});

// GET owner appointments
appts.MapGet("/owner", async (HttpRequest req, int? clinicId, DateTime? date, AppDbContext db) =>
{
    var authRes = RequireAuth(req, out _, out var sess);
    if (authRes is not Ok) return authRes;

    var roleRes = RequireRole(sess, UserRole.Owner);
    if (roleRes is not Ok) return roleRes;

    var ownedClinicIds = db.Clinics
        .Where(c => c.OwnerUserId == sess.UserId)
        .Select(c => c.Id);

    var q = db.Appointments.Where(a => ownedClinicIds.Contains(a.ClinicId));

    if (clinicId.HasValue) q = q.Where(a => a.ClinicId == clinicId.Value);

    if (date.HasValue)
    {
        var d = date.Value.Date;
        var next = d.AddDays(1);
        q = q.Where(a => a.AppointmentDateTime >= d && a.AppointmentDateTime < next);
    }

    var list = await q
        .OrderByDescending(a => a.AppointmentDateTime)
        .Select(a => new
        {
            a.Id,
            a.ClinicId,
            a.ServiceId,
            a.PatientName,
            a.Kimlik,
            a.Phone,
            a.DiseaseTypeId,
            a.AppointmentDateTime,
            status = a.Status.ToString()
        })
        .ToListAsync();

    return Results.Ok(list);
});

// PUT owner update status
appts.MapPut("/{id:int}/status", async (HttpRequest req, int id, UpdateAppointmentStatusRequest body, AppDbContext db) =>
{
    var authRes = RequireAuth(req, out _, out var sess);
    if (authRes is not Ok) return authRes;

    var roleRes = RequireRole(sess, UserRole.Owner);
    if (roleRes is not Ok) return roleRes;

    var appt = await db.Appointments.FindAsync(id);
    if (appt is null) return Results.NotFound();

    var clinic = await db.Clinics.FindAsync(appt.ClinicId);
    if (clinic is null) return Fail("Clinic not found", 500);
    if (clinic.OwnerUserId != sess.UserId) return Fail("Forbidden. Not your clinic.", 403);

    appt.Status = body.Status;
    SetUpdatedAudit(appt, sess);

    await db.SaveChangesAsync();
    return Results.Ok(new { appt.Id, status = appt.Status.ToString() });
});

// =====================
// HEALTH
// =====================
api.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();

// =====================
// Contracts (records)
// =====================
record SessionUser(int UserId, string Username, UserRole Role);

record RegisterRequest(string Username, string Password, UserRole Role);
record LoginRequest(string Username, string Password);
record ChangePasswordRequest(string OldPassword, string NewPassword);

record ClinicCreateRequest(string ClinicName, string City, string? Description);
record ClinicUpdateRequest(string? ClinicName, string? City, string? Description);

record ServiceCreateRequest(int ClinicId, string ServiceName, decimal Price, int? DurationMinutes);
record ServiceUpdateRequest(string? ServiceName, decimal? Price, int? DurationMinutes);

record ScheduleCreateRequest(int ServiceId, int DayOfWeek, TimeSpan StartTime, TimeSpan EndTime, string DoctorName);
record ScheduleUpdateRequest(int? DayOfWeek, TimeSpan? StartTime, TimeSpan? EndTime, string? DoctorName);

record AppointmentCreateRequest(
    int ClinicId,
    int ServiceId,
    string PatientName,
    string Kimlik,
    string Phone,
    int DiseaseTypeId,
    DateTime AppointmentDateTime
);

record UpdateAppointmentStatusRequest(AppointmentStatus Status);