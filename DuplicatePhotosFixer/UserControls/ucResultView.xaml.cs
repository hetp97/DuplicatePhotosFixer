using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using DuplicatePhotosFixer.Models;
using DuplicatePhotosFixer.Views;

namespace DuplicatePhotosFixer.UserControls
{
    public partial class ucResultView : UserControl
    {
        private vmResultView ViewModel;

        public ucResultView()
        {
            InitializeComponent();
            LoadStrings();
            InitializeViewModel();

            // Wire up selection events from both child views
            ucImageView.FileSelected += OnFileSelected;
            ucDetailedView.FileSelected += OnFileSelected;
        }

        /// <summary>
        /// Called when the user clicks a photo card (image view) or selects a row (detailed view).
        /// Updates the ViewModel selection, highlight, and the preview/metadata panel.
        /// </summary>
        private void OnFileSelected(DuplicateFile file)
        {
            try
            {
                if (file == null || ViewModel == null) return;

                ViewModel.SelectFile(file);
                ucInfoPanel.UpdateFileInfo(file);
            }
            catch (Exception ex)
            {
                cGlobalSettings.oLogger?.WriteLogException("ucResultView::OnFileSelected", ex);
            }
        }

        /// <summary>
        /// Reads the duplicate count from cGlobalSettings and updates the label.
        /// Call this every time the result view becomes visible.
        /// </summary>
        public void UpdateDuplicateCount()
        {
            try
            {
                lbl_DuplicatePhotosCount.Text = cGlobal.TotalDulicateFilesCount.ToString();
            }
            catch (Exception ex)
            {
                cGlobalSettings.oLogger?.WriteLogException("UpdateDuplicateCount", ex);
            }
        }

        private void InitializeViewModel()
        {
            try
            {
                if (App.oMainReference.objResultViewModel == null)
                {
                    App.oMainReference.objResultViewModel = new vmResultView();
                }

                RebindViewModel(App.oMainReference.objResultViewModel);

                cGlobalSettings.oLogger?.WriteLogVerbose("ucResultView initialized successfully");
            }
            catch (Exception ex)
            {
                cGlobalSettings.oLogger?.WriteLogException("InitializeViewModel", ex);
            }
        }

        /// <summary>
        /// Rebind the ViewModel and all child controls. Called from MainWindow.ShowResults
        /// on every scan to ensure fresh data binding.
        /// NOTE: LoadDuplicateGroups() must be called AFTER this — then call ShowCurrentSelection().
        /// </summary>
        public void RebindViewModel(vmResultView vm)
        {
            ViewModel = vm;
            this.DataContext = vm;
            ucImageView.DataContext = vm;
            ucDetailedView.DataContext = vm;
        }

        /// <summary>
        /// Update the preview/metadata panel with the ViewModel's current SelectedFile.
        /// Call this AFTER LoadDuplicateGroups() so SelectedFile is already set.
        /// </summary>
        public void ShowCurrentSelection()
        {
            try
            {
                if (ViewModel?.SelectedFile != null)
                    ucInfoPanel.UpdateFileInfo(ViewModel.SelectedFile);
                else
                    ucInfoPanel.ClearInfo();
            }
            catch (Exception ex)
            {
                cGlobalSettings.oLogger?.WriteLogException("ShowCurrentSelection", ex);
            }
        }

        private void LoadStrings()
        {
            try
            {
                btn_autoMark.Content = cResourceManager.LoadString("DPF_RESULT_UC_AUTO_MARK_TEXT");
                btn_unMarkAll.Content = cResourceManager.LoadString("DPF_RESULT_UC_UNMARK_ALL_TEXT");

                // Show image view as default
                btnView.Text = cResourceManager.LoadString("IDS_IMAGE_VIEW");
                lblSelectionAssitant.Text = cResourceManager.LoadString("DPF_RESULT_SELECTION_ASSISTANT_TEXT");
                llbl_deleteMarked.Text = cResourceManager.LoadString("DPF_FOOTER_UC_DELETE_MARKED_TEXT");

                lbl_DuplicatePhotosSubHeadingText.Text = cResourceManager.LoadString("DPF_TRIAL_PHOTO_FOUND_TEXT");
                lbl_SaveSpaceSubHeadingText.Text = cResourceManager.LoadString("DPF_TRIAL_SPACE_SAVED_TEXT");
                llbl_deleteMarked.Text = cResourceManager.LoadString("DPF_FOOTER_UC_DELETE_MARKED_TEXT");
            }
            catch (Exception ex)
            {
                cGlobalSettings.oLogger?.WriteLogException("LoadStrings", ex);
            }
        }

