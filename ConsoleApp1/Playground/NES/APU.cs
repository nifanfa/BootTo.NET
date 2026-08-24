using System;

namespace Playground.NES
{
    public sealed class APU
    {
        public const int CpuFrequency = 1789773;
        public const int PcmSampleRate = 44100;
        public const int PcmChannels = 1;
        public const int PcmBitsPerSample = 16;
        public const int PcmBytesPerSample = 2;

        private static readonly byte[] LengthTable =
        {
            10, 254, 20, 2, 40, 4, 80, 6,
            160, 8, 60, 10, 14, 12, 26, 14,
            12, 16, 24, 18, 48, 20, 96, 22,
            192, 24, 72, 26, 16, 28, 32, 30
        };

        private readonly PulseChannel pulse1 = new PulseChannel(true);
        private readonly PulseChannel pulse2 = new PulseChannel(false);
        private readonly TriangleChannel triangle = new TriangleChannel();
        private readonly NoiseChannel noise = new NoiseChannel();
        private readonly DmcChannel dmc;

        private int frameCycle;
        private bool fiveStepMode;
        private bool frameIrqInhibit;
        private bool frameIrq;
        private bool evenCpuCycle;
        private int sampleAccumulator;
        private readonly byte[] pcmBuffer = new byte[4096];
        private int pcmBufferOffset;

        private double highPass90Input;
        private double highPass90Output;
        private double highPass440Input;
        private double highPass440Output;
        private double lowPassOutput;

        public long GeneratedSampleCount { get; private set; }
        public short LastSample { get; private set; }
        public int PeakSampleMagnitude { get; private set; }
        // Little-endian signed 16-bit mono PCM at PcmSampleRate.
        // The callback is invoked with complete PCM blocks, not individual samples.
        public APU()
            : this(null)
        {
        }

        public APU(Func<int, byte> memoryReader)
        {
            dmc = new DmcChannel(memoryReader);
        }

        public void WriteRegister(int address, byte data)
        {
            switch (address)
            {
                case 0x4000: pulse1.WriteControl(data); break;
                case 0x4001: pulse1.WriteSweep(data); break;
                case 0x4002: pulse1.WriteTimerLow(data); break;
                case 0x4003: pulse1.WriteTimerHigh(data); break;
                case 0x4004: pulse2.WriteControl(data); break;
                case 0x4005: pulse2.WriteSweep(data); break;
                case 0x4006: pulse2.WriteTimerLow(data); break;
                case 0x4007: pulse2.WriteTimerHigh(data); break;
                case 0x4008: triangle.WriteControl(data); break;
                case 0x400A: triangle.WriteTimerLow(data); break;
                case 0x400B: triangle.WriteTimerHigh(data); break;
                case 0x400C: noise.WriteControl(data); break;
                case 0x400E: noise.WritePeriod(data); break;
                case 0x400F: noise.WriteLength(data); break;
                case 0x4010: dmc.WriteControl(data); break;
                case 0x4011: dmc.WriteOutput(data); break;
                case 0x4012: dmc.WriteAddress(data); break;
                case 0x4013: dmc.WriteLength(data); break;
                case 0x4015: WriteStatus(data); break;
                case 0x4017: WriteFrameCounter(data); break;
            }
        }

        public byte ReadStatus()
        {
            byte status = 0;
            if (pulse1.LengthCounter > 0) status |= 0x01;
            if (pulse2.LengthCounter > 0) status |= 0x02;
            if (triangle.LengthCounter > 0) status |= 0x04;
            if (noise.LengthCounter > 0) status |= 0x08;
            if (dmc.BytesRemaining > 0) status |= 0x10;
            if (frameIrq) status |= 0x40;
            if (dmc.IrqPending) status |= 0x80;

            frameIrq = false;
            return status;
        }

