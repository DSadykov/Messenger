using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.Core.Models;
public class MessageModel
{
    public Guid Id { get; set; }
    public string Message { get; set; }
    public string Username { get; set; }
    public string ReceiverUsername { get; set; }
    public DateTime DateSent { get; set; }

}
