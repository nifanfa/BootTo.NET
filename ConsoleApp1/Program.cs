using System;
using System.Text;

new System.Media.SoundPlayer(@"\dragon-studio-computer-startup-sound-effect.wav").PlaySync();

Console.WriteLine($"Hello world, Time: {DateTime.Now}");
unsafe
{
    byte[] time = Encoding.UTF8.GetBytes(DateTime.Now.ToString());
    printf("Hello world from printf, Time:%s\r\n"u8, time);
}

LanguageFeatureValidation.Run();

//Fall back to EfiMain
