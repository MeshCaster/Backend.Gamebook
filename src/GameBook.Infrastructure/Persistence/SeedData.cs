using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using GameBook.Domain.Entities;
using GameBook.Domain.Enums;
using NetTopologySuite.Geometries;

namespace GameBook.Infrastructure.Persistence;

public static class SeedData
{
    public static async Task SeedAsync(GameBookDbContext db, string csvPath)
    {
        if (db.Venues.Any()) return;

        var users = new[]
        {
            new User
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                SupabaseId = "test-supabase-id-1",
                DisplayName = "TestGamer",
                Email = "test@gamebook.ge",
                GamerTag = "TestGamer#001",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new User
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                SupabaseId = "test-supabase-id-2",
                DisplayName = "ProPlayer",
                Email = "pro@gamebook.ge",
                GamerTag = "ProPlayer#002",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new User
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                SupabaseId = "test-supabase-id-3",
                DisplayName = "CasualGamer",
                Email = "casual@gamebook.ge",
                GamerTag = "CasualGamer#003",
                CreatedAt = DateTimeOffset.UtcNow
            }
        };
        db.Users.AddRange(users);

        foreach (var user in users)
        {
            db.Wallets.Add(new Wallet
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Balance = 100m,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        var rows = ParseCsv(csvPath);
        var random = new Random(42);
        var usedSlugs = new HashSet<string>();

        var imageUrls = new[]
        {
            "https://images.unsplash.com/photo-1542751371-adc38448a05e?w=800",
            "https://images.unsplash.com/photo-1511512578047-dfb367046420?w=800",
            "https://images.unsplash.com/photo-1593305841991-05c297ba4575?w=800",
            "https://images.unsplash.com/photo-1612287230202-1ff1d85d1bdf?w=800",
            "https://images.unsplash.com/photo-1538481199705-c710c4e965fc?w=800",
            "https://images.unsplash.com/photo-1552820728-8b83bb6b2b28?w=800",
            "https://images.unsplash.com/photo-1600861194942-f883de0dfe96?w=800",
            "https://images.unsplash.com/photo-1586182987320-4f376d39d787?w=800"
        };

        var venues = new List<Venue>();

        foreach (var row in rows)
        {
            var name = row.GetValueOrDefault("name", "");
            if (string.IsNullOrWhiteSpace(name)) continue;

            var city = row.GetValueOrDefault("city", "");
            var district = row.GetValueOrDefault("district_area", "");
            var address = row.GetValueOrDefault("address", "");
            var notes = row.GetValueOrDefault("notes", "");
            var focusLevel = row.GetValueOrDefault("ps_focus_level", "mixed");

            var slug = GenerateUniqueSlug(name, usedSlugs);
            var description = GenerateDescription(name, city, district, focusLevel, notes);

            double.TryParse(row.GetValueOrDefault("latitude", ""), CultureInfo.InvariantCulture, out var lat);
            double.TryParse(row.GetValueOrDefault("longitude", ""), CultureInfo.InvariantCulture, out var lng);
            double.TryParse(row.GetValueOrDefault("rating", ""), CultureInfo.InvariantCulture, out var rating);
            int.TryParse(row.GetValueOrDefault("rating_count", ""), out var reviewCount);

            var opensAt = ParseTime(row.GetValueOrDefault("opens", "")) ?? new TimeOnly(12, 0);
            var closesAt = ParseTime(row.GetValueOrDefault("closes", "")) ?? new TimeOnly(0, 0);

            if (row.GetValueOrDefault("is_24h", "") == "true")
            {
                opensAt = new TimeOnly(0, 0);
                closesAt = new TimeOnly(23, 59);
            }

            var phone = string.IsNullOrWhiteSpace(row.GetValueOrDefault("phone", ""))
                ? null
                : row["phone"];

            var amenities = GenerateAmenities(focusLevel, notes);
            var imageUrl = imageUrls[venues.Count % imageUrls.Length];

            var venue = new Venue
            {
                Id = Guid.NewGuid(),
                Name = name,
                Slug = slug,
                Description = description,
                Address = address,
                ImageUrl = imageUrl,
                CoverUrl = imageUrl,
                Location = lat != 0 && lng != 0 ? new Point(lng, lat) { SRID = 4326 } : null,
                Rating = rating > 0 ? rating : 4.5,
                ReviewCount = reviewCount,
                OpensAt = opensAt,
                ClosesAt = closesAt,
                Phone = phone,
                Amenities = amenities,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-random.Next(30, 365))
            };

            venues.Add(venue);

            var stations = GenerateStations(venue.Id, focusLevel, random);
            db.Stations.AddRange(stations);
        }

        db.Venues.AddRange(venues);

        var comments = new[]
        {
            "Amazing place! Best gaming setup in the area.",
            "Great atmosphere, will definitely come back.",
            "PCs are top-notch. Staff is super friendly.",
            "A bit pricey but worth every lari.",
            "Perfect for a night out with friends.",
            "The VR setup blew my mind!",
            "Cozy place with great vibes.",
            "Clean, well-maintained, and great AC.",
            "Love the PS5 selection here!",
            "Best venue in the neighborhood for gaming.",
            "Comfortable chairs and great monitors.",
            "The staff really knows their stuff.",
            "Good snacks and fast internet.",
            "Nice spot to hang out and play with friends."
        };

        foreach (var venue in venues)
        {
            var reviewerCount = random.Next(1, users.Length + 1);
            for (var i = 0; i < reviewerCount; i++)
            {
                db.Reviews.Add(new Review
                {
                    Id = Guid.NewGuid(),
                    UserId = users[i].Id,
                    VenueId = venue.Id,
                    Rating = random.Next(3, 6),
                    Comment = comments[random.Next(comments.Length)],
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-random.Next(1, 90))
                });
            }
        }

