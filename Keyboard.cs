using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WindowsInput;
using WindowsInput.Native;
using AcrylicKeyboard.Events;
using AcrylicKeyboard.Interaction;
using AcrylicKeyboard.Layout;
using AcrylicKeyboard.Layout.Sizes;
using AcrylicKeyboard.Renderer;
using AcrylicKeyboard.Theme;

namespace AcrylicKeyboard
{
    public class Keyboard : IDisposable
    {
        public delegate void OnKeyActionDelegate(Keyboard sender, KeyActionEventArgs args);
        public delegate void OnPopupOpenDelegate(Keyboard sender, PopupOpenEventArgs args);
        public delegate void OnRenderDelegate(Keyboard sender, DrawingContext context);
        public delegate void OnResizeDelegate(Keyboard sender, ResizeEventArgs args);
        public delegate void OnLayoutChangedDelegate(Keyboard sender, LayoutChangedEventArgs args);
        public delegate void OnThemeChangedDelegate(Keyboard sender, ThemeChangedEventArgs args);
        
        private readonly Grid panel;
        private readonly DrawingCanvas canvas;
        
        private readonly Dictionary<String, String> configurationFiles = new Dictionary<String, String>();
        private readonly Dictionary<String, KeyboardLayoutConfig> keyboards = new Dictionary<String, KeyboardLayoutConfig>();
        
        private readonly List<KeyRole> activeKeyModifiers = new List<KeyRole>();
        
        private KeyboardTheme theme;
        private String selectedLanguage;
        private String selectedLayout;
        private Size canvasSize;
        private Rect keyboardBounds;
        private int keyGap;

        private readonly KeyboardRenderer keyboardRenderer;
        private readonly KeyboardPopupRenderer popupRenderer;
        private InputHandler inputHandler;
        private InputSimulator inputSimulator;
        private IKeyboardSizeResolver sizeResolver;

        public event OnKeyActionDelegate OnKeyAction;
        public event OnPopupOpenDelegate OnPopupOpen;
        public event OnRenderDelegate OnRenderBefore;
        public event OnRenderDelegate OnRenderAfter;
        public event OnResizeDelegate OnResize;
        public event OnLayoutChangedDelegate OnLayoutChanged;
        public event OnThemeChangedDelegate OnThemeChanged;
        
        public Keyboard(Grid panel)
        {
            this.panel = panel;
            
            canvas = new DrawingCanvas(OnRender);
            panel.Children.Clear();
            panel.Children.Add(canvas);
            Canvas.SizeChanged += OnSizeChanged;

            Theme = default;
            keyboardRenderer = new KeyboardRenderer(this);
            popupRenderer = new KeyboardPopupRenderer(this);
            InputHandler = new MouseInteractionHandler(this);
            InputSimulator = new InputSimulator();
            SizeResolver = new AspectRatioSizeResolver(3);
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            canvasSize = e.NewSize;
            if (SizeResolver != null)
            {
                var result = SizeResolver.ResolveSize(canvasSize);
                keyboardBounds = result.bounds;
                keyGap = result.gap;
            }
            else
            {
                keyboardBounds = new Rect(0, 0, canvasSize.Width, canvasSize.Height);
            }
            OnResize?.Invoke(this, new ResizeEventArgs(canvasSize, keyboardBounds));
        }

