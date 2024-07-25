﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix
{
    /// <summary>
    /// A class that produces a sequence of heartbeat data frames.
    /// </summary>
    /// <remarks>
    /// This data stream class must be linked to an appropriate configuration, such as a <see cref="ConfigureHeartbeat"/>,
    /// in order to stream heartbeat data.
    /// </remarks>
    public class HeartbeatData : Source<HeartbeatDataFrame>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(Heartbeat.NameConverter))]
        public string DeviceName { get; set; }

        /// <summary>
        /// Generates a sequence of <see cref="HeartbeatDataFrame"/> objects, each of which contains period signal from the
        /// acquisition system indicating that it is active.
        /// </summary>
        /// <returns>A sequence of <see cref="HeartbeatDataFrame"/> objects.</returns>
        public override IObservable<HeartbeatDataFrame> Generate()
        {
            return DeviceManager.GetDevice(DeviceName).SelectMany(deviceInfo =>
            {
                var device = deviceInfo.GetDeviceContext(typeof(Heartbeat));
                return deviceInfo.Context.FrameReceived
                    .Where(frame => frame.DeviceAddress == device.Address)
                    .Select(frame => new HeartbeatDataFrame(frame));
            });
        }
    }
}
