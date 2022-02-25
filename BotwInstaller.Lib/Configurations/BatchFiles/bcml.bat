﻿@ECHO OFF

:: Remove BCML Data
ECHO Removing BCML . . .
RMDIR "C:\Users\ArchLeaders\AppData\Local\bcml" /s /q

:: Remove BCML PyPi
START "PIP" "$python\Scripts\pip.exe" uninstall -y bcml

:: Remove Shortcuts
ECHO Removing shortcuts . . .
DEL "$desktop\BCML.lnk" /Q /F
DEL "$start\BCML.lnk" /Q /F

:: Remove Registry Key
ECHO Removing registry keys . . .
REG delete "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\BCML" /F

:: Delete Self
PAUSE
DEL "$root\bcml.bat" /Q /F