        public void PerformAction(KeyInstance key, bool isHolding)
        {
            if (panel.Dispatcher.CheckAccess())
            {
                InvokeAction();
            }
            else
            {
                panel.Dispatcher.Invoke(InvokeAction);
            }

            void InvokeAction()
            {
                if (key.Settings.KnownRole != KeyRole.Default)
                {
                    switch (key.Settings.KnownRole)
                    {
                        case KeyRole.Shift:
                            if (isHolding)
                            {
                                InputSimulator.Keyboard.KeyPress(VirtualKeyCode.CAPITAL);
                            }
                            else
                            {
                                ToggleModifier(KeyRole.Shift);
                                if (WinApiHelper.IsCapsLock)
                                {
                                    InputSimulator.Keyboard.KeyPress(VirtualKeyCode.CAPITAL);
                                    DisableModifier(KeyRole.Shift);
                                }
                            }
                            break;
                        case KeyRole.Ctrl:
                            ToggleModifier(KeyRole.Ctrl);
                            break;
                        case KeyRole.Alt:
                            ToggleModifier(KeyRole.Alt);
                            break;
                    }
                }

                if (isHolding && key.Settings.ExtraKeys != null)
                {
                    var popupOpenArgs = new PopupOpenEventArgs();
                    OnPopupOpen?.Invoke(this, popupOpenArgs);
                    if (!popupOpenArgs.PreventOpening)
                    {
                        InputHandler.InteractionMode = InteractionMode.Popup;
                        PopupRenderer.ShowPopup(key);
                    }
                }
                else
                {
                    var wasEventHandles = false;
                    var action = key.Settings.Action;
                    if (isHolding)
                    {
                        action = key.Settings.HoldingAction;
                    }
                    switch (action)
                    {
                        case KeyboardAction.InsertOnText:
                            if (key.Settings.KnownRole == KeyRole.Default)
                            {
                                InsertText(GetKeyText(key.Settings, key.Settings.InsertionText));
                                wasEventHandles = true;
                            }
                            break;
                        case KeyboardAction.CursorLeft:
                            MoveCaret(-1);
                            break;
                        case KeyboardAction.CursorRight:
                            MoveCaret(1);
                            break;
                        case KeyboardAction.Delete:
                            DeleteText();
                            break;
                        case KeyboardAction.ReloadKeyboard:
                            ReloadKeyboards();
                            break;
                        case KeyboardAction.Enter:
                            PressEnter();
                            break;
                        case KeyboardAction.SwitchLayout:
                            SwitchLayout(SelectedLanguage, key.Settings.Target);
                            break;
                        case KeyboardAction.ChangeLanguage:
                            SwitchLayout(key.Settings.Target, SelectedLayout);
                            break;
                    }

                    if (!wasEventHandles)
                    {
                        OnKeyAction?.Invoke(this, new KeyActionEventArgs(action));
                    }
                }
            }
        }

        private void ToggleModifier(KeyRole role)
        {
            if (IsModifierActive(role))
            {
                DisableModifier(role);
            }
            else
            {
                ActivateModifier(role);
            }
        }

        private void ActivateModifier(KeyRole role)
        {
            if (!activeKeyModifiers.Contains(role))
            {
                activeKeyModifiers.Add(role);
            }
        }

        private void DisableAllModifiers()
        {
            activeKeyModifiers.Clear();
        }

        private void DisableModifier(KeyRole role)
        {
            activeKeyModifiers.Remove(role);
        }

        private void MoveCaret(int to)
        {
            var keycode = to < 0 ? VirtualKeyCode.LEFT : VirtualKeyCode.RIGHT;
            to = Math.Abs(to);
            for (int i = 0; i < to; i++)
            {
                InputSimulator.Keyboard.KeyPress(keycode);
            }
        }

        private void InsertText(String text)
        {
            if (text == null) return;

            SetModifierKeyState(true);
            InputSimulator.Keyboard.TextEntry(text);
            OnKeyAction?.Invoke(this, new KeyActionEventArgs(KeyboardAction.InsertOnText, text, activeKeyModifiers));
            SetModifierKeyState(false);
            
            DisableAllModifiers();
        }

        private void PressEnter()
        {
            InputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN);
            DisableAllModifiers();
        }

        public void SwitchLayout(String language, String layout)
        {
            SelectedLanguage = language;
            SelectedLayout = layout;
            if (language != null && layout != null && keyboards.ContainsKey(language) && keyboards[language].Layouts.ContainsKey(layout))
            {
                InvalidateLayout();
            }
            InputHandler?.InvalidatePointerPosition();
        }

        private void InvalidateLayout()
        {
            OnLayoutChanged?.Invoke(this, new LayoutChangedEventArgs(SelectedLanguage, SelectedLayout, GetLayoutConfig()));
        }

