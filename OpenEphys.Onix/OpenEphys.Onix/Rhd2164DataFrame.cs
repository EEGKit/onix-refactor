﻿using System.Runtime.InteropServices;
using OpenCV.Net;

namespace OpenEphys.Onix
{
    public class Rhd2164DataFrame : BufferedDataFrame
    {
        public Rhd2164DataFrame(ulong[] clock, ulong[] hubClock, Mat amplifierData, Mat auxData)
            : base(clock, hubClock)
        {
            AmplifierData = amplifierData;
            AuxData = auxData;
        }

        public Mat AmplifierData { get; }

        public Mat AuxData { get; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct Rhd2164Payload
    {
        public ulong HubClock;
        public fixed ushort AmplifierData[Rhd2164.AmplifierChannelCount];
        public fixed ushort AuxData[Rhd2164.AuxChannelCount];
    }
}
