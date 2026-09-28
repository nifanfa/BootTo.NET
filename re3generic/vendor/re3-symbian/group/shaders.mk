MAKE = G:\Progs\MinGW\bin\mingw32-make

do_nothing :
	@rem do_nothing
	
MAKMAKE: do_nothing

BLD:
	$(MAKE) -C ../vendor/librw/src/gl/shaders/ -f Makefile all

CLEAN: do_nothing

LIB: do_nothing

CLEANLIB: do_nothing

RESOURCE: do_nothing

FREEZE: do_nothing

SAVESPACE: do_nothing

RELEASABLES: do_nothing

FINAL: do_nothing
