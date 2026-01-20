using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace SteelDesignerEngineer.Services.Auth.Startup;

public class AuthDatabaseInitializer : BackgroundService
{
    private readonly IMongoDatabase _database;
    private readonly ILogger<AuthDatabaseInitializer> _logger;

    public AuthDatabaseInitializer(IMongoDatabase database, ILogger<AuthDatabaseInitializer> logger)
    {
        _database = database;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Auth DB init: ensuring collections, indexes and default users...");

            var collections = await _database.ListCollectionNames().ToListAsync(stoppingToken);

            const string usersCollectionName = "Users";
            if (!collections.Contains(usersCollectionName))
            {
                await _database.CreateCollectionAsync(usersCollectionName, cancellationToken: stoppingToken);
                _logger.LogInformation("Created collection: {Collection}", usersCollectionName);
            }

            var users = _database.GetCollection<BsonDocument>(usersCollectionName);

            // Unique index on top-level Email (new schema)
            var emailIndex = new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("Email"),
                new CreateIndexOptions { Unique = true, Name = "ux_users_email" }
            );
            try
            {
                await users.Indexes.CreateOneAsync(emailIndex, cancellationToken: stoppingToken);
            }
            catch (MongoCommandException ex) when (
                ex.Message.Contains("Index already exists", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
            {
                // ignore
            }

            var adminEmail = Environment.GetEnvironmentVariable("SDE_ADMIN_EMAIL") ?? "admin@example.com";
            var adminFirstName = Environment.GetEnvironmentVariable("SDE_ADMIN_FIRSTNAME") ?? "Admin";
            var adminLastName = Environment.GetEnvironmentVariable("SDE_ADMIN_LASTNAME") ?? "User";
            var adminLegacyId = Environment.GetEnvironmentVariable("SDE_ADMIN_ID");

            var adminPassword = Environment.GetEnvironmentVariable("SDE_ADMIN_PASSWORD") ?? "ChangeMe_123!";
            var resetAdminPassword = string.Equals(
                Environment.GetEnvironmentVariable("SDE_ADMIN_RESET_PASSWORD"),
                "true",
                StringComparison.OrdinalIgnoreCase);

            // 1) Try find admin in NEW schema (top-level Email)
            var adminFilterNew = Builders<BsonDocument>.Filter.Eq("Email", adminEmail);
            var existingNew = await users.Find(adminFilterNew).FirstOrDefaultAsync(stoppingToken);

            // 2) Try find admin in LEGACY schema produced by Dictionary serializer: fields are inside _v
            var adminFilterLegacy = Builders<BsonDocument>.Filter.Eq("_v.Email", adminEmail);
            var existingLegacy = existingNew == null
                ? await users.Find(adminFilterLegacy).FirstOrDefaultAsync(stoppingToken)
                : null;

            // If legacy record exists, migrate it to new schema document (so login works)
            if (existingNew == null && existingLegacy != null)
            {
                _logger.LogWarning("Found legacy admin document (_v.*). Migrating to new schema: {Email}", adminEmail);

                var v = existingLegacy.GetValue("_v", new BsonDocument()).AsBsonDocument;

                var migrated = new BsonDocument
                {
                    { "Email", v.GetValue("Email", adminEmail).AsString },
                    { "FirstName", v.GetValue("FirstName", adminFirstName).AsString },
                    { "LastName", v.GetValue("LastName", adminLastName).AsString },
                    { "Role", v.GetValue("Role", "admin").AsString },
                    { "PasswordHash", v.GetValue("PasswordHash", BsonNull.Value) },
                    { "IsActive", v.GetValue("IsActive", true).ToBoolean() },
                    { "CreatedAt", v.GetValue("CreatedAt", BsonValue.Create(DateTime.UtcNow)) }
                };

                if (v.TryGetValue("Id", out var legacyIdValue) && legacyIdValue.BsonType == BsonType.String)
                {
                    migrated["Id"] = legacyIdValue.AsString;
                }

                // Insert migrated doc; then delete legacy wrapper to avoid confusion.
                await users.InsertOneAsync(migrated, cancellationToken: stoppingToken);
                await users.DeleteOneAsync(Builders<BsonDocument>.Filter.Eq("_id", existingLegacy["_id"]), stoppingToken);

                existingNew = migrated;
                _logger.LogInformation("Legacy admin migrated successfully: {Email}", adminEmail);
            }

            if (existingNew == null)
            {
                _logger.LogInformation("Seeding default admin user (new): {Email}", adminEmail);

                var adminHash = BCrypt.Net.BCrypt.HashPassword(adminPassword);

                var doc = new BsonDocument
                {
                    { "Email", adminEmail },
                    { "FirstName", adminFirstName },
                    { "LastName", adminLastName },
                    { "Role", "admin" },
                    { "PasswordHash", adminHash },
                    { "IsActive", true },
                    { "CreatedAt", DateTime.UtcNow }
                };

                if (!string.IsNullOrWhiteSpace(adminLegacyId))
                {
                    doc["Id"] = adminLegacyId;
                }

                await users.InsertOneAsync(doc, cancellationToken: stoppingToken);
                _logger.LogInformation("Default admin inserted: {Email}", adminEmail);
            }
            else
            {
                _logger.LogInformation("Default admin already exists (new schema): {Email}", adminEmail);

                var update = Builders<BsonDocument>.Update
                    .Set("FirstName", adminFirstName)
                    .Set("LastName", adminLastName)
                    .Set("Role", "admin")
                    .Set("IsActive", true);

                if (!string.IsNullOrWhiteSpace(adminLegacyId))
                {
                    update = update.Set("Id", adminLegacyId);
                }

                if (resetAdminPassword)
                {
                    var adminHash = BCrypt.Net.BCrypt.HashPassword(adminPassword);
                    update = update.Set("PasswordHash", adminHash);
                    _logger.LogWarning("Admin password reset is enabled via SDE_ADMIN_RESET_PASSWORD=true");
                }

                await users.UpdateOneAsync(adminFilterNew, update, cancellationToken: stoppingToken);
            }

            // Ensure role normalization (only admin/student/teacher)
            var roleUpdate = Builders<BsonDocument>.Update.Set("Role", "student");
            await users.UpdateManyAsync(
                Builders<BsonDocument>.Filter.Regex("Role", new BsonRegularExpression("^\\s*$")),
                roleUpdate,
                cancellationToken: stoppingToken);

            _logger.LogInformation("Auth DB init completed");
        }
        catch (OperationCanceledException)
        {
            // no-op on shutdown
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Auth DB init failed");
        }
    }
}
