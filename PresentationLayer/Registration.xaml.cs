﻿using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using ChatRoomProject.LogicLayer;

namespace ChatRoomProject.PresentationLayer
{
    /// <summary>
    /// Interaction logic for Registration.xaml
    /// </summary>
    public partial class Registration : Window
    {
        private ChatRoom chat;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("ChatRoom.cs");
        ObservableObjectChatRoom _main = new ObservableObjectChatRoom();

        public Registration(ChatRoom chat)
        {
            InitializeComponent();
            this.chat = chat;
            this.DataContext = _main; 
        }

        private void Registrate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.chat.Registration(_main.GroupId, _main.Nickname);
                log.Info("The user registered");
                MessageBox.Show("You had been registered", "Reagistration", MessageBoxButton.OK, MessageBoxImage.None);
                MainWindow window = new MainWindow(this.chat);
                window.Show();
                this.Close();

            }
            catch (Exception err)
            {
                log.Info("The user had failed to register");
                MessageBox.Show(err.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackToMain_Click(object sender, RoutedEventArgs e)
        {
            MainWindow main = new MainWindow(this.chat);
            main.Show();
            this.Close();
        }

        private void NicknameContent_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Registrate_Click(sender, e);
        }
    }
}
