# Timer
.NET windows form app like stopwatch to do tasks

Timer is a VS project

Debugged and Released at the debug and release folders

*the Shutdown tab timer cant be less than 10 seconds, for safety

```
** You can run it via cmd prompt, the arguments are used without commas:
   pathOfTimerexe hours min sec comboSelection fileToRun preventSleepMode

** -comboSelection   (0 or 1 or 2), 0 shutdown - 1 alarm - 2 File to run (0 is the autovalue)
   -fileToRun        is the path of any file you want to run             ("" is the auto value)
   -preventSleepMode (true or false)                                     (true is the autovalue)
   
** examlple 1:  C:\Users\Christos\Desktop>Timer.exe 1
   this will start the Timer.exe with 1 hour to shutdown because all the other arguments are autovalued
   
** examlple 2:  C:\Users\Christos\Desktop>Timer.exe 0 0 10 1
   this will start the Timer.exe with 0 hours, 0 mins, 10 sec and with alarm selected (the rest are autovalued)
   
** examlple 3:  C:\Users\Christos\Desktop>Timer.exe 12 0 0 2 "D:\users\Christos\Downloads\Picture.png" true
   this will run Timer.exe with 12 hours left and with ChooseFile selected to run the Picture.png
