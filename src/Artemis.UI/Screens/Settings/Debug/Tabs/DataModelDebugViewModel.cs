﻿using System;
using System.Linq;
using System.Timers;
using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.Settings.Debug.Tabs
{
    public sealed class DataModelDebugViewModel : Screen, IDisposable
    {
        private readonly IDataModelUIService _dataModelUIService;
        private readonly IPluginManagementService _pluginManagementService;
        private readonly Timer _updateTimer;

        private bool _isModuleFilterEnabled;
        private DataModelPropertiesViewModel _mainDataModel;
        private string _propertySearch;
        private Module _selectedModule;

        public DataModelDebugViewModel(IDataModelUIService dataModelUIService, IPluginManagementService pluginManagementService)
        {
            _dataModelUIService = dataModelUIService;
            _pluginManagementService = pluginManagementService;
            _updateTimer = new Timer(500);

            DisplayName = "Data model";
            Modules = new BindableCollection<Module>();
        }

        public DataModelPropertiesViewModel MainDataModel
        {
            get => _mainDataModel;
            set => SetAndNotify(ref _mainDataModel, value);
        }

        public string PropertySearch
        {
            get => _propertySearch;
            set => SetAndNotify(ref _propertySearch, value);
        }

        public BindableCollection<Module> Modules { get; }

        public Module SelectedModule
        {
            get => _selectedModule;
            set
            {
                if (!SetAndNotify(ref _selectedModule, value)) return;
                GetDataModel();
            }
        }

        public bool IsModuleFilterEnabled
        {
            get => _isModuleFilterEnabled;
            set
            {
                SetAndNotify(ref _isModuleFilterEnabled, value);

                if (!IsModuleFilterEnabled)
                    SelectedModule = null;
                else
                    GetDataModel();
            }
        }

        #region Overrides of Screen

        /// <inheritdoc />
        protected override void OnClose()
        {
            _updateTimer.Dispose();
            base.OnClose();
        }

        #endregion

        protected override void OnActivate()
        {
            GetDataModel();
            _updateTimer.Start();
            _updateTimer.Elapsed += OnUpdateTimerOnElapsed;
            _pluginManagementService.PluginFeatureEnabled += OnPluginFeatureToggled;
            _pluginManagementService.PluginFeatureDisabled += OnPluginFeatureToggled;

            PopulateModules();

            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            _updateTimer.Stop();
            _updateTimer.Elapsed -= OnUpdateTimerOnElapsed;
            _pluginManagementService.PluginFeatureEnabled -= OnPluginFeatureToggled;
            _pluginManagementService.PluginFeatureDisabled -= OnPluginFeatureToggled;

            base.OnDeactivate();
        }

        private void OnPluginFeatureToggled(object sender, PluginFeatureEventArgs e)
        {
            if (e.PluginFeature is DataModelPluginFeature)
                PopulateModules();
        }

        private void OnUpdateTimerOnElapsed(object sender, ElapsedEventArgs args)
        {
            if (MainDataModel == null)
                return;

            lock (MainDataModel)
            {
                MainDataModel.Update(_dataModelUIService, null);
            }
        }

        private void GetDataModel()
        {
            MainDataModel = SelectedModule != null
                ? _dataModelUIService.GetPluginDataModelVisualization(SelectedModule, false)
                : _dataModelUIService.GetMainDataModelVisualization();
        }

        private void PopulateModules()
        {
            Modules.Clear();
            Modules.AddRange(_pluginManagementService.GetFeaturesOfType<Module>().Where(p => p.IsEnabled).OrderBy(m => m.DisplayName));

            if (SelectedModule == null)
                _dataModelUIService.UpdateModules(MainDataModel);
            else if (!SelectedModule.IsEnabled)
                SelectedModule = null;
        }

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            _updateTimer?.Dispose();
        }

        #endregion
    }
}