using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using DuplicatePhotosFixer;
using DuplicatePhotosFixer.Models;

namespace DuplicatePhotosFixer.UserControls
{
    public partial class ucImageInfo : UserControl
    {
        public ucImageInfo()
        {
            InitializeComponent();
            LoadStrings();
        }

        void LoadStrings()
        {
            lbl_matchingLevel.Text   = cResourceManager.LoadString("DPF_IMAGEINFO_UC_MATCHING_LEVEL");
            lblPreview.Text          = cResourceManager.LoadString("DPF_IMAGEINFO_UC_PREVIEW");
            lbl_metadata.Text        = cResourceManager.LoadString("DPF_IMAGEINFO_UC_METADATA");
            lbl_fileName.Text        = cResourceManager.LoadString("DPF_IMAGEINFO_UC_FILE_NAME");
            lbl_fileSize.Text        = cResourceManager.LoadString("DPF_IMAGEINFO_UC_FILE_SIZE");
            lbl_fileLabel.Text       = cResourceManager.LoadString("DPF_IMAGEINFO_UC_FILE_LABEL");
            lbl_fileCreated.Text     = cResourceManager.LoadString("DPF_IMAGEINFO_UC_FILE_CREATED");
            lbl_fileModified.Text    = cResourceManager.LoadString("DPF_IMAGEINFO_UC_FILE_MODIFIED");
            lbl_folderName.Text      = cResourceManager.LoadString("DPF_IMAGEINFO_UC_FOLDER_NAME");
            lbl_rating.Text          = cResourceManager.LoadString("DPF_IMAGEINFO_UC_RATING");
            lbl_keywords.Text        = cResourceManager.LoadString("DPF_IMAGEINFO_UC_KEYWORDS");
            lbl_imageSize.Text       = cResourceManager.LoadString("DPF_IMAGEINFO_UC_IMAGE_SIZE");
            lbl_imageDPI.Text        = cResourceManager.LoadString("DPF_IMAGEINFO_UC_IMAGE_DPI");
            lbl_bitDepth.Text        = cResourceManager.LoadString("DPF_IMAGEINFO_UC_BIT_DEPTH");
            lbl_orientation.Text     = cResourceManager.LoadString("DPF_IMAGEINFO_UC_ORIENTATION");
            lbl_digitalZoomRatio.Text = cResourceManager.LoadString("DPF_IMAGEINFO_UC_DIGITAL_ZOOM_RATIO");
            lbl_captureDate.Text     = cResourceManager.LoadString("DPF_IMAGEINFO_UC_CAPTURE_DATE");
            lbl_editingSoftware.Text = cResourceManager.LoadString("DPF_IMAGEINFO_UC_EDITING_SOFTWARE");
            lbl_exposure.Text        = cResourceManager.LoadString("DPF_IMAGEINFO_UC_EXPOSURE");
            lbl_focalLength.Text     = cResourceManager.LoadString("DPF_IMAGEINFO_UC_FOCAL_LENGTH");
            lbl_exposureBias.Text    = cResourceManager.LoadString("DPF_IMAGEINFO_UC_EXPOSURE_BIAS");
            lbl_ISOSpeedRating.Text  = cResourceManager.LoadString("DPF_IMAGEINFO_UC_ISO_SPEED_RATING");
            lbl_exposureMode.Text    = cResourceManager.LoadString("DPF_IMAGEINFO_UC_EXPOSURE_MODE");
            lbl_exposureProgram.Text = cResourceManager.LoadString("DPF_IMAGEINFO_UC_EXPOSURE_PROGRAM");
            lbl_meteringMode.Text    = cResourceManager.LoadString("DPF_IMAGEINFO_UC_METERING_MODE");
            lbl_lightSource.Text     = cResourceManager.LoadString("DPF_IMAGEINFO_UC_LIGHT_SOURCE");
            lbl_sensingMethod.Text   = cResourceManager.LoadString("DPF_IMAGEINFO_UC_SENSING_METHOD");
            lbl_scenceCaptureType.Text = cResourceManager.LoadString("DPF_IMAGEINFO_UC_SCENCE_CAPTURE_TYPE");
            lbl_cameraMaker.Text     = cResourceManager.LoadString("DPF_IMAGEINFO_UC_CAMERA_MAKER");
            lbl_cameraModel.Text     = cResourceManager.LoadString("DPF_IMAGEINFO_UC_CAMERA_MODEL");
            lbl_cameraLensModel.Text = cResourceManager.LoadString("DPF_IMAGEINFO_UC_CAMERA_LENS_MODEL");
            lbl_cameraFirmware.Text  = cResourceManager.LoadString("DPF_IMAGEINFO_UC_CAMERA_FIRMWARE");
            lbl_colorModel.Text      = cResourceManager.LoadString("DPF_IMAGEINFO_UC_COLOR_MODEL");
            lbl_colorSpace.Text      = cResourceManager.LoadString("DPF_IMAGEINFO_UC_COLOR_SPACE");
            lbl_GPSLatitude.Text     = "GPS Lat/Long:";
        }

