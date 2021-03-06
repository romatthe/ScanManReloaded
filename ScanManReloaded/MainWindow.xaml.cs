﻿using System;
using System.Collections.Generic;
using System.IO.Ports;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ScanManReloaded.Controls;

namespace ScanManReloaded
{
    public partial class MainWindow : Window
    {
        private IModeControl activeControl;
        private SerialPort barcodeReader;

        public MainWindow()
        {
            InitializeComponent();
            GetScanner();
        }

        private void GetScanner()
        {
            try
            {
                barcodeReader = new SerialPort("COM3");
                if (!barcodeReader.IsOpen)
                {
                    barcodeReader.Open();
                    barcodeReader.DataReceived += new SerialDataReceivedEventHandler(Scanner_DataReceived);
                    this.menuItemScanner.Header = new System.Windows.Controls.Image 
                    { 
                        Source = new BitmapImage(new Uri("Resources/Images/Yes.png", UriKind.Relative)),
                        Width = 39,
                        Height= 39,
                        Margin= new System.Windows.Thickness(-6, 0, 0, 0)
                    };
                }

            }
            catch
            {
                this.menuItemScanner.Header = new System.Windows.Controls.Image
                {
                    Source = new BitmapImage(new Uri("Resources/Images/No.png", UriKind.Relative)),
                    Width = 39,
                    Height = 39,
                    Margin = new System.Windows.Thickness(-6, 0, 0, 0)
                };
            }
        }

        private void Scanner_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort port = (SerialPort)sender;
            string input = port.ReadExisting();
            //int len = inp.Length;
            this.Dispatcher.Invoke(delegate { BarcodeLogic(input.Remove(input.Length - 1)); });
            port.DiscardInBuffer();
        }

        private void BarcodeLogic(string strBarcode)
        {
            Barcode barcode = new Barcode(strBarcode);

            // It's a general command
            if (barcode.Command == "CM")
            {
                switch (barcode.Value)
                {
                    case "CLS":
                        activeControl.Clear();
                        break;
                    case "PRN":
                        activeControl.Print();
                        break;
                }
            }

            // It's a mode-selection command
            else if (barcode.Command == "MD")
            {
                switch (barcode.Value)
                {
                    case "SELECTION":
                        ChangeMode(new ModeSelectionControl());
                        break;
                    case "WIP":
                        ChangeMode(new ModeRequestControl());
                        break;
                    case "KITTING":
                        ChangeMode(new ModeKittingControl());
                        break;
                }
            }

            // Pass it on the the Active ModeControl
            else
            {
                activeControl.BarcodeLogic(barcode);
            }
        }

        public void ChangeMode(Control modeControl)
        {
            // Remove all controls on the main panel
            this.panelMain.Children.Clear();

            // Add the new IModeControl, mark it as active
            this.activeControl = (IModeControl)modeControl;
            this.panelMain.Children.Add(modeControl);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ChangeMode(new ModeSelectionControl());
        }

        private void menuItemMode_Click(object sender, RoutedEventArgs e)
        {
            ChangeMode(new ModeSelectionControl());
        }

        private void menuItemPrint_Click(object sender, RoutedEventArgs e)
        {
            this.activeControl.Print();
        }

        private void menuItemClear_Click(object sender, RoutedEventArgs e)
        {
            this.activeControl.Clear();
        }
    }
}