        await db.SaveChangesAsync();
    }

    private static List<Dictionary<string, string>> ParseCsv(string path)
    {
        var results = new List<Dictionary<string, string>>();
        var lines = File.ReadAllLines(path);
        if (lines.Length < 2) return results;

        var headers = ParseCsvLine(lines[0]);

        for (var i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var values = ParseCsvLine(line);
            var row = new Dictionary<string, string>();

            for (var j = 0; j < headers.Length; j++)
                row[headers[j]] = j < values.Length ? values[j] : "";

            results.Add(row);
        }

        return results;
    }

    private static string[] ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            if (inQuotes)
            {
                if (line[i] == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    current.Append(line[i]);
                }
            }
            else if (line[i] == '"')
            {
                inQuotes = true;
            }
            else if (line[i] == ',')
            {
                fields.Add(current.ToString().Trim());
                current.Clear();
            }
            else
            {
                current.Append(line[i]);
            }
        }

        fields.Add(current.ToString().Trim());
        return fields.ToArray();
    }

    private static string GenerateUniqueSlug(string name, HashSet<string> usedSlugs)
    {
        var slug = name.ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^\w\s-]", "");
        slug = Regex.Replace(slug, @"[\s_]+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        slug = slug.Trim('-');

        if (string.IsNullOrEmpty(slug))
            slug = "venue";

        var baseSlug = slug;
        var counter = 2;
        while (!usedSlugs.Add(slug))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        return slug;
    }

    private static string GenerateDescription(string name, string city, string district,
        string focusLevel, string notes)
    {
        var sb = new StringBuilder();

        var typeDesc = focusLevel switch
        {
            "high" => "PlayStation-focused gaming lounge",
            "vr" => "VR gaming experience center",
            "mixed" => "Multi-platform gaming venue",
            _ => "Gaming venue"
        };

        sb.Append($"{typeDesc} in {city}");
        if (!string.IsNullOrWhiteSpace(district))
            sb.Append($", {district}");
        sb.Append(". ");

        if (!string.IsNullOrWhiteSpace(notes))
        {
            sb.Append(notes.TrimEnd('.', ';', ' '));
            sb.Append('.');
        }

        return sb.ToString();
    }

    private static TimeOnly? ParseTime(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return TimeOnly.TryParseExact(value, ["HH:mm", "H:mm"], CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var time)
            ? time
            : null;
    }

    private static string[] GenerateAmenities(string focusLevel, string notes)
    {
        var amenities = new List<string> { "WiFi" };
        var n = (notes ?? "").ToLowerInvariant();

        if (n.Contains("vip")) amenities.Add("VIP Room");
        if (n.Contains("billiard") || n.Contains("pool")) amenities.Add("Billiards");
        if (n.Contains("cafe") || n.Contains("snack") || n.Contains("slush") || n.Contains("burger"))
            amenities.Add("Snack Bar");
        if (n.Contains("racing") || n.Contains("wheel") || n.Contains("sim rig") || n.Contains("sim-racing"))
            amenities.Add("Racing Sim");
        if (n.Contains("hookah") || n.Contains("shisha")) amenities.Add("Hookah");
        if (n.Contains("cinema") || n.Contains("movie")) amenities.Add("Cinema Room");
        if (n.Contains("bowling")) amenities.Add("Bowling");
        if (n.Contains("board game")) amenities.Add("Board Games");
        if (n.Contains("table tennis")) amenities.Add("Table Tennis");
        if (n.Contains("table football")) amenities.Add("Table Football");
        if (focusLevel == "vr") amenities.Add("VR Zone");

        if (amenities.Count == 1)
            amenities.Add(focusLevel == "high" ? "PS5 Pods" : "Gaming Stations");

        return amenities.ToArray();
    }

    private static List<Station> GenerateStations(Guid venueId, string focusLevel, Random random)
    {
        var stations = new List<Station>();
        var psPrice = (decimal)random.Next(8, 13);
        var pcPrice = (decimal)random.Next(5, 10);

        switch (focusLevel)
        {
            case "high":
                for (var i = 1; i <= 4; i++)
                    stations.Add(CreateStation(venueId, $"PS5-{i:D2}", StationKind.Ps5, psPrice, 2,
                        ["PS5", "4K TV", "DualSense"]));
                stations.Add(CreateStation(venueId, "PC-01", StationKind.Pc, pcPrice, 1,
                    ["RTX 4070", "i7-14700K", "16GB RAM", "165Hz"]));
                stations.Add(CreateStation(venueId, "VR-01", StationKind.Vr, 15m, 1,
                    ["Quest 3", "Play Area 3x3m"]));
                break;

            case "vr":
                for (var i = 1; i <= 3; i++)
                    stations.Add(CreateStation(venueId, $"VR-{i:D2}", StationKind.Vr, 15m, 1,
                        ["Quest 3", "Play Area 3x3m"]));
                for (var i = 1; i <= 2; i++)
                    stations.Add(CreateStation(venueId, $"PS5-{i:D2}", StationKind.Ps5, psPrice, 2,
                        ["PS5", "4K TV", "DualSense"]));
                break;

            default: // mixed & unknown
                for (var i = 1; i <= 2; i++)
                    stations.Add(CreateStation(venueId, $"PS5-{i:D2}", StationKind.Ps5, psPrice, 2,
                        ["PS5", "4K TV", "DualSense"]));
                for (var i = 1; i <= 3; i++)
                    stations.Add(CreateStation(venueId, $"PC-{i:D2}", StationKind.Pc, pcPrice, 1,
                        ["RTX 4070", "i7-14700K", "16GB RAM", "165Hz"]));
                stations.Add(CreateStation(venueId, "XBOX-01", StationKind.Xbox, psPrice, 2,
                    ["Xbox Series X", "4K TV", "Elite Controller"]));
                stations.Add(CreateStation(venueId, "VR-01", StationKind.Vr, 15m, 1,
                    ["Quest 3", "Play Area 3x3m"]));
                break;
        }

        return stations;
    }

    private static Station CreateStation(Guid venueId, string label, StationKind kind,
        decimal price, int capacity, string[] specs) => new()
    {
        Id = Guid.NewGuid(),
        VenueId = venueId,
        Label = label,
        Kind = kind,
        PricePerHour = price,
        Capacity = capacity,
        Specs = specs,
        IsActive = true,
        CreatedAt = DateTimeOffset.UtcNow
    };
}