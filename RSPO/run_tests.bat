SET TARGET=RSPO.exe
SET VAR=Debug
SET RUNNERVER=2.3.1
SET RUNTIME=net452

"..\packages\xunit.runner.console.%RUNNERVER%\tools\%RUNTIME%\xunit.console.exe" bin\%VAR%\%TARGET% -nologo -nocolor
