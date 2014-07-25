using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media;
using BluetoothConnectionManager;

namespace BluetoothClientWP8
{
    public partial class SecondPage : PhoneApplicationPage
    {
        //MainPage mainPage = new MainPage();
        
        
        public SecondPage()
        {
            InitializeComponent();
            
            
            
        }


        //public ConnectionManager connectionManager { get; set; }
        //public ListBox lstBTPaired { get; set; }
        


        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string msg = "";

            if (NavigationContext.QueryString.TryGetValue("msg", out msg))

                textBlockName.Text = msg;
           
        }

        private async void picker_ColorChanged(object sender, Color color)
        {
            this.ColorRect.Fill = new SolidColorBrush(color);
            

            //this.ColorRect.Fill = new SolidColorBrush(color);
            //this.ColorText.Text = color.ToString().Substring(3);
           //await MainPage.instance.connectionManager.SendCommand(color.ToString().Substring(3));
            await MainPage.instance.SendCommand(color.ToString().Substring(3));
        }

    }
}