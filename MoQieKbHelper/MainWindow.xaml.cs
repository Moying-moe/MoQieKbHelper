using SuperIo;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using MessageBox = System.Windows.MessageBox;
using System.Windows.Input;
using Key = SuperIo.SuperIo.Key;
using System.Collections.ObjectModel;
using Mouse = SuperIo.SuperIo.Mouse;
using System.Text.RegularExpressions;

namespace MoQieKbHelper
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly string VERSION = "v0.1.0-alpha";

        private readonly SolidColorBrush Default_BtnBorder = new SolidColorBrush(Color.FromArgb(255, 112, 112, 112));
        private readonly SolidColorBrush Default_BtnBackground = new SolidColorBrush(Color.FromArgb(255, 221, 221, 221));
        private readonly SolidColorBrush Default_TextboxBackground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));

        private class KeyListItem
        {
            public byte KeyCode { get; set; }
            public string KeyString { get; set; }
            public bool Enabled { get; set; }
        }

        private bool _status = false;
        private long _toggleTime = 0;

        private List<byte> _keys = new List<byte>();                         // 用户添加的需要按下的键
        private ObservableCollection<KeyListItem> _keyListItems = new ObservableCollection<KeyListItem>();   // 用户添加的需要按下的按键列表（显示在UI上）

        private List<UIElement> _lockElement = new List<UIElement>();       // 启动时锁定的控件

        public MainWindow()
        {
            InitializeComponent();

            this.Title = "墨切按键 - " + VERSION;

            Lb_KeyList.ItemsSource = _keyListItems;
            TryAddKey(Key.VK_F9);
            TryAddKey(Key.VK_F10, false);
            TryAddKey(Key.VK_Q, false);
            TryAddKey(Key.VK_F1, false);

            ResetTooltip();

            _lockElement.Add(Tb_KeyInput);
            _lockElement.Add(Btn_AddKey);
            _lockElement.Add(Btn_DeleteKeys);
            _lockElement.Add(Btn_MarcoWebsite);
            _lockElement.Add(Btn_SetStartKey);
            _lockElement.Add(Btn_SetStopKey);
            //_lockElement.Add(Btn_SetPauseKey);
            //_lockElement.Add(Cb_KeyMode);
            _lockElement.Add(Tb_KeyInterval);
            _lockElement.Add(Cb_Sound);
            _lockElement.Add(Lb_KeyList);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            #region 模块初始化
            bool soundInitFlag = Sound.Instance.ForceInitialize();

            bool keyboardInitFlag = SuperKeyboard.Instance.IsInitialized;
            bool hotkeyInitFlag = SuperEvent.Instance.IsInitialized;

            SuperKeyboard.Instance.SetKeyPressDelay(33);

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

            timer.Interval = TimeSpan.FromMilliseconds(50);
            timer.Tick += Timer_Tick;

            #region 热键注册
            _startKeyIsMouse = true;
            _startKeyId = SuperEvent.Instance.RegisterMouse(
                mouseEvent: Mouse.XBUTTON1DOWN,
                handler: ToolOn
            );

            _stopKeyIsMouse = true;
            _stopKeyId = SuperEvent.Instance.RegisterMouse(
                mouseEvent: Mouse.XBUTTON2UP,
                handler: ToolOff
            );
            #endregion
        }

        #region 提示文本
        /// <summary>
        /// 设置提示文本
        /// </summary>
        /// <param name="content"></param>
        private void SetTooltip(string content)
        {
            Lb_Tooltip.Text = content;
        }

        /// <summary>
        /// 将提示文本还原为默认状态
        /// </summary>
        private void ResetTooltip()
        {
            SetTooltip("遇到问题可以在Github上反馈给我");
        }
        #endregion

        #region 添加按键
        private byte _lastAcceptKey = 0;

        /// <summary>
        /// 在文本框中按下按键时，变更显示的内容
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tb_KeyInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (SuperEvent.Instance.LastPressKey == Key.VK_ESCAPE)
            {
                // 退出输入
                Btn_AddKey.Focus();
            }
            else
            {
                _lastAcceptKey = SuperEvent.Instance.LastPressKey;
                Tb_KeyInput.Text = Key.GetKeyName(_lastAcceptKey);
            }
            e.Handled = true;
        }

        /// <summary>
        /// 提示用户开始输入按键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tb_KeyInput_GotFocus(object sender, RoutedEventArgs e)
        {
            Tb_KeyInput.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 127));
            SetTooltip("如果无法输入按键，请尝试切换输入法后重试。部分特殊按键可能无法识别。");
        }

        /// <summary>
        /// 还原
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tb_KeyInput_LostFocus(object sender, RoutedEventArgs e)
        {
            Tb_KeyInput.Background = Default_TextboxBackground;
            ResetTooltip();
        }

        /// <summary>
        /// 尝试添加按键
        /// </summary>
        /// <param name="keyCode"></param>
        /// <returns></returns>
        private bool TryAddKey(byte keyCode)
        {
            return TryAddKey(keyCode, true);
        }

        private bool TryAddKey(byte keyCode, bool enabled)
        {
            if (_keys.Contains(keyCode))
            {
                return false;
            }

            _keys.Add(keyCode);
            _keyListItems.Add(new KeyListItem()
            {
                KeyCode = keyCode,
                Enabled = enabled,
                KeyString = Key.GetKeyName(keyCode)
            });
            return true;
        }

        /// <summary>
        /// 添加按键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_AddKey_Click(object sender, RoutedEventArgs e)
        {
            if (_lastAcceptKey == 0)
            {
                // 还未输入任何键
                return;
            }

            bool addSuccess = TryAddKey(_lastAcceptKey);
            if (!addSuccess)
            {
                MessageBox.Show("该按键已存在", "提示", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
            Tb_KeyInput.Text = "点此输入按键";
            _lastAcceptKey = 0;
        }

        /// <summary>
        /// 删除勾选选中的按键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_DeleteKeys_Click(object sender, RoutedEventArgs e)
        {
            for (int i = _keyListItems.Count - 1; i >= 0; i--)
            {
                if (_keyListItems[i].Enabled)
                {
                    _keys.Remove(_keyListItems[i].KeyCode);
                    _keyListItems.RemoveAt(i);
                }
            }
        }
        #endregion

        #region Timer
        private DispatcherTimer timer = new DispatcherTimer();

        private void Timer_Tick(object sender, EventArgs e) {
            foreach (KeyListItem item in _keyListItems)
            {
                if (item.Enabled)
                {
                    Console.WriteLine(item.KeyCode + "," + Key.GetKeyName(item.KeyCode));
                    SuperKeyboard.Instance.KeyPress(item.KeyCode);
                }
            }
        }
        #endregion

        private void ToolOn()
        {
            if (!_status)
            {
                if (Tools.Instance.GetTime() < _toggleTime)
                {
                    return;
                }

                _toggleTime = Tools.Instance.GetTime() + 100;

                _status = true;
                timer.Start();

                foreach (UIElement element in _lockElement)
                {
                    element.IsEnabled = false;
                }

                if (Cb_Sound.IsChecked.Value)
                {
                    Sound.Instance.PlayStart();
                }
            }
        }

        private void ToolOff()
        {
            if (_status)
            {
                if (Tools.Instance.GetTime() < _toggleTime)
                {
                    return;
                }

                _toggleTime = Tools.Instance.GetTime() + 100;

                _status = false;
                timer.Stop();

                foreach (UIElement element in _lockElement)
                {
                    element.IsEnabled = true;
                }

                if (Cb_Sound.IsChecked.Value)
                {
                    Sound.Instance.PlayStop();
                }
            }
        }

        #region 快捷键设置

        
        private int _setKeyGHandlerId = -1;
        private int _setMouseGHandlerId = -1;
        private bool _isNewHotkeyCtrl = false;
        private bool _isNewHotkeyAlt = false;
        private bool _isNewHotkeyShift = false;
        private byte _newHotkey = 0;
        private bool _isNewHotkeyMouse = false;

        private bool _startKeyIsMouse = false;
        private int _startKeyId = -1;
        private bool _stopKeyIsMouse = false;
        private int _stopKeyId = -1;
        private bool _pauseKeyIsMouse = false;
        private int _pauseKeyId = -1;

        enum SettingButton
        {
            None,
            Start,
            Stop,
            Pause
        }
        private SettingButton _curSettingButton = SettingButton.None;

        private void RegisterCurButton()
        {
            ResetCurButton();

            string s = "";
            if (_isNewHotkeyCtrl)
            {
                s += "C+";
            }
            if (_isNewHotkeyAlt)
            {
                s += "A+";
            }
            if (_isNewHotkeyShift)
            {
                s += "S+";
            }

            switch (_curSettingButton)
            {
                case SettingButton.Start:
                    if (_isNewHotkeyMouse)
                    {
                        _startKeyId = SuperEvent.Instance.RegisterMouse(
                            mouseEvent: _newHotkey,
                            handler: ToolOn,
                            ctrl: _isNewHotkeyCtrl,
                            alt: _isNewHotkeyAlt,
                            shift: _isNewHotkeyShift
                        );
                        Btn_SetStartKey.Content = s + Mouse.GetMouseName(_newHotkey);
                    }
                    else
                    {
                        _startKeyId = SuperEvent.Instance.RegisterKey(
                            key: _newHotkey,
                            keyDownHandler: ToolOn,
                            keyUpHandler: delegate () { },
                            ctrl: _isNewHotkeyCtrl,
                            alt: _isNewHotkeyAlt,
                            shift: _isNewHotkeyShift
                        );
                        Btn_SetStartKey.Content = s + Key.GetKeyName(_newHotkey);
                    }
                    break;
                case SettingButton.Stop:
                    if (_isNewHotkeyMouse)
                    {
                        _stopKeyId = SuperEvent.Instance.RegisterMouse(
                            mouseEvent: _newHotkey,
                            handler: ToolOff,
                            ctrl: _isNewHotkeyCtrl,
                            alt: _isNewHotkeyAlt,
                            shift: _isNewHotkeyShift
                        );
                        Btn_SetStopKey.Content = s + Mouse.GetMouseName(_newHotkey);
                    }
                    else
                    {
                        _stopKeyId = SuperEvent.Instance.RegisterKey(
                            key: _newHotkey,
                            keyDownHandler: ToolOff,
                            keyUpHandler: delegate () { },
                            ctrl: _isNewHotkeyCtrl,
                            alt: _isNewHotkeyAlt,
                            shift: _isNewHotkeyShift
                        );
                        Btn_SetStopKey.Content = s + Key.GetKeyName(_newHotkey);
                    }
                    break;
                case SettingButton.Pause:
                    if (_isNewHotkeyMouse)
                    {
                        Btn_SetPauseKey.Content = s + Mouse.GetMouseName(_newHotkey);
                    }
                    else
                    {
                        Btn_SetPauseKey.Content = s + Key.GetKeyName(_newHotkey);
                    }
                    break;
            }
        }

        private void ResetCurButton()
        {
            switch (_curSettingButton)
            {
                case SettingButton.Start:
                    Btn_SetStartKey.Content = "";
                    Btn_SetStartKey.IsEnabled = true;
                    Btn_SetStartKey.BorderBrush = Default_BtnBorder;
                    Btn_SetStartKey.Background = Default_BtnBackground;
                    break;
                case SettingButton.Stop:
                    Btn_SetStopKey.Content = "";
                    Btn_SetStopKey.IsEnabled = true;
                    Btn_SetStopKey.BorderBrush = Default_BtnBorder;
                    Btn_SetStopKey.Background = Default_BtnBackground;
                    break;
                case SettingButton.Pause:
                    Btn_SetPauseKey.Content = "";
                    Btn_SetPauseKey.IsEnabled = true;
                    Btn_SetPauseKey.BorderBrush = Default_BtnBorder;
                    Btn_SetPauseKey.Background = Default_BtnBackground;
                    break;
            }
        }

        private void WaitMultiKeys()
        {
            _isNewHotkeyCtrl = false;
            _isNewHotkeyAlt = false;
            _isNewHotkeyShift = false;
            _isNewHotkeyMouse = false;

            _setKeyGHandlerId = SuperEvent.Instance.AddGlobalKeyHandler(
                delegate (byte key, bool isKeyDown, bool isKeyUp)
                {
                    if (isKeyDown)
                    {
                        if (key == Key.VK_CONTROL || key == Key.VK_LCONTROL || key == Key.VK_RCONTROL)
                        {
                            _isNewHotkeyCtrl = true;
                        }
                        else if (key == Key.VK_MENU || key == Key.VK_LMENU || key == Key.VK_RMENU)
                        {
                            _isNewHotkeyAlt = true;
                        }
                        else if (key == Key.VK_SHIFT || key == Key.VK_LSHIFT || key == Key.VK_RSHIFT)
                        {
                            _isNewHotkeyShift = true;
                        }
                        else
                        {
                            if (key == Key.VK_ESCAPE)
                            {
                                ResetCurButton();
                            }
                            else
                            {
                                _newHotkey = key;
                                _isNewHotkeyMouse = false;

                                RegisterCurButton();
                            }

                            SuperEvent.Instance.RemoveGlobalKeyHandler(_setKeyGHandlerId);
                            SuperEvent.Instance.RemoveGlobalMouseHandler(_setMouseGHandlerId);
                            return false;
                        }
                    }
                    else if (isKeyUp)
                    {
                        bool flag = false;
                        if (key == Key.VK_CONTROL || key == Key.VK_LCONTROL || key == Key.VK_RCONTROL)
                        {
                            flag = true;
                            _isNewHotkeyCtrl = false;
                        }
                        else if (key == Key.VK_MENU || key == Key.VK_LMENU || key == Key.VK_RMENU)
                        {
                            flag = true;
                            _isNewHotkeyAlt = false;
                        }
                        else if (key == Key.VK_SHIFT || key == Key.VK_LSHIFT || key == Key.VK_RSHIFT)
                        {
                            flag = true;
                            _isNewHotkeyShift = false;
                        }

                        if (flag)
                        {
                            if (!_isNewHotkeyCtrl && !_isNewHotkeyAlt && !_isNewHotkeyShift)
                            {
                                // 单击某修饰键并松开 则判定为使用该key作为快捷键
                                _newHotkey = key;
                                _isNewHotkeyMouse = false;

                                RegisterCurButton();
                                SuperEvent.Instance.RemoveGlobalKeyHandler(_setKeyGHandlerId);
                                SuperEvent.Instance.RemoveGlobalMouseHandler(_setMouseGHandlerId);
                                return false;
                            }
                        }
                    }

                    return false;
                }
            );
            _setMouseGHandlerId = SuperEvent.Instance.AddGlobalMouseHandler(
                delegate (byte mEvent)
                {
                    if (mEvent == Mouse.MBUTTONDOWN || mEvent == Mouse.MOUSEWHEEL || mEvent == Mouse.XBUTTON1DOWN || mEvent == Mouse.XBUTTON2DOWN)
                    {
                        _newHotkey = mEvent;
                        _isNewHotkeyMouse = true;

                        RegisterCurButton();
                        SuperEvent.Instance.RemoveGlobalKeyHandler(_setKeyGHandlerId);
                        SuperEvent.Instance.RemoveGlobalMouseHandler(_setMouseGHandlerId);
                    }

                    return false;
                }
            );
        }

        private void Btn_SetStartKey_Click(object sender, RoutedEventArgs e)
        {
            if (_startKeyIsMouse)
            {
                SuperEvent.Instance.UnregisterMouse(_startKeyId);
            }
            else
            {
                SuperEvent.Instance.UnregisterKey(_startKeyId);
            }

            Btn_SetStartKey.Content = "等待输入";
            Btn_SetStartKey.IsEnabled = false;
            Btn_SetStartKey.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
            Btn_SetStartKey.Background = new SolidColorBrush(Color.FromArgb(255, 247, 247, 247));

            _curSettingButton = SettingButton.Start;
            WaitMultiKeys();
        }

        private void Btn_SetStopKey_Click(object sender, RoutedEventArgs e)
        {
            if (_stopKeyIsMouse)
            {
                SuperEvent.Instance.UnregisterMouse(_stopKeyId);
            }
            else
            {
                SuperEvent.Instance.UnregisterKey(_stopKeyId);
            }

            Btn_SetStopKey.Content = "等待输入";
            Btn_SetStopKey.IsEnabled = false;
            Btn_SetStopKey.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
            Btn_SetStopKey.Background = new SolidColorBrush(Color.FromArgb(255, 247, 247, 247));

            _curSettingButton = SettingButton.Stop;
            WaitMultiKeys();
        }

        private void Btn_SetPauseKey_Click(object sender, RoutedEventArgs e)
        {
            if (_pauseKeyIsMouse)
            {
                SuperEvent.Instance.UnregisterMouse(_pauseKeyId);
            }
            else
            {
                SuperEvent.Instance.UnregisterKey(_pauseKeyId);
            }

            Btn_SetPauseKey.Content = "等待输入";
            Btn_SetPauseKey.IsEnabled = false;
            Btn_SetPauseKey.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
            Btn_SetPauseKey.Background = new SolidColorBrush(Color.FromArgb(255, 247, 247, 247));

            _curSettingButton = SettingButton.Pause;
            WaitMultiKeys();
        }
        #endregion

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

        private void Tb_KeyInterval_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = new Regex("[^0-9]+").IsMatch(e.Text);
        }

        private void Tb_KeyInterval_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int interval = 0;
            if (!string.IsNullOrEmpty(Tb_KeyInterval.Text))
            {
                interval = int.Parse(Tb_KeyInterval.Text);
            }
            Tb_KeyInterval.Text = interval.ToString();

            timer.Interval = TimeSpan.FromMilliseconds(interval);
        }
    }
}
