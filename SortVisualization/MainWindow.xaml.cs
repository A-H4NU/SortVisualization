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

        public static Color4 Normal = Color4.SpringGreen, Compare = Color4.IndianRed, Swap = Color4.MediumBlue;

        public MainWindow()
        {
            InitializeComponent();

            foreach (var value in Enum.GetNames(typeof(SortType)))
                SortTypes.Add(value);
            foreach (var value in Enum.GetNames(typeof(VisualizationType)))
                VisualizationTypes.Add(value);
            foreach (var value in Enum.GetNames(typeof(ColorType)))
                ColorTypes.Add(value);

            var settings = new GLWpfControlSettings
            {
                MajorVersion = 3,
                MinorVersion = 6
            };
            OpenTkControl.Start(settings);
            GL.ClearColor(Color4.White);

            _program = CreateProgram(ColorVertexPath, ColorFragmentPath);
            _colorTypeCmb.SelectedIndex = _visualTypeCmb.SelectedIndex = 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private int _size = 100;
        public int Size
        {
            get => _size;
            set
            {
                _size = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Size"));
            }
        }

        private int _speed = 50;
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
        private VisualizationType _visualType = VisualizationType.Bar;
        private ColorType _colorType = ColorType.Solid;

        public ObservableCollection<string> SortTypes { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> VisualizationTypes { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> ColorTypes { get; } = new ObservableCollection<string>();

        private SortStep[] _steps;
        private float _currentStep = 0f;

        private bool _working = false;

        private RenderObject[] _renderObjects = null;

        private void OpenTkControl_OnRender(TimeSpan delta)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Matrix4 projection = Matrix4.CreateOrthographicOffCenter(0f, 1f, 0f, 1f, -1f, 1f);
            int stepInt = (int)_currentStep;
            if (_steps != null)
            {
                SortStep current = _steps[stepInt];
                UpdateRenderObjectsAndRender(stepInt, ref projection);
                (TotalComparison, TotalSwap) = (current.Comparison, current.Swap);
                if (_working && delta.TotalSeconds < 0.5)
                {
                    _currentStep += (float)(delta.TotalSeconds * Math.Pow(_speed, 1.1) * 14.03);
                    _currentStep = Math.Min(_currentStep, _steps.Length - 1);
                }
                if (_currentStep >= _steps.Length - 1)
                {
                    Stop_Click(this, new RoutedEventArgs());
                }
            }
        }

        private void UpdateRenderObjectsAndRender(int stepInt, ref Matrix4 projection)
        {
            var step = _steps[stepInt];
            for (int i = 0; i < step.Count; ++i)
            {
                Color4 color = default;
                switch (_colorType)
                {
                    case ColorType.Solid:
                        color = Normal;
                        foreach (var (idx, clr) in step.Processing)
                        {
                            if (idx == i)
                            {
                                color = clr; break;
                            }
                        }
                        break;
                    case ColorType.Spectrum:
                        color = HSVtoRGB((float)step[i] / step.Count * 360, 0.8f, 1f); break;
                }
                switch (_visualType)
                {
                    case VisualizationType.Bar:
                        _renderObjects[i].Scale = new Vector3(1f / step.Count, (float)step[i] / step.Count, 1f);
                        break;
                    case VisualizationType.Radial:
                        float tworad = (float)Math.Sqrt((float)step[i] / step.Count);
                        _renderObjects[i].Scale = 0.5f * new Vector3(tworad, tworad, 2f);
                        _renderObjects[i].Rotation = new Vector3(0f, 0f, (float)i / step.Count * MathHelper.TwoPi);
                        break;
                    case VisualizationType.Dots:
                        _renderObjects[i].Position = new Vector3((i + 0.5f) / step.Count, (step[i] - 0.5f) / step.Count, 0f);
                        break;
                }
                GL.Uniform4(20, color);
                _renderObjects[i].Render(ref projection);
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

        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            TotalComparison = TotalSwap = 0;
            _currentStep = 0;
            _startButton.IsEnabled = false;
            int[] array = Enumerable.Range(1, Size).ToArray();
            Shuffle(array);
            var sortmethod = typeof(Sort).GetMethod(Enum.GetName(typeof(SortType), _sortType.Value));
            var sortTask = Task.Run(() =>
            {
                var list = (sortmethod.Invoke(null, new object[] { array.Clone() }) as IEnumerable<SortStep>).ToList();
                list.Add(new SortStep(
                    list.Last().GetClonedSequence(),
                    Array.Empty<(int, Color4)>(),
                    list.Last().Comparison,
                    list.Last().Swap));
                _steps = list.ToArray();
            });
            if (_size == 1)
            {
                _steps = new SortStep[] { new SortStep(new int[] { 1 }, Array.Empty<(int, Color4)>(), 0, 0) };
                return;
            }
            InitializeRenderObjects(array.Length);
            await sortTask;
            var toToggle = new UIElement[] { _sizeText, _sizeSlider, _sortList };
            foreach (var element in toToggle)
                element.IsEnabled = false;
            _working = true;
            _startButton.Visibility = Visibility.Hidden;
            _pauseButton.Visibility = _stopButton.Visibility = Visibility.Visible;
        }

        private void InitializeRenderObjects(int length)
        {
            if (_renderObjects != null)
            {
                foreach (var ro in _renderObjects)
                    ro.Dispose();
            }
            _renderObjects = new RenderObject[length];
            for (int i = 0; i < length; ++i)
            {
                switch (_visualType)
                {
                    case VisualizationType.Bar:
                        _renderObjects[i] = new RenderObject(ObjectFactory.Rectangle(1f, 1f), _program)
                        {
                            Position = new Vector3((i + 0.5f) / length, 0.5f, 0f)
                        };
                        break;
                    case VisualizationType.Radial:
                        _renderObjects[i] = new RenderObject(ObjectFactory.Arc(1f, MathHelper.TwoPi / length), _program)
                        {
                            Position = new Vector3(0.5f, 0.5f, 0f)
                        };
                        break;
                    case VisualizationType.Dots:
                        _renderObjects[i] = new RenderObject(ObjectFactory.Rectangle(1f, 1f), _program)
                        {
                            Scale = new Vector3((float)1 / length, (float)Math.Pow(length, 0.25) / length, 1f)
                        };
                        break;
                }
            }
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
            _startButton.IsEnabled = true;
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
            if (_renderObjects != null)
            {
                foreach (var ro in _renderObjects)
                {
                    ro.Dispose();
                }
            }

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

        private void _visualTypeCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox combo)
            {
                _visualType = (VisualizationType)combo.SelectedIndex;
                InitializeRenderObjects(Size);
            }
        }

        private void _colorTypeCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox combo)
            {
                _colorType = (ColorType)combo.SelectedIndex;
            }
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

        /// <param name="H">in degree</param>
        private static Color4 HSVtoRGB(float H, float S, float V)
        {
            H %= 360;
            float C = V * S;
            float X = C * (1 - Math.Abs((H / 60) % 2 - 1));
            float m = V - C;
            float r = 0f, g = 0f, b = 0f;
            if (H < 60) (r, g, b) = (C, X, 0);
            else if (H < 120) (r, g, b) = (X, C, 0);
            else if (H < 180) (r, g, b) = (0, C, X);
            else if (H < 240) (r, g, b) = (0, X, C);
            else if (H < 300) (r, g, b) = (X, 0, C);
            else (r, g, b) = (C, 0, X);
            return new Color4(r+m, g+m, b+m, 1f);
        }
    }
}