        public void Clock(int cpuCycles)
        {
            for (int i = 0; i < cpuCycles; i++)
            {
                evenCpuCycle = !evenCpuCycle;
                if (evenCpuCycle)
                {
                    pulse1.ClockTimer();
                    pulse2.ClockTimer();
                    noise.ClockTimer();
                }

                triangle.ClockTimer();
                dmc.ClockTimer();
                ClockFrameSequencer();

                sampleAccumulator += PcmSampleRate;
                if (sampleAccumulator >= CpuFrequency)
                {
                    sampleAccumulator -= CpuFrequency;
                    EmitSample();
                }
            }
        }

        // Emits a final partial PCM block. Call this when stopping or seeking;
        // normal emulation should leave this buffered for the next Clock call.
        public void FlushPcm()
        {
            if (pcmBufferOffset == 0)
                return;

            WaveOutAudio.WritePcm(pcmBuffer, 0, pcmBufferOffset, PcmChannels, PcmSampleRate);
            pcmBufferOffset = 0;
        }

        private void WriteStatus(byte data)
        {
            pulse1.SetEnabled((data & 0x01) != 0);
            pulse2.SetEnabled((data & 0x02) != 0);
            triangle.SetEnabled((data & 0x04) != 0);
            noise.SetEnabled((data & 0x08) != 0);
            dmc.SetEnabled((data & 0x10) != 0);
        }

        private void WriteFrameCounter(byte data)
        {
            fiveStepMode = (data & 0x80) != 0;
            frameIrqInhibit = (data & 0x40) != 0;
            if (frameIrqInhibit)
                frameIrq = false;

            frameCycle = 0;
            if (fiveStepMode)
            {
                ClockQuarterFrame();
                ClockHalfFrame();
            }
        }

        private void ClockFrameSequencer()
        {
            frameCycle++;

            if (fiveStepMode)
            {
                if (frameCycle == 7457 || frameCycle == 22371)
                {
                    ClockQuarterFrame();
                }
                else if (frameCycle == 14913 || frameCycle == 37281)
                {
                    ClockQuarterFrame();
                    ClockHalfFrame();
                }

                if (frameCycle >= 37282)
                    frameCycle = 0;
            }
            else
            {
                if (frameCycle == 7457 || frameCycle == 22371)
                {
                    ClockQuarterFrame();
                }
                else if (frameCycle == 14913 || frameCycle == 29829)
                {
                    ClockQuarterFrame();
                    ClockHalfFrame();
                    if (frameCycle == 29829 && !frameIrqInhibit)
                        frameIrq = true;
                }

                if (frameCycle >= 29830)
                    frameCycle = 0;
            }
        }

        private void ClockQuarterFrame()
        {
            pulse1.ClockEnvelope();
            pulse2.ClockEnvelope();
            triangle.ClockLinearCounter();
            noise.ClockEnvelope();
        }

        private void ClockHalfFrame()
        {
            pulse1.ClockLengthAndSweep();
            pulse2.ClockLengthAndSweep();
            triangle.ClockLength();
            noise.ClockLength();
        }

        private void EmitSample()
        {
            int pulseSum = pulse1.Output + pulse2.Output;
            double pulseOutput = pulseSum == 0 ? 0.0 : 95.88 / ((8128.0 / pulseSum) + 100.0);

            double tndInput = triangle.Output / 8227.0 + noise.Output / 12241.0 + dmc.Output / 22638.0;
            double tndOutput = tndInput == 0.0 ? 0.0 : 159.79 / ((1.0 / tndInput) + 100.0);
            double mixed = pulseOutput + tndOutput;

            double highPass90 = mixed - highPass90Input + 0.996039 * highPass90Output;
            highPass90Input = mixed;
            highPass90Output = highPass90;

            double highPass440 = highPass90 - highPass440Input + 0.939063 * highPass440Output;
            highPass440Input = highPass90;
            highPass440Output = highPass440;

            lowPassOutput += 0.815686 * (highPass440 - lowPassOutput);
            int value = (int)(lowPassOutput * 28000.0);
            if (value > short.MaxValue) value = short.MaxValue;
            if (value < short.MinValue) value = short.MinValue;

            LastSample = (short)value;
            int magnitude = value < 0 ? -value : value;
            if (magnitude > PeakSampleMagnitude)
                PeakSampleMagnitude = magnitude;
            GeneratedSampleCount++;
            pcmBuffer[pcmBufferOffset++] = (byte)(LastSample & 0xFF);
            pcmBuffer[pcmBufferOffset++] = (byte)((LastSample >> 8) & 0xFF);
            if (pcmBufferOffset == pcmBuffer.Length)
            {
                WaveOutAudio.WritePcm(pcmBuffer, 0, pcmBuffer.Length, PcmChannels, PcmSampleRate);
                pcmBufferOffset = 0;
            }
        }

