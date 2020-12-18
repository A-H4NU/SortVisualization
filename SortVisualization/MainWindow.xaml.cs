using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SortVisualization
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();

            _sortTypes = new ObservableCollection<string>();
            foreach (var value in Enum.GetNames(typeof(SortType)))
            {
                SortTypes.Add(value);
            }
            list.ItemsSource = _sortTypes;

            var settings = new GLWpfControlSettings
            {
                MajorVersion = 3,
                MinorVersion = 6
            };
            OpenTkControl.Start(settings);
        }

        private ObservableCollection<string> _sortTypes;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<string> SortTypes { get => _sortTypes; }

        private string _mystring;
        public string MyString
        {
            get => _mystring;
            set
            {
                _mystring = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MyString"));
            }
        }

        private void OpenTkControl_OnRender(TimeSpan delta)
        {
            GL.ClearColor(Color4.WhiteSmoke);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            MyString = "test";
            MessageBox.Show((list.ItemsSource == null).ToString());
        }
    }
}
