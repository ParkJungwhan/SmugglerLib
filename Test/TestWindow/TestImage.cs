using SmugControl;

namespace TestWindow
{
    public class TestImage : IconManagerBase
    {
        public override void LoadImage()
        {
            Initialize(@"Data\Images", "Item_Icon_", "png", 1, 1, 89, 89, 11, 11);
        }
    }
}