using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.ComponentModel;
using System.Globalization;

namespace WindowsFormsApp1
{
    abstract class Filters
    {
        public int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }
        //уникальна для каждого фильтра 
        public abstract Color calculateNewPixelColor(Bitmap sourceImage, int x, int y);
        //General part
        public virtual Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                if (worker.CancellationPending)
                    return null;
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));


                }
            }
            return resultImage;
        }
    }
    class InvertFilter : Filters
    {
        public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            Color resultColor = Color.FromArgb(255 - sourceColor.R, 255 - sourceColor.G, 255 - sourceColor.B);
            return resultColor;

        }
    }
    class GrayScaleFilter : Filters
    {
        public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            int intensity = (int)(0.299 * sourceColor.R + 0.287 * sourceColor.G + 0.114 * sourceColor.B);
            Color resultColor = Color.FromArgb(intensity, intensity, intensity);
            return resultColor;


        }
    }
    class SepiaFilter : Filters
    {
        public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            int k = 15;
            int intensity = (int)(0.299 * sourceColor.R + 0.287 * sourceColor.G + 0.114 * sourceColor.B);
            int resultR = (int)(intensity + 2 * k);
            int resultG = (int)(intensity + 0.5 * k);
            int resultB = (int)(intensity - 1 * k);
            Color resultColor = Color.FromArgb(Clamp(resultR, 0, 255), Clamp(resultG, 0, 255), Clamp(resultB, 0, 255));
            return resultColor;


        }
    }
    class BrightnessFilter : Filters
    {
        public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            int k = 100;
            int resultR = (int)(sourceColor.R + k);
            int resultG = (int)(sourceColor.G + k);
            int resultB = (int)(sourceColor.B + k);
            Color resultColor = Color.FromArgb(Clamp(resultR, 0, 255), Clamp(resultG, 0, 255), Clamp(resultB, 0, 255));
            return resultColor;


        }
    }
    class CarryFilter : Filters
    {
        public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int k = Clamp((int)x - 50, 0, sourceImage.Width - 1);
            int l = y;
            return sourceImage.GetPixel(k, l);
        }
    }
    class WavesFilter1 : Filters
    {
        public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int k = Clamp((int)(x - (20 * Math.Sin(2 * Math.PI * y / 60))), 0, sourceImage.Width - 1);
            int l = y;

            return sourceImage.GetPixel(k, l);
        }
    }
    class WavesFilter2 : Filters
    {
        public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int k = Clamp((int)(x - (20 * Math.Sin(2 * Math.PI * x / 60))), 0, sourceImage.Width - 1);
            int l = y;

            return sourceImage.GetPixel(k, l);
        }
    }
    class RotationFilter : Filters
    {
        public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int x0 = sourceImage.Width / 2;
            int y0 = sourceImage.Height / 2;
            double angle = Math.PI / 6;

            int k = Clamp((int)((x - x0) * Math.Cos(angle) - (y - y0) * Math.Sin(angle) + x0), 0, sourceImage.Width - 1);
            int l = Clamp((int)((x - x0) * Math.Sin(angle) + (y - y0) * Math.Cos(angle) + y0), 0, sourceImage.Height - 1);

            return sourceImage.GetPixel(k, l);
        }
    }
    class GlassFilter : Filters
    {
        Random rand = new Random();
        public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int k = Clamp((int)(x + (rand.NextDouble() - 0.5) * 10), 0, sourceImage.Width - 1);
            int l = Clamp((int)(y + (rand.NextDouble() - 0.5) * 10), 0, sourceImage.Height - 1);

            return sourceImage.GetPixel(k, l);
        }
    }

    class MatrixFilter : Filters
    {
        protected float[,] kernel = null;
        protected MatrixFilter() { }
        public MatrixFilter(float[,] kernel)
        {
            this.kernel = kernel;
        }
        public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            float resultR = 0;
            float resultG = 0;
            float resultB = 0;

            for (int l = -radiusY; l <= radiusY; l++)
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + k, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    resultR += kernel[k + radiusX, l + radiusY] * neighborColor.R;
                    resultG += kernel[k + radiusX, l + radiusY] * neighborColor.G;
                    resultB += kernel[k + radiusX, l + radiusY] * neighborColor.B;
                }
            return Color.FromArgb(
            Clamp((int)resultR, 0, 255),
            Clamp((int)resultG, 0, 255),
            Clamp((int)resultB, 0, 255)
            );
        }
    }
    class BlurFilter : MatrixFilter
    {
        public BlurFilter()
        {
            int sizeX = 3;
            int sizeY = 3;
            kernel = new float[sizeX, sizeY];
            for (int i = 0; i < sizeX; i++)
                for (int j = 0; j < sizeY; j++)
                    kernel[i, j] = 1.0f / (float)(sizeX * sizeY);

        }
    }

    class GaussianFilter : MatrixFilter
    {
        public void createGaussianKernal(int radius, float sigma)
        {
            int size = 2 * radius + 1;
            kernel = new float[size, size];
            float norm = 0;
            for (int i = -radius; i <= radius; i++)
                for (int j = -radius; j <= radius; j++)
                {
                    kernel[i + radius, j + radius] = (float)(Math.Exp(-(i * i + j * j) / (2 * sigma * sigma)));
                    norm += kernel[i + radius, j + radius];
                }
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    kernel[i, j] /= norm;


        }
        public GaussianFilter()
        {
            createGaussianKernal(3, 2);
        }
    }
    class SobelFilterX : MatrixFilter
    {
        public SobelFilterX()
        {

            kernel = new float[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };

        }
    }
    class SobelFilterY : MatrixFilter
    {
        public SobelFilterY()
        {

            kernel = new float[,] { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };

        }
    }
    class SobelFilter : MatrixFilter
    {
        Filters FilterX;
        Filters FilterY;

        public SobelFilter()
        {

            FilterX = new SobelFilterX();
            FilterY = new SobelFilterY();

        }
        public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color dX = FilterX.calculateNewPixelColor(sourceImage, x, y);
            Color dY = FilterY.calculateNewPixelColor(sourceImage, x, y);
            float resultR = (float)Math.Sqrt(dX.R * dX.R + dY.R * dY.R);
            float resultG = (float)Math.Sqrt(dX.G * dX.G + dY.G * dY.G);
            float resultB = (float)Math.Sqrt(dX.B * dX.B + dY.B * dY.B);

            return Color.FromArgb(
            Clamp((int)resultR, 0, 255),
            Clamp((int)resultG, 0, 255),
            Clamp((int)resultB, 0, 255)
            );
        }
    }

    class SharpnessFilter : MatrixFilter
    {
        public SharpnessFilter()
        {
            kernel = new float[,] { { 0, -1, 0 }, { -1, 5, -1 }, { 0, -1, 0 } };
        }
    }
    class EmbossingFilterCalc : Filters
    {
        protected float[,] kernel = null;
        protected EmbossingFilterCalc() { }
        public EmbossingFilterCalc(float[,] kernel)
        {
            this.kernel = kernel;
        }
        public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            float resultR = 0;
            float resultG = 0;
            float resultB = 0;

            for (int l = -radiusY; l <= radiusY; l++)
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + k, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    float inten = (float)(0.299 * neighborColor.R + 0.287 * neighborColor.G + 0.114 * neighborColor.B);
                    resultR += (inten) * kernel[k + radiusX, l + radiusY];
                    resultG += (inten) * kernel[k + radiusX, l + radiusY];
                    resultB += (inten) * kernel[k + radiusX, l + radiusY];
                }
            return Color.FromArgb(
            Clamp(((int)resultR + 255) / 2, 0, 255),
            Clamp(((int)resultG + 255) / 2, 0, 255),
            Clamp(((int)resultB + 255) / 2, 0, 255)
            );
        }
    }
    class EmbossingFilter : EmbossingFilterCalc
    {
        public EmbossingFilter()
        {
            kernel = new float[,] { { 0, 1, 0 }, { 1, 0, -1 }, { 0, -1, 0 } };
        }
    }
    class SharraFilterX : MatrixFilter
    {
        public SharraFilterX()
        {

            kernel = new float[,] { { 3, 10, -3 }, { 10, 0, -10 }, { 3, 0, -3 } };

        }
    }
    class SharraFilterY : MatrixFilter
    {
        public SharraFilterY()
        {

            kernel = new float[,] { { 3, 10, 3 }, { 0, 0, 0 }, { -3, -10, -3 } };

        }
    }

    class SharraFilter : MatrixFilter
    {
        Filters FilterX;
        Filters FilterY;

        public SharraFilter()
        {

            FilterX = new SharraFilterX();
            FilterY = new SharraFilterY();

        }
        public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color dX = FilterX.calculateNewPixelColor(sourceImage, x, y);
            Color dY = FilterY.calculateNewPixelColor(sourceImage, x, y);
            float resultR = (float)Math.Sqrt(dX.R * dX.R + dY.R * dY.R);
            float resultG = (float)Math.Sqrt(dX.G * dX.G + dY.G * dY.G);
            float resultB = (float)Math.Sqrt(dX.B * dX.B + dY.B * dY.B);

            return Color.FromArgb(
            Clamp((int)resultR, 0, 255),
            Clamp((int)resultG, 0, 255),
            Clamp((int)resultB, 0, 255)
            );
        }
    }
    class PruitteFilterX : MatrixFilter
    {
        public PruitteFilterX()
        {

            kernel = new float[,] { { -1, 0, -1 }, { -1, 0, 1 }, { -1, 0, 1 } };

        }
    }
    class PruitteFilterY : MatrixFilter
    {
        public PruitteFilterY()
        {

            kernel = new float[,] { { -1, -1, -1 }, { 0, 0, 0 }, { 1, 1, 1 } };

        }
    }
    class PruitteFilter : MatrixFilter
    {
        Filters FilterX;
        Filters FilterY;

        public PruitteFilter()
        {

            FilterX = new PruitteFilterX();
            FilterY = new PruitteFilterY();

        }
        public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color dX = FilterX.calculateNewPixelColor(sourceImage, x, y);
            Color dY = FilterY.calculateNewPixelColor(sourceImage, x, y);
            float resultR = (float)Math.Sqrt(dX.R * dX.R + dY.R * dY.R);
            float resultG = (float)Math.Sqrt(dX.G * dX.G + dY.G * dY.G);
            float resultB = (float)Math.Sqrt(dX.B * dX.B + dY.B * dY.B);

            return Color.FromArgb(
            Clamp((int)resultR, 0, 255),
            Clamp((int)resultG, 0, 255),
            Clamp((int)resultB, 0, 255)
            );
        }
    }
    class SharpnessFilter2 : MatrixFilter
    {
        public SharpnessFilter2()
        {
            kernel = new float[,] { { -1, -1, -1 }, { -1, 9, -1 }, { -1, -1, -1 } };
        }
    }
    class MotionBlurFilter : MatrixFilter
    {
        public MotionBlurFilter()
        {
            int n = 9;
            kernel = new float[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i == j)
                    {
                        kernel[i, j] = 1.0f / n;
                    }
                    else
                    {
                        kernel[i, j] = 0;
                    }
                }
            }
        }
    }
    class GrayWorldFilter
    {
        public int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }
            if (value > max)
            {
                return max;
            }
            return value;
        }

        public Bitmap processImage(Bitmap sourceImage)
        {
            float N = sourceImage.Width * sourceImage.Height;
            int sumR = 0;
            int sumG = 0;
            int sumB = 0;

            for (int i = 0; i < sourceImage.Width; i++)
            {
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    Color sourcecolor1 = sourceImage.GetPixel(i, j);
                    sumR += sourcecolor1.R;
                    sumG += sourcecolor1.G;
                    sumB += sourcecolor1.B;

                }
            }

            float avgR = sumR / N;
            float avgG = sumG / N;
            float avgB = sumB / N;
            double avg = (avgR + avgG + avgB) / 3;

            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);

            for (int i = 0; i < sourceImage.Width; i++)
            {
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    Color sourceColor = sourceImage.GetPixel(i, j);
                    Color resultColor = Color.FromArgb(Clamp((int)(sourceColor.R * avg / avgR), 0, 255), Clamp((int)(sourceColor.G * avg / avgG), 0, 255), Clamp((int)(sourceColor.B * avg / avgB), 0, 255));
                    resultImage.SetPixel(i, j, resultColor);

                }
            }
            return resultImage;

        }
    }
    class GistFilter
    {
        public int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }
            if (value > max)
            {
                return max;
            }
            return value;
        }
        public Bitmap processImage(Bitmap sourceImage)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            float min = 255;
            float max = 0;
            for (int i = 0; i < sourceImage.Width; i++)
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    float intensity = (sourceImage.GetPixel(i, j).R + sourceImage.GetPixel(i, j).G + sourceImage.GetPixel(i, j).B) / 3;
                    if (intensity > max)
                    {
                        max = intensity;
                    }
                    if (intensity < min)
                    {
                        min = intensity;
                    }
                }
            if (min == max)
            {
                max++;
            }
            for (int i = 0; i < sourceImage.Width; i++)
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    float intensity = (sourceImage.GetPixel(i, j).R + sourceImage.GetPixel(i, j).G + sourceImage.GetPixel(i, j).B) / 3;
                    float g = (intensity - min) * (255 / (max - min));
                    float resR = sourceImage.GetPixel(i, j).R / intensity;
                    float resG = sourceImage.GetPixel(i, j).G / intensity;
                    float resB = sourceImage.GetPixel(i, j).B / intensity;
                    Color resultColor = Color.FromArgb(Clamp((int)(g * resR), 0, 255), Clamp((int)(g * resG), 0, 255), Clamp((int)(g * resB), 0, 255));
                    resultImage.SetPixel(i, j, resultColor);
                }
            return resultImage;
        }
    }
    class MedianFilter : MatrixFilter
    {
        int Sort(int n, int[] mass)
        {
            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 0; j < n - i - 1; j++)
                {
                    if (mass[j] > mass[j + 1])
                    {
                        int temp;
                        temp = mass[j];
                        mass[j] = mass[j + 1];
                        mass[j + 1] = temp;
                    }
                }
            }
            return (mass[n / 2 + 1]);
        }

        public Bitmap processImage(int radius, Bitmap image)
        {
            int[] massR = new Int32[radius * radius];
            int[] massG = new Int32[radius * radius];
            int[] massB = new Int32[radius * radius];
            Bitmap resultImage = new Bitmap(image.Width, image.Height);
            int resultR = 0;
            int resultG = 0;
            int resultB = 0;
            int kkk = image.Height;
            for (int x = radius / 2 + 1; x < image.Height - radius / 2 - 2; x++)
                for (int y = (radius / 2 + 1); y < image.Width - radius / 2 - 2; y++)
                {
                    int k = 0;
                    for (int i = y - radius / 2; i < y + radius / 2 + 1; i++)
                    {
                        for (int j = x - radius / 2; j < x + radius / 2 + 1; j++)
                        {
                            Color sourceColor = image.GetPixel(i, j);
                            massR[k] = (Int32)(sourceColor.R);
                            massG[k] = (Int32)(sourceColor.G);
                            massB[k] = (Int32)(sourceColor.B);
                            k++;
                        }
                    }
                    resultR = Sort(radius * radius, massR);
                    resultG = Sort(radius * radius, massG);
                    resultB = Sort(radius * radius, massB);
                    Color resultColor = Color.FromArgb(
                    Clamp(resultR, 0, 255),
                    Clamp(resultG, 0, 255),
                    Clamp(resultB, 0, 255)
                    );
                    resultImage.SetPixel(y, x, resultColor);
                }
            return resultImage;
        }
    }
    abstract class MorfologeFilter : Filters
    {
        protected int MW = 3, MH = 3;
        protected int[,] Mask = { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } };

        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);

            for (int i = MW / 2; i < sourceImage.Width - MW / 2; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                if (worker.CancellationPending)
                    return null;

                for (int j = MH / 2; j < sourceImage.Height - MH / 2; j++)
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
            }

            return resultImage;
        }
    }
    class ErosionFilter : MorfologeFilter
    {
        public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color min = Color.FromArgb(255, 255, 255);

            for (int j = -MH / 2; j <= MH / 2; j++)
                for (int i = -MW / 2; i <= MW / 2; i++)
                {
                    Color pixel = sourceImage.GetPixel(x + i, y + j);

                    if (Mask[i + MW / 2, j + MH / 2] != 0 && pixel.R < min.R && pixel.G < min.G && pixel.B < min.B)
                        min = pixel;
                }

            return min;
        }
    }
    class DilationFilter : MorfologeFilter
    {
        public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color max = Color.FromArgb(0, 0, 0);

            for (int j = -MH / 2; j <= MH / 2; j++)
                for (int i = -MW / 2; i <= MW / 2; i++)
                {
                    Color pixel = sourceImage.GetPixel(x + i, y + j);

                    if (Mask[i + MW / 2, j + MH / 2] == 1 && pixel.R > max.R && pixel.G > max.G && pixel.B > max.B)
                        max = pixel;
                }

            return max;
        }
    }
    class OpeningFilter : MorfologeFilter
    {
        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap erosion = new Bitmap(sourceImage.Width, sourceImage.Height);

            for (int i = MW / 2; i < erosion.Width - MW / 2; i++)
            {
                worker.ReportProgress((int)((float)i / erosion.Width * 50));
                if (worker.CancellationPending)
                    return null;

                for (int j = MH / 2; j < erosion.Height - MH / 2; j++)
                    erosion.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
            }

            Bitmap result = new Bitmap(erosion);

            for (int i = MW / 2; i < result.Width - MW / 2; i++)
            {
                worker.ReportProgress((int)((float)i / result.Width * 50 + 50));
                if (worker.CancellationPending)
                    return null;

                for (int j = MH / 2; j < result.Height - MH / 2; j++)
                    result.SetPixel(i, j, calcDilation(erosion, i, j));
            }

            return result;
        }

        public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color min = Color.FromArgb(255, 255, 255);

            for (int j = -MH / 2; j <= MH / 2; j++)
                for (int i = -MW / 2; i <= MW / 2; i++)
                {
                    Color pixel = sourceImage.GetPixel(x + i, y + j);

                    if (Mask[i + MW / 2, j + MH / 2] != 0 && pixel.R < min.R && pixel.G < min.G && pixel.B < min.B)
                        min = pixel;
                }

            return min;
        }

        private Color calcDilation(Bitmap sourceImage, int x, int y)
        {
            Color max = Color.FromArgb(0, 0, 0);

            for (int j = -MH / 2; j <= MH / 2; j++)
                for (int i = -MW / 2; i <= MW / 2; i++)
                {
                    Color pixel = sourceImage.GetPixel(x + i, y + j);

                    if (Mask[i + MW / 2, j + MH / 2] == 1 && pixel.R > max.R && pixel.G > max.G && pixel.B > max.B)
                        max = pixel;
                }

            return max;
        }
    }

    class ClosingFilter : MorfologeFilter
    {
        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap dilation = new Bitmap(sourceImage.Width, sourceImage.Height);

            for (int i = MW / 2; i < dilation.Width - MW / 2; i++)
            {
                worker.ReportProgress((int)((float)i / dilation.Width * 50));
                if (worker.CancellationPending)
                    return null;

                for (int j = MH / 2; j < dilation.Height - MH / 2; j++)
                    dilation.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
            }

            Bitmap result = new Bitmap(dilation);

            for (int i = MW / 2; i < result.Width - MW / 2; i++)
            {
                worker.ReportProgress((int)((float)i / result.Width * 50 + 50));
                if (worker.CancellationPending)
                    return null;

                for (int j = MH / 2; j < result.Height - MH / 2; j++)
                    result.SetPixel(i, j, calcErosion(dilation, i, j));
            }

            return result;
        }

        public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {

            Color max = Color.FromArgb(0, 0, 0);

            for (int j = -MH / 2; j <= MH / 2; j++)
                for (int i = -MW / 2; i <= MW / 2; i++)
                {
                    Color pixel = sourceImage.GetPixel(x + i, y + j);

                    if (Mask[i + MW / 2, j + MH / 2] == 1 && pixel.R > max.R && pixel.G > max.G && pixel.B > max.B)
                        max = pixel;
                }

            return max;
        }

        private Color calcErosion(Bitmap sourceImage, int x, int y)
        {
            Color min = Color.FromArgb(255, 255, 255);

            for (int j = -MH / 2; j <= MH / 2; j++)
                for (int i = -MW / 2; i <= MW / 2; i++)
                {
                    Color pixel = sourceImage.GetPixel(x + i, y + j);

                    if (Mask[i + MW / 2, j + MH / 2] != 0 && pixel.R < min.R && pixel.G < min.G && pixel.B < min.B)
                        min = pixel;
                }

            return min;
        }
    }

}
