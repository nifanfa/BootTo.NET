using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

[TestClass]
public sealed class PcmInputConverterTests
{
    [TestMethod]
    public void Stereo44100IsOneOutputFramePerInputFrame()
    {
        byte[] input = MakeStereoInput(1000);
        WaveOutAudio.PcmInputConverter converter = new WaveOutAudio.PcmInputConverter();
        TestSink sink = new TestSink();

        int consumed = converter.Append(input, 0, input.Length, 2, 44100, sink);
        converter.Flush(sink);

        Assert.AreEqual(input.Length, consumed);
        Assert.AreEqual(1000, sink.Frames.Count);
        Assert.IsFalse(converter.HasPendingInput);
        Assert.AreEqual(ReadInt16(input, 0), sink.Frames[0].Left);
        Assert.AreEqual(ReadInt16(input, 2), sink.Frames[0].Right);
        Assert.AreEqual(ReadInt16(input, input.Length - 4), sink.Frames[999].Left);
        Assert.AreEqual(ReadInt16(input, input.Length - 2), sink.Frames[999].Right);
    }

    [TestMethod]
    public void MonoInputIsDuplicatedWithoutDroppingFrames()
    {
        byte[] input = MakeMonoInput(1000);
        WaveOutAudio.PcmInputConverter converter = new WaveOutAudio.PcmInputConverter();
        TestSink sink = new TestSink();

        int consumed = converter.Append(input, 0, input.Length, 1, 44100, sink);
        converter.Flush(sink);

        Assert.AreEqual(input.Length, consumed);
        Assert.AreEqual(1000, sink.Frames.Count);
        for (int index = 0; index < sink.Frames.Count; index++)
        {
            Assert.AreEqual(sink.Frames[index].Left, sink.Frames[index].Right);
            Assert.AreEqual(ReadInt16(input, index * 2), sink.Frames[index].Left);
        }
    }

    [TestMethod]
    public void ChunkedWritesWithShortOutputCapacityPreserveTheSameSequence()
    {
        byte[] input = MakeStereoInput(12000);
        WaveOutAudio.PcmInputConverter oneShot = new WaveOutAudio.PcmInputConverter();
        TestSink expectedSink = new TestSink();
        Assert.AreEqual(input.Length, oneShot.Append(input, 0, input.Length, 2, 44100, expectedSink));
        oneShot.Flush(expectedSink);

        WaveOutAudio.PcmInputConverter chunked = new WaveOutAudio.PcmInputConverter();
        TestSink actualSink = new TestSink();
        int offset = 0;
        while (offset < input.Length)
        {
            int request = Math.Min(16 * 1024, input.Length - offset);
            request -= request % 4;
            int consumed = 0;
            while (consumed < request)
            {
                actualSink.Remaining = 17;
                int progress = chunked.Append(input, offset + consumed, request - consumed, 2, 44100, actualSink);
                Assert.IsTrue(progress > 0 || actualSink.Remaining == 0);
                consumed += progress;
            }
            offset += consumed;
        }
        actualSink.Remaining = int.MaxValue;
        chunked.Flush(actualSink);

        Assert.AreEqual(expectedSink.Frames.Count, actualSink.Frames.Count);
        for (int index = 0; index < expectedSink.Frames.Count; index++)
        {
            Assert.AreEqual(expectedSink.Frames[index].Left, actualSink.Frames[index].Left, $"left frame {index}");
            Assert.AreEqual(expectedSink.Frames[index].Right, actualSink.Frames[index].Right, $"right frame {index}");
        }
    }

    [TestMethod]
    public void ResamplingConsumesEverySourceFrameAndProducesExpected44100Count()
    {
        const int sourceFrames = 4800;
        byte[] input = MakeMonoInput(sourceFrames);
        WaveOutAudio.PcmInputConverter converter = new WaveOutAudio.PcmInputConverter();
        TestSink sink = new TestSink();

        int consumed = converter.Append(input, 0, input.Length, 1, 48000, sink);
        converter.Flush(sink);

        Assert.AreEqual(input.Length, consumed);
        Assert.AreEqual(4410, sink.Frames.Count);
        Assert.IsFalse(converter.HasPendingInput);
    }

    private static byte[] MakeMonoInput(int frameCount)
    {
        byte[] result = new byte[frameCount * 2];
        for (int index = 0; index < frameCount; index++)
        {
            short value = (short)(index - frameCount / 2);
            result[index * 2] = (byte)value;
            result[index * 2 + 1] = (byte)(value >> 8);
        }
        return result;
    }

    private static byte[] MakeStereoInput(int frameCount)
    {
        byte[] result = new byte[frameCount * 4];
        for (int index = 0; index < frameCount; index++)
        {
            short left = (short)(index - frameCount / 2);
            short right = (short)(frameCount / 2 - index);
            result[index * 4] = (byte)left;
            result[index * 4 + 1] = (byte)(left >> 8);
            result[index * 4 + 2] = (byte)right;
            result[index * 4 + 3] = (byte)(right >> 8);
        }
        return result;
    }

    private static short ReadInt16(byte[] buffer, int offset)
    {
        return (short)(buffer[offset] | (buffer[offset + 1] << 8));
    }

    private sealed class TestSink : IPcmSampleSink
    {
        internal readonly List<Frame> Frames = new List<Frame>();
        internal int Remaining = int.MaxValue;

        public bool TryWriteSample(short left, short right)
        {
            if (Remaining == 0)
                return false;
            if (Remaining != int.MaxValue)
                Remaining--;
            Frames.Add(new Frame(left, right));
            return true;
        }
    }

    private readonly struct Frame
    {
        internal readonly short Left;
        internal readonly short Right;

        internal Frame(short left, short right)
        {
            Left = left;
            Right = right;
        }
    }
}
