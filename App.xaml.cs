﻿using System.Windows;

namespace AcrylicKeyboard
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            MainWindow mainView = new MainWindow();
            mainView.Show();
        }
    }
}