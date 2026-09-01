using System;
using System.Text;

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
