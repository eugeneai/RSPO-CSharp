.PHONY:

TARGET=RSPO.exe
VAR=Debug
TRM="TERM=xterm XBUILD_COLORS=disable"
#BUILD=msbuild
#BUILD=dotnet build /property:Configuration="Debug" /verbosity:normal /nologo
BUILD=xbuild /property:Configuration="Debug" /verbosity:normal /nologo
RUNNERVER=2.3.1
RUNTIME=net452

compile: export TERM=xterm
compile: export XBUILD_COLORS=disable
compile:
	$(BUILD)

run: export TERM=xterm
run: export MONO_COLORS=disable
run: compile
	cd bin/$(VAR) && mono $(TARGET)

tests: export TERM=xterm
tests: export MONO_COLORS=disable
tests: compile
	mono ../packages/xunit.runner.console.$(RUNNERVER)/tools/$(RUNTIME)/xunit.console.exe bin/$(VAR)/RSPO.exe -nologo -nocolor
