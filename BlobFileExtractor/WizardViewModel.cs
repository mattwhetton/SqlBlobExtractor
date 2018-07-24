using System.ComponentModel;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace BlobFileExtractor
{
    public class WizardViewModel : INotifyPropertyChanged
    {

        public WizardViewModel()
        {
            IsTextFilename = true;
            IsTextFileExtension = true;
        }

        private string _connectionString;
        public string ConnectionString
        {
            get {
                return _connectionString; 
            }
            set
            {
                _connectionString = value;
                OnPropertyChanged("ConnectionString");
            }
        }

        private string _sql;
        public string SQL
        {
            get {
                return _sql; 
            }
            set
            {
                _sql = value;
                OnPropertyChanged("SQL");
            }
        }

        private ObservableCollection<string> _dataColumns;
        public ObservableCollection<string> DataColumns
        {
            get
            {
                if (_dataColumns == null)
                    _dataColumns = new ObservableCollection<string>();
                return _dataColumns;
            }
        }

        private string _selectedDataColumn;
        public string SelectedDataColumn
        {
            get
            {
                return _selectedDataColumn; 
            }
            set
            {
                _selectedDataColumn = value;
                OnPropertyChanged("SelectedDataColumn");
            }
        }

        private string _selectedFilenameColumn;
        public string SelectedFilenameColumn
        {
            get
            {
                return _selectedFilenameColumn; 
            }
            set
            {
                _selectedFilenameColumn = value;
                OnPropertyChanged("SelectedFilenameColumn");
            }
        }

        private string _selectedFiletypeColumn;
        public string SelectedFiletypeColumn
        {
            get
            {
                return _selectedFiletypeColumn; 
            }
            set
            {
                _selectedFiletypeColumn = value;
                OnPropertyChanged("SelectedFiletypeColumn");
            }
        }

        private bool _isTextFilename;
        public bool IsTextFilename
        {
            get
            {
                return _isTextFilename; 
            }
            set
            {
                _isTextFilename = value;
                OnPropertyChanged("IsTextFilename");
            }
        }

        private bool _isTextFileExtension;
        public bool IsTextFileExtension
        {
            get
            {
                return _isTextFileExtension; 
            }
            set
            {
                _isTextFileExtension = value;
                OnPropertyChanged("TextFileExtension");
            }
        }

        private string _folder;
        public string Folder
        {
            get
            {
                return _folder;
            }
            set
            {
                _folder = value;
                OnPropertyChanged("Folder");
            }
        }

        private string _fileName;
        public string FileName
        {
            get
            {
                return _fileName;
            }
            set
            {
                _fileName = value;
                OnPropertyChanged("FileName");
            }
        }        
        
        private string _fileNameExtension;
        public string FileNameExtension
        {
            get
            {
                return _fileNameExtension;
            }
            set
            {
                _fileNameExtension = value;
                OnPropertyChanged("FileNameExtension");
            }
        }

        public void PopulateData(){
            try
            {
	            using (var conn = new SqlConnection(ConnectionString))
	            {
					conn.Open();
		            using (var cmd = new SqlCommand(SQL, conn))
		            {
			            var reader = cmd.ExecuteReader();
						
			            DataColumns.Clear();

						for(int i=0;i<reader.FieldCount;i++)
							DataColumns.Add(reader.GetName(i));

			            OnPropertyChanged("DataColumns");
			            OnPropertyChanged("DataItemCount");
		            }
	            }
            }
            catch (System.Exception)
            {
                throw;
            }
        }
        public void WriteFile()
        {
            if (String.IsNullOrEmpty(SelectedDataColumn) 
                || String.IsNullOrEmpty(Folder) 
                || (!IsTextFileExtension && String.IsNullOrWhiteSpace(SelectedFiletypeColumn))
                || (String.IsNullOrEmpty(FileName) && String.IsNullOrEmpty(SelectedFilenameColumn)))
                throw new ArgumentNullException();

	        using (var conn = new SqlConnection(ConnectionString))
	        {
		        conn.Open();
		        using (var cmd = new SqlCommand(SQL, conn))
		        {
			        var reader = cmd.ExecuteReader();

			        while (reader.Read())
			        {
			            var fileExtension = String.IsNullOrWhiteSpace(FileNameExtension) ? String.Empty : String.Format(".{0}", FileNameExtension);

			            if (!IsTextFileExtension)
			            {
			                var mimeType = reader[SelectedFiletypeColumn] == DBNull.Value ? String.Empty : reader[SelectedFiletypeColumn] as string;
                            fileExtension = GetDefaultExtension(mimeType);
			            }

			            var filename = IsTextFilename ? FileName : string.Format("{0}{1}", reader[SelectedFilenameColumn], fileExtension);

						foreach (var c in Path.GetInvalidFileNameChars())
							filename = filename.Replace(c, '_');

						filename = filename.Trim();

						if (String.IsNullOrWhiteSpace(filename))
						{
							throw new ArgumentException();
						}

						string fullName = Path.Combine(Folder, filename);

						if (File.Exists(fullName))
							File.Delete(fullName);

						byte[] filebytes;


						if (reader[SelectedDataColumn] == DBNull.Value)
							continue;

				        var data = reader[SelectedDataColumn] as string;
				        if (data != null)
						{
							var content = data;
							filebytes = Convert.FromBase64String(content);
						}
						else
						{
							filebytes = (byte[])reader[SelectedDataColumn];
						}


						using (var fs = new FileStream(fullName, FileMode.CreateNew, FileAccess.Write, FileShare.None))
						{
							fs.Write(filebytes, 0, filebytes.Length);
							fs.Close();
						}
                
			        }

		        }
	        }
        }

        protected virtual void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public static string GetDefaultExtension(string mimeType)
        {
            string result;
            RegistryKey key;
            object value;

            key = Registry.ClassesRoot.OpenSubKey(@"MIME\Database\Content Type\" + mimeType, false);
            value = key != null ? key.GetValue("Extension", null) : null;
            result = value != null ? value.ToString() : string.Empty;

            return result;
        }

    }
}