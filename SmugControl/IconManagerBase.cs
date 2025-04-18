using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SmugControl
{
    public abstract class IconManagerBase
    {
        // default image
        private static ImageSource Xbox;

        public Dictionary<int, ImageSource> ImageSourceDic;

        public abstract void LoadImage();

        public void Initialize(string path, string filenamePrefix, string extention, int left, int top, int width, int height, int rows, int cols)
        {
            ImageSourceDic = new Dictionary<int, ImageSource>();
            string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);

            if (false == Directory.Exists(imagePath)) return;

            var filelist = Directory.GetFiles(imagePath, $"{filenamePrefix}*.{extention}").OrderBy(x => x);
            foreach (var filepath in filelist)
            {
                FileInfo info = new FileInfo(filepath);
                if (false == info.Exists) continue;

                if (false == int.TryParse(info.Name.Replace(filenamePrefix, "").Replace($".{extention}", ""), out int oFileNo))
                    continue;

                int No = oFileNo * rows * cols;

                Image img = new Image()
                {
                    Source = new BitmapImage(new Uri(filepath))
                };

                int x = left;
                int y = top;

                for (int r = 0; r < rows; r++)
                {
                    x = left;
                    for (int c = 0; c < cols; c++)
                    {
                        CroppedBitmap cropped = new CroppedBitmap(img.Source as BitmapSource, new Int32Rect(x, y, width, height));
                        ImageSourceDic.Add(No++, cropped);
                        x += width;
                    }
                    y += height;
                }
                //oFileNo
                //CroppedBitmap
            }
        }

        public ImageSource Get(int No) => ImageSourceDic.ContainsKey(No) ? ImageSourceDic[No] : Xbox;
    }
}