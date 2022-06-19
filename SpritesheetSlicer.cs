using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

internal class SpritesheetSlicer
{
    public int Cells = 0;
    public StringBuilder logBuilder { get; set; }
    public string Name { get; set; }

    private Bitmap spritesheetBitmap;

    public SpritesheetSlicer(Bitmap image, string name)
    {
        spritesheetBitmap = image;
        Name = name;
    }

    public void WriteImagesToDisk(string directory, List<Bitmap> images)
    {
        for (int i = 0; i < images.Count; i++)
        {
            var img = images[i];
            logBuilder.AppendLine($"Wrote image {i} for {Name}");
            img.Save(Path.Combine(directory, $"{Name}_{i:0000}.png"), ImageFormat.Png);
        }
    }

    public List<Bitmap> SliceUsingInput(SizeInput input)
    {
        switch (input.type)
        {
            case SizeType.Dimensions:
                return SliceToDimensions(input.w, input.h);
            case SizeType.Grid:
                return SliceToGridSize(input.w, input.h);
            default:
                throw new ArgumentException("Provided input SizeType was not valid.");
        }
    }

    public List<Bitmap> SliceToDimensions(int widthpx, int heightpx)
    {
        return SliceTo(new (spritesheetBitmap.Width / widthpx, spritesheetBitmap.Height / heightpx), new (widthpx, heightpx));
    }

    public List<Bitmap> SliceToGridSize(int width, int height)
    {
        return SliceTo(new (width, height), new(spritesheetBitmap.Width / width, spritesheetBitmap.Height / height));
    }

    private List<Bitmap> SliceTo(Vector2 grid, Vector2 sliceSize)
    {
        if (spritesheetBitmap == null) return null;

        int cells = Cells;
        if (cells <= 0)
        {
            cells = (int)(grid.X * grid.Y);
        }
        List<Bitmap> slicedImages = new List<Bitmap>(cells);

        for (int i = 0; i < cells; i++)
        {
            int currentFrame = i;
            int yRow = (int)Math.Floor(currentFrame / grid.X);
            Vector2 rowColumn = new(currentFrame % grid.X, yRow);

            int startX = 0;
            int startY = 0;
            if(rowColumn.X > 0)
            {
                startX = (int)(sliceSize.X * rowColumn.X);
            }
            if (rowColumn.Y > 0)
            {
                startY = (int)(sliceSize.Y * rowColumn.Y);
            }

            var slicedImage = SliceSingle((int)sliceSize.X, (int)sliceSize.Y, startX, startY);
            if(slicedImage != null)
            {
                slicedImages.Add(slicedImage);
            }
        }
        logBuilder.AppendLine($"Spliced image '{Name}' into '{cells}' seperate images");
        return slicedImages;
    }

    public void WriteMKSForSprites(int cellCount, string directory)
    {
        using (var stream = File.CreateText(Path.Combine(directory, $"{Name}.mks")))
        {
            stream.WriteLine("sequence 0");

            for(int i = 0; i < cellCount; i++)
            {
                stream.WriteLine($"frame {Name}_{i:0000}.png 1");
            }
        }
    }

    private Bitmap? SliceSingle(int width, int height, int startX, int startY)
    {
        logBuilder.AppendLine($"Creating slice ({width}x{height}) at x:{startX} y:{startY}");
        Bitmap slicedBitmap = new Bitmap(width, height);
        bool allTransparent = true;
        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                Color color = spritesheetBitmap.GetPixel(startX + x, startY + y);
                if (color.A != 0) allTransparent = false;
                slicedBitmap.SetPixel(x, y, color);
            }
        }
        return allTransparent ? null : slicedBitmap;
    }
}
