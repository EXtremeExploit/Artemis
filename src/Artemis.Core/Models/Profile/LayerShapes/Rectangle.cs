﻿using Artemis.Storage.Entities.Profile;
using SkiaSharp;

namespace Artemis.Core.Models.Profile.LayerShapes
{
    public class Rectangle : LayerShape
    {
        public Rectangle(Layer layer) : base(layer)
        {
        }

        internal Rectangle(Layer layer, ShapeEntity shapeEntity) : base(layer, shapeEntity)
        {
        }

        public override void CalculateRenderProperties(SKPoint shapePosition, SKSize shapeSize)
        {
            var width = Layer.AbsoluteRectangle.Width;
            var height = Layer.AbsoluteRectangle.Height;
            var rect = SKRect.Create(shapePosition.X * width, shapePosition.Y * height, shapeSize.Width * width, shapeSize.Height * height);
            var path = new SKPath();
            path.AddRect(rect);
            path.Transform(SKMatrix.MakeTranslation(Layer.Rectangle.Left, Layer.Rectangle.Top));

            RenderPath = path;
            RenderRectangle = path.GetRect();
        }

        public override void ApplyToEntity()
        {
            base.ApplyToEntity();
            Layer.LayerEntity.ShapeEntity.Type = ShapeEntityType.Rectangle;
        }
    }
}