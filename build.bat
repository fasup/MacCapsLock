@echo off
cd /d "%~dp0"
"%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /codepage:65001 /target:winexe /optimize+ /out:MacCapsLock.exe /r:System.Windows.Forms.dll /r:System.Drawing.dll MacCapsLock.cs
if %errorlevel%==0 echo Built MacCapsLock.exe