        private sealed class Envelope
        {
            private int period;
            private int divider;
            private int decay;
            private bool constantVolume;
            private bool loop;
            private bool start;

            internal int Output { get { return constantVolume ? period : decay; } }

            internal void Write(byte data)
            {
                period = data & 0x0F;
                constantVolume = (data & 0x10) != 0;
                loop = (data & 0x20) != 0;
            }

            internal void Restart()
            {
                start = true;
            }

            internal void Clock()
            {
                if (start)
                {
                    start = false;
                    decay = 15;
                    divider = period;
                }
                else if (divider > 0)
                {
                    divider--;
                }
                else
                {
                    divider = period;
                    if (decay > 0)
                        decay--;
                    else if (loop)
                        decay = 15;
                }
            }
        }

        private sealed class PulseChannel
        {
            private static readonly byte[,] DutyTable =
            {
                { 0, 1, 0, 0, 0, 0, 0, 0 },
                { 0, 1, 1, 0, 0, 0, 0, 0 },
                { 0, 1, 1, 1, 1, 0, 0, 0 },
                { 1, 0, 0, 1, 1, 1, 1, 1 }
            };

            private readonly bool firstChannel;
            private readonly Envelope envelope = new Envelope();
            private bool enabled;
            private bool lengthHalt;
            private int duty;
            private int sequence;
            private int timerPeriod;
            private int timerCounter;
            private bool sweepEnabled;
            private int sweepPeriod;
            private bool sweepNegate;
            private int sweepShift;
            private int sweepDivider;
            private bool sweepReload;

            internal int LengthCounter { get; private set; }

            internal int Output
            {
                get
                {
                    int target = SweepTarget();
                    if (!enabled || LengthCounter == 0 || timerPeriod < 8 || target > 0x7FF || target < 0)
                        return 0;
                    return DutyTable[duty, sequence] == 0 ? 0 : envelope.Output;
                }
            }

            internal PulseChannel(bool firstChannel)
            {
                this.firstChannel = firstChannel;
            }

            internal void SetEnabled(bool value)
            {
                enabled = value;
                if (!enabled)
                    LengthCounter = 0;
            }

            internal void WriteControl(byte data)
            {
                duty = (data >> 6) & 0x03;
                lengthHalt = (data & 0x20) != 0;
                envelope.Write(data);
            }

            internal void WriteSweep(byte data)
            {
                sweepEnabled = (data & 0x80) != 0;
                sweepPeriod = ((data >> 4) & 0x07) + 1;
                sweepNegate = (data & 0x08) != 0;
                sweepShift = data & 0x07;
                sweepReload = true;
            }

            internal void WriteTimerLow(byte data)
            {
                timerPeriod = (timerPeriod & 0x0700) | data;
            }

            internal void WriteTimerHigh(byte data)
            {
                timerPeriod = (timerPeriod & 0x00FF) | ((data & 0x07) << 8);
                if (enabled)
                    LengthCounter = LengthTable[data >> 3];
                sequence = 0;
                envelope.Restart();
            }

            internal void ClockTimer()
            {
                if (timerCounter == 0)
                {
                    timerCounter = timerPeriod;
                    sequence = (sequence + 1) & 0x07;
                }
                else
                {
                    timerCounter--;
                }
            }

            internal void ClockEnvelope()
            {
                envelope.Clock();
            }

