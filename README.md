# TimeStamp
Simple utility to automatically log working times and activities.

## Intended use
- This utility is made for least-effort time tracking. It is best used for office workers, who use a single computer (desktop or notebook), which is turned on / resumed when arriving in the office and turned off / stand-by when going home. The app hides in the system tray, logs start/end/pause times and provides a quick-menu for switching activities, which are also logged with start/end times. Pop-up notifications at certain events (e.g. unlocking the computer) reminding which activity is currently being tracked, but everything can be easily modified afterwards, if switching has been forgotten etc. 


## Features
- Automatically track start time when unlocking the computer for the first time of the day
- Automatically track end time when locking the computer for the last time of the day
- Automatically track lunch time when being away from the computer during noon time (recognized by mouse movement activity)
- Automatically track your time in lieu and see your current balance
- Edit all logged times later on
- Minimize hides to system tray
- Easily track activities by clicking 'start' buttons in the UI or in the system tray context menu
- Small Pop-up notifications reminding which activity is currently being tracked, e.g. upon unlocking the OS (when bringing the notebook to a meeting and back to the desk for example)
- Excel export function to create an excel file for a certain year, containing a worksheet for each month, listing all activities for each day (rounded to full, quarter, half and three-quarter hours)

| ![Alt text](/../screenshots/Screenshots/Features.png?raw=true "Features") | ![Alt text](/../screenshots/Screenshots/Features%20(2).png?raw=true "Features") |
|:-------------:|:-------------:|



## Quick Setup
- [Download the latest setup here](https://github.com/Johannes34/TimeStamp/releases/latest)
- A Start menu and Autostart shortcut is automatically being created
- Upon first launch, click the 'Manage activities' button and set up your desired activities.

## Custom Setup
[![Build Status](https://jojo-meier.visualstudio.com/TimeStamp/_apis/build/status/Johannes34.TimeStamp?branchName=master)](https://jojo-meier.visualstudio.com/TimeStamp/_build/latest?definitionId=4&branchName=master)
- Build the binaries yourself 
- Copy them to a location on your computer where it has read/write permissions
- Execute TimeStamp.exe
- The times are logged in a file next to the .exe called StampFile.xml
- It is recommended to add it to your Windows startup files, by creating a shortcut to it in %AppData%\Microsoft\Windows\Start Menu\Programs\Startup
- For automatic backup you can copy into and run the program files from a synchronized OneDrive / Google Drive directory
