using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Messenger.Core.Models;

namespace Messanger.Client.Services;

public class MessageService
{
    public required string Username { get; set; }

    internal void BeginListening()
    {
    }

    internal IEnumerable<MessageModel> RecieveMessages()
    {
        return new List<MessageModel>
        {
            new()
            {

                    Message="Pepega2",
                    DateSent=DateTime.Now,
                    Username="Me",
                    ReceiverUsername="You"

            },
            new()
            {

                    Message="Pepega3",
                    DateSent=DateTime.Now.AddMinutes(5),
                    Username="Me",

                    ReceiverUsername="You"

            },new()
            {

                    Message="Pepega4",
                    DateSent=DateTime.Now.AddMinutes(-2),
                    Username="Me",
                    ReceiverUsername="You"

            }
            ,new()
            {

                    Message="Pepega1",
                    DateSent=DateTime.Now.AddMinutes(1),
                    Username="You",
                    ReceiverUsername="Me"

            },new()
            {

                    Message="Pepega2",
                    DateSent=DateTime.Now.AddMinutes(-1),
                    Username="You",
                    ReceiverUsername="Me"

            },
        };


    }

    internal void SendMessage(string message)
    {
    }
}
