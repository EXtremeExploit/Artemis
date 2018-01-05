﻿using System;
using System.Drawing;
using System.Threading;
using Artemis.DeviceProviders.Corsair.Utilities;
using CUE.NET;
using CUE.NET.Brushes;
using CUE.NET.Devices.Generic.Enums;
using Ninject.Extensions.Logging;

namespace Artemis.DeviceProviders.Corsair
{
    public class CorsairGeneric : DeviceProvider
    {
        private readonly ImageBrush _genericBrush;

        public CorsairGeneric(ILogger logger)
        {
            Logger = logger;
            Type = DeviceType.Generic;
            _genericBrush = new ImageBrush();
        }

        public ILogger Logger { get; set; }

        public override bool TryEnable()
        {
            try
            {
                lock (CorsairUtilities.SDKLock)
                {
                    CanUse = CanInitializeSdk();
                    if (CanUse && !CueSDK.IsInitialized)
                        CueSDK.Initialize(true);
                }
            }
            catch (Exception)
            {
                CanUse = false;
            }

            Logger.Debug("Attempted to enable Corsair generic devices. CanUse: {0}", CanUse);

            if (CanUse)
            {
                CueSDK.UpdateMode = UpdateMode.Manual;
                CueSDK.HeadsetStandSDK.Brush = _genericBrush;
            }

            return CanUse;
        }

        public override void Disable()
        {
            throw new NotSupportedException("Can only disable a keyboard");
        }

        public override void UpdateDevice(Bitmap bitmap)
        {
            if (!CanUse || bitmap == null)
                return;
            if (bitmap.Width != bitmap.Height)
                throw new ArgumentException("Bitmap must be a perfect square");

            _genericBrush.Image = bitmap;
            CueSDK.HeadsetStandSDK.Update();
        }

        private static bool CanInitializeSdk()
        {
            // This will skip the check-loop if the SDK is initialized
            if (CueSDK.IsInitialized)
                return CueSDK.IsSDKAvailable(CorsairDeviceType.HeadsetStand);

            for (var tries = 0; tries < 9; tries++)
            {
                if (CueSDK.IsSDKAvailable(CorsairDeviceType.HeadsetStand))
                    return true;
                Thread.Sleep(2000);
            }
            return false;
        }
    }
}
