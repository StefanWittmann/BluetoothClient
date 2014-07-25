using BluetoothClientWP8.Resources;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Windows.Networking.Sockets;
using Windows.Networking.Proximity;
using System.Diagnostics;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using BluetoothConnectionManager;
using System.Windows.Media;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Reactive;
using Microsoft.Xna.Framework;
using Windows.Devices.Sensors;
using Windows.Phone.Speech.Recognition;
using Windows.Phone.Speech.Synthesis;



namespace BluetoothClientWP8
{


    public partial class MainPage : PhoneApplicationPage
    {
        public ConnectionManager connectionManager;

        private StateManager stateManager;

        public static ConnectionManager instance { get; set; }
            
            // Constructor
        public MainPage()
        {
            InitializeComponent();
            connectionManager = new ConnectionManager();
            connectionManager.MessageReceived += connectionManager_MessageReceived;
            stateManager = new StateManager();
            Loaded += MainPage_Loaded;


            this.connectionManager.ConnectAppToDeviceButton = this.ConnectAppToDeviceButton;
            this.connectionManager.lstBTPaired = this.lstBTPaired;

            //this.connectionManager.SendCommand = this.
            instance = this.connectionManager;

        }



        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshPairedDevicesList();
            //PeerFinder.Start();
            //PeerFinder.AlternateIdentities["Bluetooth:Paired"] = ""; // Find/Get All Paired BT Devices
            //var peers = await PeerFinder.FindAllPeersAsync(); // Make peers the container for All BT Devices

            //rausgenommen am 25.07.
            //txtBTStatus.Text = "Finding Paired Devices..."; // Tell UI what is going on in case it's Slow
            //ConnectAppToDeviceButton.IsEnabled = false;
            //ConnectAppToDeviceButton.Content = "not connected2";


            // Only want 1 Device to Show? Uncomment Below
            // lstBTPaired.Items.Add(peers[0].DisplayName); // 1 Paired Device to Show 

            // Show only Specific Device
            // peers[0].DisplayName.Contains("RN42-5");

            // Let's show only the first 2 Devices Paired
            //for (int i = 0; i < peers.Count; i++)
            //{
            //    lstBTPaired.Items.Add(peers[i].DisplayName);
            //}
            //if (peers.Count <= 2)
            //{
            //    txtBTStatus.Text = "Found " + peers.Count + " Devices";
            //}

        }


        private async void RefreshPairedDevicesList()
        {
            try
            {
                // Search for all paired devices
                PeerFinder.Start();
                lstBTPaired.Items.Clear();
                PeerFinder.AlternateIdentities["Bluetooth:Paired"] = "";
                var peers = await PeerFinder.FindAllPeersAsync();

                // By clearing the backing data, we are effectively clearing the ListBox
                

                if (peers.Count == 0)
                {
                    MessageBox.Show("Bluetooth ist deaktiviert");
                }
                else
                {
                    // Found paired devices.
                    for (int i = 0; i < peers.Count; i++)
                    {
                        

                            lstBTPaired.Items.Add(peers[i].DisplayName);
                        
                    }
                    //if (peers.Count <= 3)
                    //{
                        txtBTStatus.Text = "Found " + peers.Count + " Devices";
                    //}


                        




                }
            }
            catch (Exception ex)
            {
                if ((uint)ex.HResult == 0x8007048F)
                {
                    var result = MessageBox.Show("Bluetooth off","Bluetooth Off", MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.OK)
                    {
                        ShowBluetoothcControlPanel();
                    }
                }
                else if ((uint)ex.HResult == 0x80070005)
                {
                    MessageBox.Show("missing caps");
                }
                else
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }


        private void ShowBluetoothcControlPanel()
        {
            ConnectionSettingsTask connectionSettingsTask = new ConnectionSettingsTask();
            connectionSettingsTask.ConnectionSettingsType = ConnectionSettingsType.Bluetooth;
            connectionSettingsTask.Show();
        }



        async void connectionManager_MessageReceived(string message)
        {
            Debug.WriteLine("Message received:" + message);
            string[] messageArray = message.Split(':');
            switch (messageArray[0])
            {
                case "LED_RED":
                    stateManager.RedLightOn = messageArray[1] == "ON" ? true : false;
                    Dispatcher.BeginInvoke(delegate()
                    {
                        RedButton.Background = stateManager.RedLightOn ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Black);
                    });
                break;
                case "LED_GREEN":
                    stateManager.GreenLightOn = messageArray[1] == "ON" ? true : false;
                    Dispatcher.BeginInvoke(delegate()
                    {
                        GreenButton.Background = stateManager.GreenLightOn ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Black);
                    });
                break;
                //case "LED_YELLOW":
                //    stateManager.YellowLightOn = messageArray[1] == "ON" ? true : false;
                //    Dispatcher.BeginInvoke(delegate()
                //    {
                //        YellowButton.Background = stateManager.YellowLightOn ? new SolidColorBrush(Colors.Yellow) : new SolidColorBrush(Colors.Black);
                //    });
                //break;
                case "PROXIMITY":
                    stateManager.BodyDetected = messageArray[1] == "DETECTED" ? true : false;
                    if (stateManager.BodyDetected)
                    {
                        Dispatcher.BeginInvoke(delegate()
                        {
                            BodyDetectionStatus.Text = "Intruder detected!!!";
                            BodyDetectionStatus.Foreground = new SolidColorBrush(Colors.Red);
                        });
                        await connectionManager.SendCommand("TURN_ON_RED");
                    } else {
                        Dispatcher.BeginInvoke(delegate()
                        {
                            BodyDetectionStatus.Text = "No body detected";
                            BodyDetectionStatus.Foreground = new SolidColorBrush(Colors.White);
                        });
                    }
                    break;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            connectionManager.Initialize();
            stateManager.Initialize();
            lstBTPaired.Items.Clear();
            //RefreshPairedDevicesList();


        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            //connectionManager.Terminate();
        }

