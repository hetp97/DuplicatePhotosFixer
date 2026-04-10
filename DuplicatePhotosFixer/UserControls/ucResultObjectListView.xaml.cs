using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using DuplicatePhotosFixer;
using DuplicatePhotosFixer.Models;

namespace DuplicatePhotosFixer.UserControls
{
    public partial class ucResultObjectListView : UserControl
    {
        /// <summary>Raised when the user clicks a file row.</summary>
        public event Action<DuplicateFile> FileSelected;

        public ucResultObjectListView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles click on a file data row.
        /// Fires FileSelected so the parent (ucResultView) updates preview/metadata.
        /// </summary>
        private void FileRow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var element = sender as FrameworkElement;
                var row = element?.Tag as DetailedViewRow;
                if (row?.File != null)
                    FileSelected?.Invoke(row.File);
            }
            catch (Exception ex)
            {
                cGlobalSettings.oLogger?.WriteLogException("FileRow_MouseLeftButtonDown", ex);
            }
        }

        /// <summary>
        /// Validates the n-1 rule before toggling a file's checked state.
        /// If checking this file would leave 0 unchecked files in the group, the
        /// check is blocked and the user sees an information dialog.
        /// </summary>
        private void FileCheckBox_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var cb = sender as CheckBox;
                var row = cb?.DataContext as DetailedViewRow;
                if (row?.File == null) return;

                bool newValue = cb.IsChecked == true;

                if (newValue)
                {
                    var vm = DataContext as vmResultView;
                    if (vm != null && !vm.CanCheckFile(row.File))
                    {
                        // Revert the checkbox — binding is OneWay so we set it manually
                        cb.IsChecked = false;
                        MessageBox.Show(
                            "All the photos of a group cannot be marked!",
                            "Duplicate Photos Fixer Pro",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        return;
                    }
                }

                row.File.IsSelected = newValue;
            }
            catch (Exception ex)
            {
                cGlobalSettings.oLogger?.WriteLogException("FileCheckBox_Click", ex);
            }
        }

        private void DeleteMarked_Click(object sender, RoutedEventArgs e)
        {
            // Group-level "Delete Marked" — placeholder for future delete logic
        }
    }
}
