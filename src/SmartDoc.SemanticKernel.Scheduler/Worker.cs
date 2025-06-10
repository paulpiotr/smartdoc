#region using

using System;
using System.Threading;
using System.Threading.Tasks;
using SmartDoc.SemanticKernel.Ingestion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

#endregion

namespace SmartDoc.SemanticKernel.Scheduler;

public class Worker(IServiceProvider services) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var nextRun = new DateTime(now.Year, now.Month, now.Day).AddDays(1);
            var delay = nextRun - now;
            await Task.Delay(delay, stoppingToken);

            using var scope = services.CreateScope();
            var ingestionService = scope.ServiceProvider.GetRequiredService<QdrantIngestionService>();
            // TODO: add logic to get list of files to ingest
            // await ingestionService.IngestAsync("path/to/new/files");
        }
    }
}