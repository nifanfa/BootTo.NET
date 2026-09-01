using System;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

Console.CursorVisible = false;
{
    Bitmap icon = new Bitmap(@"\Icon.png");
    Graphics g = CreateGraphics();
    g.Clear(Color.FromArgb(104, 42, 123));
    g.DrawImage(icon, (g.VisibleClipBounds.Width / 2) - (icon.Width / 2), (g.VisibleClipBounds.Height / 2) - (icon.Height / 2));
    await Task.Delay(1000);
    g.Clear(Color.Black);
}
Console.CursorVisible = true;

new System.Media.SoundPlayer(@"\dragon-studio-computer-startup-sound-effect.wav").PlaySync();

Console.WriteLine($"Hello world, Time: {DateTime.Now}");
unsafe
{
    byte[] time = Encoding.UTF8.GetBytes(DateTime.Now.ToString());
    printf("Hello world from printf, Time:%s\n"u8, time);
}

LanguageFeatureValidation.Run();

#if false
// Open ftp://admin:12345@127.0.0.1 in explorer
// Listen on port 21, data transfers use ports 50001–50005
FtpServer server = new FtpServer("admin", "12345")
{
    PassiveAddress = System.Net.IPAddress.Loopback
};
// This is non-blocking and runs in the background.
server.Start();
#endif

//Fall back to EfiMain
