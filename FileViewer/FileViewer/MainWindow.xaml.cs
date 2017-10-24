using System.IO;
using System.Windows;
using System.Windows.Input;
using TestHelper;

namespace FileViewer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            NextCommand = new RelayCommand(Next);
            PreviousCommand = new RelayCommand(Previous);

            InitializeComponent();
            
            string dir = @"C:\Users\litao\Pictures\";
            if (!Directory.Exists(dir))
            {
                dir = @"./";
            }
            string[] filesPath = Directory.GetFiles(dir, "*.*");
            _fileCollection.AddPaths(filesPath);
            PreviewControl.ReplaceContent(filesPath[0]);
        }

        public ICommand NextCommand { get; private set; }

        public ICommand PreviousCommand { get; private set; }

        private readonly PathCollection _fileCollection = new PathCollection();

        private void Next()
        {
            string filePath = _fileCollection.GetNextPath();
            PreviewControl.ReplaceContent(filePath);
            this.Title = filePath;
        }

        private void Previous()
        {
            string filePath = _fileCollection.GetPreviousPath();
            PreviewControl.ReplaceContent(filePath);
            this.Title = filePath;
        }
    }
}
