﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Artemis.Core.DataModelExpansions;
using SkiaSharp;

namespace Artemis.Core.Modules
{
    /// <summary>
    ///     Allows you to add support for new games/applications while utilizing Artemis' profile engine and your own data
    ///     model
    /// </summary>
    public abstract class ProfileModule<T> : ProfileModule where T : DataModel
    {
        /// <summary>
        ///     The data model driving this module
        ///     <para>Note: This default data model is automatically registered upon plugin enable</para>
        /// </summary>
        public T DataModel
        {
            get => InternalDataModel as T ?? throw new InvalidOperationException("Internal datamodel does not match the type of the data model");
            internal set => InternalDataModel = value;
        }

        /// <summary>
        ///     Gets or sets whether this module must also expand the main data model
        ///     <para>
        ///         Note: If expanding the main data model is all you want your plugin to do, create a
        ///         <see cref="BaseDataModelExpansion" /> plugin instead.
        ///     </para>
        /// </summary>
        public bool ExpandsDataModel
        {
            get => InternalExpandsMainDataModel;
            set => InternalExpandsMainDataModel = value;
        }

        /// <summary>
        ///     Override to provide your own data model description. By default this returns a description matching your plugin
        ///     name and description
        /// </summary>
        /// <returns></returns>
        public virtual DataModelPropertyAttribute GetDataModelDescription()
        {
            return new() {Name = Plugin.Info.Name, Description = Plugin.Info.Description};
        }

        /// <summary>
        ///     Hide the provided property using a lambda expression, e.g. HideProperty(dm => dm.TimeDataModel.CurrentTimeUTC)
        /// </summary>
        /// <param name="propertyLambda">A lambda expression pointing to the property to ignore</param>
        public void HideProperty<TProperty>(Expression<Func<T, TProperty>> propertyLambda)
        {
            PropertyInfo propertyInfo = ReflectionUtilities.GetPropertyInfo(DataModel, propertyLambda);
            if (!HiddenPropertiesList.Any(p => p.Equals(propertyInfo)))
                HiddenPropertiesList.Add(propertyInfo);
        }

        /// <summary>
        ///     Stop hiding the provided property using a lambda expression, e.g. ShowProperty(dm =>
        ///     dm.TimeDataModel.CurrentTimeUTC)
        /// </summary>
        /// <param name="propertyLambda">A lambda expression pointing to the property to stop ignoring</param>
        public void ShowProperty<TProperty>(Expression<Func<T, TProperty>> propertyLambda)
        {
            PropertyInfo propertyInfo = ReflectionUtilities.GetPropertyInfo(DataModel, propertyLambda);
            HiddenPropertiesList.RemoveAll(p => p.Equals(propertyInfo));
        }

        internal override void InternalEnable()
        {
            DataModel = Activator.CreateInstance<T>();
            DataModel.Feature = this;
            DataModel.DataModelDescription = GetDataModelDescription();
            base.InternalEnable();
        }

        internal override void InternalDisable()
        {
            Deactivate(true);
            base.InternalDisable();
        }
    }

    /// <summary>
    ///     Allows you to add support for new games/applications while utilizing Artemis' profile engine
    /// </summary>
    public abstract class ProfileModule : Module
    {
        /// <summary>
        ///     Gets a list of all properties ignored at runtime using <c>IgnoreProperty(x => x.y)</c>
        /// </summary>
        protected internal readonly List<PropertyInfo> HiddenPropertiesList = new();
        private readonly object _lock = new();

        /// <summary>
        ///     Creates a new instance of the <see cref="ProfileModule" /> class
        /// </summary>
        protected ProfileModule()
        {
            OpacityOverride = 1;
        }

        /// <summary>
        ///     Gets a list of all properties ignored at runtime using <c>IgnoreProperty(x => x.y)</c>
        /// </summary>
        public ReadOnlyCollection<PropertyInfo> HiddenProperties => HiddenPropertiesList.AsReadOnly();

        /// <summary>
        ///     Gets the currently active profile
        /// </summary>
        public Profile? ActiveProfile { get; private set; }

