using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace TestWindow
{
    /// <summary>
    /// ImageViewControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ImageViewControl : UserControl
    {
        public ObservableCollection<CardModel> DataList { get; set; }

        public ImageViewControl()
        {
            InitializeComponent();

            TestImage images = new TestImage();
            images.LoadImage();

            List<CardModel> lstCard = new List<CardModel>();
            foreach (var img in images.ImageSourceDic)
            {
                CardModel card = new CardModel
                {
                    No = img.Key,
                    ContentImg = images.Get(img.Key)
                };
                lstCard.Add(card);
            }

            DataList = new ObservableCollection<CardModel>(lstCard);

            //dgList.ItemsSource = DataList;
            lstImg.ItemsSource = DataList;
        }
    }

    public class CardModel
    {
        public int No { get; set; }

        public ImageSource ContentImg { get; set; }
    }
}