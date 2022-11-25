# MoQieKbHelper

[![License-GPL-3.0](https://img.shields.io/badge/license-GPL--3.0-orange)](https://github.com/Moying-moe/MoQieKbHelper/blob/master/LICENSE)
![version v0.1.0-alpha](https://img.shields.io/badge/version-v0.1.0--alpha-green)

墨切按键 for JX3 Online

使用的按键库: [SuperIo](https://github.com/Moying-moe/SuperIo)

## 如何下载

你可以在[Release](https://github.com/Moying-moe/MoQieKbHelper/releases/)中下载压缩包并解压。

如果你无法访问Github，或者下载缓慢，也可以在[蓝奏云(密码:1111)](https://moyingmoe.lanzouy.com/b03pk2gyb)下载。

## 如何使用

请**先解压**，然后双击`墨切按键.exe`打开软件。**切勿在压缩包中直接打开！**

### 添加按键

单击左上角的输入框，然后按下想要自动按下的按键，可以看到输入框中会显示按下按键的名称。然后单击右侧的**添加按键**按钮，就可以看到左侧的列表中添加了对应的按键选项。

### 设置快捷键

在右侧可以设置软件的启动和停止快捷键，默认是鼠标侧键1和侧键2。设置快捷键的方式和剑网三类似，单击按钮以后，按下想要使用的快捷键就可以了。

快捷键可以包含Ctrl, Alt和Shift。如果想单独设置这几个键为快捷键，则只需要轻按一下即可。

请注意：**切勿将启动键和左侧列表中打勾的键设置成相同的的！！** 比如左侧列表中Q键被勾选，则切勿将启动键也设置为Q，否则可能会导致软件崩溃或者卡死。

*备注：目前似乎Alt键无法被单独设置为快捷键，未来会试图修复。*

### 开始使用

在左侧的按键列表中，勾选需要重复按下的键，然后切换到游戏中，按下启动快捷键即可。

## 常见问题

### 双击以后没反应

确保文件夹中有SuperIo.dll文件。如果有的话，请稍等片刻，可能是软件正在加载模块。

### 提示XXX模块加载失败

请确保文件夹中有以下文件：

- WinRing0.dll
- WinRing0.sys
- WinRing0x64.dll
- WinRing0x64.sys

### 启动按键以后没反应

请先确认是启动按键失败还是启动按键之后没反应。

如果启动按键成功，你会听到声音提示，并看到界面中绝大部分的按钮和输入框变为灰色无法操作。如果你发现按下设置的启动按键后界面没有任何变化，请在[这里](https://github.com/Moying-moe/MoQieKbHelper/issues)创建一个issue。我看到以后会尽快排查修复。

如果按键启动成功，但是并不会自动按下对应的按键，请检查：

- 左侧列表中是否勾选了需要按下的按键
- 右下角设置的按键间隔是否过长？

同时，部分电脑可能无法通过串口模拟键盘操作。墨切按键可能会在未来增加其他按键模拟方式来解决这个问题。但是目前，您只能使用其他同类软件了。

### 修改设置以后重新打开软件，之前的设置又变回去了

目前v0.1.0-alpha版本暂时没有保存配置的功能，未来会加入。

### 使用期间弹窗报错了

请将报错完整内容截图，并附上报错时您正在做的操作的说明，在[这里](https://github.com/Moying-moe/MoQieKbHelper/issues)创建一个issue。我看到以后会尽快排查修复。
