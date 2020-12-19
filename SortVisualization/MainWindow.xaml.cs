using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
        public const string ColorVertexPath = @"color_vertex.vert";
        public const string ColorFragmentPath = @"color_fragment.frag";

        private readonly int _program;

        public static Color4 Normal = Color4.LightGreen, Compare = Color4.IndianRed, Swap = Color4.CadetBlue;

        public MainWindow()
        {
            InitializeComponent();

            foreach (var value in Enum.GetNames(typeof(SortType)))
            {
                SortTypes.Add(value);
            }

            var settings = new GLWpfControlSettings
            {
                MajorVersion = 3,
                MinorVersion = 6
            };
            OpenTkControl.Start(settings);

            _program = CreateProgram(ColorVertexPath, ColorFragmentPath);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private int _size = 500;
        public int Size
        {
            get => _size;
            set
            {
                _size = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Size"));
            }
        }

        private int _speed = 100;
        public int Speed
        {
            get => _speed;
            set
            {
                _speed = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Speed"));
            }
        }

        private int _totalComparison = 0;
        public int TotalComparison
        {
            get => _totalComparison;
            set
            {
                _totalComparison = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TotalComparison"));
            }
        }

        private int _totalSwap = 0;
        public int TotalSwap
        {
            get => _totalSwap;
            set
            {
                _totalSwap = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TotalSwap"));
            }
        }

        private SortType? _sortType = null;

        public ObservableCollection<string> SortTypes { get; } = new ObservableCollection<string>();

        private SortStep[] _steps;
        private float _currentStep = 0f;

        private bool _working = false;

        private void OpenTkControl_OnRender(TimeSpan delta)
        {
            GL.ClearColor(Color4.WhiteSmoke);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Matrix4 projection = Matrix4.CreateOrthographicOffCenter(0f, 1f, 0f, 1f, -1f, 1f);
            int stepInt = (int)_currentStep;
            if (_steps != null)
            {
                SortStep current = _steps[stepInt];
                for (int i = 0; i < _steps[stepInt].Count; ++i)
                {
                    int val = current[i];
                    Color4 color = Normal;
                    var processing = current.Processing.ToList();
                    int idx = processing.FindIndex((p) => p.Item1 == i);
                    if (idx >= 0)
                    {
                        color = processing[idx].Item2;
                    }
                    RenderObject ro = new RenderObject(ObjectFactory.Rectangle(1f, 1f, color), _program)
                    {
                        Position = new Vector3((i + 0.5f) / current.Count, 0.5f, 0f),
                        Scale = new Vector3(1f / current.Count, (float)val / current.Count, 1f)
                    };
                    ro.Render(ref projection);
                    ro.Dispose();
                }
                (TotalComparison, TotalSwap) = (current.Comparison, current.Swap);
                if (_working && delta.TotalSeconds < 0.5)
                {
                    _currentStep += (float)(delta.TotalSeconds * _speed * 30);
                    _currentStep = Math.Min(_currentStep, _steps.Length - 1);
                }
                if (_currentStep >= _steps.Length - 1)
                {
                    Stop_Click(this, new RoutedEventArgs());
                }
            }
        }

        private void SortType_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton button)
            {
                if (Enum.TryParse(button.Content.ToString(), out SortType sortType))
                {
                    _sortType = sortType;
                    _startButton.IsEnabled = true;
                }
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            var toToggle = new UIElement[] { _sizeText, _sizeSlider, _sortList };
            TotalComparison = TotalSwap = 0;
            _currentStep = 0;
            if (_size == 1)
            {
                _steps = new SortStep[] { new SortStep(new int[] { 1 }, Array.Empty<(int, Color4)>(), 0, 0) };
                return;
            }
            foreach (var element in toToggle)
                element.IsEnabled = false;
            int[] array = Enumerable.Range(1, Size).ToArray();
            Shuffle(array);
            var sortmethod = typeof(Sort).GetMethod(Enum.GetName(typeof(SortType), _sortType.Value));
            var list = (sortmethod.Invoke(null, new object[] { array }) as IEnumerable<SortStep>).ToList();
            list.Add(new SortStep(
                list.Last().GetClonedSequence(),
                Array.Empty<(int, Color4)>(),
                list.Last().Comparison,
                list.Last().Swap));
            _steps = list.ToArray();
            _working = true;
            _startButton.Visibility = Visibility.Hidden;
            _pauseButton.Visibility = _stopButton.Visibility = Visibility.Visible;
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            if (_working)
            {
                _working = false;
                _pauseButton.Content = "Resume";
            }
            else
            {
                _working = true;
                _pauseButton.Content = "Pause";
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            _working = false;
            _startButton.Visibility = Visibility.Visible;
            _pauseButton.Visibility = _stopButton.Visibility = Visibility.Hidden;
            _pauseButton.Content = "Pause";
            var toToggle = new UIElement[] { _sizeText, _sizeSlider, _sortList };
            foreach (var element in toToggle)
                element.IsEnabled = true;
            if (sender is Button)
            {
                _currentStep = TotalComparison = TotalSwap = 0;
                _steps = null;
            }
        }

        private void SizeText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox text)
            {
                if (_sizeSlider != null && Int32.TryParse(text.Text, out int size))
                {
                    size = (int)(size < _sizeSlider.Minimum ? _sizeSlider.Minimum : (size > _sizeSlider.Maximum ? _sizeSlider.Maximum : size));
                    Size = size;
                }
            }
        }

        private void SpeedText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox text)
            {
                if (_speedSlider != null && Int32.TryParse(text.Text, out int speed))
                {
                    speed = (int)(speed < _speedSlider.Minimum ? _speedSlider.Minimum : (speed > _speedSlider.Maximum ? _speedSlider.Maximum : speed));
                    Speed = speed;
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {

            base.OnClosed(e);
        }

        private static int CreateProgram(string vertexPath, string fragmentPath)
        {
            int program = GL.CreateProgram();
            List<int> shaders = new List<int>
            {
                CompileShader(ShaderType.VertexShader, vertexPath),
                CompileShader(ShaderType.FragmentShader, fragmentPath)
            };

            foreach (int shader in shaders)
            {
                GL.AttachShader(program, shader);
            }

            GL.LinkProgram(program);
            string info = GL.GetProgramInfoLog(program);
            if (!String.IsNullOrWhiteSpace(info))
            {
                Debug.WriteLine($"GL.LinkProgram had info log: {info}");
            }

            foreach (int shader in shaders)
            {
                GL.DetachShader(program, shader);
                GL.DeleteShader(shader);
            }
            return program;
        }

        private static int CompileShader(ShaderType type, string filepath)
        {
            int shader = GL.CreateShader(type);
            string source = File.ReadAllText(filepath);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);
            string info = GL.GetShaderInfoLog(shader);
            if (!String.IsNullOrWhiteSpace(info))
            {
                Debug.WriteLine($"GL.CompileShader [{type}] had info log: {info}");
            }
            return shader;
        }

        private static void Shuffle(int[] array)
        {
            Random random = new Random();
            for (int i = 0; i < array.Length; ++i)
            {
                int j = random.Next(array.Length);
                (array[i], array[j]) = (array[j], array[i]);
            }
        }
    }
}