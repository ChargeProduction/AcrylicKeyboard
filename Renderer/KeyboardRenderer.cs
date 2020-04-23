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
        private readonly Keyboard keyboard;

        private readonly List<KeyValuePair<KeyInstance, KeyRenderer>> renderList =
            new List<KeyValuePair<KeyInstance, KeyRenderer>>();

        private int keyWidth;
        private int keyHeight;
        private KeyInstance[][] keys;

        public KeyboardRenderer(Keyboard keyboard)
        {
            this.keyboard = keyboard;
            keyboard.OnResize += OnResize;
            keyboard.OnLayoutChanged += KeyboardOnOnLayoutChanged;
            CalculateKeyBounds();
        }

        public void OnUpdate(double delta)
        {
            ForEachkey(key => key.OnUpdate(delta));
        }

        public void OnRender(DrawingContext context)
        {
            if (Bounds.Width > 0 && Bounds.Height > 0)
            {
                context.PushTransform(new TranslateTransform(Bounds.X, Bounds.Y));
                for (var i = 0; i < renderList.Count; i++)
                {
                    var kvp = renderList[i];
                    kvp.Value.Render(Keyboard, context, kvp.Key, this);
                }

                context.Pop();
            }
        }

        /// <summary>
        ///     Invokes an action for every key in the 2-dimensional matrix.
        /// </summary>
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
            CalculateKeyBounds();
        }

        /// <summary>
        ///     Calculates bounds for every key on the keyboard.
        /// </summary>
        private void CalculateKeyBounds()
        {
            if (keys != null && keys.Length > 0 && !Bounds.IsEmpty)
            {
                keyWidth = (int) Bounds.Width / 13;
                keyHeight = (int) Bounds.Height / keys.Length;
                for (var i = 0; i < keys.Length; i++)
                {
                    var horizontalSpacePx = 0;
                    var fillerCount = 0;
                    for (var j = 0; j < keys[i].Length; j++)
                    {
                        var rendererSettings = keys[i][j].Settings;
                        if (!rendererSettings.IsSizeStar)
                        {
                            horizontalSpacePx += (int) (rendererSettings.SizeValue * KeyWidth);
                        }
                        else
                        {
                            fillerCount++;
                        }
                    }

                    CalculateKeyBoundsRow(keys[i], (int) Bounds.Height / keys.Length * i, horizontalSpacePx,
                        fillerCount);
                }
            }
        }

        /// <summary>
        ///     Calculates the bounds for a given row of the keyboard.
        /// </summary>
        /// <param name="row">The row to calculate the bounds for.</param>
        /// <param name="offsetY">The y axis offset in pixels.</param>
        /// <param name="usedHorizontalSpace">Total horizontal space without filling keys.</param>
        /// <param name="fillerCount">Amount of keys which have the "*" (star) size property.</param>
        private void CalculateKeyBoundsRow(KeyInstance[] row, int offsetY, int usedHorizontalSpace, double fillerCount)
        {
            var remainingSize = (int) Bounds.Width - usedHorizontalSpace;
            var fillerSize = remainingSize / fillerCount;
            double fractionalFillerSize = 0;
            double offsetX = 0;
            for (var j = 0; j < row.Length; j++)
            {
                var key = row[j];
                var widthValue = (int) fillerSize;
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

                key.Resize(new Rect(offsetX, offsetY, widthValue, keyHeight));
                offsetX += widthValue;
            }
        }

        /// <summary>
        ///     Preserves all key states with the <see cref="KeySettings.Identity" /> property set to not null.
        ///     Then all keys are recreated for the new layout and the preserved states are being assigned to the
        ///     keys with the same <see cref="KeySettings.Identity" />.
        /// </summary>
        private void KeyboardOnOnLayoutChanged(Keyboard sender, LayoutChangedEventArgs args)
        {
            var preservedStates = PreserveStates();
            keys = null;
            var config = Keyboard.GetLayoutConfig();
            if (config != null && Keyboard.SelectedLayout != null &&
                config.Layouts.TryGetValue(Keyboard.SelectedLayout, out var layout))
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

                CalculateKeyBounds();
                ApplyStates(preservedStates);
            }
        }

        public KeyInstance GetKeyAt(int x, int y)
        {
            x -= (int) Bounds.X;
            y -= (int) Bounds.Y;
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

        /// <summary>
        ///     Preserves all keys states which have the <see cref="KeySettings.Identity" /> property set to not null by
        ///     saving them to a dictionary.
        /// </summary>
        private Dictionary<string, KeyInstance> PreserveStates()
        {
            if (keys != null)
            {
                var preservedStates = new Dictionary<string, KeyInstance>();
                for (var i = 0; i < keys.Length; i++)
                {
                    for (var j = 0; j < keys[i].Length; j++)
                    {
                        var key = keys[i][j];
                        string identity;
                        if (!string.IsNullOrEmpty(key.Settings.Identity))
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

        /// <summary>
        ///     Applies all preserved states to keys with the same <see cref="KeySettings.Identity" />.
        /// </summary>
        /// <param name="preservedStates"></param>
        private void ApplyStates(Dictionary<string, KeyInstance> preservedStates)
        {
            Debug.Assert(keys != null);
            if (preservedStates != null)
            {
                for (var i = 0; i < keys.Length; i++)
                {
                    for (var j = 0; j < keys[i].Length; j++)
                    {
                        var key = keys[i][j];
                        string identity;
                        if (!string.IsNullOrEmpty(key.Settings.Identity))
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

        /// <summary>
        ///     Gets the key width in pixel.
        /// </summary>
        public int KeyWidth => keyWidth;

        /// <summary>
        ///     Gets the key height in pixel.
        /// </summary>
        public int KeyHeight => keyHeight;

        /// <summary>
        ///     Gets the keyboard bounds.
        /// </summary>
        public Rect Bounds => Keyboard.KeyboardBounds;

        /// <summary>
        ///     Gets the keyboard.
        /// </summary>
        public Keyboard Keyboard => keyboard;

        /// <summary>
        ///     Gets the key matrix.
        /// </summary>
        public KeyInstance[][] Keys => keys;
    }
}