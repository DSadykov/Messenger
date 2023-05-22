using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Messenger.Core.Models;

using Brush = System.Windows.Media.Brush;

namespace Messanger.Client.ViewModel
{
    public class MessageViewModel
    {
        public MessageModel Message { get; set; }
        public HorizontalAlignment HorizontalAlignment { get; set; }
        public Brush Background { get; set; }
        public Visibility? ImageVisibility { get; set; } = Visibility.Collapsed;
        public BitmapSource Image { get; set; }
    }
}
