using System.Windows;

namespace DuplicatePhotosFixer.Views
{
    public partial class frmDeleteConfirm : Window
    {
        public bool DontShowAgain => chk_dontShowAgain.IsChecked == true;

        public frmDeleteConfirm()
        {
            InitializeComponent();
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
