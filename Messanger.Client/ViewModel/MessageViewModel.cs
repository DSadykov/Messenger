using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

using Messenger.Core.Models;

namespace Messanger.Client.ViewModel
{
    public class MessageViewModel
    {
        public MessageModel Message { get; set; }
        public HorizontalAlignment HorizontalAlignment { get; set; }
        public Brush Background { get; set; }
    }
}
