using Microsoft.UI.Xaml;

namespace HandySub.DiffViewer
{
    public interface ISideBySideDiffViewer
    {
        void RenderDiff(string left, string right, ElementTheme theme);
    }
}
