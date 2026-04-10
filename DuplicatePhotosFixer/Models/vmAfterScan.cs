using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace DuplicatePhotosFixer.Models
{
    public class vmAfterScan : INotifyPropertyChanged
    {
        #region Properties

        private int _duplicateCount;
        public int DuplicateCount
        {
            get { return _duplicateCount; }
            set
            {
                _duplicateCount = value;
                OnPropertyChanged(nameof(DuplicateCount));
            }
        }

        private string _totalSize;
        public string TotalSize
        {
            get { return _totalSize; }
            set
            {
                _totalSize = value;
                OnPropertyChanged(nameof(TotalSize));
            }
        }

        private string _description;
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        private ObservableCollection<BitmapImage> _thumbnailImages;
        public ObservableCollection<BitmapImage> ThumbnailImages
        {
            get { return _thumbnailImages; }
            set
            {
                _thumbnailImages = value;
                OnPropertyChanged(nameof(ThumbnailImages));
            }
        }

        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Constructor
        public vmAfterScan()
        {
            Init();
        }
        #endregion

        #region Initialization
        void Init()
        {
            try
            {
                // Load data from cGlobal
                DuplicateCount = cGlobal.TotalDulicateFilesCount;
                TotalSize = FormatFileSize(cGlobal.TotalDuplicateFileSize);

                // Build description
                UpdateDescription();

                // Load thumbnail images from duplicate files
                LoadThumbnailImages();
            }
            catch (Exception ex)
            {
                cGlobalSettings.oLogger.WriteLogException("vmDuplicateResult :: Init : ", ex);
            }
        }

        private void UpdateDescription()
        {
            try
            {
                Description = $"We found {DuplicateCount}" +
                    $" duplicates occupying {TotalSize} space on your computer. " + 
                             "It is recommended to delete these duplicate photos immediately to de-clutter your system and recover extra storage space.";
            }
            catch (Exception ex)
            {
                cGlobalSettings.oLogger.WriteLogException("UpdateDescription", ex);
            }
        }
        #endregion

        #region Helper Methods

        /// <summary>
        /// Collects up to 8 thumbnail BitmapImages from the actual duplicate groups found during scan.
        /// </summary>
        private void LoadThumbnailImages()
        {
            try
            {
                var images = new ObservableCollection<BitmapImage>();

                if (cGlobalSettings.arrAllFilesGrouping == null)
                {
                    ThumbnailImages = images;
                    return;
                }

                // Snapshot the collection safely — scan is already complete at this point
                var groups = cGlobalSettings.arrAllFilesGrouping.ToArray();
                int count = 0;

                foreach (var group in groups)
                {
                    if (count >= 8) break;
                    if (group?.childrenKey == null) continue;

                    foreach (var key in group.childrenKey)
                    {
                        if (count >= 8) break;
                        if (!cGlobalSettings.listImageFileInfo.TryGetValue(key, out var fileInfo)) continue;

                        // Use ThumbnailPath if available, otherwise use actual file path
                        string imgPath = !string.IsNullOrEmpty(fileInfo.ThumbnailPath) && File.Exists(fileInfo.ThumbnailPath)
                            ? fileInfo.ThumbnailPath
                            : fileInfo.filePath;

                        if (string.IsNullOrEmpty(imgPath) || !File.Exists(imgPath)) continue;

                        try
                        {
                            BitmapImage bmp;
                            using (var stream = File.OpenRead(imgPath))
                            {
                                bmp = new BitmapImage();
                                bmp.BeginInit();
                                bmp.StreamSource = stream;
                                bmp.DecodePixelWidth = 84;
                                bmp.CacheOption = BitmapCacheOption.OnLoad;
                                bmp.EndInit();
                                bmp.Freeze();
                            }
                            images.Add(bmp);
                            count++;
                        }
                        catch { }
                    }
                }

                ThumbnailImages = images;
            }
            catch (Exception ex)
            {
                cGlobalSettings.oLogger.WriteLogException("vmAfterScan :: LoadThumbnailImages", ex);
            }
        }

        /// <summary>
        /// Format file size in human-readable format
        /// </summary>
        private string FormatFileSize(long sizeInBytes)
        {
            try
            {
                if (sizeInBytes == 0) return "0 MB";

                string[] sizes = { "bytes", "KB", "MB", "GB", "TB" };
                double size = sizeInBytes;
                int order = 0;

                while (size >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    size = size / 1024;
                }

                return $"{size:0.#} {sizes[order]}";
            }
            catch
            {
                return $"{sizeInBytes} bytes";
            }
        }
        #endregion

        #region Button Actions
        public void OnSelectManually()
        {
            try
            {
                // Navigate to manual selection screen
                // Add your navigation logic here
                cGlobalSettings.oLogger.WriteLog("User selected manual selection");

                //  logic to open the results view where user can manually select duplicates
            }
            catch (Exception ex)
            {
                cGlobalSettings.oLogger.WriteLogException("OnSelectManually", ex);
            }
        }

        public void OnAutoMark()
        {
            try
            {
                // Auto mark duplicates based on your algorithm
                cGlobalSettings.oLogger.WriteLog("User selected auto mark");

                // Add your auto-marking logic here
                // This could automatically select duplicates based on criteria like:
                // - Keep oldest/newest file
                // - Keep highest resolution
                // - Keep file with shortest path, etc.
            }
            catch (Exception ex)
            {
                cGlobalSettings.oLogger.WriteLogException("OnAutoMark", ex);
            }
        }
        #endregion
    }
}
