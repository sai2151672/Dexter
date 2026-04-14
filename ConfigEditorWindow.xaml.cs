using System;
using System.Windows;
using System.Windows.Input;

namespace Dexter
{
    public partial class ConfigEditorWindow : Window
    {
        private readonly MainWindow _mainWindow;

        public ConfigEditorWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            LoadEditorFromDisk();
        }

        private void LoadEditorFromDisk()
        {
            try
            {
                ConfigTextBox.Text = ConfigService.LoadConfigText();
                EditorStatusTextBlock.Text = "Loaded config from file.";
            }
            catch (Exception ex)
            {
                EditorStatusTextBlock.Text = $"Load failed: {ex.Message}";
            }
        }

        private void ReloadConfigButton_Click(object sender, RoutedEventArgs e)
        {
            LoadEditorFromDisk();
        }

        private void SaveConfigButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string json = ConfigTextBox.Text;
                DexterConfig parsedConfig = ConfigService.ParseConfig(json);
                ConfigService.SaveConfig(parsedConfig);

                _mainWindow.ApplySavedConfig(parsedConfig);
                EditorStatusTextBlock.Text = "Config saved successfully.";
            }
            catch (Exception ex)
            {
                EditorStatusTextBlock.Text = $"Save failed: {ex.Message}";
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}