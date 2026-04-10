using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DuplicatePhotosFixer.ClassDictionary;

namespace DuplicatePhotosFixer.Models
{
    public class vmResultView : INotifyPropertyChanged
    {
        #region Properties

        private ObservableCollection<DuplicateGroup> _duplicateGroups;
        public ObservableCollection<DuplicateGroup> DuplicateGroups
        {
            get { return _duplicateGroups; }
            set
            {
                _duplicateGroups = value;
                OnPropertyChanged("DuplicateGroups");
            }
        }

        /// <summary>
        /// Flat list of all duplicate files for the DataGrid detailed view.
        /// Each item carries groupNumber, fileName, filePath etc. from csImageFileInfo.
        /// </summary>
        private ObservableCollection<DetailedViewRow> _detailedRows;
        public ObservableCollection<DetailedViewRow> DetailedRows
        {
            get { return _detailedRows; }
            set
            {
                _detailedRows = value;
                OnPropertyChanged("DetailedRows");
            }
        }

        private int _totalDuplicates;
        public int TotalDuplicates
        {
            get { return _totalDuplicates; }
            set
            {
                _totalDuplicates = value;
                OnPropertyChanged("TotalDuplicates");
            }
        }

        private string _spaceToSave;
        public string SpaceToSave
        {
            get { return _spaceToSave; }
            set
            {
                _spaceToSave = value;
                OnPropertyChanged("SpaceToSave");
            }
        }

        private bool _isImageView = true;
        public bool IsImageView
        {
            get { return _isImageView; }
            set
            {
                _isImageView = value;
                OnPropertyChanged("IsImageView");
            }
        }

        /// <summary>
        /// The currently selected file (used for preview and metadata panel).
        /// </summary>
        private DuplicateFile _selectedFile;
        public DuplicateFile SelectedFile
        {
            get { return _selectedFile; }
            set
            {
                _selectedFile = value;
                OnPropertyChanged("SelectedFile");
                OnPropertyChanged("SelectedFilePath");
            }
        }

        /// <summary>Full path of the currently highlighted file — shown in the footer.</summary>
        public string SelectedFilePath => _selectedFile?.FilePath ?? "";

        // ── Footer: marked summary ──────────────────────────────────────────
        private int _markedCount;
        private long _markedSizeBytes;

        /// <summary>"X Marked (Y MB)" shown in the footer middle section.</summary>
        public string MarkedSummary
        {
            get => _markedCount > 0
                ? $"{_markedCount} Marked ({FormatFileSize(_markedSizeBytes)})"
                : "0 Marked";
        }

        /// <summary>"X duplicate photos in Y groups" shown in the footer left section.</summary>
        public string DuplicateSummary
        {
            get => $"{TotalDuplicates} duplicate photos in {DuplicateGroups?.Count ?? 0} groups";
        }

        #endregion

