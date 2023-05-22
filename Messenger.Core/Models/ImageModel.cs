using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.Core.Models;
public class ImageModel
{
    public Guid Id { get; set; }
    public Guid MessageId { get; set; } 
    public string ImageBase64 { get; set; }
}
