.PHONY:

TARGET=RSPO.exe
VAR=Debug
#BUILD=msbuild
BUILD=xbuild
TRM="TERM=xterm"


compile:
	TERM=xterm \
	$(BUILD)

run: compile
	TERM=xterm \
	cd bin/$(VAR) && mono $(TARGET)
