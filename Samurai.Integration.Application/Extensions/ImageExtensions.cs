using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace Samurai.Integration.Application.Extensions
{
    public static class ImageExtensions
    {

        public static Bitmap ResizekeepAspectRatio(this Bitmap imgPhoto, int width, int height)
            => ResizekeepAspectRatioPrivate(imgPhoto, width, height);


        public static Bitmap ResizekeepAspectRatio(this Bitmap imgPhoto, int quality)
        {
            if (quality > 100) quality = 100;

            var percentage = (float)quality / 100;

            var width = Convert.ToInt32(imgPhoto.Width * percentage);
            var height = Convert.ToInt32(imgPhoto.Height * percentage);

            return ResizekeepAspectRatioPrivate(imgPhoto, width, height);
        }

        public static Bitmap ResizeImage(this Image image, int width, int height)
            => ResizeImagePrivate(image, width, height);


        public static Bitmap ResizeImage(this Image image, int quality)
        {
            if (quality > 100) quality = 100;

            var percentage = (float)quality / 100;

            var width = Convert.ToInt32(image.Width * percentage);
            var height = Convert.ToInt32(image.Height * percentage);

            return ResizeImagePrivate(image, width, height);
        }

        private static Bitmap ResizeImagePrivate(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        private static Bitmap ResizekeepAspectRatioPrivate(Bitmap imgPhoto, int Width, int Height)
        {
            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)Width / (float)sourceWidth);
            nPercentH = ((float)Height / (float)sourceHeight);
            if (nPercentH < nPercentW)
            {
                nPercent = nPercentH;
                destX = System.Convert.ToInt16((Width -
                              (sourceWidth * nPercent)) / 2);
            }
            else
            {
                nPercent = nPercentW;
                destY = System.Convert.ToInt16((Height -
                              (sourceHeight * nPercent)) / 2);
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap bmPhoto = new Bitmap(Width, Height,
                              PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(300, 300);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.Clear(Color.White);
            grPhoto.InterpolationMode =
                    InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(imgPhoto,
                new Rectangle(destX, destY, destWidth, destHeight),
                new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();
            return bmPhoto;
        }

        public static string ValidateExtension(string image)
        {
            string imageExtension = Path.GetExtension(image);
            string[] extensions = new string[] { ".jpg", ".png", ".jpeg" };

            if (!extensions.Contains(imageExtension))
                return Path.ChangeExtension(image, "jpg");

            return image;
        }
    }
}
