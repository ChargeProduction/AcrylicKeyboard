using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Media;
using AcrylicKeyboard.Layout;
using AcrylicKeyboard.Renderer;

namespace AcrylicKeyboard.Theme
{
    public sealed class KeyboardTheme
    {
        public static readonly KeyboardTheme Default = new KeyboardTheme
        {
            Colors =
            {
                {ThemeColor.PopupBackground, Color.FromArgb(255, 0, 0, 0)},

                {ThemeColor.KeyBackground, Color.FromArgb(230, 100, 100, 102)},
                {ThemeColor.KeyWithRoleBackground, Color.FromArgb(128, 96, 96, 98)},

                {ThemeColor.KeyForeground, Color.FromArgb(255, 229, 229, 234)},
                {ThemeColor.KeyWithRoleForeground, Color.FromArgb(255, 229, 229, 234)},

                {ThemeColor.KeyHoverBackground, Color.FromArgb(230, 229, 229, 234)},
                {ThemeColor.KeyHoverForeground, Color.FromArgb(255, 51, 51, 51)},

                {ThemeColor.KeyDownBackground, Color.FromArgb(230, 0, 97, 176)},
                {ThemeColor.KeyDownForeground, Color.FromArgb(255, 229, 229, 234)}
            }
        };

        private readonly Dictionary<ThemeColor, Color> colors = new Dictionary<ThemeColor, Color>();
        private readonly Dictionary<string, KeyRenderer> renderers = new Dictionary<string, KeyRenderer>();

        private readonly Dictionary<string, Func<KeyInstance>> keyInstantiators =
            new Dictionary<string, Func<KeyInstance>>();

        private KeyRenderer fallbackRenderer;
        private Func<KeyInstance> fallbackKeyInstantiator;

        private KeyboardTheme()
        {
            FallbackRenderer = default;
            FallbackKeyInstantiator = default;
        }

        /// <summary>
        ///     Gets the color from the registered colors.
        ///     If no color is found, the color from the default layout will be used.
        /// </summary>
        public Color GetColor(ThemeColor color)
        {
            if (colors.TryGetValue(color, out var result))
            {
                return result;
            }

            if (Default.Colors.TryGetValue(color, out result))
            {
                return result;
            }

            return System.Windows.Media.Colors.Fuchsia;
        }

        /// <summary>
        ///     Registers a custom renderer for the specified key role.
        /// </summary>
        /// <param name="role">The key role which should use a custom renderer.</param>
        public void RegisterRenderer<T>(string role) where T : KeyRenderer, new()
        {
            Debug.Assert(role != null);
            renderers[role] = new T();
        }

        /// <summary>
        ///     Registers a custom key instantiator for the specified key role.
        /// </summary>
        /// <param name="role">The key role which should use a custom key instance.</param>
        public void RegisterKey<T>(string role) where T : KeyInstance, new()
        {
            Debug.Assert(role != null);
            keyInstantiators[role] = () => new T();
        }

        /// <summary>
        ///     Gets the renderer by role. If no renderer is found the <see cref="FallbackRenderer" /> is being used.
        /// </summary>
        /// <param name="role">The registered key role</param>
        public KeyRenderer GetRenderer(string role)
        {
            if (renderers.TryGetValue(role, out var renderer))
            {
                return renderer;
            }

            return fallbackRenderer;
        }

        /// <summary>
        ///     Gets the instantiator by role. If no instantiator is found the <see cref="FallbackKeyInstantiator" /> is being
        ///     used.
        /// </summary>
        /// <param name="role">The registered key role</param>
        public KeyInstance GetKeyInstance(string role, Func<KeyInstance> fallback = null)
        {
            if (keyInstantiators.TryGetValue(role, out var func))
            {
                return func();
            }

            return fallback?.Invoke() ?? fallbackKeyInstantiator();
        }

        /// <summary>
        ///     Gets the registered colors.
        /// </summary>
        public Dictionary<ThemeColor, Color> Colors => colors;

        /// <summary>
        ///     Gets or sets the fallback renderer.
        ///     If value is null, a new <see cref="KeyRenderer" /> is being used.
        /// </summary>
        public KeyRenderer FallbackRenderer
        {
            get => fallbackRenderer;
            set => fallbackRenderer = value ?? new KeyRenderer();
        }

        /// <summary>
        ///     Gets or sets the fallback instantiator.
        ///     If value is null, a <see cref="KeyInstance" /> instantiator function is being used.
        /// </summary>
        public Func<KeyInstance> FallbackKeyInstantiator
        {
            get => fallbackKeyInstantiator;
            set { fallbackKeyInstantiator = value ?? (() => new KeyInstance()); }
        }

        /// <summary>
        ///     Gets the registered renderers
        /// </summary>
        public Dictionary<string, KeyRenderer> Renderers => renderers;
    }
}