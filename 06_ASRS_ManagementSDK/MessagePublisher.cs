// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.SignalR.Management;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.SignalR.Samples.Management
{
    class NewMessage
    {
        public string ConnectionId { get; }
        public string Sender { get; }
        public string Text { get; }

        public NewMessage(string message)
        {
            Sender = "publisher";
            ConnectionId = "Dkfdggnmgmfg";
            Text = message;
        }
    }
    public class MessagePublisher
    {
        private const string Target = "broadcastMessage";
        private const string message = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
        private const string HubName = "simplechat";
        private readonly string _connectionString;
        private readonly ServiceTransportType _serviceTransportType;
        private ServiceHubContext _hubContext;

        public MessagePublisher(string connectionString, ServiceTransportType serviceTransportType)
        {
            _connectionString = connectionString;
            _serviceTransportType = serviceTransportType;
        }

        public async Task InitAsync()
        {
            var serviceManager = new ServiceManagerBuilder().WithOptions(option =>
            {
                option.ConnectionString = _connectionString;
                option.ServiceTransportType = _serviceTransportType;
            })
            //Uncomment the following line to get more logs
            //.WithLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()))
            .BuildServiceManager();

            _hubContext = await serviceManager.CreateHubContextAsync(HubName, default);
        }

        public Task ManageUserGroup(string command, string userId, string groupName)
        {
            switch (command)
            {
                case "add":
                    return _hubContext.UserGroups.AddToGroupAsync(userId, groupName);
                case "remove":
                    return _hubContext.UserGroups.RemoveFromGroupAsync(userId, groupName);
                default:
                    Console.WriteLine($"Can't recognize command {command}");
                    return Task.CompletedTask;
            }
        }

        public Task SendMessages(string command, string receiver, string message)
        {
            switch (command)
            {
                case "broadcast":
                    return _hubContext.Clients.All.SendAsync(Target, "publisher", message) ;
                case "user":
                    var userId = receiver;
                    return _hubContext.Clients.User(userId).SendAsync(Target, "publisher", message);
                case "users":
                    var userIds = receiver.Split(',');
                    return _hubContext.Clients.Users(userIds).SendAsync(Target, "publisher", message);
                case "group":
                    var groupName = receiver;
                    return _hubContext.Clients.Group(groupName).SendAsync(Target, "publisher", message);
                case "groups":
                    var groupNames = receiver.Split(',');
                    return _hubContext.Clients.Groups(groupNames).SendAsync(Target, "publisher", message);
                default:
                    Console.WriteLine($"Can't recognize command {command}");
                    return Task.CompletedTask;
            }
        }

        public async Task SlowClient(string connectionId)
        {
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 50 };

            await Parallel.ForEachAsync(Enumerable.Range(0, 1000), parallelOptions, async (i, token) =>
            {
                await _hubContext.Clients.All.SendAsync("newMessage", new NewMessage(message));
            });
        }

        public Task CloseConnection(string connectionId, string reason)
        {
            return _hubContext.ClientManager.CloseConnectionAsync(connectionId, reason);
        }

        public Task<bool> CheckExist(string type, string id)
        {
            return type switch
            {
                "connection" => _hubContext.ClientManager.ConnectionExistsAsync(id),
                "user" => _hubContext.ClientManager.UserExistsAsync(id),
                "group" => _hubContext.ClientManager.UserExistsAsync(id),
                _ => throw new NotSupportedException(),
            };
        }

        public Task DisposeAsync() => _hubContext?.DisposeAsync();
    }
}