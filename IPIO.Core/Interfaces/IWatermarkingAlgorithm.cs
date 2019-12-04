using System.Drawing;
using System.Threading.Tasks;

namespace IPIO.Core.Interfaces
{
    public interface IWatermarkingAlgorithm
    {
        Task<Bitmap> EmbedAsync(Bitmap originalImage, Bitmap message);

        Task<Bitmap> RetrieveAsync(Bitmap originalImage, Bitmap watermarkedImage, int watermarkLength);
    }
}
