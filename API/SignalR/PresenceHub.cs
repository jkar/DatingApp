using System;
using System.Threading.Tasks;
using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    [Authorize]
    public class PresenceHub : Hub
    {
        public readonly PresenceTracker _tracker;
        public PresenceHub(PresenceTracker tracker)
        {
            _tracker = tracker;
        }

        //vlepei an o xristis einai online kai enimerwnei sto front olous toys allous xristes oti einai autos online
        //kai travaei tous online users k enimerwnei sto front auton ton xristi gia tous upoloipous online users
        //parallila vazei sto dictionary to connectionId gia tous onlineusers gia na exoun eikona tou oi alloi
        public override async Task OnConnectedAsync()
        {
            //tsekarei an o xristis einai online
            var isOnline = await _tracker.UserConnected(Context.User.GetUsername(), Context.ConnectionId);
            //an einai online stelnei s olous tous allous xristes enimerwsi sto front oti einai online gia na kanoun update tous online-users
            if (isOnline)
            {
                await Clients.Others.SendAsync("UserIsOnLine", Context.User.GetUsername());
               
            }
            //fernei olous tous online-users apo ton tracker (dictionary me online-uers, key -> username  - value List -> connections in groups)
            var currentUsers = await _tracker.GetOnlineUsers();
            // await Clients.All.SendAsync("GetOnlineUsers", currentUsers);
            
            //mono ston caller gurnaei ti lista me tou energous xrhstes kai oxi se olous
            await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);
        }

        //vlepei an o xristis einai offline kai enimerwnei sto front olous toys allous xristes oti einai autos offline
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            //tsekarei an o xristis einai online
            var isOffLine = await _tracker.UserDissconnected(Context.User.GetUsername(), Context.ConnectionId);
            //an einai offline stelnei s olous tous allous xristes enimerwsi sto front oti einai offline gia na kanoun update tous online-users
            if (isOffLine)
            {
                await Clients.Others.SendAsync("UserIsOffLine", Context.User.GetUsername());
            }

            //an uparxei provlima the petaxei exception
            await base.OnDisconnectedAsync(exception);
        }
    }
}