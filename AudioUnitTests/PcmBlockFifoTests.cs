using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public sealed class PcmBlockFifoTests
{
    [TestMethod]
    public void EnqueueAndDequeuePreservesOrderAndByteCount()
    {
        WaveOutAudio.PcmBlockFifo<int> fifo = new WaveOutAudio.PcmBlockFifo<int>();

        fifo.Enqueue(10, 4);
        fifo.Enqueue(20, 8);
        fifo.Enqueue(30, 16);

        Assert.AreEqual(3, fifo.Count);
        Assert.AreEqual(28, fifo.ByteCount);
        Assert.AreEqual(4, fifo.GetLengthAt(0));
        Assert.AreEqual(8, fifo.GetLengthAt(1));
        Assert.AreEqual(16, fifo.GetLengthAt(2));

        Assert.IsTrue(fifo.TryDequeue(out int first, out int firstLength));
        Assert.AreEqual(10, first);
        Assert.AreEqual(4, firstLength);
        Assert.AreEqual(24, fifo.ByteCount);

        Assert.IsTrue(fifo.TryDequeue(out int second, out int secondLength));
        Assert.AreEqual(20, second);
        Assert.AreEqual(8, secondLength);
        Assert.IsTrue(fifo.TryDequeue(out int third, out int thirdLength));
        Assert.AreEqual(30, third);
        Assert.AreEqual(16, thirdLength);
        Assert.AreEqual(0, fifo.Count);
        Assert.AreEqual(0, fifo.ByteCount);
    }

    [TestMethod]
    public void EmptyDequeueDoesNotProduceAPhantomBlock()
    {
        WaveOutAudio.PcmBlockFifo<int> fifo = new WaveOutAudio.PcmBlockFifo<int>();

        Assert.IsFalse(fifo.TryDequeue(out int value, out int length));
        Assert.AreEqual(0, value);
        Assert.AreEqual(0, length);
        Assert.AreEqual(0, fifo.ByteCount);
    }

    [TestMethod]
    public void ClearReleasesEveryQueuedBlock()
    {
        WaveOutAudio.PcmBlockFifo<int> fifo = new WaveOutAudio.PcmBlockFifo<int>();
        fifo.Enqueue(1, 2);
        fifo.Enqueue(2, 4);
        fifo.Enqueue(3, 8);
        List<int> released = new List<int>();

        fifo.Clear(released.Add);

        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, released);
        Assert.AreEqual(0, fifo.Count);
        Assert.AreEqual(0, fifo.ByteCount);
    }

    [TestMethod]
    public void InvalidBlockLengthIsRejected()
    {
        WaveOutAudio.PcmBlockFifo<int> fifo = new WaveOutAudio.PcmBlockFifo<int>();

        Assert.ThrowsExactly<ArgumentException>(() => fifo.Enqueue(1, 0));
        Assert.ThrowsExactly<ArgumentException>(() => fifo.Enqueue(1, -1));
    }
}
