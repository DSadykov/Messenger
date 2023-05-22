using System.Windows;
using System.Windows.Media.Imaging;

using Messenger.Core.Models;

using Brush = System.Windows.Media.Brush;

namespace Messanger.Client.ViewModel;

public class MessageViewModel
{
    public required MessageModel Message { get; set; }
    public HorizontalAlignment HorizontalAlignment { get; set; }
    public required Brush Background { get; set; }
    public Visibility? ImageVisibility { get; set; } = Visibility.Collapsed;
    public required BitmapSource Image { get; set; }
}
