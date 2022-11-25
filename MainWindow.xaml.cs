using SuperIo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using MessageBox = System.Windows.MessageBox;

namespace SuperIoTestProgram
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private class KeyListItem
        {
            public string KeyString { get; set; }
            public bool Enabled { get; set; }
        }


        private bool _status = false;

        private byte _lastHotkey = SuperIo.SuperIo.Key.VK_ADD;
        private int _setKeyGHandlerId = -1;
        private bool _setKeyCtrl = false;
        private bool _setKeyAlt = false;
        private bool _setKeyShift = false;

        private DispatcherTimer timer = new DispatcherTimer();

        private List<byte> keys = new List<byte>();                         // 用户添加的需要按下的键
        private List<KeyListItem> keyListItems = new List<KeyListItem>();   // 用户添加的需要按下的按键列表（显示在UI上）

        public MainWindow()
        {
            InitializeComponent();

            #region 绑定KeyList
            keyListItems.Add(new KeyListItem()
            {
                KeyString = "F9",
                Enabled = true
            });
            keyListItems.Add(new KeyListItem()
            {
                KeyString = "F10",
                Enabled = false
            });

            Lb_KeyList.ItemsSource = keyListItems;
            #endregion
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 初始化

            #region 模块初始化
            bool soundInitFlag = Sound.Instance.ForceInitialize();

            bool keyboardInitFlag = SuperKeyboard.Instance.IsInitialized;
            bool hotkeyInitFlag = SuperEvent.Instance.IsInitialized;

            if (!keyboardInitFlag)
            {
                MessageBox.Show("键盘模拟模块加载失败！", "FATAL");
                Close();
            }
            if (!hotkeyInitFlag)
            {
                MessageBox.Show("热键模块加载失败！", "FATAL");
                Close();
            }
            #endregion

            timer.Interval = TimeSpan.FromMilliseconds(10);
            timer.Tick += Timer_Tick;

            #region 热键注册
            SuperEvent.Instance.RegisterMouse(
                mouseEvent: SuperIo.SuperIo.Mouse.XBUTTON1DOWN,
                handler: ToolOn
            );

            SuperEvent.Instance.RegisterMouse(
                mouseEvent: SuperIo.SuperIo.Mouse.XBUTTON2UP,
                handler: ToolOff
            );
            #endregion
        }

        private long GetTime()
        {
            DateTime dd = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            DateTime timeUTC = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);//本地时间转成UTC时间
            TimeSpan ts = (timeUTC - dd);
            return (Int64)ts.TotalMilliseconds;//精确到毫秒
        }

        private void Timer_Tick(object sender, EventArgs e) {
            SuperKeyboard.Instance.KeyPress(SuperIo.SuperIo.Key.VK_F9);
        }

        private void ToolOn()
        {
            if (!_status)
            {
                _status = true;
                timer.Start();

                //LbStatus.Content = "已开启";
                this.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 127));
                //BtnSetHotkey.IsEnabled = false;
                //CbTopmost.IsEnabled = false;

                Sound.Instance.PlayStart();
            }
        }

        private void ToolOff()
        {
            if (_status)
            {
                _status = false;

                timer.Stop();

                //LbStatus.Content = "未开启";
                this.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 255));
                //BtnSetHotkey.IsEnabled = true;
                //CbTopmost.IsEnabled = true;

                Sound.Instance.PlayStop();
            }
        }

        private void BtnSetHotkey_Click(object sender, RoutedEventArgs e)
        {
            SuperEvent.Instance.UnregisterKey(_lastHotkey);

            //Overlay.Visibility = Visibility.Visible;
            //LbKeyWait.Content = "等待按键";

            _setKeyCtrl = false;
            _setKeyAlt = false;
            _setKeyShift = false;

            _setKeyGHandlerId = SuperEvent.Instance.AddGlobalKeyHandler(
                delegate (byte key, bool isKeyDown, bool isKeyUp)
                {
                    if (isKeyDown)
                    {
                        if (key == SuperIo.SuperIo.Key.VK_CONTROL || key == SuperIo.SuperIo.Key.VK_LCONTROL || key == SuperIo.SuperIo.Key.VK_RCONTROL)
                        {
                            _setKeyCtrl = true;
                        }
                        else if (key == SuperIo.SuperIo.Key.VK_MENU || key == SuperIo.SuperIo.Key.VK_LMENU || key == SuperIo.SuperIo.Key.VK_RMENU)
                        {
                            _setKeyAlt = true;
                        }
                        else if (key == SuperIo.SuperIo.Key.VK_SHIFT || key == SuperIo.SuperIo.Key.VK_LSHIFT || key == SuperIo.SuperIo.Key.VK_RSHIFT)
                        {
                            _setKeyShift = true;
                        }
                        else
                        {
                            _lastHotkey = key;
                            SuperEvent.Instance.RegisterKey(
                                ctrl: _setKeyCtrl,
                                alt: _setKeyAlt,
                                shift: _setKeyShift,
                                key: key,
                                keyDownHandler: ToolOn,
                                keyUpHandler: delegate () { }
                            );
                            string s = "";
                            if (_setKeyCtrl)
                            {
                                s += "C+";
                            }
                            if (_setKeyAlt)
                            {
                                s += "A+";
                            }
                            if (_setKeyShift)
                            {
                                s += "S+";
                            }
                            //LbHotkey.Content = "开关快捷键: " + s + key;
                            //Overlay.Visibility = Visibility.Collapsed;
                            SuperEvent.Instance.RemoveAllGlobalKeyHandlers();
                        }
                    }
                    else
                    {
                        if (key == SuperIo.SuperIo.Key.VK_CONTROL || key == SuperIo.SuperIo.Key.VK_LCONTROL || key == SuperIo.SuperIo.Key.VK_RCONTROL)
                        {
                            _setKeyCtrl = false;
                        }
                        else if (key == SuperIo.SuperIo.Key.VK_MENU || key == SuperIo.SuperIo.Key.VK_LMENU || key == SuperIo.SuperIo.Key.VK_RMENU)
                        {
                            _setKeyAlt = false;
                        }
                        else if (key == SuperIo.SuperIo.Key.VK_SHIFT || key == SuperIo.SuperIo.Key.VK_LSHIFT || key == SuperIo.SuperIo.Key.VK_RSHIFT)
                        {
                            _setKeyShift = false;
                        }
                    }

                    if (_setKeyCtrl || _setKeyAlt || _setKeyShift)
                    {
                        string s = "";
                        if (_setKeyCtrl)
                        {
                            s += "Ctrl + ";
                        }
                        if (_setKeyAlt)
                        {
                            s += "Alt + ";
                        }
                        if (_setKeyShift)
                        {
                            s += "Shift + ";
                        }
                        //LbKeyWait.Content = s;
                    }
                    else
                    {
                        //LbKeyWait.Content = "等待按键";
                    }
                    return false;
                }
            );
        }

        private void BtnSetWuxueKey_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Btn_MarcoWebsite_Click(object sender, RoutedEventArgs e)
        {
            Tools.Instance.OpenUrlInBrowser("https://www.jx3box.com/macro/#/");
        }

        private void Lb_GithubLink_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Tools.Instance.OpenUrlInBrowser("https://github.com/Moying-moe/");
            }
        }
    }
}
