
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RoomBookingSystem.Models;

namespace RoomBookingSystem.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            var userMgr = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleMgr = services.GetRequiredService<RoleManager<IdentityRole>>();
            var db = services.GetRequiredService<BookingDbContext>();

            // roles
            string[] roles = new[] { "Admin", "User" };
            foreach (var r in roles)
            {
                if (!await roleMgr.RoleExistsAsync(r))
                    await roleMgr.CreateAsync(new IdentityRole(r));
            }

            // admin
            var adminEmail = "admin@local.com";
            var admin = await userMgr.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                await userMgr.CreateAsync(admin, "Admin@12345");
                await userMgr.AddToRoleAsync(admin, "Admin");
            }

            // normal user
            var userEmail = "user@local.com";
            var user = await userMgr.FindByEmailAsync(userEmail);
            if (user == null)
            {
                user = new ApplicationUser { UserName = userEmail, Email = userEmail, EmailConfirmed = true };
                await userMgr.CreateAsync(user, "User@12345");
                await userMgr.AddToRoleAsync(user, "User");
            }

            // Rooms seed if empty
            if (!await db.Rooms.AnyAsync())
            {
                db.Rooms.AddRange(new Room { Name = "Classroom A", Capacity = 30 },
                                  new Room { Name = "Classroom B", Capacity = 50 },
                                  new Room { Name = "Boardroom", Capacity = 20, IsBoardroom = true });
                await db.SaveChangesAsync();
            }

            // Sample bookings
            if (!await db.Bookings.AnyAsync())
            {
                var classroomA = await db.Rooms.FirstAsync(r => r.Name == "Classroom A");
                var boardroom = await db.Rooms.FirstAsync(r => r.Name == "Boardroom");

                var nowLocal = DateTime.Now;
                var start = new DateTime(nowLocal.Year, nowLocal.Month, nowLocal.Day, 10, 0, 0);
                var end = start.AddHours(1);

                db.Bookings.Add(new Booking {
                    RoomId = classroomA.Id,
                    UserId = user.Id,
                    Title = "Demo: Lecture",
                    StartTime = start.ToUniversalTime(),
                    EndTime = end.ToUniversalTime(),
                    Status = BookingStatus.Confirmed
                });

                // recurring sample: weekly on next Monday 14:00 - show recurrence as 'Weekly'
                var nextMon = DateTime.Today.AddDays(((int)DayOfWeek.Monday - (int)DateTime.Today.DayOfWeek + 7) % 7);
                var recStart = new DateTime(nextMon.Year, nextMon.Month, nextMon.Day, 14, 0, 0);
                db.Bookings.Add(new Booking {
                    RoomId = boardroom.Id,
                    UserId = user.Id,
                    Title = "Weekly Board Meeting",
                    StartTime = recStart.ToUniversalTime(),
                    EndTime = recStart.AddHours(1).ToUniversalTime(),
                    Recurrence = "Weekly",
                    RecurrenceEnd = DateTime.UtcNow.AddMonths(3),
                    Status = BookingStatus.Confirmed
                });

                await db.SaveChangesAsync();
            }
        }
    }
}
