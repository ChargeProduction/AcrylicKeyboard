using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using AcrylicKeyboard.Events;
using AcrylicKeyboard.Renderer.Animation;
using AcrylicKeyboard.Theme;
using GlmSharp;

namespace AcrylicKeyboard
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private string fps;
        private Rect targetBounds = new Rect(100, 100, 800, 300);
        private SolidColorBrush titlebarCloseHoverColor = new SolidColorBrush(Colors.Black);
        private string titlebarText;
        private SolidColorBrush titlebarTextColor = new SolidColorBrush(Colors.Black);
        private int titlebarWidth;
        private double topOffset;
        private TransformAnimation topOffsetEnterAnimation;
        private TransformAnimation topOffsetExitAnimation;

        public MainWindow()
        {
            Hide();
            VisualTextRenderingMode = TextRenderingMode.Auto;
            Loaded += OnLoaded;

            Width = targetBounds.Width;
            Height = targetBounds.Height;
            Left = targetBounds.X;
            Top = targetBounds.Y;
            Topmost = true;

            InitializeComponent();

            Keyboard = new Keyboard(MainGrid);
            Keyboard.OnResize += OnKeyboardResize;
            Keyboard.OnLayoutChanged += OnKeyboardLayoutChanged;
            Keyboard.OnThemeChanged += OnKeyboardThemeChanged;
            Keyboard.OnUpdate += OnUpdate;

            Keyboard.RegisterKeyboard("DE", Environment.CurrentDirectory + @"\keyboard\keyboard_de.json");
            Keyboard.RegisterKeyboard("EN", Environment.CurrentDirectory + @"\keyboard\keyboard_en.json");

            Keyboard.Theme = KeyboardTheme.Default;

            InitAnimation();

            var chrome = new WindowChrome();
            chrome.CaptionHeight = 0;
            WindowChrome.SetWindowChrome(this, chrome);
        }

        public Keyboard Keyboard { get; }

        public int TitlebarWidth
        {
            get => titlebarWidth;
            set
            {
                titlebarWidth = Math.Max(0, value);
                OnPropertyChanged();
            }
        }

        public string TitlebarText
        {
            get => titlebarText;
            set
            {
                titlebarText = value;
                OnPropertyChanged();
            }
        }

        public SolidColorBrush TitlebarTextColor
        {
            get => titlebarTextColor;
            set
            {
                titlebarTextColor = value;
                OnPropertyChanged();
            }
        }

        public SolidColorBrush TitlebarCloseHoverColor
        {
            get => titlebarCloseHoverColor;
            set
            {
                titlebarCloseHoverColor = value;
                OnPropertyChanged();
            }
        }

        public string Fps
        {
            get => fps;
            set
            {
                if (fps != value)
                {
                    fps = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnUpdate(Keyboard sender, double delta)
        {
            var hasEnteredCompletely = topOffsetEnterAnimation.HasFinished;

            if (!hasEnteredCompletely)
                SetWindowTop(topOffsetEnterAnimation.CurrentFrame.Position.y);
            else if (topOffsetExitAnimation.HasStarted) SetWindowTop(topOffsetExitAnimation.CurrentFrame.Position.y);

            Fps = (int) Keyboard.Canvas.SmoothFps + " FPS";
        }

        private void InitAnimation()
        {
            var top = targetBounds.Top;
            SetWindowTop(top);
            topOffsetEnterAnimation = new TransformAnimation();
            topOffsetExitAnimation = new TransformAnimation();

            ApplySlideTransformFrame(topOffsetEnterAnimation);

            topOffsetEnterAnimation.OnFinish += sender => { FreeRootElement(); };
        }

        private void ApplySlideTransformFrame(TransformAnimation animation, bool reverse = false)
        {
            var startPosition = new TransformFrame
            {
                Duration = 0.7,
                Position = new dvec2(0, targetBounds.Top + Height)
            };
            var endPosition = new TransformFrame
            {
                Duration = 0.5,
                Position = new dvec2(0, targetBounds.Top)
            };
            if (reverse)
            {
                animation.AddFrame(endPosition);
                animation.AddFrame(startPosition);
            }
            else
            {
                animation.AddFrame(startPosition);
                animation.AddFrame(endPosition);
            }
        }

        private void OnKeyboardThemeChanged(Keyboard sender, ThemeChangedEventArgs args)
        {
            if (args.Theme != null)
            {
                TitlebarTextColor = new SolidColorBrush(args.Theme.GetColor(ThemeColor.KeyForeground));
                var color = TitlebarTextColor.Color;
                color.A = 128;
                TitlebarTextColor.Color = color;
                TitlebarCloseHoverColor = new SolidColorBrush(args.Theme.GetColor(ThemeColor.KeyForeground));
            }
        }

        private void OnKeyboardLayoutChanged(Keyboard sender, LayoutChangedEventArgs args)
        {
            TitlebarText = "Acrylic Keyboard: " + args.Config.Title;
        }

        private void OnKeyboardResize(Keyboard sender, ResizeEventArgs args)
        {
            TitlebarWidth = (int) args.KeyboardBounds.Width;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            FixRootElement();
            Height = 0;
            MinHeight = 0;
            WinApiHelper.EnableBlur(this, WinApiHelper.AccentState.ACCENT_ENABLE_BLURBEHIND);
            WinApiHelper.MakeUnfocusable(this);
            Keyboard.Animator.Play(topOffsetEnterAnimation);
            Show();
        }

        private void CloseButtonClicked(object sender, RoutedEventArgs e)
        {
            targetBounds = new Rect(Left, Top, ActualWidth, ActualHeight);
            ApplySlideTransformFrame(topOffsetExitAnimation, true);
            FixRootElement();
            topOffsetExitAnimation.OnFinish += obj =>
            {
                Close();
                Application.Current.Shutdown();
            };
            Keyboard.Animator.Play(topOffsetExitAnimation);
        }

        public void FixRootElement()
        {
            RootElement.VerticalAlignment = VerticalAlignment.Top;
            RootElement.HorizontalAlignment = HorizontalAlignment.Left;
            RootElement.MinWidth = RootElement.ActualWidth;
            RootElement.MinHeight = RootElement.ActualHeight;
            Keyboard.FreezeLayout = true;
        }

        public void FreeRootElement()
        {
            RootElement.VerticalAlignment = VerticalAlignment.Stretch;
            RootElement.HorizontalAlignment = HorizontalAlignment.Stretch;
            RootElement.MinWidth = 0;
            RootElement.MinHeight = 0;
            Keyboard.FreezeLayout = false;
        }

        public void SetWindowTop(double value)
        {
            if (topOffset != value)
                // Invoke async because the DrawingCanvas does not allow size changes while rendering.
                Dispatcher.InvokeAsync(() =>
                {
                    topOffset = value;
                    Top = topOffset;
                    Height = Math.Max(0, targetBounds.Height - (Top - targetBounds.Y));
                });
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void TitlebarMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }
}