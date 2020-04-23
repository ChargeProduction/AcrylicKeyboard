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
using AcrylicKeyboard.Renderer.Animation;
using AcrylicKeyboard.Theme;

namespace AcrylicKeyboard
{
    public class Keyboard : IDisposable
    {
        public delegate void OnKeyActionDelegate(Keyboard sender, KeyActionEventArgs args);

        public delegate void OnPopupOpenDelegate(Keyboard sender, PopupOpenEventArgs args);

        public delegate void OnUpdateDelegate(Keyboard sender, double delta);

        public delegate void OnRenderDelegate(Keyboard sender, DrawingContext context);

        public delegate void OnResizeDelegate(Keyboard sender, ResizeEventArgs args);

        public delegate void OnLayoutChangedDelegate(Keyboard sender, LayoutChangedEventArgs args);

        public delegate void OnThemeChangedDelegate(Keyboard sender, ThemeChangedEventArgs args);

        private readonly Grid panel;
        private readonly DrawingCanvas canvas;
        private readonly KeyboardRenderer keyboardRenderer;
        private readonly KeyboardPopupRenderer popupRenderer;
        private readonly Animator animator = new Animator();
        private readonly List<KeyModifier> activeKeyModifiers = new List<KeyModifier>();
        private readonly Dictionary<String, String> configurationFiles = new Dictionary<String, String>();

        private readonly Dictionary<String, KeyboardLayoutConfig> keyboards =
            new Dictionary<String, KeyboardLayoutConfig>();

        private KeyboardTheme theme;
        private String selectedLanguage;
        private String selectedLayout;
        private Size canvasSize;
        private Rect keyboardBounds;
        private int keyGap;
        private bool hasInitedRenderers;

        private InputHandler inputHandler;
        private InputSimulator inputSimulator;
        private IKeyboardSizeResolver sizeResolver;

        public event OnKeyActionDelegate OnKeyAction;
        public event OnPopupOpenDelegate OnPopupOpen;
        public event OnUpdateDelegate OnUpdate;
        public event OnRenderDelegate OnRenderBefore;
        public event OnRenderDelegate OnRenderAfter;
        public event OnResizeDelegate OnResize;
        public event OnLayoutChangedDelegate OnLayoutChanged;
        public event OnThemeChangedDelegate OnThemeChanged;

        public Keyboard(Grid panel)
        {
            this.panel = panel;

            canvas = new DrawingCanvas(OnUpdateEvent, OnRenderEvent);
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
            InvalidateRenderer();
        }

