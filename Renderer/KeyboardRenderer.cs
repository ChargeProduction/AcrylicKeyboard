using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using AcrylicKeyboard.Events;
using AcrylicKeyboard.Layout;

namespace AcrylicKeyboard.Renderer
{
    public class KeyboardRenderer : IKeyGroupRenderer
    {
        private Keyboard keyboard;
        private int keyGap = 3;
        private int keyWidth;
        private int keyHeight;

        private SolidColorBrush background = new SolidColorBrush(Colors.Fuchsia);

        private KeyInstance[][] keys = null;
        private List<KeyValuePair<KeyInstance, KeyRenderer>> renderList = new List<KeyValuePair<KeyInstance, KeyRenderer>>();
        
        public KeyboardRenderer(Keyboard keyboard)
        {
            this.keyboard = keyboard;
            keyboard.OnResize += OnResize;
            keyboard.OnLayoutChanged += KeyboardOnOnLayoutChanged;
            RecalculateKeyBounds();
        }

        public void OnRender(DrawingContext c)
        {
            ForEachkey(key => key.OnUpdate());
            
            if (Bounds.Width > 0 && Bounds.Height > 0)
            {
                c.PushTransform(new TranslateTransform(Bounds.X, Bounds.Y));
                for (var i = 0; i < renderList.Count; i++)
                {
                    var kvp = renderList[i];
                    kvp.Value.Render(Keyboard, c, kvp.Key, this);
                }
                c.Pop();
            }
        }

        private void ForEachkey(Action<KeyInstance> action)
        {
            if (keys != null)
            {
                for (var i = 0; i < renderList.Count; i++)
                {
                    action(renderList[i].Key);
                }
            }
        }

        private void OnResize(Keyboard sender, ResizeEventArgs args)
        {
            RecalculateKeyBounds();
        }

        private void RecalculateKeyBounds()
        {
            if (keys != null && keys.Length > 0 && !Bounds.IsEmpty)
            {
                keyWidth = (int) Bounds.Width / 13;
                keyHeight = (int) Bounds.Height / keys.Length;
                for (var i = 0; i < keys.Length; i++)
                {
                    int usedHorizontalSpace = 0;
                    int fillerCount = 0;
                    for (var j = 0; j < keys[i].Length; j++)
                    {
                        var rendererSettings = keys[i][j].Settings;
                        if (!rendererSettings.IsSizeStar)
                        {
                            usedHorizontalSpace += (int)(rendererSettings.SizeValue * KeyWidth);
                        }
                        else
                        {
                            fillerCount++;
                        }
                    }
                    ResizeKeyRendererRow(keys[i], (int)Bounds.Height / keys.Length * i, usedHorizontalSpace, fillerCount);
                }
            }
        }

        private void ResizeKeyRendererRow(KeyInstance[] row, int offsetY, int usedHorizontalSpace, double fillerCount)
        {
            int remainingSize = (int)Bounds.Width - usedHorizontalSpace;
            double fillerSize = remainingSize / fillerCount;
            double fractionalFillerSize = 0;
            double offsetX = 0;
            for (var j = 0; j < row.Length; j++)
            {
                var key = row[j];
                int widthValue = (int) fillerSize;
                if (key.Settings.IsSizeStar)
                {
                    fractionalFillerSize += fillerSize - widthValue;
                    if (fractionalFillerSize > 1)
                    {
                        fractionalFillerSize = 0;
                        widthValue++;
                    }

                }
                else
                {
                    widthValue = (int) (KeyWidth * key.Settings.SizeValue);
                }
                key.Resize(new Rect(offsetX, offsetY, widthValue, keyHeight), this);
                offsetX += widthValue;
            }
        }

        private void KeyboardOnOnLayoutChanged(Keyboard sender, LayoutChangedEventArgs args)
        {
            var preservedStates = PreserveStates();
            keys = null;
            var config = Keyboard.GetLayoutConfig();
            if (config != null && Keyboard.SelectedLayout != null && config.Layouts.TryGetValue(Keyboard.SelectedLayout, out var layout))
            {
                renderList.Clear();
                keys = new KeyInstance[layout.Length][];
                for (var i = 0; i < keys.Length; i++)
                {
                    keys[i] = new KeyInstance[layout[i].Length];
                    for (var j = 0; j < Keys[i].Length; j++)
                    {
                        var settings = layout[i][j];
                        var key = Keyboard.Theme.GetKeyInstance(settings.Role);
                        var renderer = Keyboard.Theme.GetRenderer(settings.Role);
                        key.Init(Keyboard, settings);
                        renderList.Add(new KeyValuePair<KeyInstance, KeyRenderer>(key, renderer));
                        keys[i][j] = key;
                    }
                }
                RecalculateKeyBounds();
                ApplyStates(preservedStates);
            }
        }
        
        public KeyInstance GetKeyAt(int x, int y)
        {
            x -= (int)Bounds.X;
            y -= (int)Bounds.Y;
            var renderer = Keyboard.KeyboardRenderer;
            var keys = renderer.Keys;
            if (keys != null)
            {
                for (var i = 0; i < renderer.Keys.Length; i++)
                {
                    for (var j = 0; j < renderer.Keys[i].Length; j++)
                    {
                        var key = renderer.Keys[i][j];
                        if (key.Bounds.Contains(x, y))
                        {
                            return key;
                        }
                    }
                }
            }
            return null;
        }

        private Dictionary<String, KeyInstance> PreserveStates()
        {
            if (keys != null)
            {
                Dictionary<String, KeyInstance> preservedStates = new Dictionary<string, KeyInstance>();
                    for (var i = 0; i < keys.Length; i++)
                    {
                        for (var j = 0; j < keys[i].Length; j++)
                        {
                            var key = keys[i][j];
                            String identity;
                            if (!String.IsNullOrEmpty(key.Settings.Identity))
                            {
                                identity = key.Settings.Identity;
                                preservedStates[identity] = key;
                            }
                        }
                    }
                return preservedStates;
            }

            return null;
        }

        private void ApplyStates(Dictionary<String, KeyInstance> preservedStates)
        {
            Debug.Assert(keys != null);
            if (preservedStates != null)
            {
                for (var i = 0; i < keys.Length; i++)
                {
                    for (var j = 0; j < keys[i].Length; j++)
                    {
                        var key = keys[i][j];
                        String identity;
                        if (!String.IsNullOrEmpty(key.Settings.Identity))
                        {
                            identity = key.Settings.Identity;
                        }
                        else
                        {
                            identity = key.Bounds.ToString();
                        }
                        if (preservedStates.TryGetValue(identity, out var instance))
                        {
                            key.ApplyStates(instance);
                        }
                    }
                }
            }
        }

        public int KeyWidth => keyWidth;
        
        public int KeyHeight => keyHeight;

        public int KeyGap => keyGap;
        
        public Rect Bounds => Keyboard.KeyboardBounds;

        public Keyboard Keyboard => keyboard;

        public KeyInstance[][] Keys => keys;
    }
}