using EnBref.Application.Features;
using Quartz;
using MediatR;

namespace EnBref.Infrastructure.ScheduledJobs;

public class GenerateDailyRecapJob(ISender mediator) : IJob
{
    public static readonly JobKey Key = new ("generate-recap", "en-bref");
    
    public async Task Execute(IJobExecutionContext context)
    {
        await Console.Out.WriteLineAsync($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} Generate Recap Job is running.");
        var request = new GenerateDailyRecap.Request();
        var response = await mediator.Send(request);

        if(response.IsSuccess)
        {
            await Console.Out.WriteLineAsync($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} Recap generated for {response.Date}");
        }
        else
        {
            await Console.Out.WriteLineAsync($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} Recap generation failed.");
        }
        await Console.Out.WriteLineAsync($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} Generate Recap Job completed.");
    }
}