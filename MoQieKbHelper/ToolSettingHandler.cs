/*
  Copyright (c) Moying-moe All rights reserved. Licensed under the MIT license.
  See LICENSE in the project root for license information.
*/

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;
using static SuperIo.SuperIo;

namespace MoQieKbHelper
{
    public sealed class ToolSettingHandler
    {
        #region Singleton
        private static readonly Lazy<ToolSettingHandler> lazy = new Lazy<ToolSettingHandler>(() => new ToolSettingHandler());
        public static ToolSettingHandler Instance { get => lazy.Value; }
        #endregion

        private const string SETTING_PATH = "./setting.json";

        private ToolSetting _settingObj = null;
        public ToolSetting Setting { get => _settingObj; set => _settingObj = value; }

        private ToolSettingHandler()
        {
            LoadSetting();
        }

        public void CreateDefaultSetting()
        {
            _settingObj = new ToolSetting();

            _settingObj.StartKey = new HotkeyInfo()
            {
                IsMouse = true,
                KeyCode = Mouse.XBUTTON1DOWN,
            };
            _settingObj.StopKey = new HotkeyInfo()
            {
                IsMouse = true,
                KeyCode = Mouse.XBUTTON2DOWN,
            };
            _settingObj.PauseKey = new HotkeyInfo()
            {
                IsMouse = false,
                KeyCode = Key.VK_LMENU,
            };
            _settingObj.KeyMode = 0;
            _settingObj.KeyInterval = 50;
            _settingObj.Sound = true;
            _settingObj.KeyList.Add(new KeyItem() { Key = Key.VK_F9, Enabled = true });
            _settingObj.KeyList.Add(new KeyItem() { Key = Key.VK_F10, Enabled = false });

            SaveSetting();
        }

        public void LoadSetting()
        {
            if (!(_settingObj is null))
            {
                return;
            }

            if (!File.Exists(SETTING_PATH))
            {
                CreateDefaultSetting();
                return;
            }

            string jsonString = File.ReadAllText(SETTING_PATH);
            _settingObj = JsonConvert.DeserializeObject<ToolSetting>(jsonString);
        }

        public void SaveSetting()
        {
            if (_settingObj is null)
            {
                throw new InvalidOperationException("ToolSettingHandler还未读取设置");
            }
            string jsonString = JsonConvert.SerializeObject(_settingObj, Formatting.Indented);
            File.WriteAllText(SETTING_PATH, jsonString);
        }


        #region Update
        private bool _updateLock = false;

        public void LockUpdate()
        {
            _updateLock = true;
        }

        public void UnlockUpdate()
        {
            _updateLock = false;
        }

        public void UpdateKeyList(ObservableCollection<MainWindow.KeyListItem> newList)
        {
            if (_updateLock)
            {
                return;
            }

            _settingObj.KeyList.Clear();

            foreach (MainWindow.KeyListItem item in newList)
            {
                _settingObj.KeyList.Add(new KeyItem()
                {
                    Key = item.KeyCode,
                    Enabled = item.Enabled
                });
            }
            SaveSetting();
        }

        public void UpdateHotkey(MainWindow.SettingButton settingButton, byte keyCode, bool isMouse, bool ctrl, bool alt, bool shift)
        {
            if (_updateLock)
            {
                return;
            }

            switch (settingButton)
            {
                case MainWindow.SettingButton.Start:
                    _settingObj.StartKey.IsMouse = isMouse;
                    _settingObj.StartKey.KeyCode = keyCode;
                    _settingObj.StartKey.Ctrl = ctrl;
                    _settingObj.StartKey.Alt = alt;
                    _settingObj.StartKey.Shift = shift;
                    break;
                case MainWindow.SettingButton.Stop:
                    _settingObj.StopKey.IsMouse = isMouse;
                    _settingObj.StopKey.KeyCode = keyCode;
                    _settingObj.StopKey.Ctrl = ctrl;
                    _settingObj.StopKey.Alt = alt;
                    _settingObj.StopKey.Shift = shift;
                    break;
                case MainWindow.SettingButton.Pause:
                    _settingObj.PauseKey.IsMouse = isMouse;
                    _settingObj.PauseKey.KeyCode = keyCode;
                    _settingObj.PauseKey.Ctrl = ctrl;
                    _settingObj.PauseKey.Alt = alt;
                    _settingObj.PauseKey.Shift = shift;
                    break;
            }
            SaveSetting();
        }

        public void UpdateKeyInterval(int newInterval)
        {
            if (_updateLock)
            {
                return;
            }

            _settingObj.KeyInterval = newInterval;
            SaveSetting();
        }

        public void UpdateSound(bool newSound)
        {
            if (_updateLock)
            {
                return;
            }

            _settingObj.Sound = newSound;
            SaveSetting();
        }

        public void UpdateKeyMode(int newMode)
        {
            if (_updateLock)
            {
                return;
            }

            _settingObj.KeyMode = newMode;
            SaveSetting();
        }
        #endregion
    }

    [StructLayout(LayoutKind.Sequential)]
    public class ToolSetting
    {
        public HotkeyInfo StartKey = new HotkeyInfo()
        {
            IsMouse = true,
            KeyCode = Mouse.XBUTTON1DOWN,
        };
        public HotkeyInfo StopKey = new HotkeyInfo()
        {
            IsMouse = true,
            KeyCode = Mouse.XBUTTON2DOWN,
        };
        public HotkeyInfo PauseKey = new HotkeyInfo()
        {
            IsMouse = false,
            KeyCode = Key.VK_LMENU,
        };
        public int KeyMode = 0;
        public int KeyInterval = 50;
        public bool Sound = true;
        public List<KeyItem> KeyList = new List<KeyItem>();
    }

    [StructLayout(LayoutKind.Sequential)]
    public class KeyItem
    {
        public byte Key;
        public bool Enabled = true;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class HotkeyInfo
    {
        public bool IsMouse = false;
        public byte KeyCode = 0;
        public bool Ctrl = false;
        public bool Alt = false;
        public bool Shift = false;
    }
}
