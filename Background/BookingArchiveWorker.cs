
using Microsoft.Extensions.Hosting;
using RoomBookingSystem.Services;

namespace RoomBookingSystem.Background
{
    public class BookingArchiveWorker : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(15);
        public BookingArchiveWorker(IServiceProvider sp) => _sp = sp;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _sp.CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<IBookingService>();
                try { await svc.ArchivePastBookingsAsync(); await svc.GenerateRecurringInstancesAsync(); }
                catch { }
                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}
