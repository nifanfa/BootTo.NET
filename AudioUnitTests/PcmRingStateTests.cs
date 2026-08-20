using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public sealed class PcmRingStateTests
{
    [TestMethod]
    public void CallbackSizedSubmissionsRetireAllQueuedBytesExactlyOnce()
    {
        const int capacity = 512 * 1024;
        WaveOutAudio.PcmRingState ring = new WaveOutAudio.PcmRingState(
            capacity,
            WaveOutAudio.PcmRingState.DefaultSubmissionBytes,
            WaveOutAudio.PcmRingState.DefaultSubmissionBytes);

        int committed = 0;
        while (committed < capacity)
        {
            Assert.IsTrue(ring.TryCommitWrite(4));
            committed += 4;
        }

        int completed = 0;
        while (ring.BufferedByteCount > 0)
        {
            Assert.IsTrue(ring.TryStartNext(false, out _, out int length));
            Assert.AreEqual(WaveOutAudio.PcmRingState.DefaultSubmissionBytes, length);
            Assert.IsTrue(ring.CompleteCurrent(out int completedLength));
            Assert.AreEqual(length, completedLength);
            completed += completedLength;
        }

        Assert.AreEqual(committed, completed);
        Assert.AreEqual(0, ring.BufferedByteCount);
    }

    [TestMethod]
    public void LiveStreamWaitsForAFullBlockInsteadOfStartingAnUnderflowTail()
    {
        WaveOutAudio.PcmRingState ring = new WaveOutAudio.PcmRingState(16, 8, 16);

        Assert.IsTrue(ring.TryCommitWrite(4));
        Assert.IsFalse(ring.TryStartNext(false, out _, out _));

        Assert.IsTrue(ring.TryCommitWrite(4));
        Assert.IsTrue(ring.TryStartNext(false, out _, out int length));
        Assert.AreEqual(8, length);
    }

    [TestMethod]
    public void LiveStreamSubmitsAContiguousTailOnlyToReachWrappedBytes()
    {
        WaveOutAudio.PcmRingState ring = new WaveOutAudio.PcmRingState(16, 8, 16);

        Assert.IsTrue(ring.TryCommitWrite(12));
        Assert.IsTrue(ring.TryStartNext(true, out _, out int firstLength));
        Assert.AreEqual(12, firstLength);
        Assert.IsTrue(ring.CompleteCurrent(out _));

        Assert.IsTrue(ring.TryCommitWrite(8));
        Assert.IsTrue(ring.TryStartNext(false, out _, out int wrappedTailLength));
        Assert.AreEqual(4, wrappedTailLength);
        Assert.IsTrue(ring.CompleteCurrent(out _));
        Assert.AreEqual(4, ring.BufferedByteCount);
    }
}