            internal void ClockLengthAndSweep()
            {
                if (!lengthHalt && LengthCounter > 0)
                    LengthCounter--;

                if (sweepDivider == 0 && sweepEnabled && sweepShift > 0)
                {
                    int target = SweepTarget();
                    if (timerPeriod >= 8 && target >= 0 && target <= 0x7FF)
                        timerPeriod = target;
                }

                if (sweepDivider == 0 || sweepReload)
                {
                    sweepDivider = sweepPeriod;
                    sweepReload = false;
                }
                else
                {
                    sweepDivider--;
                }
            }

            private int SweepTarget()
            {
                if (sweepShift == 0)
                    return timerPeriod;

                int change = timerPeriod >> sweepShift;
                if (!sweepNegate)
                    return timerPeriod + change;
                return timerPeriod - change - (firstChannel ? 1 : 0);
            }
        }

        private sealed class TriangleChannel
        {
            private static readonly byte[] Sequence =
            {
                15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0,
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15
            };

            private bool enabled;
            private bool controlFlag;
            private int linearReloadValue;
            private int linearCounter;
            private bool linearReload;
            private int timerPeriod;
            private int timerCounter;
            private int sequence;

            internal int LengthCounter { get; private set; }
            internal int Output { get { return enabled && LengthCounter > 0 && linearCounter > 0 && timerPeriod > 1 ? Sequence[sequence] : 0; } }

            internal void SetEnabled(bool value)
            {
                enabled = value;
                if (!enabled)
                    LengthCounter = 0;
            }

            internal void WriteControl(byte data)
            {
                controlFlag = (data & 0x80) != 0;
                linearReloadValue = data & 0x7F;
            }

            internal void WriteTimerLow(byte data)
            {
                timerPeriod = (timerPeriod & 0x0700) | data;
            }

            internal void WriteTimerHigh(byte data)
            {
                timerPeriod = (timerPeriod & 0x00FF) | ((data & 0x07) << 8);
                if (enabled)
                    LengthCounter = LengthTable[data >> 3];
                linearReload = true;
            }

            internal void ClockTimer()
            {
                if (timerCounter == 0)
                {
                    timerCounter = timerPeriod;
                    if (enabled && LengthCounter > 0 && linearCounter > 0 && timerPeriod > 1)
                        sequence = (sequence + 1) & 0x1F;
                }
                else
                {
                    timerCounter--;
                }
            }

            internal void ClockLinearCounter()
            {
                if (linearReload)
                    linearCounter = linearReloadValue;
                else if (linearCounter > 0)
                    linearCounter--;

                if (!controlFlag)
                    linearReload = false;
            }

            internal void ClockLength()
            {
                if (!controlFlag && LengthCounter > 0)
                    LengthCounter--;
            }
        }

        private sealed class NoiseChannel
        {
            private static readonly int[] PeriodTable =
            {
                4, 8, 16, 32, 64, 96, 128, 160,
                202, 254, 380, 508, 762, 1016, 2034, 4068
            };

            private readonly Envelope envelope = new Envelope();
            private bool enabled;
            private bool lengthHalt;
            private bool shortMode;
            private int timerPeriod = PeriodTable[0];
            private int timerCounter;
            private int shiftRegister = 1;

            internal int LengthCounter { get; private set; }
            internal int Output { get { return enabled && LengthCounter > 0 && (shiftRegister & 1) == 0 ? envelope.Output : 0; } }

            internal void SetEnabled(bool value)
            {
                enabled = value;
                if (!enabled)
                    LengthCounter = 0;
            }

            internal void WriteControl(byte data)
            {
                lengthHalt = (data & 0x20) != 0;
                envelope.Write(data);
            }

            internal void WritePeriod(byte data)
            {
                shortMode = (data & 0x80) != 0;
                timerPeriod = PeriodTable[data & 0x0F];
            }

            internal void WriteLength(byte data)
            {
                if (enabled)
                    LengthCounter = LengthTable[data >> 3];
                envelope.Restart();
            }