        private void ConnectAppToDeviceButton_Click_1(object sender, RoutedEventArgs e)
        {

            
                connectionManager.Terminate();
                //ConnectAppToDeviceButton.IsEnabled = false;
                ConnectAppToDeviceButton.Content = "not connected";
                lstBTPaired.SelectedIndex = -1;
                lstBTPaired.IsEnabled = true;
                connectionManager.Initialize();
                stateManager.Initialize();
            
            
            
            //AppToDevice();
        }

        /*private async void AppToDevice()
        {
            ConnectAppToDeviceButton.Content = "Connecting...";
            PeerFinder.AlternateIdentities["Bluetooth:Paired"] = "";
            var pairedDevices = await PeerFinder.FindAllPeersAsync();

            if (pairedDevices.Count == 0)
            {
                Debug.WriteLine("No paired devices were found.");
            }
            else
            { 
                foreach (var pairedDevice in pairedDevices)
                {
                    if (pairedDevice.DisplayName == DeviceName.Text)
                    {
                        connectionManager.Connect(pairedDevice.HostName);
                        ConnectAppToDeviceButton.Content = "Connected";
                        DeviceName.IsReadOnly = true;
                        ConnectAppToDeviceButton.IsEnabled = false;
                        continue;
                    }
                }
            }
        }*/

        






        private async void RedButton_Click_1(object sender, RoutedEventArgs e)
        {
            string command = stateManager.RedLightOn ? "TURN_OFF_RED" : "TURN_ON_RED";
            await connectionManager.SendCommand(command);
        }

        private async void GreenButton_Click_1(object sender, RoutedEventArgs e)
        {
            string command = stateManager.GreenLightOn ? "TURN_OFF_GREEN" : "TURN_ON_GREEN";
            await connectionManager.SendCommand(command);
        }

        private async void YellowButton_Click_1(object sender, RoutedEventArgs e)
        {
            string command = stateManager.YellowLightOn ? "TURN_OFF_YELLOW" : "TURN_ON_YELLOW";
            await connectionManager.SendCommand(command);
        }

        public async void lstBTPaired_Tap_1(object sender, GestureEventArgs e)
            
        {
            if (lstBTPaired.SelectedItem == null) // To prevent errors, make sure something is Selected
            {
                //btnConnectArduino.IsEnabled = false; // Make sure it's False if you want to use a Button
                txtBTStatus.Text = "No Device Selected! Try again..."; // Set UI Output
                return;
            }
            else
                if (lstBTPaired.SelectedItem != null) // Just making sure something was Selected
                {

                    // btnConnectArduino.IsEnabled = true; // Since an item is Selected, Enable Connect Button (If using a Button)


                    /* This is a trick to Grab the Item and Remove '(' and ')' if using the Hostname & want just the Contents (00:00:00)
                    // Of course we don't HAVE to do this, but this is a C# Trick/Hack to learn String Functions
                    string ba = lstBTPaired.SelectedItem.ToString(); // Store the Tapped/Selected Item
                    int found = 0; // Set the Found to 0
                    found = ba.IndexOf("("); // Let's get the Index of the "(" in the String (ba)
                    ba = ba.Substring(found + 1); // Use Substring with the IndexOf
                    ba = ba.Replace(")", ""); // Now remove the last ")" in the String to be "00:00:00:00:00"
                    // Test our Hack by Uncommenting Below...
                    //MessageBox.Show(ba); - This is just to make sure we did it right */

                    PeerFinder.AlternateIdentities["Bluetooth:Paired"] = ""; // Grab Paired Devices
                    var PF = await PeerFinder.FindAllPeersAsync(); // Store Paired Devices
                    //try
                    //{

                        //StreamSocket socket = new StreamSocket();



                        //await socket.ConnectAsync(PF[lstBTPaired.SelectedIndex].HostName, "1");


                        connectionManager.Connect(PF[lstBTPaired.SelectedIndex].HostName);
                        

                    //}
                    //catch (Exception exp)
                    //{
                        //if ((uint)ex.HResult == 0x8007274c)
                        //{
                        //MessageBox.Show(ex.Message);    
                      //  MessageBox.Show("Not in Range2");
                        //ConnectAppToDeviceButton.IsEnabled = false;
                        //ConnectAppToDeviceButton.Content = "not connected";
                        //stateManager.Initialize();
                        //lstBTPaired.Items.Clear();
                        //RefreshPairedDevicesList();
                        //connectionManager.Terminate();
                        //}
                    //}

                    //finally
                    //{
                        ConnectAppToDeviceButton.Content = "disconnect";
                        //DeviceName.IsReadOnly = true;
                        ConnectAppToDeviceButton.IsEnabled = true;
                        //lstBTPaired.IsEnabled = false;
                    //}


                    
                    //continue;

                        //BTSock = new StreamSocket(); // Create a new Socket Connection
                    //await BTSock.ConnectAsync(PF[lstBTPaired.SelectedIndex].HostName, "1"); // Connect using Socket to Selected Item

                    // Once Connected, let's give Arduino a HELLO
                    //var datab = GetBufferFromByteArray(Encoding.UTF8.GetBytes("HELLO")); // Create Buffer/Packet for Sending
                    //await BTSock.OutputStream.WriteAsync(datab); // Send Arduino Buffer/Packet Message

                    //btnSendCommand.IsEnabled = true; // Allow commands to be sent via Command Button (Enabled)
                }
        }

        public void NavigateButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/SecondPage.xaml?msg=" + lstBTPaired.SelectedItem, UriKind.Relative));
        }
    }
}