        /// <summary>
        /// Reload duplicate groups when view becomes visible
        /// </summary>
        public void RefreshData()
        {
            try
            {
                ViewModel?.LoadDuplicateGroups();
                UpdateStatisticsLabels();
                cGlobalSettings.oLogger?.WriteLogVerbose("Result view data refreshed");
            }
            catch (Exception ex)
            {
                cGlobalSettings.oLogger?.WriteLogException("RefreshData", ex);
            }
        }

        /// <summary>
        /// Update the statistics labels in the header
        /// </summary>
        private void UpdateStatisticsLabels()
        {
            try
            {
                if (ViewModel != null)
                {
                    // Update duplicate count - find the TextBlock in DockPanel
                    var dockPanel = lbl_DuplicatePhotosSubHeadingText.Parent as DockPanel;
                    if (dockPanel != null && dockPanel.Children.Count > 1)
                    {
                        var countTextBlock = dockPanel.Children[1] as TextBlock;
                        if (countTextBlock != null)
                        {
                            countTextBlock.Text = ViewModel.TotalDuplicates.ToString();
                        }
                    }

                    // Update space to save
                    var dockPanel2 = lbl_SaveSpaceSubHeadingText.Parent as DockPanel;
                    if (dockPanel2 != null && dockPanel2.Children.Count > 1)
                    {
                        var spaceTextBlock = dockPanel2.Children[1] as TextBlock;
                        if (spaceTextBlock != null)
                        {
                            spaceTextBlock.Text = ViewModel.SpaceToSave;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                cGlobalSettings.oLogger?.WriteLogException("UpdateStatisticsLabels", ex);
            }
        }

        public class ComboBoxItem
        {
            public string ImagePath { get; set; }
            public string Text { get; set; }
        }

        #region Button Click Handlers

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Back button
            try
            {
                App.oMainReference.ShowHome();
            }
            catch (Exception ex)
            {
                cGlobalSettings.oLogger?.WriteLogException("Back button click", ex);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // Auto Mark button
            try
            {
                ViewModel?.AutoMark();
                UpdateStatisticsLabels();
                cGlobalSettings.oLogger?.WriteLogVerbose("Auto Mark clicked");
            }
            catch (Exception ex)
            {
                cGlobalSettings.oLogger?.WriteLogException("Auto Mark button click", ex);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            // UnMark All button
            try
            {
                ViewModel?.UnmarkAll();
                UpdateStatisticsLabels();
                cGlobalSettings.oLogger?.WriteLogVerbose("UnMark All clicked");
            }
            catch (Exception ex)
            {
                cGlobalSettings.oLogger?.WriteLogException("UnMark All button click", ex);
            }
        }

        private void unMarkbtn_Click(object sender, RoutedEventArgs e)
        {
            // UnMark All button (duplicate handler)
            Button_Click_2(sender, e);
        }

        private void btn_selectionAssistanceOptionA_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                App.oMainReference.ShowAssistantSelection();
            }
            catch (Exception ex)
            {
                cGlobalSettings.oLogger?.WriteLogException("Selection Assistant A click", ex);
            }
        }

        private void btn_selectionAssistanceOptionB_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                App.oMainReference.ShowClearCaheDialog();
            }
            catch (Exception ex)
            {
                cGlobalSettings.oLogger?.WriteLogException("Selection Assistant B click", ex);
            }
        }

        /// <summary>
        /// Opens the delete flow: Summary popup → Confirm → Progress → Refresh.
        /// Wired to both the header "Delete Marked" link and the footer button.
        /// </summary>
        private void DeleteMarked_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ViewModel == null) return;

