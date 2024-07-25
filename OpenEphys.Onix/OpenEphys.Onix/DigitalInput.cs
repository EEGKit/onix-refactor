﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix
{
    /// <summary>
    /// A class that produces a sequence of digital input data frames.
    /// </summary>
    /// <remarks>
    /// This data stream class must be linked to an appropriate configuration, such as a <see cref="ConfigureDigitalIO"/>,
    /// in order to stream data.
    /// </remarks>
    public class DigitalInput : Source<DigitalInputDataFrame>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(DigitalIO.NameConverter))]
        public string DeviceName { get; set; }

        /// <summary>
        /// Generates a sequence of <see cref="DigitalInputDataFrame"/> objects, which contains information about breakout
        /// board's digital input state.
        /// </summary>
        /// <remarks>
        /// Digital inputs are not regularly sampled. Instead, a new <see cref="DigitalInputDataFrame"/> is produced each
        /// whenever any digital state (i.e. a digital input pin, button, or switch state) changes.
        /// </remarks>
        /// <returns>A sequence of <see cref="DigitalInputDataFrame"/> objects.</returns>
        public unsafe override IObservable<DigitalInputDataFrame> Generate()
        {
            return DeviceManager.GetDevice(DeviceName).SelectMany(deviceInfo =>
            {
                var device = deviceInfo.GetDeviceContext(typeof(DigitalIO));
                return deviceInfo.Context.FrameReceived
                    .Where(frame => frame.DeviceAddress == device.Address)
                    .Select(frame => new DigitalInputDataFrame(frame));
            });
        }
    }
}
