using System;
using System.Windows;
using System.Windows.Input;

namespace DuplicatePhotosFixer.Views
{
    public partial class frmDeleteSummary : Window
    {
        /// <summary>Set to true when the user clicks "Remove Photos".</summary>
        public bool RemoveRequested { get; private set; }

        public frmDeleteSummary(int totalDuplicates, string spaceToSave, int markedCount, string markedSize)
        {
            InitializeComponent();

            txt_totalCount.Text = totalDuplicates.ToString();
            txt_totalSize.Text = $"Size: {spaceToSave}";
            txt_dupCount.Text = totalDuplicates.ToString();
            txt_spaceSaved.Text = spaceToSave;
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void btn_removePhotos_Click(object sender, RoutedEventArgs e)
        {
            RemoveRequested = true;
            DialogResult = true;
            Close();
        }
    }
}
