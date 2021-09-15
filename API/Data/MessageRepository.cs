using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public MessageRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;

        }

        public void addGroup(Group group)
        {
            _context.Groups.Add(group);
        }

        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Connection> GetConnection(string connectionId)
        {
            return await _context.Connections.FindAsync(connectionId);
        }

        public async Task<Group> GetGroupForConnection(string connectionId)
        {
            return await _context.Groups
                .Include(c => c.Connections)
                .Where(c => c.Connections.Any(x => x.ConnectionId == connectionId))
                .FirstOrDefaultAsync();
        }

        //φερνει το μνμα με βαση το id του απο το table Message μαζι με τα relations Sender, Recipient
        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages
                            .Include(u => u.Sender)
                            .Include(u => u.Recipient)
                            .SingleOrDefaultAsync(x => x.Id == id);
        }

        //travaei apo to table Groups me relation to table Connections ta group sunomilias metaxu xristwn me vasi to groupname
        // (to groupname einai se alphavitiki seira ta onomata twn 2 user p exoun to group)
        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await _context.Groups
                .Include(x => x.Connections)
                .FirstOrDefaultAsync(x => x.Name == groupName);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            //etoimazei to query k thetei to order na nai me vasi to messageSent
            var query = _context.Messages
                .OrderByDescending(m => m.MessageSent)
                .AsQueryable();

            //αν το container ='inbox', φέρνει αυτά που του στελνουν
            //αν ειναι το container='outbox', φέρνει αυτα που έχει στείλει
            //αλλιως, φέρνει τα εισρχομενα οπως το πρωτο case, αλλά να μην εχουν διαβαστεί κιόλας
            query = messageParams.Container switch
            {
                //για τα εισερχομενα, πρεπει το username παραλήπτη (recipient) να ναι ισο με αυτό της παραμέτρου και να μην εχουν διαγραφεί απο την πλευρά του τα μηνύματα
                "Inbox" => query.Where(u => u.Recipient.UserName == messageParams.Username && u.RecipientDeleted == false),
                //για τα εξερχόμενα, πρεπει το username αυτου που στέλνει (sender) να ναι ισο με αυτό της παραμέτρου και να μην εχουν διαγραφεί απο την πλευρά του τα μηνύματα
                "Outbox" => query.Where(u => u.Sender.UserName == messageParams.Username && u.SenderDeleted == false),
                //αλλιως, φέρνει τα εισρχομενα οπως το πρωτο case, αλλά να μην εχουν διαβαστεί κιόλας
                _ => query.Where(u => u.Recipient.UserName == messageParams.Username && u.RecipientDeleted == false && u.DateRead == null)
            };

            //thetei to queryable na metatrapei se morfi MessageDto 
            var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);

            //sto createAsync tou PagedList εκτελείται το query, και περνάμε και pageNumber, pageSize
            //για να φέρει τα αποτελέσματα με βαση το offset και το limit που ορίζεται απο τα pageNumber, pageSize
            //sto Response που φέρνει το endpoit έχει τις παρακατω πληροφορίες στα Headers
            //p.x Headers -> pagination : {"currentPage":1,"itemsPerPage":10,"totalItems":1,"totalPages":1}
            return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
        {
            //Get conversation of the users
            //fernei ta mhnumata μαζι με τον sender και τον recipient και τις φωτογραφιες τους οπου, ο χρηστης ειναι παραλήπτης(recipient) και
            //τα εισερχομενα(recipient messages) δεν ειναι διαγραμενα και συγχρονως ο sender ειναι ο άλλος χρήστης
            // Ή
            //ο χρηστης ειναι o sender και  τα μηνυματα (εξερχομενα) που στελνει δεν ειναι διαγραμενα και ο παραλήπτης ειναι ο αλλος χρηστης.
            
            //ΚΟΙΝΩΣ ΟΛΑ ΤΑ ΜΝΜΤΑ ΜΕΤΑΞΥ ΤΩΝ ΔΥΟ ΧΡΗΣΤΩΝ ΠΟΥ ΔΕΝ ΕΧΕΙ ΔΙΑΓΡΑΨΕΙ Ο CURENTUSER
            var messages = await _context.Messages
                .Include(u => u.Sender).ThenInclude(p => p.Photos)
                .Include(u => u.Recipient).ThenInclude(p => p.Photos)
                .Where(m => m.Recipient.UserName == currentUsername && m.RecipientDeleted == false
                && m.Sender.UserName == recipientUsername
                || m.Recipient.UserName == recipientUsername
                && m.Sender.UserName == currentUsername && m.SenderDeleted == false
                )
                .OrderBy(m => m.MessageSent)
                .ToListAsync();

            //Get unread messages for the current user that received
            //κραταει ολα τα αδιαβαστα μηνυματα που ειναι παραληπτης ο CurrentUser
            var unreadMessages = messages.Where(m => m.DateRead== null
                && m.Recipient.UserName == currentUsername).ToList();

            //we mark them as read
            //kai ta μαρκαρει πλεον σαν διαβασμενα με ημερομηνια την τωρινη
            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.DateRead = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
            }

            // we return the messageDtos
            return _mapper.Map<IEnumerable<MessageDto>>(messages);
        }

        public void RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}