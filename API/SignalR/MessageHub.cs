using System;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    public class MessageHub : Hub
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly IHubContext<PresenceHub> _presenceHub;
        private readonly PresenceTracker _tracker;
        public MessageHub(IMessageRepository messageRepository,
                          IMapper mapper,
                          IUserRepository userRepository,
                          IHubContext<PresenceHub> presenceHub,
                          PresenceTracker tracker)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _messageRepository = messageRepository;
            _presenceHub = presenceHub;
            _tracker = tracker;
        }

    //δημιουργεί το group μεταξυ των δυο χρηστων αν δεν υπαρχει, δημιουργει το connection που σχετιζεται με το group, kai ενημερωνει το front για το group και για τα μνμτα μεταξυ των δυο
    //ενημερωνει τον αλλο user με το "UpdatedGroup" kai sto front tou αλλου χρηστη κανει τα μνμντα read γιατι πλεον με το OnConnectedAsync, ο χρηστης ειναι στη συνομιλια.
    //ενημερωνει και την βαση στο table Message ότι τα μνμτα που χει στείλει ο άλλος χρήστης είναι read για΄τι ο current μπαίνει στην συνομιλια με το OnConnectedAsync
    public override async Task OnConnectedAsync()
    {
        //apo to HubCallerContext, fernei to httpContext 
        var httpContext = Context.GetHttpContext();
        //apo to request, sto query yparxei to variable user, to travaei k exei pleon to onoma tou xristi (recipient) pou thelei na tou steilei o sender
        var otherUser = httpContext.Request.Query["user"].ToString();
        //dhmiourgei to onoma tou group me vasi ta onomata twn sender-recipient
        var groupName = this.GetGroupName(Context.User.GetUsername(), otherUser);
        //dhmiourgei to group sti vasi me vasi to parapanw onoma  kai to connectionId
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        // ενημερωνει την βάση οτι ο user ειναι μεσα στην συνομιλία (group) τώρα με την εγγραφη στο connection table
        var group = await AddToGroup(groupName);
        //stelnei sto front enimerwsi gia to neo group (sunomilia) (στο front, αν ο αλλος χρηστης ειναι ηδη στην συνομιλια, κανει τα μνμντα read)
        await Clients.Group(groupName).SendAsync("UpdatedGroup", group);
        //φερνει ΟΛΑ ΤΑ ΜΝΜΤΑ ΜΕΤΑΞΥ ΤΩΝ ΔΥΟ ΧΡΗΣΤΩΝ ΠΟΥ ΔΕΝ ΕΧΕΙ ΔΙΑΓΡΑΨΕΙ Ο CURENTUSER
        //και συγχρόνως στo table Messages κανει ta unread se read (όσα σχετίζονται μεταξύ τους και ο currentUser είναι recipient).
        var messages = await _messageRepository.GetMessageThread(Context.User.GetUsername(), otherUser);
        //τα στέλνει στο front του χρηστη (aytou που συνδεθηκε κ ειναι το token του) για να ενημρωσει τα μνμτα που φαινονται
        await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
    }

    //vriskei k diagrafei to connection row tou xristi pou sxetizetai me to group, telos enimerwnei to front gia ti diagrafi tou connection apo to related group
    public override async Task OnDisconnectedAsync(System.Exception exception)
    {
        //vriskei to connection row tou xristi me vasi to connectionid apo to group kai svinei to connection
        var group = await RemoveFromMessageGroup();
        //ενημερώνει το front για τη διαγραφή του connection sto group
        await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);
        //an uparxei provlima, petaei exception
        await base.OnDisconnectedAsync(exception);
    }

    //η μεθοδος βρισκει το username απο to HubCaller Context kai mesw autou ton xristi, mesw createMessageDto vriskei k ton recipient
    //vriskei to group apo ta username tous, tsekarei an o recipient einai stin sunomilia gia na thesei to message read, an den einai
    //alla einai online, apla ton enimerwnei oti irthe mnma, telos swzei to mnma sti vasi k enimerwnei to front me to mnma gia na kanei
    //to front update ta messages
    public async Task sendMessage(CreateMessageDto createMessageDto)
    {
        //travaei to username mesa apo to HubCaller Context
        var username = Context.User.GetUsername();

        //checkarei an to username einai idio me auto tis parametrou, gia na ton apotrepsi na steilei mnm ston eayto tou
        if (username == createMessageDto.RecipientUsername.ToLower())
        {
            throw new HubException("You cannot send messages to yourself");
        }

        //vriskei xristi (sender autos p stelnei k to mnm)  me vasi to username 
        var sender = await _userRepository.GetUserByUsernameAsync(username);
        //vriskei xisti (recipient) me vasi to username stin parametro
        var recipient = await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

        if (recipient == null) throw new HubException("Not found user");

        //etoimazei to mmm pou tha perastei stin vasi sto table Message
        var message = new Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender.UserName,
            RecipientUsername = recipient.UserName,
            Content = createMessageDto.Content
        };

        //H method GetGroupName thetei to onoma tou group, edw ti xrhsimopoioume gia na match-aroume to onoma pou perimenoume na vroume stin vasi
        var groupName = GetGroupName(sender.UserName, recipient.UserName);

        //travaei apo to table Groups me relation to table Connections to group sunomilias metaxu xristwn me vasi to groupname
        // (to groupname einai se alphavitiki seira ta onomata twn 2 user p exoun to group p.x lisa-todd)
        var group = await _messageRepository.GetMessageGroup(groupName);

        //τσεκαρει στο table Connections αν ο recipent είναι μέσα στην συνομιλία (otan einai sti sunomilia, στο connections έχει εγγραφή (username, GroupName, connectionId)
        //kai uparxei foreign key to groupname).
        //an uparxei tote ,epd o recipient vlepei ta mnmta, to message.DateRead pairnei timi
        if (group.Connections.Any(x => x.Username == recipient.UserName))
        {
            message.DateRead = DateTime.UtcNow;
        }
        //alliws, me ton _tracker (PresenceTracker) tsekarei an o recipient einai online
        //an einai, stelnei sto front tou recipient mnm oti exei erthei neo mnm gia na ton enimerwsei (pou einai online alla den einai mesa stin sunomilia tous)
        else
        {
            var connections = await _tracker.GetConnectionsForUser(recipient.UserName);
            if (connections != null)
            {
                await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived",
                new {
                    username = sender.UserName, knownAs = sender.KnownAs
                });
            }
        }

        //Kanei tracking sto table Message to neo munhma
        _messageRepository.AddMessage(message);

        //swzei tis allages sti vasi kai
        //kanei mapping apo to message (type Message) se MessageDto object
        if (await _messageRepository.SaveAllAsync())
        {
            //enimerwnei to front twn clients pou einai s auto to group (sender-recipient) oti irthe neo mnm gia na enimewsei ta mnmta sto front
            //()
            await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
        }
    }

    //me vasi to groupName psaxnei to group sti vasi k an den yparxei to ftiaxnei, etoimazei to neo connection me vasi to connectionId kai to username
    //(to groupName fantazomai to pernaei automata sti vasi epd exei to relationship me to group)
    //swzei to neo connection sti vasi mesa apo to group

    //KOINWS ενημερωνει την βάση οτι ο user ειναι μεσα στην συνομιλία (group) τώρα με την εγγραφη στο connection table 
    private async Task<Group> AddToGroup(string groupName)
    {
        //fernei to group nesw tou groupName (parameter) kai me to relation fernei k ta connections
        var group = await _messageRepository.GetMessageGroup(groupName);

        //etoimazei to neo connection
        var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());

        //an den yparxei idi group, to dimiourgei kai to kanei tracking 
        if (group == null)
        {
            group = new Group(groupName);
            _messageRepository.addGroup(group);
        }

        //kanei track to neo connection mesw tou group k tou relationship p exei me to coonection
        group.Connections.Add(connection);

        //swzei tin allagi sti vasi kai epistrefei to group
        if (await _messageRepository.SaveAllAsync()) return group;

        throw new HubException("Failed to join group");
    }

    //vriskei to connection row tou xristi me vasi to connectionid apo to group
    //kai svinei to connection 
    private async Task<Group> RemoveFromMessageGroup()
    {
        //fernei to group me vasi to connectionId to opoio einai sto relation me ta connnections
        var group = await _messageRepository.GetGroupForConnection(Context.ConnectionId);

        //fernei to connection me vasi to connectionId 
        var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
        //kanei tracking sto table Connection gia na svisei to connection
        _messageRepository.RemoveConnection(connection);
        //swzei tin allagi sti vasi
        if (await _messageRepository.SaveAllAsync()) return group;

        throw new HubException("Failed to remove from group");
    }

    //Onomazw to group me alphabhtikh seira
    private string GetGroupName(string caller, string other)
    {
        var stringCompare = string.CompareOrdinal(caller, other) < 0;
        return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }

}
}