        /// <summary>
        /// Performs the action of the specified <see cref="KeyInstance"/>.
        /// The action is always dispatched on the Dispatcher thread.
        /// </summary>
        /// <param name="key">The <see cref="KeyInstance"/> of the action.</param>
        /// <param name="isHolding">Determines whether or not the holding action should be invoked.</param>
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
                if (key.Settings.KnownModifier != KeyModifier.None)
                {
                    switch (key.Settings.KnownModifier)
                    {
                        case KeyModifier.Shift:
                            if (isHolding)
                            {
                                InputSimulator.Keyboard.KeyPress(VirtualKeyCode.CAPITAL);
                            }
                            else
                            {
                                ToggleModifier(KeyModifier.Shift);
                                if (WinApiHelper.IsCapsLock)
                                {
                                    InputSimulator.Keyboard.KeyPress(VirtualKeyCode.CAPITAL);
                                    DisableModifier(KeyModifier.Shift);
                                }
                            }

                            break;
                        case KeyModifier.Ctrl:
                            ToggleModifier(KeyModifier.Ctrl);
                            break;
                        case KeyModifier.Alt:
                            ToggleModifier(KeyModifier.Alt);
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
                            if (key.Settings.KnownModifier == KeyModifier.None)
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
                            PressReturnKey();
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

        /// <summary>
        /// Toggles a key role as modifier.
        /// </summary>
        /// <param name="modifier">The <see cref="KeyModifier"/> to toggle.</param>
        private void ToggleModifier(KeyModifier modifier)
        {
            if (IsModifierActive(modifier))
            {
                DisableModifier(modifier);
            }
            else
            {
                ActivateModifier(modifier);
            }
        }

        /// <summary>
        /// Explicitly activates a key modifier.
        /// </summary>
        /// <param name="modifier">The <see cref="KeyModifier"/> to activate.</param>
        private void ActivateModifier(KeyModifier modifier)
        {
            if (!activeKeyModifiers.Contains(modifier))
            {
                activeKeyModifiers.Add(modifier);
            }
        }

        /// <summary>
        /// Disables all active key modifiers.
        /// </summary>
        private void DisableAllModifiers()
        {
            activeKeyModifiers.Clear();
        }

        /// <summary>
        /// Explicitly disables a key modifier.
        /// </summary>
        /// <param name="modifier">The <see cref="KeyModifier"/> to disable.</param>
        private void DisableModifier(KeyModifier modifier)
        {
            activeKeyModifiers.Remove(modifier);
        }

        /// <summary>
        /// Moves the cared by a specified amount.
        /// </summary>
        /// <param name="to">The amount of steps in a given direction. Below 0 is left.</param>
        private void MoveCaret(int to)
        {
            var keycode = to < 0 ? VirtualKeyCode.LEFT : VirtualKeyCode.RIGHT;
            to = Math.Abs(to);
            for (int i = 0; i < to; i++)
            {
                InputSimulator.Keyboard.KeyPress(keycode);
            }
        }

        /// <summary>
        /// Simulates the insertion of a specified string.
        /// </summary>
        /// <param name="text">The string to insert.</param>
        private void InsertText(String text)
        {
            if (text == null) return;

            SetModifierKeyState(true);
            InputSimulator.Keyboard.TextEntry(text);
            OnKeyAction?.Invoke(this, new KeyActionEventArgs(KeyboardAction.InsertOnText, text, activeKeyModifiers));
            SetModifierKeyState(false);

            DisableAllModifiers();
        }

        /// <summary>
        /// Simulates the press of the return key.
        /// </summary>
        private void PressReturnKey()
        {
            InputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN);
            DisableAllModifiers();
        }

        /// <summary>
        /// Switches the layout to the given language and layout name.
        /// </summary>
        /// <param name="language">The registered language to switch to.</param>
        /// <param name="layout">The registered layout to switch to.</param>
        public void SwitchLayout(String language, String layout)
        {
            selectedLanguage = language;
            selectedLayout = layout;
            if (language != null && layout != null && keyboards.ContainsKey(language) &&
                keyboards[language].Layouts.ContainsKey(layout))
            {
                InvalidateLayout();
            }

            InputHandler?.InvalidatePointerPosition();
        }

        /// <summary>
        /// Invalidates the layout and invokes the event.
        /// </summary>
        private void InvalidateLayout()
        {
            OnLayoutChanged?.Invoke(this,
                new LayoutChangedEventArgs(SelectedLanguage, SelectedLayout, GetLayoutConfig()));
        }

        /// <summary>
        /// Simulates the press or release of a modifier key.
        /// </summary>
        /// <param name="down">Determines whether or not the simulator should press down.</param>
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
                    case KeyModifier.Shift:
                        InvokeActionInternal(VirtualKeyCode.SHIFT);
                        break;
                    case KeyModifier.Ctrl:
                        InvokeActionInternal(VirtualKeyCode.CONTROL);
                        break;
                    case KeyModifier.Alt:
                        InvokeActionInternal(VirtualKeyCode.MENU);
                        break;
                }
            }
        }

        /// <summary>
        /// Simulates a delete key press.
        /// </summary>
        private void DeleteText()
        {
            InputSimulator.Keyboard.KeyPress(VirtualKeyCode.BACK);
        }

        /// <summary>
        /// Registers a file to a specified language.
        /// </summary>
        /// <param name="language">The language of the file.</param>
        /// <param name="file">The file name.</param>
        public void RegisterKeyboard(string language, string file)
        {
            language = language?.ToUpper();
            if (language != null)
            {
                configurationFiles[language] = file;
                LoadKeyboard(language, file);
            }
        }

        /// <summary>
        /// Reloads all registered keyboard files.
        /// </summary>
        public void ReloadKeyboards()
        {
            keyboards.Clear();
            selectedLanguage = null;
            selectedLayout = null;
            foreach (var kvp in configurationFiles)
            {
                LoadKeyboard(kvp.Key, kvp.Value);
            }

            InvalidateLayout();
        }

        /// <summary>
        /// Registers a single keyboard file specified by language.
        /// </summary>
        /// <param name="language">The language string.</param>
        /// <param name="file">The file name.</param>
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

        private void OnUpdateEvent(double delta)
        {
            Animator.Update(delta);
            NotifyRenderers(renderer => renderer.Update(this, delta));
            OnUpdate?.Invoke(this, delta);
        }

        private void OnRenderEvent(DrawingContext context)
        {
            if (!hasInitedRenderers)
            {
                NotifyRenderers(renderer => renderer.Init(context));
                hasInitedRenderers = true;
            }

            if (HasConfig)
            {
                OnRenderBefore?.Invoke(this, context);
                KeyboardRenderer?.OnRender(context);
                PopupRenderer?.OnRender(context);
                OnRenderAfter?.Invoke(this, context);
            }
        }

        /// <summary>
        /// Invokes an action for all renderers.
        /// </summary>
        private void NotifyRenderers(Action<KeyRenderer> action)
        {
            if (Theme.FallbackRenderer != null)
            {
                action(Theme.FallbackRenderer);
            }

            foreach (var kvp in Theme.Renderers)
            {
                if (Theme.FallbackRenderer != kvp.Value)
                {
                    action(kvp.Value);
                }
            }
        }

        /// <summary>
        /// Gets the selected language;
        /// </summary>
        public string SelectedLanguage
        {
            get => selectedLanguage;
        }

        /// <summary>
        /// Determines whether or not a specified <see cref="KeyModifier"/> is active.
        /// </summary>
        public bool IsModifierActive(KeyModifier modifier)
        {
            return activeKeyModifiers.Contains(modifier);
        }

        /// <summary>
        /// Returns the current <see cref="KeyboardLayoutConfig"/>. 
        /// </summary>
        public KeyboardLayoutConfig GetLayoutConfig()
        {
            if (keyboards.TryGetValue(SelectedLanguage ?? "EN", out var result))
            {
                return result;
            }

            return null;
        }

        /// <summary>
        /// Returns a upper or lower case variant of a specified string depending on an active
        /// <see cref="KeyModifier.Shift"/> modifier.
        /// If <see cref="KeySettings.IgnoreCap"/> is set to true, no changes will be made.
        /// </summary>
        /// <param name="settings">The keys settings.</param>
        /// <param name="text">The string to transform.</param>
        /// <returns>A lower or upper variant of the string.</returns>
        public String GetKeyText(KeySettings settings, String text)
        {
            if (settings != null && !settings.IgnoreCap && text != null)
            {
                if (IsModifierActive(KeyModifier.Shift) || WinApiHelper.IsCapsLock)
                {
                    return text.ToUpper();
                }

                return text.ToLower();
            }

            return text;
        }

        /// <summary>
        /// Gets the selected layout.
        /// </summary>
        public string SelectedLayout => selectedLayout;

        public void Dispose()
        {
            canvas.Dispose();
        }

        /// <summary>
        /// Gets or sets the keyboard theme.
        /// </summary>
        public KeyboardTheme Theme
        {
            get => theme;
            set
            {
                theme = value ?? KeyboardTheme.Default;
                OnThemeChanged?.Invoke(this, new ThemeChangedEventArgs(theme));
                InvalidateRenderer();
            }
        }

        /// <summary>
        /// Gets the <see cref="DrawingCanvas"/>.
        /// </summary>
        public DrawingCanvas Canvas => canvas;

        /// <summary>
        /// Gets the keyboard bounds.
        /// </summary>
        public Rect KeyboardBounds => keyboardBounds;

        /// <summary>
        /// Determines whether or not the keyboard has a selected <see cref="KeyboardLayoutConfig"/>.
        /// </summary>
        private bool HasConfig => SelectedLanguage != null && keyboards.ContainsKey(SelectedLanguage);

        /// <summary>
        /// Gets or sets the <see cref="InputHandler"/>.
        /// </summary>
        public InputHandler InputHandler
        {
            get => inputHandler;
            set
            {
                inputHandler = value;
                inputHandler?.Init();
            }
        }

        /// <summary>
        /// Gets the key gap.
        /// </summary>
        public int KeyGap => keyGap;

        /// <summary>
        /// Gets the <see cref="Animator"/>.
        /// </summary>
        public Animator Animator => animator;

        /// <summary>
        /// Gets the <see cref="KeyboardRenderer"/>.
        /// </summary>
        public KeyboardRenderer KeyboardRenderer => keyboardRenderer;

        /// <summary>
        /// Gets the <see cref="KeyboardPopupRenderer"/>.
        /// </summary>
        public KeyboardPopupRenderer PopupRenderer => popupRenderer;

        /// <summary>
        /// Gets or sets the <see cref="InputSimulator"/>.
        /// </summary>
        public InputSimulator InputSimulator
        {
            get => inputSimulator;
            set => inputSimulator = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="SizeResolver"/>.
        /// </summary>
        public IKeyboardSizeResolver SizeResolver
        {
            get => sizeResolver;
            set => sizeResolver = value;
        }

        /// <summary>
        /// Invalidates the canvas and forces redraw.
        /// </summary>
        public void InvalidateRenderer()
        {
            Canvas?.InvalidateRender();
        }
    }
}