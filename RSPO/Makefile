.PHONY:

TARGET=RSPO.exe
VAR=Debug
#BUILD=msbuild
BUILD=xbuild

compile:
	$(BUILD)

run: compile
	cd bin/$(VAR) && mono $(TARGET)
