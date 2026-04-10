using System;
using System.Windows;
using DuplicatePhotosFixer.Models;
using static DuplicatePhotosFixer.Views.cNagDialogue;

namespace DuplicatePhotosFixer.Views
{
    public partial class frmAfterScan : Window
    {
        /// <summary>True when the user clicked "Auto Mark" (vs "Select Manually").</summary>
        public bool AutoMarkRequested { get; private set; } = false;

        public frmAfterScan()
        {
            InitializeComponent();
        }

        private void frmAfterScanMark_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PreInit();
                LoadStrings();
            }
            catch (Exception ex)
            {
                cGlobalSettings.oLogger.WriteLogException("frmAfterScanMark_Loaded", ex);
            }
        }

        void PreInit()
        {
            try
            {
                if (App.oMainReference.objAfterScanViewModel == null)
                {
                    App.oMainReference.objAfterScanViewModel = new vmAfterScan();
                }
                this.DataContext = App.oMainReference.objAfterScanViewModel;
            }
            catch (Exception ex)
            {
                cGlobalSettings.oLogger.WriteLogException("PreInit", ex);
            }
        }

        void LoadStrings()
        {
            try
            {
                lbl_heading.Content = cGlobalSettings.GetProductName();
                //label1.Text = cResourceManager.LoadString("IDS_NAG_MSG_DUPLICATE_FOUND");
                //lbl_spaceSaved.Text = cResourceManager.LoadString("IDS_NAG_MSG_SPACE_SAVED");
                btn_selectManually.Content = cResourceManager.LoadString("IDS_SELECT_MANUALLY");
                btn_autoMark.Content = cResourceManager.LoadString("DPF_RESULT_UC_AUTO_MARK_TEXT");
                txt_progressUpdate.Text = cResourceManager.LoadString("DPF_SCANNED");

                // Directly set values from ViewModel as a fallback in case bindings
                // haven't resolved yet or DataContext was set after XAML load
                var vm = App.oMainReference.objAfterScanViewModel;
                if (vm != null)
                {
                    //cbtn_totalCount.Text = vm.DuplicateCount.ToString();
                    //lbl_spaceSavedValue.Text = vm.TotalSize ?? "0 MB";
                }
            }
            catch (Exception ex)
            {
                cGlobalSettings.oLogger.WriteLogException("LoadStrings", ex);
            }
        }

        void CloseForm()
        {
            try
            {
                cGlobal.frmNagReturnCode = FORM_RETURN_CODES.DO_NOTHING;

                this.Close();

            }
            catch (Exception ex)
            {

            }
        }


       
        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                App.oMainReference.LoadUrlForTrackingSilently(cGlobal.CombineUrlADUTracking(Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["ADU_TRACKING"]), eUTM_BTN_TYPE.BtnClose.ToString(), eOfferName.AfterScanMark.ToString()));

                CloseForm();
                
            }
            catch (Exception ex)
            {
                cGlobalSettings.oLogger.WriteLogException("btn_close_Click", ex);
            }
        }

        private void btn_selectManually_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Set dialog result to true and close
                this.DialogResult = true;
             //   App.oMainReference.objAfterScanViewModel.OnSelectManually();
                this.Close();
            }
            catch (Exception ex)
            {
                cGlobalSettings.oLogger.WriteLogException("btn_selectManually_Click", ex);
            }
        }

        private void btn_autoMark_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AutoMarkRequested = true;
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                cGlobalSettings.oLogger.WriteLogException("btn_autoMark_Click", ex);
            }
        }
    }
}