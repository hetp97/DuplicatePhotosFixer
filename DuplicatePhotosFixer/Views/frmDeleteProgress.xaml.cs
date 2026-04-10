using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using Microsoft.VisualBasic.FileIO;

namespace DuplicatePhotosFixer.Views
{
    public partial class frmDeleteProgress : Window
    {
        private CancellationTokenSource _cts;
        private readonly List<string> _filePaths;

        /// <summary>Number of files successfully deleted.</summary>
        public int DeletedCount { get; private set; }

        /// <summary>Paths that were successfully deleted (for post-delete cleanup).</summary>
        public List<string> DeletedPaths { get; } = new List<string>();

        public frmDeleteProgress(List<string> filePaths)
        {
            InitializeComponent();
            _filePaths = filePaths;
            txt_deleteMarkedCount.Text = filePaths.Count.ToString();
            txt_deletedCount.Text = "0";
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Start ring rotation animation
            var animation = new DoubleAnimation(0, 360, TimeSpan.FromSeconds(1.5))
            {
                RepeatBehavior = RepeatBehavior.Forever
            };
            ringRotation.BeginAnimation(System.Windows.Media.RotateTransform.AngleProperty, animation);

            // Start deleting
            _cts = new CancellationTokenSource();
            bool completed = await DeleteFilesAsync(_cts.Token);

            // Stop animation
            ringRotation.BeginAnimation(System.Windows.Media.RotateTransform.AngleProperty, null);

            if (completed)
            {
                DialogResult = true;
            }
            else
            {
                DialogResult = false;
            }
            Close();
        }

        private async Task<bool> DeleteFilesAsync(CancellationToken ct)
        {
            int deleted = 0;

            foreach (var path in _filePaths)
            {
                if (ct.IsCancellationRequested)
                    return false;

                try
                {
                    if (File.Exists(path))
                    {
                        FileSystem.DeleteFile(
                            path,
                            UIOption.OnlyErrorDialogs,
                            RecycleOption.SendToRecycleBin);

                        DeletedPaths.Add(path);
                        deleted++;
                        DeletedCount = deleted;

                        // Update UI on dispatcher thread
                        Dispatcher.Invoke(() =>
                        {
                            txt_deletedCount.Text = deleted.ToString();
                        });
                    }
                    else
                    {
                        // File already gone — count as deleted
                        DeletedPaths.Add(path);
                        deleted++;
                        DeletedCount = deleted;
                        Dispatcher.Invoke(() => txt_deletedCount.Text = deleted.ToString());
                    }
                }
                catch (Exception ex)
                {
                    cGlobalSettings.oLogger?.WriteLogException($"Failed to delete: {path}", ex);
                }

                // Small delay so UI updates are visible
                await Task.Delay(80, CancellationToken.None);
            }

            return true;
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
        }
    }
}