        /// <summary>
        /// Update preview + metadata panel for the selected DuplicateFile.
        /// </summary>
        public void UpdateFileInfo(DuplicateFile file)
        {
            try
            {
                if (file == null) { ClearInfo(); return; }

                // Preview image
                previewimg.Source = file.ImageSource;

                // --- Basic file fields ---
                val_fileName.Text = file.FileName ?? "---";
                val_fileSize.Text = file.FileSizeFormatted ?? "---";
                val_fileLabel.Text = file.FileType ?? "---";

                val_fileCreated.Text = file.CreatedDate != DateTime.MinValue
                    ? file.CreatedDate.ToString("M/d/yyyy h:mm:ss tt") : "---";

                val_fileModified.Text = file.ModifiedDate != DateTime.MinValue
                    ? file.ModifiedDate.ToString("M/d/yyyy h:mm:ss tt") : "---";

                // Folder Name = last segment of folder path, not the full path
                val_folderName.Text = GetFolderName(file.FolderPath);

                // Image dimensions from DuplicateFile
                val_imageSize.Text = file.ImageWidth > 0 && file.ImageHeight > 0
                    ? $"{file.ImageWidth} x {file.ImageHeight}" : "---";

                // --- Extended EXIF from engine's csImageFileInfo ---
                if (int.TryParse(file.FileId, out int fileKey) &&
                    cGlobalSettings.listImageFileInfo != null &&
                    cGlobalSettings.listImageFileInfo.TryGetValue(fileKey, out var csFile))
                {
                    var exif = csFile.exifInfo;
                    if (exif != null)
                    {
                        // Image DPI
                        val_imageDPI.Text = exif.HorizontalResolution > 0
                            ? $"{exif.HorizontalResolution:0.####} x {exif.VerticalResolution:0.####}"
                            : "---";

                        // Bit Depth
                        val_bitDepth.Text = exif.BitDepth > 0
                            ? exif.BitDepth.ToString() : "---";

                        // Orientation
                        val_orientation.Text = !string.IsNullOrEmpty(exif.Orientation)
                            ? exif.Orientation : "---";

                        // Capture Date
                        val_captureDate.Text = exif.CaptureDate != DateTime.MinValue &&
                                               exif.CaptureDate != DateTime.MaxValue
                            ? exif.CaptureDate.ToString("M/d/yyyy h:mm:ss tt") : "---";

                        // GPS
                        if (exif.latitudeDegree != 0 || exif.longitudeDegree != 0)
                            val_GPSLatitude.Text = $"{exif.latitudeDegree:F6}, {exif.longitudeDegree:F6}";
                        else
                            val_GPSLatitude.Text = "---";
                    }
                    else
                    {
                        val_imageDPI.Text = "---";
                        val_bitDepth.Text = "---";
                        val_orientation.Text = "---";
                        val_captureDate.Text = "---";
                        val_GPSLatitude.Text = "---";
                    }
                }
                else
                {
                    val_imageDPI.Text = "---";
                    val_bitDepth.Text = "---";
                    val_orientation.Text = "---";
                    val_captureDate.Text = "---";
                    val_GPSLatitude.Text = "---";
                }

                // Fallback: read Image Size, DPI, Bit Depth directly from file if EXIF data was missing
                if ((val_imageSize.Text == "---" || val_imageDPI.Text == "---" || val_bitDepth.Text == "---")
                    && !string.IsNullOrEmpty(file.FilePath) && File.Exists(file.FilePath))
                {
                    try
                    {
                        using (var stream = File.OpenRead(file.FilePath))
                        {
                            var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.Default);
                            var frame = decoder.Frames[0];
                            if (val_imageSize.Text == "---")
                                val_imageSize.Text = $"{frame.PixelWidth} x {frame.PixelHeight}";
                            if (val_imageDPI.Text == "---")
                                val_imageDPI.Text = $"{frame.DpiX:0.####} x {frame.DpiY:0.####}";
                            if (val_bitDepth.Text == "---")
                                val_bitDepth.Text = frame.Format.BitsPerPixel.ToString();
                        }
                    }
                    catch { }
                }

                // Fields without available data
                val_rating.Text            = "---";
                val_keywords.Text          = "---";
                val_digitalZoomRatio.Text  = "---";
                val_editingSoftware.Text   = "---";
                val_exposure.Text          = "---";
                val_focalLength.Text       = "---";
                val_exposureBias.Text      = "---";
                val_ISOSpeedRating.Text    = "---";
                val_exposureMode.Text      = "---";
                val_exposureProgram.Text   = "---";
                val_meteringMode.Text      = "---";
                val_lightSource.Text       = "---";
                val_sensingMethod.Text     = "---";
                val_scenceCaptureType.Text = "---";
                val_cameraMaker.Text       = "---";
                val_cameraModel.Text       = "---";
                val_cameraLensModel.Text   = "---";
                val_cameraFirmware.Text    = "---";
                val_colorModel.Text        = "---";
                val_colorSpace.Text        = "---";
            }
            catch (Exception ex)
            {
                cGlobalSettings.oLogger?.WriteLogException("ucImageInfo::UpdateFileInfo", ex);
            }
        }

