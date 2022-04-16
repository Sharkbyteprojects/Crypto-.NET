using System;
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
using System.Windows.Navigation;
using System.Security.Cryptography;
using System.Windows.Shapes;
using System.IO;

using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Crypto
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string stt = "";
        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
        static extern int SetWindowRgn(IntPtr hWnd, Rect rect, int boole);
        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
        static extern int GetWindowRect(IntPtr hWnd, out Rect r);
        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
        static extern int SetWindowPos(IntPtr hWnd, IntPtr opt, int x, int y, int cx, int cy, uint uflags);

        public MainWindow()
        {
            InitializeComponent();
        }

        private void dec_Click(object sender, RoutedEventArgs e)
        {
            setT("Loading");
            try
            {
                var fileContent = string.Empty;
                var filePath = string.Empty;

                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                openFileDialog.Filter = "Sharkenc files (*.sharkenc)|*.sharkenc|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == true)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;
                    SaveFileDialog sFileDialog = new SaveFileDialog();
                    sFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    sFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                    sFileDialog.FilterIndex = 2;
                    sFileDialog.RestoreDirectory = true;
                    if (sFileDialog.ShowDialog() != true) { setT(); return; }
                    string encFile = sFileDialog.FileName;

                    if (File.Exists(encFile))
                        if (MessageBox.Show(
                            string.Format("\"{0}\" already exists. Overwrite it?", encFile), "ENCMODULE", MessageBoxButton.YesNo
                            ) == MessageBoxResult.No
                            ) { setT(); return; };

                    //Read the contents of the file into a stream
                    var fileStream = openFileDialog.OpenFile();

                    using (BinaryReader reader = new BinaryReader(fileStream))
                    {
                        using (Aes aesAlg = Aes.Create())
                        {
                            setT("Decrypt");
                            aesAlg.Key = getPW();
                            //aesAlg.IV = IV;
                            const int mil = 16;
                            if ((int)reader.BaseStream.Length <= mil) { MessageBox.Show("OOOOPS! IV Damaged!"); setT(); return; }
                            aesAlg.IV = reader.ReadBytes(mil);
                            // Create an encryptor to perform the stream transform.
                            ICryptoTransform encryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                            // Create the streams used for encryption.
                            using (MemoryStream msEncrypt = new MemoryStream())
                            {
                                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                                {
                                    using (BinaryWriter swEncrypt = new BinaryWriter(csEncrypt))
                                    {
                                        //Write all data to the stream.
                                        swEncrypt.Write(reader.ReadBytes((int)reader.BaseStream.Length - mil));
                                    }
                                    reader.Close();
                                    setT("Write File");
                                    using (var stream = File.Open(encFile, FileMode.Create))
                                    {
                                        using (BinaryWriter sw = new BinaryWriter(stream))
                                        {
                                            sw.Write(msEncrypt.ToArray());
                                        }
                                        stream.Close();
                                        setT("Complete");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error: {0}", ex.Message), "ENCMODULE");
            }
            setT();
        }

        byte[] getPW()
        {
            SHA256 mySHA256 = SHA256.Create();
            return mySHA256.ComputeHash(Encoding.ASCII.GetBytes(pwB.Password.ToString()));
        }
        void setT(string e = "")
        {
            win.Title = string.Concat(stt, e == "" ? "" : " - ", e);
        }
        private void enc_Click(object sender, RoutedEventArgs e)
        {
            setT("Loading");
            try
            {
                var fileContent = string.Empty;
                var filePath = string.Empty;

                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == true)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;
                    SaveFileDialog sFileDialog = new SaveFileDialog();
                    sFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    sFileDialog.Filter = "Sharkenc files (*.sharkenc)|*.sharkenc|All files (*.*)|*.*";
                    sFileDialog.FilterIndex = 1;
                    sFileDialog.RestoreDirectory = true;
                    if (sFileDialog.ShowDialog() != true) { setT(); return; };
                    string encFile = sFileDialog.FileName;

                    if (File.Exists(encFile))
                        if (MessageBox.Show(
                            string.Format("\"{0}\" already exists. Overwrite it?", encFile), "ENCMODULE", MessageBoxButton.YesNo
                            ) == MessageBoxResult.No
                            ) { setT(); return; };

                    //Read the contents of the file into a stream
                    var fileStream = openFileDialog.OpenFile();

                    using (BinaryReader reader = new BinaryReader(fileStream))
                    {
                        using (Aes aesAlg = Aes.Create())
                        {
                            setT("Encrypt");
                            aesAlg.GenerateIV();
                            aesAlg.Key = getPW();
                            //aesAlg.IV = IV;

                            // Create an encryptor to perform the stream transform.
                            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                            // Create the streams used for encryption.
                            using (MemoryStream msEncrypt = new MemoryStream())
                            {
                                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                                {
                                    using (BinaryWriter swEncrypt = new BinaryWriter(csEncrypt))
                                    {
                                        //Write all data to the stream.
                                        swEncrypt.Write(reader.ReadBytes((int)reader.BaseStream.Length));
                                    }
                                    reader.Close();
                                    setT("Write File");
                                    using (var stream = File.Open(encFile, FileMode.Create))
                                    {
                                        using (BinaryWriter sw = new BinaryWriter(stream))
                                        {
                                            sw.Write(aesAlg.IV);
                                            sw.Write(msEncrypt.ToArray());
                                        }
                                        stream.Close();
                                        setT("Complete");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error: {0}", ex.Message), "ENCMODULE");
            }
            setT();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Environment.OSVersion.ToString().Contains("Windows"))
            {
                var wih = new System.Windows.Interop.WindowInteropHelper(this);
                var hWnd = wih.Handle;
                Rect rect = new Rect();
                GetWindowRect(hWnd, out rect);
                SetWindowRgn(hWnd, rect, 1);
                SetWindowPos(hWnd, (IntPtr)null, 0, 0, 0, 0, 0x0001 | 0x0040);
                SetForegroundWindow(hWnd);
            }
            stt = win.Title;
        }
    }
}
