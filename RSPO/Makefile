.PHONY:

TARGET=RSPO.exe
VAR=Debug
#BUILD=msbuild
BUILD=dotnet build /p:MonoReferenceAssemblies=/usr/lib/mono /property:Configuration="Debug" /verbosity:normal /nologo
#BUILD=xbuild /property:Configuration="Debug" /verbosity:normal /nologo
TRM="TERM=xterm XBUILD_COLORS=disable"
RUNNERVER=2.3.1
RUNTIME=net452

compile:
	TERM=xterm \
	$(BUILD)

run: compile
	cd bin/$(VAR) && (TERM=xterm mono $(TARGET))

tests: compile
	TERM=xterm \
	mono ../packages/xunit.runner.console.$(RUNNERVER)/tools/$(RUNTIME)/xunit.console.exe bin/$(VAR)/RSPO.exe