        /// <summary>Reset all fields to "---" and clear preview.</summary>
        public void ClearInfo()
        {
            previewimg.Source          = null;
            val_fileName.Text          = "---";
            val_fileSize.Text          = "---";
            val_fileLabel.Text         = "---";
            val_fileCreated.Text       = "---";
            val_fileModified.Text      = "---";
            val_folderName.Text        = "---";
            val_rating.Text            = "---";
            val_keywords.Text          = "---";
            val_imageSize.Text         = "---";
            val_imageDPI.Text          = "---";
            val_bitDepth.Text          = "---";
            val_orientation.Text       = "---";
            val_digitalZoomRatio.Text  = "---";
            val_captureDate.Text       = "---";
            val_editingSoftware.Text   = "---";
            val_exposure.Text          = "---";
            val_focalLength.Text       = "---";
            val_exposureBias.Text      = "---";
            val_ISOSpeedRating.Text    = "---";
            val_exposureMode.Text      = "---";
            val_exposureProgram.Text   = "---";
            val_meteringMode.Text      = "---";
            val_lightSource.Text       = "---";
            val_sensingMethod.Text     = "---";
            val_scenceCaptureType.Text = "---";
            val_cameraMaker.Text       = "---";
            val_cameraModel.Text       = "---";
            val_cameraLensModel.Text   = "---";
            val_cameraFirmware.Text    = "---";
            val_colorModel.Text        = "---";
            val_colorSpace.Text        = "---";
            val_GPSLatitude.Text       = "---";
        }

        /// <summary>
        /// Returns just the folder name (last path segment), not the full path.
        /// e.g. "C:\Users\patel\Desktop\photocopy\" → "photocopy"
        /// </summary>
        private static string GetFolderName(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath)) return "---";
            string trimmed = folderPath.TrimEnd('\\', '/');
            int idx = trimmed.LastIndexOfAny(new[] { '\\', '/' });
            return idx >= 0 ? trimmed.Substring(idx + 1) : trimmed;
        }

        private void matchLevelSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
        }
    }
}
