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
        public static readonly KeyboardTheme Default = new KeyboardTheme()
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
        
        private Dictionary<ThemeColor, Color> colors = new Dictionary<ThemeColor, Color>();
        private Dictionary<String, KeyRenderer> renderers = new Dictionary<string, KeyRenderer>();
        private Dictionary<String, Func<KeyInstance>> keyInstantiators = new Dictionary<string, Func<KeyInstance>>();
        private KeyRenderer fallbackRenderer;
        private Func<KeyInstance> fallbackKeyInstantiator;

        private KeyboardTheme()
        {
            FallbackRenderer = default;
            FallbackKeyInstantiator = default;
        }

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

        public void RegisterRenderer<T>(String role) where T : KeyRenderer, new()
        {
            Debug.Assert(role != null);
            renderers[role] = new T();
        }

        public void RegisterKey<T>(String role) where T : KeyInstance, new()
        {
            Debug.Assert(role != null);
            keyInstantiators[role] = () => new T();
        }

        public KeyRenderer GetRenderer(String role, Func<KeyRenderer> fallback = null)
        {
            if (renderers.TryGetValue(role, out var renderer))
            {
                return renderer;
            }
            return fallback?.Invoke() ?? fallbackRenderer;
        }

        public KeyInstance GetKeyInstance(String role, Func<KeyInstance> fallback = null)
        {
            if (keyInstantiators.TryGetValue(role, out var func))
            {
                return func();
            }
            return fallback?.Invoke() ?? fallbackKeyInstantiator();
        }
        
        public Dictionary<ThemeColor, Color> Colors => colors;

        public KeyRenderer FallbackRenderer
        {
            get => fallbackRenderer;
            set
            {
                fallbackRenderer = value ?? new KeyRenderer();
            }
        }

        public Func<KeyInstance> FallbackKeyInstantiator
        {
            get => fallbackKeyInstantiator;
            set
            {
                fallbackKeyInstantiator = value ?? (() => new KeyInstance());
            }
        }

        public Dictionary<string, KeyRenderer> Renderers => renderers;
    }
}