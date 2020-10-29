using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.LayerEffects;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Entities.Profile.Abstract;
using SkiaSharp;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a folder in a <see cref="Profile" />
    /// </summary>
    public sealed class Folder : RenderProfileElement
    {
        private SKBitmap _folderBitmap;

        /// <summary>
        ///     Creates a new instance of the <see cref="Folder" /> class and adds itself to the child collection of the provided
        ///     <paramref name="parent" />
        /// </summary>
        /// <param name="parent">The parent of the folder</param>
        /// <param name="name">The name of the folder</param>
        public Folder(ProfileElement parent, string name)
        {
            FolderEntity = new FolderEntity();
            EntityId = Guid.NewGuid();

            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            Profile = Parent.Profile;
            Name = name;
            Enabled = true;

            _layerEffects = new List<BaseLayerEffect>();
            _expandedPropertyGroups = new List<string>();

            Parent.AddChild(this);
        }

        internal Folder(Profile profile, ProfileElement parent, FolderEntity folderEntity)
        {
            FolderEntity = folderEntity;
            EntityId = folderEntity.Id;

            Profile = profile;
            Parent = parent;
            Name = folderEntity.Name;
            Enabled = folderEntity.Enabled;
            Order = folderEntity.Order;

            _layerEffects = new List<BaseLayerEffect>();
            _expandedPropertyGroups = new List<string>();

            Load();
            UpdateChildrenTimelineLength();
        }

        /// <summary>
        /// Gets a boolean indicating whether this folder is at the root of the profile tree
        /// </summary>
        public bool IsRootFolder => Parent == Profile;

        /// <summary>
        /// Gets the longest timeline of all this folders children
        /// </summary>
        public Timeline LongestChildTimeline { get; private set; }

        internal FolderEntity FolderEntity { get; set; }

        internal override RenderElementEntity RenderElementEntity => FolderEntity;

        /// <inheritdoc />
        public override List<ILayerProperty> GetAllLayerProperties()
        {
            List<ILayerProperty> result = new List<ILayerProperty>();
            foreach (BaseLayerEffect layerEffect in LayerEffects)
                if (layerEffect.BaseProperties != null)
                    result.AddRange(layerEffect.BaseProperties.GetAllLayerProperties());

            return result;
        }

        public override void Update(double deltaTime)
        {
            if (Disposed)
                throw new ObjectDisposedException("Folder");

            if (!Enabled)
                return;

            UpdateDisplayCondition();
            UpdateTimeline(deltaTime);

            foreach (ProfileElement child in Children)
                child.Update(deltaTime);
        }

        /// <inheritdoc />
        public override void Reset()
        {
            DisplayConditionMet = false;
            Timeline.JumpToStart();

            foreach (ProfileElement child in Children)
                child.Reset();
        }

        /// <inheritdoc />
        public override void AddChild(ProfileElement child, int? order = null)
        {
            if (Disposed)
                throw new ObjectDisposedException("Folder");

            base.AddChild(child, order);
            UpdateChildrenTimelineLength();
            CalculateRenderProperties();
        }

        /// <inheritdoc />
        public override void RemoveChild(ProfileElement child)
        {
            if (Disposed)
                throw new ObjectDisposedException("Folder");

            base.RemoveChild(child);
            UpdateChildrenTimelineLength();
            CalculateRenderProperties();
        }

        public override string ToString()
        {
            return $"[Folder] {nameof(Name)}: {Name}, {nameof(Order)}: {Order}";
        }

        public void CalculateRenderProperties()
        {
            if (Disposed)
                throw new ObjectDisposedException("Folder");

            SKPath path = new SKPath {FillType = SKPathFillType.Winding};
            foreach (ProfileElement child in Children)
                if (child is RenderProfileElement effectChild && effectChild.Path != null)
                    path.AddPath(effectChild.Path);

            Path = path;

            // Folder render properties are based on child paths and thus require an update
            if (Parent is Folder folder)
                folder.CalculateRenderProperties();

            OnRenderPropertiesUpdated();
        }

        private void UpdateChildrenTimelineLength()
        {
            Timeline longest = new Timeline() {MainSegmentLength = TimeSpan.Zero};
            foreach (ProfileElement profileElement in Children)
            {
                if (profileElement is Folder folder && folder.LongestChildTimeline.Length > longest.Length)
                    longest = folder.LongestChildTimeline;
                else if (profileElement is Layer layer && 
                         layer.Timeline.PlayMode == TimelinePlayMode.Once && 
                         layer.Timeline.StopMode == TimelineStopMode.Finish &&
                         layer.Timeline.Length > longest.Length)
                    longest = layer.Timeline;
            }

            LongestChildTimeline = longest;
        }

        protected override void Dispose(bool disposing)
        {
            Disposed = true;

            foreach (ProfileElement profileElement in Children)
                profileElement.Dispose();

            _folderBitmap?.Dispose();
            base.Dispose(disposing);
        }

        internal override void Load()
        {
            _expandedPropertyGroups.AddRange(FolderEntity.ExpandedPropertyGroups);

            // Load child folders
            foreach (FolderEntity childFolder in Profile.ProfileEntity.Folders.Where(f => f.ParentId == EntityId))
                ChildrenList.Add(new Folder(Profile, this, childFolder));
            // Load child layers
            foreach (LayerEntity childLayer in Profile.ProfileEntity.Layers.Where(f => f.ParentId == EntityId))
                ChildrenList.Add(new Layer(Profile, this, childLayer));

            // Ensure order integrity, should be unnecessary but no one is perfect specially me
            ChildrenList = ChildrenList.OrderBy(c => c.Order).ToList();
            for (int index = 0; index < ChildrenList.Count; index++)
                ChildrenList[index].Order = index + 1;

            LoadRenderElement();
        }

        internal override void Save()
        {
            if (Disposed)
                throw new ObjectDisposedException("Folder");

            FolderEntity.Id = EntityId;
            FolderEntity.ParentId = Parent?.EntityId ?? new Guid();

            FolderEntity.Order = Order;
            FolderEntity.Name = Name;
            FolderEntity.Enabled = Enabled;

            FolderEntity.ProfileId = Profile.EntityId;
            FolderEntity.ExpandedPropertyGroups.Clear();
            FolderEntity.ExpandedPropertyGroups.AddRange(_expandedPropertyGroups);

            SaveRenderElement();
        }

        #region Rendering

        public override void Render(SKCanvas canvas, SKImageInfo canvasInfo)
        {
            if (Disposed)
                throw new ObjectDisposedException("Folder");

            if (!Enabled || !Children.Any(c => c.Enabled))
                return;

            // Ensure the folder is ready
            if (Path == null)
                return;

            RenderFolder(Timeline, canvas, canvasInfo);
        }

        private void PrepareForRender(Timeline timeline)
        {
            foreach (BaseLayerEffect baseLayerEffect in LayerEffects.Where(e => e.Enabled))
            {
                baseLayerEffect.BaseProperties?.Update(timeline);
                baseLayerEffect.Update(timeline.LastDelta.TotalSeconds);
            }
        }

        private void RenderFolder(Timeline timeline, SKCanvas canvas, SKImageInfo canvasInfo)
        {
            if (Timeline.IsFinished && LongestChildTimeline.IsFinished)
                return;

            PrepareForRender(timeline);

            if (_folderBitmap == null)
            {
                _folderBitmap = new SKBitmap(new SKImageInfo((int) Path.Bounds.Width, (int) Path.Bounds.Height));
            }
            else if (_folderBitmap.Info.Width != (int) Path.Bounds.Width || _folderBitmap.Info.Height != (int) Path.Bounds.Height)
            {
                _folderBitmap.Dispose();
                _folderBitmap = new SKBitmap(new SKImageInfo((int) Path.Bounds.Width, (int) Path.Bounds.Height));
            }

            using SKPath folderPath = new SKPath(Path);
            using SKCanvas folderCanvas = new SKCanvas(_folderBitmap);
            using SKPaint folderPaint = new SKPaint();
            folderCanvas.Clear();

            folderPath.Transform(SKMatrix.MakeTranslation(folderPath.Bounds.Left * -1, folderPath.Bounds.Top * -1));

            SKPoint targetLocation = Path.Bounds.Location;
            if (Parent is Folder parentFolder)
                targetLocation -= parentFolder.Path.Bounds.Location;

            canvas.Save();

            using SKPath clipPath = new SKPath(folderPath);
            clipPath.Transform(SKMatrix.MakeTranslation(targetLocation.X, targetLocation.Y));
            canvas.ClipPath(clipPath);

            foreach (BaseLayerEffect baseLayerEffect in LayerEffects.Where(e => e.Enabled))
                baseLayerEffect.PreProcess(folderCanvas, _folderBitmap.Info, folderPath, folderPaint);

            // No point rendering if the alpha was set to zero by one of the effects
            if (folderPaint.Color.Alpha == 0)
                return;

            // Iterate the children in reverse because the first layer must be rendered last to end up on top
            for (int index = Children.Count - 1; index > -1; index--)
            {
                folderCanvas.Save();
                ProfileElement profileElement = Children[index];
                profileElement.Render(folderCanvas, _folderBitmap.Info);
                folderCanvas.Restore();
            }

            // If required, apply the opacity override of the module to the root folder
            if (IsRootFolder && Profile.Module.OpacityOverride < 1)
            {
                double multiplier = Easings.SineEaseInOut(Profile.Module.OpacityOverride);
                folderPaint.Color = folderPaint.Color.WithAlpha((byte) (folderPaint.Color.Alpha * multiplier));
            }

            foreach (BaseLayerEffect baseLayerEffect in LayerEffects.Where(e => e.Enabled))
                baseLayerEffect.PostProcess(canvas, canvasInfo, folderPath, folderPaint);
            canvas.DrawBitmap(_folderBitmap, targetLocation, folderPaint);

            canvas.Restore();
        }

        #endregion

        #region Events

        public event EventHandler RenderPropertiesUpdated;

        private void OnRenderPropertiesUpdated()
        {
            RenderPropertiesUpdated?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}