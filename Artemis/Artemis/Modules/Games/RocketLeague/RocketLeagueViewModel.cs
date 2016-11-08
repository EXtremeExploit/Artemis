﻿using Artemis.DAL;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Settings;
using Artemis.Utilities;
using Artemis.ViewModels.Abstract;
using Ninject;

namespace Artemis.Modules.Games.RocketLeague
{
    public sealed class RocketLeagueViewModel : GameViewModel
    {
        private string _versionText;

        public RocketLeagueViewModel(MainManager main, IKernel kernel, [Named("RocketLeagueModel")] GameModel model)
            : base(main, model, kernel)
        {
            DisplayName = "Rocket League";
            SetVersionText();
        }

        public string VersionText
        {
            get { return _versionText; }
            set
            {
                if (value == _versionText) return;
                _versionText = value;
                NotifyOfPropertyChange(() => VersionText);
            }
        }

        public RocketLeagueModel RocketLeagueModel { get; set; }

        private void SetVersionText()
        {
            if (!SettingsProvider.Load<GeneralSettings>().EnablePointersUpdate)
            {
                VersionText = "Note: You disabled pointer updates, this could result in the " +
                              "Rocket League effect not working after a game update.";
                return;
            }

            Updater.GetPointers();
            var version = SettingsProvider.Load<OffsetSettings>().RocketLeague.GameVersion;
            VersionText = $"Note: Requires patch {version}. When a new patch is released Artemis downloads " +
                          "new pointers for the latest version (unless disabled in settings).";
        }
    }
}