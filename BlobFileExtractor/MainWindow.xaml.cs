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
using System.Data.SqlClient;
using Microsoft.Win32;
using System.IO;
using Microsoft.Data.ConnectionUI;

namespace BlobFileExtractor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WizardViewModel _dataContext;


        public MainWindow()
        {
            InitializeComponent();

            _dataContext = new WizardViewModel();
            _dataContext.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(dataContext_PropertyChanged);
            this.DataContext = _dataContext;
        }

        void dataContext_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DataColumns")
            {
                
            }
        }

        private void Wizard_CancelClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Wizard_NextClick(object sender, RoutedEventArgs e)
        {
            
        }

        private void Wizard_PageChanged(object sender, RoutedEventArgs e)
        {
            if (Wizard.ActivePageIndex == 1)
            {
                if (string.IsNullOrEmpty(_dataContext.ConnectionString) || !checkConnectionString(_dataContext.ConnectionString) || string.IsNullOrEmpty(_dataContext.SQL))
                {
                    MessageBox.Show("Please ensure that the Connection String and SQL are properly defined.");
                    Wizard.ActivePageIndex = 0;
                    return;
                }

                try
                {
                    _dataContext.PopulateData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    Wizard.ActivePageIndex = 0;
                    return;
                }

            }
            else if (Wizard.ActivePageIndex == 2)
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to write the selected file(s)?", "Confirm Write", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _dataContext.WriteFile();
                        SuccessMessage.Visibility = System.Windows.Visibility.Visible;
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Please ensure all data is fully completed.");
                        Wizard.ActivePageIndex = 1;
                    }
                }
                else
                    Wizard.ActivePageIndex = 1;
            }
        }

        private void Wizard_FinishClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private static bool checkConnectionString(string connectionString)
        {
            var conn = new SqlConnection(connectionString);
            try
            {
                conn.Open();
                conn.Close();
                return true;
            }
            catch (Exception ex) { return false; }
        }

        private void BuildConnStringBtn_Click(object sender, RoutedEventArgs e)
        {
            var connectionDialog = new DataConnectionDialog();
            var connectionConfig = new DataConnectionConfiguration(null);
            connectionConfig.LoadConfiguration(connectionDialog);

            System.Windows.Forms.DialogResult result = DataConnectionDialog.Show(connectionDialog);
            if (result == System.Windows.Forms.DialogResult.OK)
                _dataContext.ConnectionString = connectionDialog.ConnectionString;
        }

    }
}
