using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace HandySub.Common
{
    public static class ContentDialogExtension
    {
        private static TaskCompletionSource<ContentDialog> _contentDialogShowRequest;

        public static async Task<ContentDialogResult> ShowAsyncQueue(this ContentDialog dialog)
        {
            while (_contentDialogShowRequest != null)
            {
                await _contentDialogShowRequest.Task;
            }

            var request = _contentDialogShowRequest = new TaskCompletionSource<ContentDialog>();
            var result = await dialog.ShowAsync();
            _contentDialogShowRequest = null;
            request.SetResult(dialog);

            return result;
        }
    }
}
