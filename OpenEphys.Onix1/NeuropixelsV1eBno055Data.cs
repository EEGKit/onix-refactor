﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Produces a sequence of <see cref="Bno055DataFrame"/> objects from a NeuropixelsV1e headstage.
    /// </summary>
    /// <remarks>
    /// This data IO operator must be linked to an appropriate configuration, such as a <see
    /// cref="ConfigureNeuropixelsV1eBno055"/>, using a shared <c>DeviceName</c>.
    /// </remarks>
    [Description("Produces a sequence of Bno055DataFrame objects from a NeuropixelsV1e headstage.")]
    public class NeuropixelsV1eBno055Data : Source<Bno055DataFrame>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(NeuropixelsV1eBno055.NameConverter))]
        [Description(SingleDeviceFactory.DeviceNameDescription)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public string DeviceName { get; set; }

        /// <summary>
        /// Generates a sequence of <see cref="Bno055DataFrame">Bno055DataFrames</see> at approximately 100
        /// Hz.
        /// </summary>
        /// <remarks>
        /// This will generate a sequence of <see cref="Bno055DataFrame">Bno055DataFrames</see> at approximately 100 Hz.
        /// This rate may be limited by the hardware.
        /// </remarks>
        /// <returns>A sequence of <see cref="Bno055DataFrame">Bno055DataFrames</see>.</returns>
        public override IObservable<Bno055DataFrame> Generate()
        {
            // Max of 100 Hz, but limited by I2C bus
            var source = Observable.Interval(TimeSpan.FromSeconds(0.01));
            return Generate(source);
        }

        /// <summary>
        /// Generates a sequence of <see cref="Bno055DataFrame">Bno055DataFrames</see> that is driven by an
        /// input sequence.
        /// </summary>
        /// <remarks>
        /// This will attempt to produce a sequence of <see cref="Bno055DataFrame">Bno055DataFrames</see> that is updated whenever
        /// an item in the <paramref name="source"/> sequence is received. This rate is be limited by the
        /// hardware and has a maximum meaningful rate of 100 Hz.
        /// </remarks>
        /// <param name="source">A sequence to drive sampling.</param>
        /// <returns>A sequence of <see cref="Bno055DataFrame"/> objects.</returns>
        public unsafe IObservable<Bno055DataFrame> Generate<TSource>(IObservable<TSource> source)
        {
            return DeviceManager.GetDevice(DeviceName).SelectMany(
                deviceInfo =>
                {
                return !((NeuropixelsV1eBno055DeviceInfo)deviceInfo).Enable
                    ? Observable.Empty<Bno055DataFrame>()
                    : Observable.Create<Bno055DataFrame>(observer =>
                    {
                        var device = deviceInfo.GetDeviceContext(typeof(NeuropixelsV1eBno055));
                        var passthrough = device.GetPassthroughDeviceContext(typeof(DS90UB9x));
                        var i2c = new I2CRegisterContext(passthrough, NeuropixelsV1eBno055.BNO055Address);

                        return source.SubscribeSafe(observer, _ =>
                        {
                            Bno055DataFrame frame = default;
                            device.Context.EnsureContext(() =>
                            {
                                var data = i2c.ReadBytes(NeuropixelsV1eBno055.DataAddress, sizeof(Bno055DataPayload));
                                ulong clock = passthrough.ReadRegister(DS90UB9x.LASTI2CL);
                                clock += (ulong)passthrough.ReadRegister(DS90UB9x.LASTI2CH) << 32;
                                fixed (byte* dataPtr = data)
                                {
                                    frame = new Bno055DataFrame(clock, (Bno055DataPayload*)dataPtr);
                                }
                            });

                            if (frame != null)
                            {
                                observer.OnNext(frame);
                            }
                        });
                    });
                });
        }
    }
}
