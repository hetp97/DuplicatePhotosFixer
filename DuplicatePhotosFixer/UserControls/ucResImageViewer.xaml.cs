using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DuplicatePhotosFixer.Models;

namespace DuplicatePhotosFixer.UserControls
{
    /// <summary>
    /// Interaction logic for ucResImageViewer.xaml
    /// </summary>
    public partial class ucResImageViewer : UserControl
    {
        /// <summary>
        /// Raised when the user clicks on a photo card.
        /// The parent (ucResultView) listens to update preview/metadata.
        /// </summary>
        public event Action<DuplicateFile> FileSelected;

        public ucResImageViewer()
        {
            InitializeComponent();
        }

        private void PhotoCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var border = sender as Border;
                var file = border?.Tag as DuplicateFile;
                if (file != null)
                {
                    FileSelected?.Invoke(file);
                }
            }
            catch (Exception ex)
            {
                cGlobalSettings.oLogger?.WriteLogException("PhotoCard_MouseLeftButtonDown", ex);
            }
        }

        private void PhotoCheckBox_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var cb = sender as CheckBox;
                var file = cb?.DataContext as DuplicateFile;
                if (file == null) return;

                bool newValue = cb.IsChecked == true;

                if (newValue)
                {
                    var vm = DataContext as vmResultView;
                    if (vm != null && !vm.CanCheckFile(file))
                    {
                        cb.IsChecked = false;
                        MessageBox.Show(
                            "All the photos of a group cannot be marked!",
                            "Duplicate Photos Fixer Pro",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        return;
                    }
                }

                file.IsSelected = newValue;
            }
            catch (Exception ex)
            {
                cGlobalSettings.oLogger?.WriteLogException("PhotoCheckBox_Click", ex);
            }
        }

        private void chkbox_Click(object sender, RoutedEventArgs e)
        {
        }

        private void chk_isSelected_Checked(object sender, RoutedEventArgs e)
        {
        }

        private void imageListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void duplicateThumbnailView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }

        private void chk_isSelected_Unchecked(object sender, RoutedEventArgs e)
        {
        }
    }
}
