.PHONY:

TARGET=RSPO.exe
VAR=Debug
#BUILD=msbuild
BUILD=xbuild
TRM="TERM=xterm"
RUNNERVER=2.3.1
RUNTIME=net452

compile:
	TERM=xterm \
	$(BUILD)

run: compile
	TERM=xterm \
	cd bin/$(VAR) && mono $(TARGET)

tests: compile
	TERM=xterm \
	mono ../packages/xunit.runner.console.$(RUNNERVER)/tools/$(RUNTIME)/xunit.console.exe bin/$(VAR)/RSPO.exe
