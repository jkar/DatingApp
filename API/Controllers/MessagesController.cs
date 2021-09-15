using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class MessagesController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        public readonly IMessageRepository _messageRepository;
        public readonly IMapper _mapper;
        public MessagesController(IUserRepository userRepository, IMessageRepository messageRepository, IMapper mapper)
        {
            _mapper = mapper;
            _messageRepository = messageRepository;
            _userRepository = userRepository;
        }

        // [HttpPost]
        // public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        // {
            // var username = User.GetUsername();

            // if (username == createMessageDto.RecipientUsername.ToLower())
            // {
            //     return BadRequest("You cannot send messages to yourself");
            // }

            // var sender = await _userRepository.GetUserByUsernameAsync(username);
            // var recipient = await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            // if (recipient == null) return NotFound();

            // var message = new Message
            // {
            //     Sender = sender,
            //     Recipient = recipient,
            //     SenderUsername = sender.UserName,
            //     RecipientUsername = recipient.UserName,
            //     Content = createMessageDto.Content
            // };

            // _messageRepository.AddMessage(message);

            // //kanei mapping apo to message (type Message) se MessageDto object
            // if (await _messageRepository.SaveAllAsync()) return Ok(_mapper.Map<MessageDto>(message));

            // return BadRequest("Failed to send message");
        // }

        //queryParams  = pageNumber, pageSize , container(π.χ 'Unread')
        //φερνει τα μηνυματα που σχετίζεται ο χρήστης.
        //αν το container ='inbox', φέρνει αυτά που του στελνουν
        //αν ειναι το container='outbox', φέρνει αυτα που έχει στείλει
        //αλλιως, φέρνει τα εισρχομενα οπως το πρωτο case, αλλά να μην εχουν διαβαστεί κιόλας
        //και τα φερνει και σε μορφη pagination
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
        {
            //fernei to username apo to token-Claims(extension)
            messageParams.Username = User.GetUsername();

            //
            var messages = await _messageRepository.GetMessagesForUser(messageParams);

            //pairnei apo ta query params ta pageNumber, pageSize kai gyrnaei sto response 
            //Π.Χ //Headers -> pagination : {"currentPage":1,"itemsPerPage":10,"totalItems":1,"totalPages":1}
            Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages);

            return messages;
        }

        //to username tou xristi pou thelei na epikoinwnhsei (paraliptis-recipient)
        [HttpGet("thread/{username}")]
        //Φερνει ολα τα μνμντα μεταξυ των 2 χρηστων που δεν εχει διαγραψει ο currentUser
        //και κάνει τα unread messages read με σημερινη-τωρινη ημερομηνία
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
        {
            //fernei to username apo to token-Claims(extension)
            var currentUsername = User.GetUsername();

            return Ok(await _messageRepository.GetMessageThread(currentUsername, username));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            //fernei to username apo to token-Claims(extension)
            var username = User.GetUsername();

            //fernei to mnm me vasi to id tou, mazi me ta relations Sender, Recipient
            var message = await _messageRepository.GetMessage(id);

            //an to username den exei sxesi me ton sender i ton recipient tote den borei na to diagrapsei autos o xristis
            if (message.Sender.UserName != username && message.Recipient.UserName != username) return Unauthorized();

            //an to diagrafei autos pou to steile
            if (message.Sender.UserName == username)
            {
                message.SenderDeleted = true;
            }

            //an to diagrafei aytos pou to eleave
            if (message.Recipient.UserName == username)
            {
                message.RecipientDeleted = true;
            }

            //an to diagrafoun kai oi duo
            if (message.SenderDeleted && message.RecipientDeleted)
            {
                _messageRepository.DeleteMessage(message);
            }

            if (await _messageRepository.SaveAllAsync()) return Ok();

            return BadRequest("Problem deleting the message");

        }
    }
}