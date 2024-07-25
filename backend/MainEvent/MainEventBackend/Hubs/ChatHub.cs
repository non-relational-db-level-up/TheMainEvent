using Confluent.Kafka;
using MainEvent.DTO;
using MainEvent.Helpers;
using MainEvent.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace MainEvent.Hubs
{
    public class ChatHub : Hub
    {
        Itopic _topic;
        public ChatHub(Itopic topic)
        {
            _topic = topic;
        }

        [Authorize]
        public override async Task OnConnectedAsync()
        {
            
            await Clients.Caller.SendAsync("StartMessage", _topic);
            await base.OnConnectedAsync();

        }

        //[Authorize]
        //public async Task SendMessage(string user, string message)
        //{
        //    var data = new MessageDataDto(1, 1, "#f5aa42");

        //    string jsonString = JsonSerializer.Serialize(data);


        //    await Clients.All.SendAsync("ReceiveMessage", user, jsonString);
        //}
    }
}