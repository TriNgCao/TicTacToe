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
using System.Windows.Shapes;

namespace CaroGame
{
    /// <summary>
    /// Interaction logic for ModeWindow.xaml
    /// </summary>
    public partial class ModeWindow : Window
    {
        public ModeWindow()
        {
            InitializeComponent();
        }

        private void OnePlayerCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            TwoPlayerCheckBox.IsChecked = false;
            PlayerChoiceComboBox.Visibility = Visibility.Visible;
            PlayerChoiceComboBox.SelectedIndex = 0;
        }

        private void OnePlayerCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            PlayerChoiceComboBox.Visibility = Visibility.Collapsed;
        }

        private void TwoPlayerCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            OnePlayerCheckBox.IsChecked = false; // Bỏ chọn "1 Player"
            PlayerChoiceComboBox.Visibility = Visibility.Collapsed;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (OnePlayerCheckBox.IsChecked == false && TwoPlayerCheckBox.IsChecked == false)
            {
                MessageBox.Show("You must choose a mode to play game", "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            MainWindow m = new();
            this.Hide();
            if (OnePlayerCheckBox.IsChecked == true)
            {
                m.Mode = 1;
                if (PlayerChoiceComboBox.SelectedIndex == 0)
                    m.Role = 1;
                else
                {
                    m.Role = 2;

                }

            }
            else
                m.Mode = 2;

            m.ShowDialog();

        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }
    }
}
