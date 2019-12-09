using System.Drawing;
using System.Threading.Tasks;

namespace IPIO.Core.Interfaces
{
    public interface IStringEmbeddingAlgorithm
    {
        Task<Bitmap> EmbedAsync(Bitmap bitmap, string message);

        Task<string> RetrieveAsync(Bitmap bitmap);
    }
}
