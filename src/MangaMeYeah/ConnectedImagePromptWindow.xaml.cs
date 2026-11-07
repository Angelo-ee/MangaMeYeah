using System.Windows;

namespace MangaMeYeah;

public partial class ConnectedImagePromptWindow : Window
{
    public bool SuppressFuturePrompts => SuppressPromptCheckBox.IsChecked == true;

    public ConnectedImagePromptWindow(string detectedDescription)
    {
        InitializeComponent();
        MessageTextBlock.Text = $"检测到拼接图像（{detectedDescription}）。\n是否切换为拼接显示？";
    }

    private void YesButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void NoButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
