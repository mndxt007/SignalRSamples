using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetFrameClientCoreSignalR
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var url = "https://localhost:7124";
            var proxy = await ConnectAsync(url + "/chat", Console.Out);
            var currentUser = Guid.NewGuid().ToString("N");

            Mode mode = Mode.Broadcast;
            if (args.Length > 0)
            {
                Enum.TryParse(args[0], true, out mode);
            }

            Console.WriteLine($"Logged in as user {currentUser}");
            var input = Console.ReadLine();
            while (!string.IsNullOrEmpty(input))
            {
                switch (mode)
                {
                    case Mode.Broadcast:
                        await proxy.InvokeAsync("Send", currentUser, input);
                        break;
                    case Mode.Echo:
                        await proxy.InvokeAsync("echo", input);
                        break;
                    default:
                        break;
                }

                input = Console.ReadLine();
            }
        }
        private static async Task<HubConnection> ConnectAsync(string url, TextWriter output, CancellationToken cancellationToken = default)
        {
            var connection = new HubConnectionBuilder()
                .WithUrl(url)
                  .ConfigureLogging(logging =>
                  {
                      // Log to the Console
                      logging.AddConsole();

                      // This will set ALL logging to Debug level
                      logging.SetMinimumLevel(LogLevel.Information);
                  })
                //.WithAutomaticReconnect(new AlwaysRetryPolicy())
                .Build();

            connection.On<string, string>("broadcastMessage", BroadcastMessage);
            connection.On<string>("Echo", Echo);
            connection.Closed += error =>
            {
                Debug.Assert(connection.State == HubConnectionState.Disconnected);

                Console.WriteLine($"disconnected, Error - {error?.ToString()}");

                return Task.CompletedTask;
            };

            await StartAsyncWithRetry(connection, output, cancellationToken);

            return connection;
        }

        private static async Task StartAsyncWithRetry(HubConnection connection, TextWriter output, CancellationToken cancellationToken)
        {
            // while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await connection.StartAsync(cancellationToken);
                    return;
                }
                catch (Exception e)
                {
                    output.WriteLine($"Error starting: {e.Message}, retry...");
                    await Task.Delay(GetRandomDelayMilliseconds());
                }
            }
        }

        private sealed class AlwaysRetryPolicy : IRetryPolicy
        {
            public TimeSpan? NextRetryDelay(RetryContext retryContext)
            {
                return TimeSpan.FromMilliseconds(GetRandomDelayMilliseconds());
            }
        }

        private static int GetRandomDelayMilliseconds()
        {
            return new Random().Next(1000, 2000);
        }

        private static void BroadcastMessage(string name, string message)
        {
            Console.WriteLine($"{name}: {message}");
        }

        private static void Echo(string message)
        {
            Console.WriteLine(message);
        }

        private enum Mode
        {
            Broadcast,
            Echo,
        }
    }
}
