using IPIO.Core.Algorithms.Formulas;
using System.Drawing;
using System.Threading.Tasks;

namespace IPIO.Core.Interfaces
{
    public interface IWatermarkingAlgorithm
    {
        public IFormula Formula { get; }

        Task<Bitmap> EmbedAsync(Bitmap originalImage, Bitmap message);

        Task<Bitmap> RetrieveAsync(Bitmap originalImage, Bitmap watermarkedImage, int watermarkLength);
    }
}