        private Dispatcher _uiDispatcher;

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                if (_uiDispatcher != null && !_uiDispatcher.CheckAccess())
                {
                    _uiDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        handler(this, new PropertyChangedEventArgs(propertyName));
                    }));
                }
                else
                {
                    handler(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }

        public vmResultView()
        {
            try
            {
                _uiDispatcher = System.Windows.Application.Current?.Dispatcher;
            }
            catch { }

            DuplicateGroups = new ObservableCollection<DuplicateGroup>();
            DetailedRows = new ObservableCollection<DetailedViewRow>();
            TotalDuplicates = 0;
            SpaceToSave = "0 KB";
        }

        /// <summary>
        /// Load duplicate groups from the engine's arrAllFilesGrouping (the authoritative
        /// list of duplicate groups populated during scanning).
        /// </summary>
        public void LoadDuplicateGroups()
        {
            try
            {
                cGlobalSettings.oLogger?.WriteLogVerbose("Loading duplicate groups...");

                DuplicateGroups.Clear();
                DetailedRows.Clear();
                TotalDuplicates = 0;
                long totalSize = 0;
                int groupIndex = 1;

                if (cGlobalSettings.arrAllFilesGrouping == null)
                {
                    cGlobalSettings.oLogger?.WriteLogVerbose("No duplicate data found (arrAllFilesGrouping is null)");
                    SpaceToSave = "0 KB";
                    return;
                }

                var allGroups = cGlobalSettings.arrAllFilesGrouping.ToArray();

                foreach (var group in allGroups)
                {
                    if (group?.childrenKey == null || group.childrenKey.Count < 2) continue;

                    var dupGroup = new DuplicateGroup
                    {
                        GroupId = groupIndex.ToString(),
                        GroupName = $"Group {groupIndex}",
                        Files = new ObservableCollection<DuplicateFile>(),
                        TotalSize = 0,
                        FileCount = 0
                    };

                    // Add a dedicated group header row (no file attached)
                    DetailedRows.Add(new DetailedViewRow
                    {
                        GroupNumber = groupIndex,
                        GroupLabel = $"Group {groupIndex}  ({group.childrenKey.Count} Photos)",
                        IsGroupHeader = true,
                        File = null,
                        Key = -1
                    });

                    foreach (var key in group.childrenKey)
                    {
                        if (!cGlobalSettings.listImageFileInfo.TryGetValue(key, out var file)) continue;

                        // Determine image path: use ThumbnailPath if it exists, otherwise use actual file
                        string imgPath = !string.IsNullOrEmpty(file.ThumbnailPath) && File.Exists(file.ThumbnailPath)
                            ? file.ThumbnailPath
                            : file.filePath;

                        // Load a BitmapImage for the UI (with DecodePixelWidth for performance)
                        BitmapImage bmpImage = null;
                        try
                        {
                            if (!string.IsNullOrEmpty(imgPath) && File.Exists(imgPath))
                            {
                                bmpImage = new BitmapImage();
                                bmpImage.BeginInit();
                                bmpImage.UriSource = new Uri(imgPath, UriKind.Absolute);
                                bmpImage.DecodePixelWidth = 270;
                                bmpImage.CacheOption = BitmapCacheOption.OnLoad;
                                bmpImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                                bmpImage.EndInit();
                                bmpImage.Freeze();
                            }
                        }
                        catch
                        {
                            bmpImage = null;
                        }

                        // Use dimensions from engine; fall back to loaded BitmapImage if 0
                        long imgWidth  = file.Width  > 0 ? file.Width  : (bmpImage?.PixelWidth  ?? 0);
                        long imgHeight = file.Height > 0 ? file.Height : (bmpImage?.PixelHeight ?? 0);

                        var dupFile = new DuplicateFile
                        {
                            FileId = file.key.ToString(),
                            FileName = file.fileName,
                            FilePath = file.filePath,
                            FileSize = file.fileSize,
                            FileSizeFormatted = file.fileSizeWithUnit ?? FormatFileSize(file.fileSize),
                            CreatedDate = file.createDate,
                            ModifiedDate = file.modDate,
                            ThumbnailPath = file.ThumbnailPath,
                            ImageSource = bmpImage,
                            IsSelected = false,
                            FolderPath = file.folderPath,
                            FileType = file.Filetype,
                            ImageWidth = imgWidth,
                            ImageHeight = imgHeight,
                            RowIndex = groupIndex
                        };

                        dupGroup.Files.Add(dupFile);

                        // Track IsSelected changes for footer Marked summary
                        dupFile.PropertyChanged += (s, e) =>
                        {
                            if (e.PropertyName == "IsSelected")
                                RecalculateMarked();
                        };

                        // Add file row (IsGroupHeader = false)
                        DetailedRows.Add(new DetailedViewRow
                        {
                            GroupNumber = groupIndex,
                            GroupLabel = "",
                            IsGroupHeader = false,
                            File = dupFile,
                            Key = file.key
                        });

                        dupGroup.TotalSize += file.fileSize;
                        dupGroup.FileCount++;
                        TotalDuplicates++;
                        totalSize += file.fileSize;
                    }

                    if (dupGroup.Files.Count > 0)
                    {
                        // Set group name from the first file's folder path
                        dupGroup.GroupName = $"Group {groupIndex}";
                        DuplicateGroups.Add(dupGroup);
                        groupIndex++;
                    }
                }

                SpaceToSave = FormatFileSize(totalSize);
                RecalculateMarked();
                OnPropertyChanged("DuplicateSummary");

                // Auto-select the first file for preview/metadata (Requirement 3)
                if (DuplicateGroups.Count > 0 && DuplicateGroups[0].Files.Count > 0)
                {
                    SelectFile(DuplicateGroups[0].Files[0]);
                }

                cGlobalSettings.oLogger?.WriteLogVerbose($"Loaded {DuplicateGroups.Count} duplicate groups, Total duplicates: {TotalDuplicates}, Total size: {SpaceToSave}");
            }
            catch (Exception ex)
            {
                cGlobalSettings.oLogger?.WriteLogException("LoadDuplicateGroups", ex);
            }
        }

        /// <summary>
        /// Select a file for preview/metadata display and highlight it with blue border.
        /// Clears the previous highlight.
        /// </summary>
        public void SelectFile(DuplicateFile file)
        {
            try
            {
                // Clear previous highlight
                if (SelectedFile != null)
                    SelectedFile.IsHighlighted = false;

                SelectedFile = file;

                if (file != null)
                    file.IsHighlighted = true;
            }
            catch (Exception ex)
            {
                cGlobalSettings.oLogger?.WriteLogException("vmResultView::SelectFile", ex);
            }
        }

        /// <summary>
        /// Auto mark duplicates (keep oldest, mark rest for deletion)
        /// </summary>
        public void AutoMark()
        {
            try
            {
                cGlobalSettings.oLogger?.WriteLogVerbose("Auto Mark started");

                foreach (var group in DuplicateGroups)
                {
                    if (group.Files.Count > 1)
                    {
                        // Sort by modified date, keep the oldest
                        var sorted = group.Files.OrderBy(f => f.ModifiedDate).ToList();

                        // Mark all except the first (oldest) one
                        for (int i = 1; i < sorted.Count; i++)
                        {
                            sorted[i].IsSelected = true;
                        }
                    }
                }

                RecalculateMarked();
                cGlobalSettings.oLogger?.WriteLogVerbose("Auto Mark completed");
            }
            catch (Exception ex)
            {
                cGlobalSettings.oLogger?.WriteLogException("AutoMark", ex);
            }
        }

        /// <summary>
        /// Unmark all duplicates
        /// </summary>
        public void UnmarkAll()
        {
            try
            {
                cGlobalSettings.oLogger?.WriteLogVerbose("Unmark All started");

                foreach (var group in DuplicateGroups)
                {
                    foreach (var file in group.Files)
                    {
                        file.IsSelected = false;
                    }
                }

                RecalculateMarked();
                cGlobalSettings.oLogger?.WriteLogVerbose("Unmark All completed");
            }
            catch (Exception ex)
            {
                cGlobalSettings.oLogger?.WriteLogException("UnmarkAll", ex);
            }
        }

        /// <summary>
        /// Recount marked files across all groups and update footer summary.
        /// </summary>
        private void RecalculateMarked()
        {
            long size = 0;
            int count = 0;
            foreach (var group in DuplicateGroups)
                foreach (var f in group.Files)
                    if (f.IsSelected) { count++; size += f.FileSize; }
            _markedCount = count;
            _markedSizeBytes = size;
            OnPropertyChanged("MarkedSummary");
        }

        /// <summary>
        /// Returns true if <paramref name="file"/> may be checked (n-1 rule:
        /// at least one file per group must remain unchecked).
        /// </summary>
        public bool CanCheckFile(DuplicateFile file)
        {
            var group = DuplicateGroups.FirstOrDefault(g => g.Files.Contains(file));
            if (group == null) return true;
            int alreadySelected = group.Files.Count(f => f.IsSelected);
            return alreadySelected < group.Files.Count - 1;
        }

        /// <summary>
        /// Returns the file paths of all currently marked (IsSelected) files.
        /// </summary>
        public List<string> GetMarkedFilePaths()
        {
            var paths = new List<string>();
            foreach (var group in DuplicateGroups)
                foreach (var f in group.Files)
                    if (f.IsSelected && !string.IsNullOrEmpty(f.FilePath))
                        paths.Add(f.FilePath);
            return paths;
        }

        /// <summary>
        /// Returns the formatted total size of all marked files.
        /// </summary>
        public string GetMarkedSizeFormatted()
        {
            return FormatFileSize(_markedSizeBytes);
        }

        /// <summary>
        /// After deletion, remove the deleted files from groups and rebuild DetailedRows.
        /// Groups that end up with fewer than 2 files are removed entirely.
        /// </summary>
        public void RemoveDeletedFiles(List<string> deletedPaths)
        {
            try
            {
                var deletedSet = new HashSet<string>(deletedPaths, StringComparer.OrdinalIgnoreCase);

                // Remove deleted files from each group
                for (int g = DuplicateGroups.Count - 1; g >= 0; g--)
                {
                    var group = DuplicateGroups[g];
                    for (int f = group.Files.Count - 1; f >= 0; f--)
                    {
                        if (deletedSet.Contains(group.Files[f].FilePath))
                            group.Files.RemoveAt(f);
                    }

                    // Remove group if it now has fewer than 2 files (no longer duplicates)
                    if (group.Files.Count < 2)
                        DuplicateGroups.RemoveAt(g);
                }

                // Recalculate stats
                TotalDuplicates = 0;
                long totalSize = 0;
                int groupIndex = 1;
                DetailedRows.Clear();

                foreach (var group in DuplicateGroups)
                {
                    group.GroupId = groupIndex.ToString();
                    group.GroupName = $"Group {groupIndex}";
                    group.FileCount = group.Files.Count;

                    // Add group header row
                    DetailedRows.Add(new DetailedViewRow
                    {
                        GroupNumber = groupIndex,
                        GroupLabel = $"Group {groupIndex}  ({group.Files.Count} Photos)",
                        IsGroupHeader = true,
                        File = null,
                        Key = -1
                    });

                    long groupSize = 0;
                    foreach (var file in group.Files)
                    {
                        file.RowIndex = groupIndex;
                        TotalDuplicates++;
                        totalSize += file.FileSize;
                        groupSize += file.FileSize;

                        DetailedRows.Add(new DetailedViewRow
                        {
                            GroupNumber = groupIndex,
                            GroupLabel = "",
                            IsGroupHeader = false,
                            File = file,
                            Key = int.TryParse(file.FileId, out int k) ? k : 0
                        });
                    }
                    group.TotalSize = groupSize;
                    groupIndex++;
                }

                SpaceToSave = FormatFileSize(totalSize);
                RecalculateMarked();
                OnPropertyChanged("DuplicateSummary");

                // Re-select first file if current selection was deleted
                if (SelectedFile != null && deletedSet.Contains(SelectedFile.FilePath))
                {
                    if (DuplicateGroups.Count > 0 && DuplicateGroups[0].Files.Count > 0)
                        SelectFile(DuplicateGroups[0].Files[0]);
                    else
                        SelectFile(null);
                }

                cGlobalSettings.oLogger?.WriteLogVerbose($"RemoveDeletedFiles: {deletedPaths.Count} removed, {DuplicateGroups.Count} groups remain");
            }
            catch (Exception ex)
            {
                cGlobalSettings.oLogger?.WriteLogException("RemoveDeletedFiles", ex);
            }
        }

        /// <summary>
        /// Format file size to human-readable format
        /// </summary>
        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }
    }

    /// <summary>
    /// Represents a group of duplicate files
    /// </summary>
    public class DuplicateGroup : INotifyPropertyChanged
    {
        private string _groupId;
        public string GroupId
        {
            get { return _groupId; }
            set
            {
                _groupId = value;
                OnPropertyChanged("GroupId");
            }
        }

        private string _groupName;
        public string GroupName
        {
            get { return _groupName; }
            set
            {
                _groupName = value;
                OnPropertyChanged("GroupName");
            }
        }

        private ObservableCollection<DuplicateFile> _files;
        public ObservableCollection<DuplicateFile> Files
        {
            get { return _files; }
            set
            {
                _files = value;
                OnPropertyChanged("Files");
            }
        }

        /// <summary>Alias used by ucResImageViewer XAML binding</summary>
        public ObservableCollection<DuplicateFile> Photos
        {
            get { return _files; }
        }

        /// <summary>Alias used by ucResImageViewer XAML binding</summary>
        public int PhotoCount
        {
            get { return _fileCount; }
        }

        private long _totalSize;
        public long TotalSize
        {
            get { return _totalSize; }
            set
            {
                _totalSize = value;
                OnPropertyChanged("TotalSize");
            }
        }

        private int _fileCount;
        public int FileCount
        {
            get { return _fileCount; }
            set
            {
                _fileCount = value;
                OnPropertyChanged("FileCount");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    /// <summary>
    /// Represents a single duplicate file
    /// </summary>
    public class DuplicateFile : INotifyPropertyChanged
    {
        private string _fileId;
        public string FileId
        {
            get { return _fileId; }
            set
            {
                _fileId = value;
                OnPropertyChanged("FileId");
            }
        }

        private string _fileName;
        public string FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
                OnPropertyChanged("FileName");
            }
        }

        private string _filePath;
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                _filePath = value;
                OnPropertyChanged("FilePath");
            }
        }

        private long _fileSize;
        public long FileSize
        {
            get { return _fileSize; }
            set
            {
                _fileSize = value;
                OnPropertyChanged("FileSize");
            }
        }

        private string _fileSizeFormatted;
        public string FileSizeFormatted
        {
            get { return _fileSizeFormatted; }
            set
            {
                _fileSizeFormatted = value;
                OnPropertyChanged("FileSizeFormatted");
            }
        }

        private DateTime _createdDate;
        public DateTime CreatedDate
        {
            get { return _createdDate; }
            set
            {
                _createdDate = value;
                OnPropertyChanged("CreatedDate");
            }
        }

        private DateTime _modifiedDate;
        public DateTime ModifiedDate
        {
            get { return _modifiedDate; }
            set
            {
                _modifiedDate = value;
                OnPropertyChanged("ModifiedDate");
            }
        }

        private string _thumbnailPath;
        public string ThumbnailPath
        {
            get { return _thumbnailPath; }
            set
            {
                _thumbnailPath = value;
                OnPropertyChanged("ThumbnailPath");
            }
        }

        private BitmapImage _imageSource;
        public BitmapImage ImageSource
        {
            get { return _imageSource; }
            set
            {
                _imageSource = value;
                OnPropertyChanged("ImageSource");
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        private string _folderPath;
        public string FolderPath
        {
            get { return _folderPath; }
            set
            {
                _folderPath = value;
                OnPropertyChanged("FolderPath");
            }
        }

        private string _fileType;
        public string FileType
        {
            get { return _fileType; }
            set
            {
                _fileType = value;
                OnPropertyChanged("FileType");
            }
        }

        private long _imageWidth;
        public long ImageWidth
        {
            get { return _imageWidth; }
            set
            {
                _imageWidth = value;
                OnPropertyChanged("ImageWidth");
            }
        }

        private long _imageHeight;
        public long ImageHeight
        {
            get { return _imageHeight; }
            set
            {
                _imageHeight = value;
                OnPropertyChanged("ImageHeight");
            }
        }

        /// <summary>Row number within its duplicate group (1-based). Used by the detailed view.</summary>
        public int RowIndex { get; set; }

        /// <summary>
        /// True when this file is the currently selected/highlighted file in the UI.
        /// Drives the blue border highlight in the image view.
        /// </summary>
        private bool _isHighlighted;
        public bool IsHighlighted
        {
            get { return _isHighlighted; }
            set
            {
                _isHighlighted = value;
                OnPropertyChanged("IsHighlighted");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    /// <summary>
    /// Flat row for the DataGrid detailed view. Each row maps to one DuplicateFile
    /// and carries the group number for grouping/display.
    /// </summary>
    public class DetailedViewRow : INotifyPropertyChanged
    {
        public int GroupNumber { get; set; }
        public string GroupLabel { get; set; }
        public bool IsGroupHeader { get; set; }
        public int Key { get; set; }
        public DuplicateFile File { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}