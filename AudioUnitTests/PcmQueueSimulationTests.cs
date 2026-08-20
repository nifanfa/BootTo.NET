using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

[TestClass]
public sealed class PcmQueueSimulationTests
{
    [TestMethod]
    public void BackpressuredFifoRetiresEvery44100HzInputFrame()
    {
        const int inputFrames = 12000;
        byte[] input = new byte[inputFrames * 2];
        for (int index = 0; index < inputFrames; index++)
        {
            short sample = (short)(index * 3);
            input[index * 2] = (byte)sample;
            input[index * 2 + 1] = (byte)(sample >> 8);
        }

        SimulatedStream stream = new SimulatedStream();
        int offset = 0;
        while (offset < input.Length)
        {
            int count = Math.Min(200, input.Length - offset);
            count -= count % 2;
            stream.Write(input, offset, count);
            offset += count;
        }

        stream.Flush();
        while (stream.HasWork)
            stream.Pump();

        Assert.AreEqual(inputFrames, stream.OutputFrames);
        Assert.AreEqual(0, stream.BufferedByteCount);
    }

    private sealed class SimulatedStream : IPcmSampleSink
    {
        private sealed class Block
        {
            internal readonly byte[] Data;
            internal int Offset;

            internal Block(byte[] data) => Data = data;
        }

        private readonly Queue<Block> input = new Queue<Block>();
        private readonly WaveOutAudio.PcmInputConverter converter = new WaveOutAudio.PcmInputConverter();
        private readonly byte[] native = new byte[2048];
        private readonly WaveOutAudio.PcmRingState ring = new WaveOutAudio.PcmRingState(2048, 512, 512);
        private int currentLength;
        private bool flushed;

        internal int OutputFrames { get; private set; }
        internal int BufferedByteCount => ring.BufferedByteCount;
        internal bool HasWork => currentLength != 0 || ring.BufferedByteCount != 0 || input.Count != 0 || converter.HasPendingInput;

        internal void Write(byte[] data, int offset, int count)
        {
            byte[] copy = new byte[count];
            Array.Copy(data, offset, copy, 0, count);
            input.Enqueue(new Block(copy));
            flushed = false;
            Drain();
            StartNext(false);
            while (currentLength != 0 && ring.WritableByteCount < 4)
                Pump();
        }

        internal void Flush()
        {
            Drain();
            if (input.Count == 0)
                converter.Flush(this);
            flushed = true;
            StartNext(true);
        }

        internal void Pump()
        {
            if (currentLength == 0)
                return;
            Assert.IsTrue(ring.CompleteCurrent(out int length));
            Assert.AreEqual(currentLength, length);
            currentLength = 0;
            Drain();
            if (flushed && input.Count == 0)
                converter.Flush(this);
            StartNext(flushed);
        }

        public bool TryWriteSample(short left, short right)
        {
            if (ring.WritableByteCount < 4)
                return false;
            int offset = ring.WriteOffset;
            native[offset] = (byte)left;
            native[offset + 1] = (byte)(left >> 8);
            native[offset + 2] = (byte)right;
            native[offset + 3] = (byte)(right >> 8);
            Assert.IsTrue(ring.TryCommitWrite(4));
            OutputFrames++;
            return true;
        }

        private void Drain()
        {
            while (input.Count != 0)
            {
                Block block = input.Peek();
                int consumed = converter.Append(block.Data, block.Offset, block.Data.Length - block.Offset, 1, 44100, this);
                if (consumed == 0)
                    return;
                block.Offset += consumed;
                if (block.Offset == block.Data.Length)
                    input.Dequeue();
            }
        }

        private void StartNext(bool allowShort)
        {
            if (currentLength != 0 || !ring.TryStartNext(allowShort, out _, out int length))
                return;
            currentLength = length;
        }
    }
}