                var markedPaths = ViewModel.GetMarkedFilePaths();
                if (markedPaths.Count == 0)
                {
                    MessageBox.Show("No photos are marked for deletion.",
                        "Duplicate Photos Fixer Pro", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Step 1: Delete Summary popup
                var summary = new frmDeleteSummary(
                    ViewModel.TotalDuplicates,
                    ViewModel.SpaceToSave,
                    markedPaths.Count,
                    ViewModel.GetMarkedSizeFormatted());
                summary.Owner = Window.GetWindow(this);

                if (summary.ShowDialog() != true || !summary.RemoveRequested)
                    return;

                // Step 2: Confirmation dialog
                var confirm = new frmDeleteConfirm();
                confirm.Owner = Window.GetWindow(this);

                if (confirm.ShowDialog() != true)
                    return;

                // Step 3: Progress dialog — performs the actual deletion
                var progress = new frmDeleteProgress(markedPaths);
                progress.Owner = Window.GetWindow(this);
                progress.ShowDialog();

                if (progress.DeletedPaths.Count > 0)
                {
                    // Step 4: Remove deleted files from the ViewModel and refresh views
                    ViewModel.RemoveDeletedFiles(progress.DeletedPaths);

                    // Step 5: Remove deleted file paths from the home scan list so rescanning
                    // doesn't show stale entries or return "No similar photos found"
                    App.oMainReference?.objHomeViewModel?.RemoveDeletedFilePaths(progress.DeletedPaths);

                    // Update header labels
                    lbl_DuplicatePhotosCount.Text = ViewModel.TotalDuplicates.ToString();
                    lbl_SpaceSavedValue.Text = ViewModel.SpaceToSave;

                    // Update preview panel
                    ShowCurrentSelection();

                    cGlobalSettings.oLogger?.WriteLogVerbose(
                        $"Delete completed: {progress.DeletedCount} of {markedPaths.Count} files deleted");
                }
            }
            catch (Exception ex)
            {
                cGlobalSettings.oLogger?.WriteLogException("DeleteMarked_Click", ex);
            }
        }

        #endregion

        #region View Switching

        private void ImgeView_change(object sender, RoutedEventArgs e)
        {
            try
            {
                resultViewGridSelection.IsOpen = true;
                btnDetailedView.Visibility = Visibility.Visible;
                btnImageView.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                cGlobalSettings.oLogger?.WriteLogException("Image view change", ex);
            }
        }

        private void Detailed_View_Change(object sender, RoutedEventArgs e)
        {
            try
            {
                resultViewGridSelection.IsOpen = true;
                btnImageView.Visibility = Visibility.Visible;
                btnDetailedView.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                cGlobalSettings.oLogger?.WriteLogException("Detailed view change", ex);
            }
        }

        private void btnDetailedView_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                imageViewbtn.Visibility = Visibility.Collapsed;
                detailedViewbtn.Visibility = Visibility.Visible;
                btnDetailedView.Visibility = Visibility.Collapsed;
                svDetailed.Visibility = Visibility.Visible;
                svImage.Visibility = Visibility.Collapsed;

                if (ViewModel != null)
                    ViewModel.IsImageView = false;

                cGlobalSettings.oLogger?.WriteLogVerbose("Switched to Detailed View");
            }
            catch (Exception ex)
            {
                cGlobalSettings.oLogger?.WriteLogException("btnDetailedView_Click", ex);
            }
        }

        private void btnImageView_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                imageViewbtn.Visibility = Visibility.Visible;
                detailedViewbtn.Visibility = Visibility.Collapsed;
                btnImageView.Visibility = Visibility.Collapsed;
                svImage.Visibility = Visibility.Visible;
                svDetailed.Visibility = Visibility.Collapsed;

                if (ViewModel != null)
                    ViewModel.IsImageView = true;

                cGlobalSettings.oLogger?.WriteLogVerbose("Switched to Image View");
            }
            catch (Exception ex)
            {
                cGlobalSettings.oLogger?.WriteLogException("btnImageView_Click", ex);
            }
        }

        #endregion
    }
}