            internal void ClockTimer()
            {
                if (timerCounter == 0)
                {
                    timerCounter = timerPeriod;
                    int tap = shortMode ? 6 : 1;
                    int feedback = (shiftRegister & 1) ^ ((shiftRegister >> tap) & 1);
                    shiftRegister = (shiftRegister >> 1) | (feedback << 14);
                }
                else
                {
                    timerCounter--;
                }
            }

            internal void ClockEnvelope()
            {
                envelope.Clock();
            }

            internal void ClockLength()
            {
                if (!lengthHalt && LengthCounter > 0)
                    LengthCounter--;
            }
        }

        private sealed class DmcChannel
        {
            private static readonly int[] RateTable =
            {
                428, 380, 340, 320, 286, 254, 226, 214,
                190, 160, 142, 128, 106, 85, 72, 54
            };

            private readonly Func<int, byte> memoryReader;
            private bool enabled;
            private bool irqEnabled;
            private bool loop;
            private int timerPeriod = RateTable[0];
            private int timerCounter;
            private int outputLevel;
            private int sampleAddress = 0xC000;
            private int sampleLength = 1;
            private int currentAddress;
            private int sampleBuffer;
            private bool sampleBufferEmpty = true;
            private int shiftRegister;
            private int bitsRemaining = 8;
            private bool silence = true;

            internal int BytesRemaining { get; private set; }
            internal bool IrqPending { get; private set; }
            internal int Output { get { return outputLevel; } }

            internal DmcChannel(Func<int, byte> memoryReader)
            {
                this.memoryReader = memoryReader;
            }

            internal void WriteControl(byte data)
            {
                irqEnabled = (data & 0x80) != 0;
                loop = (data & 0x40) != 0;
                timerPeriod = RateTable[data & 0x0F];
                if (!irqEnabled)
                    IrqPending = false;
            }

            internal void WriteOutput(byte data)
            {
                outputLevel = data & 0x7F;
            }

            internal void WriteAddress(byte data)
            {
                sampleAddress = 0xC000 | (data << 6);
            }

            internal void WriteLength(byte data)
            {
                sampleLength = (data << 4) + 1;
            }

            internal void SetEnabled(bool value)
            {
                enabled = value;
                IrqPending = false;
                if (!enabled)
                {
                    BytesRemaining = 0;
                }
                else if (BytesRemaining == 0)
                {
                    RestartSample();
                }
            }

            internal void ClockTimer()
            {
                FillSampleBuffer();

                if (timerCounter == 0)
                {
                    timerCounter = timerPeriod - 1;
                    ClockOutputUnit();
                }
                else
                {
                    timerCounter--;
                }
            }

            private void ClockOutputUnit()
            {
                if (!silence)
                {
                    if ((shiftRegister & 1) != 0)
                    {
                        if (outputLevel <= 125) outputLevel += 2;
                    }
                    else if (outputLevel >= 2)
                    {
                        outputLevel -= 2;
                    }
                }

                shiftRegister >>= 1;
                bitsRemaining--;
                if (bitsRemaining != 0)
                    return;

                bitsRemaining = 8;
                if (sampleBufferEmpty)
                {
                    silence = true;
                }
                else
                {
                    silence = false;
                    shiftRegister = sampleBuffer;
                    sampleBufferEmpty = true;
                }
            }

            private void FillSampleBuffer()
            {
                if (!sampleBufferEmpty || BytesRemaining == 0)
                    return;

                sampleBuffer = memoryReader == null ? 0 : memoryReader(currentAddress);
                sampleBufferEmpty = false;
                currentAddress = currentAddress == 0xFFFF ? 0x8000 : currentAddress + 1;
                BytesRemaining--;

                if (BytesRemaining != 0)
                    return;
                if (loop)
                    RestartSample();
                else if (irqEnabled)
                    IrqPending = true;
            }

            private void RestartSample()
            {
                currentAddress = sampleAddress;
                BytesRemaining = sampleLength;
            }
        }
    }
}
