﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Artemis.Core.Extensions;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Abstract.ViewModels;
using Artemis.Core.Plugins.LayerBrush;
using Artemis.Core.Services.Interfaces;
using Artemis.Core.Services.Storage.Interfaces;
using Artemis.Storage.Entities.Profile;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using SkiaSharp;

namespace Artemis.Core.Utilities
{
    internal class IntroAnimation
    {
        private readonly ILogger _logger;
        private readonly IProfileService _profileService;
        private readonly ISurfaceService _surfaceService;

        public IntroAnimation(ILogger logger, IProfileService profileService, ISurfaceService surfaceService)
        {
            _logger = logger;
            _profileService = profileService;
            _surfaceService = surfaceService;
            CreateIntroProfile();
        }

        private void CreateIntroProfile()
        {
            try
            {
                // Load the intro profile from JSON into a ProfileEntity
                var json = File.ReadAllText(Path.Combine(Constants.ApplicationFolder, "Resources", "intro-profile.json"));
                var profileEntity = JsonConvert.DeserializeObject<ProfileEntity>(json);
                // Inject every LED on the surface into each layer
                foreach (var profileEntityLayer in profileEntity.Layers)
                {
                    profileEntityLayer.Leds.AddRange(_surfaceService.ActiveSurface.Devices.SelectMany(d => d.Leds).Select(l => new LedEntity()
                    {
                        DeviceIdentifier = l.Device.RgbDevice.GetDeviceIdentifier(),
                        LedName = l.RgbLed.Id.ToString(),
                    }));
                }

                var profile = new Profile(new DummyModule(), profileEntity);
                profile.Activate(_surfaceService.ActiveSurface);

                _profileService.InstantiateProfile(profile);
                AnimationProfile = profile;
            }
            catch (Exception e)
            {
                _logger.Warning(e, "Failed to load intro profile");
            }
        }

        public Profile AnimationProfile { get; set; }

        public void Render(double deltaTime, SKCanvas canvas, SKImageInfo bitmapInfo)
        {
            if (AnimationProfile == null)
                return;

            AnimationProfile.Update(deltaTime);
            AnimationProfile.Render(deltaTime, canvas, bitmapInfo);
        }
    }

    internal class DummyModule : ProfileModule
    {
        public override void EnablePlugin()
        {
        }

        public override void DisablePlugin()
        {
        }

        public override IEnumerable<ModuleViewModel> GetViewModels()
        {
            return new List<ModuleViewModel>();
        }
    }
}