        /// <summary>
        ///     Disables updating the profile, rendering does continue
        /// </summary>
        public bool IsProfileUpdatingDisabled { get; set; }

        /// <summary>
        ///     Overrides the opacity of the root folder
        /// </summary>
        public double OpacityOverride { get; set; }

        /// <summary>
        ///     Indicates whether or not a profile change is being animated
        /// </summary>
        public bool AnimatingProfileChange { get; private set; }

        /// <summary>
        ///     Called after the profile has updated
        /// </summary>
        /// <param name="deltaTime">Time in seconds since the last update</param>
        public virtual void ProfileUpdated(double deltaTime)
        {
        }

        /// <summary>
        ///     Called after the profile has rendered
        /// </summary>
        /// <param name="deltaTime">Time since the last render</param>
        /// <param name="canvas"></param>
        /// <param name="canvasInfo"></param>
        public virtual void ProfileRendered(double deltaTime, SKCanvas canvas, SKImageInfo canvasInfo)
        {
        }

        internal override void InternalUpdate(double deltaTime)
        {
            if (IsUpdateAllowed)
                Update(deltaTime);

            lock (_lock)
            {
                OpacityOverride = AnimatingProfileChange
                    ? Math.Max(0, OpacityOverride - 0.1)
                    : Math.Min(1, OpacityOverride + 0.1);

                // Update the profile
                if (!IsProfileUpdatingDisabled)
                    ActiveProfile?.Update(deltaTime);
            }

            ProfileUpdated(deltaTime);
        }

        internal override void InternalRender(double deltaTime, SKCanvas canvas, SKImageInfo canvasInfo)
        {
            Render(deltaTime, canvas, canvasInfo);

            lock (_lock)
            {
                // Render the profile
                ActiveProfile?.Render(canvas);
            }

            ProfileRendered(deltaTime, canvas, canvasInfo);
        }

        internal async Task ChangeActiveProfileAnimated(Profile? profile, IEnumerable<ArtemisDevice> devices)
        {
            if (profile != null && profile.Module != this)
                throw new ArtemisCoreException($"Cannot activate a profile of module {profile.Module} on a module of plugin {this}.");
            if (!IsActivated)
                throw new ArtemisCoreException("Cannot activate a profile on a deactivated module");

            if (profile == ActiveProfile || AnimatingProfileChange)
                return;

            AnimatingProfileChange = true;

            while (OpacityOverride > 0)
                await Task.Delay(50);

            ChangeActiveProfile(profile, devices);
            AnimatingProfileChange = false;

            while (OpacityOverride < 1)
                await Task.Delay(50);
        }

        internal void ChangeActiveProfile(Profile? profile, IEnumerable<ArtemisDevice> devices)
        {
            if (profile != null && profile.Module != this)
                throw new ArtemisCoreException($"Cannot activate a profile of module {profile.Module} on a module of plugin {this}.");
            if (!IsActivated)
                throw new ArtemisCoreException("Cannot activate a profile on a deactivated module");

            lock (_lock)
            {
                if (profile == ActiveProfile)
                    return;

                ActiveProfile?.Dispose();

                ActiveProfile = profile;
                ActiveProfile?.Activate(devices);
            }

            OnActiveProfileChanged();
        }

        internal override void Deactivate(bool isOverride)
        {
            base.Deactivate(isOverride);

            Profile? profile = ActiveProfile;
            ActiveProfile = null;
            profile?.Dispose();
        }

        internal override void Reactivate(bool isDeactivateOverride, bool isActivateOverride)
        {
            if (!IsActivated)
                return;

            // Avoid disposing the profile
            base.Deactivate(isDeactivateOverride);
            Activate(isActivateOverride);
        }

        #region Events

        /// <summary>
        ///     Occurs when the <see cref="ActiveProfile" /> has changed
        /// </summary>
        public event EventHandler? ActiveProfileChanged;

        /// <summary>
        ///     Invokes the <see cref="ActiveProfileChanged" /> event
        /// </summary>
        protected virtual void OnActiveProfileChanged()
        {
            ActiveProfileChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}