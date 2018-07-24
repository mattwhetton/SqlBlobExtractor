using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BlobFileExtractor
{
    /// <summary>
    /// Interaction logic for OutputLocationView.xaml
    /// </summary>
    public partial class OutputLocationView : UserControl
    {
        public OutputLocationView()
        {
            InitializeComponent();
        }



        private void ChooseFilename_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            //SaveFileDialog dialog = new SaveFileDialog();

            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                ((WizardViewModel)DataContext).Folder = dialog.SelectedPath;
            }
        }
    }
}