        private void SetModifierKeyState(bool down)
        {
            void InvokeActionInternal(VirtualKeyCode key)
            {
                if (down)
                {
                    InputSimulator.Keyboard.KeyDown(key);
                }
                else
                {
                    InputSimulator.Keyboard.KeyUp(key);
                }
            }

            foreach (var modifier in activeKeyModifiers)
            {
                switch (modifier)
                {
                    case KeyRole.Shift:
                        InvokeActionInternal(VirtualKeyCode.SHIFT);
                        break;
                    case KeyRole.Ctrl:
                        InvokeActionInternal(VirtualKeyCode.CONTROL);
                        break;
                    case KeyRole.Alt:
                        InvokeActionInternal(VirtualKeyCode.MENU);
                        break;
                }
            }
        }

        private void DeleteText()
        {
            InputSimulator.Keyboard.KeyPress(VirtualKeyCode.BACK);
        }

        public void RegisterKeyboard(string language, string file)
        {
            language = language?.ToUpper();
            if (language != null)
            {
                configurationFiles[language] = file;
                LoadKeyboard(language, file);
            }
        }

        public void ReloadKeyboards()
        {
            keyboards.Clear();
            SelectedLanguage = null;
            SelectedLayout = null;
            foreach (var kvp in configurationFiles)
            {
                LoadKeyboard(kvp.Key, kvp.Value);
            }
            InvalidateLayout();
        }

        private void LoadKeyboard(String language, String file)
        {
            keyboards[language] = KeyboardLayoutConfig.FromFile(file);
            
            if (selectedLanguage == null)
            {
                SwitchLayout(language, SelectedLayout);
            }

            if (selectedLayout == null)
            {
                var layout = GetLayoutConfig()?.Layouts?.FirstOrDefault().Key;
                SwitchLayout(SelectedLanguage, layout);
            }
        }

        private void OnRender(DrawingContext context)
        {
            if (HasConfig)
            {
                Theme.FallbackRenderer?.BeforeRender(this);
                foreach (var kvp in Theme.Renderers)
                {
                    if (Theme.FallbackRenderer != kvp.Value)
                    {
                        kvp.Value.BeforeRender(this);
                    }
                }
                
                OnRenderBefore?.Invoke(this, context);
                KeyboardRenderer?.OnRender(context);
                PopupRenderer?.OnRender(context);
                OnRenderAfter?.Invoke(this, context);
            }
        }
        
        public string SelectedLanguage
        {
            get => selectedLanguage;
            set => selectedLanguage = value;
        }

        public bool IsModifierActive(KeyRole role)
        {
            return activeKeyModifiers.Contains(role);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public KeyboardLayoutConfig GetLayoutConfig()
        {
            if (keyboards.TryGetValue(SelectedLanguage ?? "EN", out var result))
            {
                return result;
            }
            return null;
        }

        public String GetKeyText(KeySettings settings, String text)
        {
            if (settings != null && !settings.IgnoreCap && text != null)
            {
                if (IsModifierActive(KeyRole.Shift) || WinApiHelper.IsCapsLock)
                {
                    return text.ToUpper();
                }

                return text.ToLower();
            }
            return text;
        }

        public string SelectedLayout
        {
            get => selectedLayout;
            set => selectedLayout = value;
        }

        public void Dispose()
        {
            canvas.Dispose();
        }

        public KeyboardTheme Theme
        {
            get => theme;
            set
            {
                theme = value ?? KeyboardTheme.Default;
                OnThemeChanged?.Invoke(this, new ThemeChangedEventArgs(theme));
            }
        }

        public DrawingCanvas Canvas => canvas;

        public Rect KeyboardBounds => keyboardBounds;

        private bool HasConfig => SelectedLanguage != null && keyboards.ContainsKey(SelectedLanguage);

        public InputHandler InputHandler
        {
            get => inputHandler;
            set
            {
                inputHandler = value;
                inputHandler?.Init();
            }
        }

        public int KeyGap => keyGap;

        public KeyboardRenderer KeyboardRenderer => keyboardRenderer;

        public KeyboardPopupRenderer PopupRenderer => popupRenderer;

        public InputSimulator InputSimulator
        {
            get => inputSimulator;
            set => inputSimulator = value;
        }

        public IKeyboardSizeResolver SizeResolver
        {
            get => sizeResolver;
            set => sizeResolver = value;
        }
    }
}