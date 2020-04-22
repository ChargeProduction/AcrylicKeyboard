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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private Keyboard keyboard;
        private int titlebarWidth;
        private String titlebarText;
        private SolidColorBrush titlebarTextColor = new SolidColorBrush(Colors.Black);
        private SolidColorBrush titlebarCloseHoverColor = new SolidColorBrush(Colors.Black); 
        private double topOffset;
        private TransformAnimation topOffsetEnterAnimation;
        private TransformAnimation topOffsetExitAnimation;
        private bool hasLoaded;
        Rect targetBounds = new Rect(100, 100, 800, 300);
        
        public MainWindow()
        {
            VisualTextRenderingMode = TextRenderingMode.Auto;
            Loaded += OnLoaded;
            InitializeComponent();
            keyboard = new Keyboard(MainGrid);
            Keyboard.OnResize += OnKeyboardResize;
            Keyboard.OnLayoutChanged += OnKeyboardLayoutChanged;
            Keyboard.OnThemeChanged += OnKeyboardThemeChanged;
            Keyboard.OnRenderBefore += OnUpdate;
            
            keyboard.RegisterKeyboard("DE", Environment.CurrentDirectory + @"\keyboard\keyboard_de.json");
            keyboard.RegisterKeyboard("EN", Environment.CurrentDirectory + @"\keyboard\keyboard_en.json");
            
            keyboard.Theme = KeyboardTheme.Default;
            
            Width = targetBounds.Width;
            Height = targetBounds.Height;
            Left = targetBounds.X;
            Top = targetBounds.Y;
            Topmost = true;
            InitAnimation();

            var chrome = new WindowChrome();
            chrome.CaptionHeight = 0;
            WindowChrome.SetWindowChrome(this, chrome);
            HideWindow();
        }

        private void OnUpdate(Keyboard sender, DrawingContext context)
        {
            var hasEnteredCompletely = topOffsetEnterAnimation.HasFinished;
            topOffsetEnterAnimation.Update();
            topOffsetExitAnimation.Update();

            if (!hasEnteredCompletely)
            {
                SetWindowTop(topOffsetEnterAnimation.CurrentFrame.Position.y);
            }
            else if (topOffsetExitAnimation.HasStarted)
            {
                SetWindowTop(topOffsetExitAnimation.CurrentFrame.Position.y);
            }
        }

        private void InitAnimation()
        {
            var top = targetBounds.Top;
            SetWindowTop(top);
            topOffsetEnterAnimation = new TransformAnimation();
            topOffsetExitAnimation = new TransformAnimation();

            ApplySlideTransformFrame(topOffsetEnterAnimation);

            topOffsetEnterAnimation.OnFinish += (sender) => { FreeRootElement(); };
        }

        private void ApplySlideTransformFrame(TransformAnimation animation, bool reverse = false)
        {
            var startPosition = new TransformFrame()
            {
                Duration = 700,
                Position = new dvec2(0, targetBounds.Top + Height)
            };
            var endPosition = new TransformFrame()
            {
                Duration = 500,
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
            TitlebarWidth = (int)args.KeyboardBounds.Width;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            FixRootElement();
            Height = 0;
            MinHeight = 0;
            ShowWindow();
            WinApiHelper.EnableBlur(this, WinApiHelper.AccentState.ACCENT_ENABLE_BLURBEHIND);
            WinApiHelper.MakeUnfocusable(this);
            topOffsetEnterAnimation.Start();
            hasLoaded = true;
        }

        private void CloseButtonClicked(object sender, RoutedEventArgs e)
        {
            targetBounds = new Rect(Left, Top, ActualWidth, ActualHeight);
            ApplySlideTransformFrame(topOffsetExitAnimation, true);
            FixRootElement();
            topOffsetExitAnimation.OnFinish += (obj) =>
            {
                Close();
                Application.Current.Shutdown();
            };
            topOffsetExitAnimation.Start();
        }

        public Keyboard Keyboard => keyboard;

        public int TitlebarWidth
        {
            get { return titlebarWidth; }
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

        private void HideWindow()
        {
            WinApiHelper.ClipWindow(this, new Rect(0, 0, 0, 0));
        }

        private void ShowWindow()
        {
            WinApiHelper.ClipWindow(this, Rect.Empty);
        }

        public void FixRootElement()
        {
            RootElement.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            RootElement.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            RootElement.MinWidth = RootElement.ActualWidth;
            RootElement.MinHeight = RootElement.ActualHeight;
        }

        public void FreeRootElement()
        {
            RootElement.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            RootElement.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            RootElement.MinWidth = 0;
            RootElement.MinHeight = 0;
        }

        public void SetWindowTop(double value)
        {
            if (topOffset != value)
            {
                // Invoke async because the DrawingCanvas does not allow size changes while rendering.
                Dispatcher.InvokeAsync(() =>
                {
                    topOffset = value;
                    Top = topOffset;
                    Height = Math.Max(0, targetBounds.Height - (Top - targetBounds.Y));
                });
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void TitlebarMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}