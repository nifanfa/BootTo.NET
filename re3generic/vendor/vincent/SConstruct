inc_path = ['include','src','ug','src/linux-x11','src/codegen']
lib_path = ['bin']
libs     = ['EGL','GLES_CM','ug','m','pthread','stdc++','X11','Xext']
flags    = ['-fpermissive']
cxxflags = ['-fpermissive','-xc++']

egl_srcs  = Glob('src/linux-x11/*.cpp')

gles_srcs = Glob('src/*.cpp') + Glob('src/arm/*.cpp') + Glob('src/codegen/*.c')

ug_srcs   = ['ug/ug_x11.c', 'ug/uglu.c', 'ug/ugshape.c'] 

SharedLibrary(target = 'bin/libEGL.so',source = egl_srcs,CPPPATH=inc_path,CCFLAGS=flags)

SharedLibrary(target = 'bin/libGLES_CM.so',source = gles_srcs,CPPPATH=inc_path,CCFLAGS=flags)

SharedLibrary(target = 'bin/libug.so',source = ug_srcs,CPPPATH=inc_path,CCFLAGS=cxxflags)

Program(target = 'bin/aargb',source = 'redbook/aargb.c' ,CPPPATH=inc_path,LIBPATH=lib_path,LIBS=libs,CCFLAGS=cxxflags)
Program(target = 'bin/alpha3D',source = 'redbook/alpha3D.c' ,CPPPATH=inc_path,LIBPATH=lib_path,LIBS=libs,CCFLAGS=cxxflags)
Program(target = 'bin/alpha',source = 'redbook/alpha.c' ,CPPPATH=inc_path,LIBPATH=lib_path,LIBS=libs,CCFLAGS=cxxflags)
Program(target = 'bin/checker',source = 'redbook/checker.c' ,CPPPATH=inc_path,LIBPATH=lib_path,LIBS=libs,CCFLAGS=cxxflags)
Program(target = 'bin/colormat',source = 'redbook/colormat.c' ,CPPPATH=inc_path,LIBPATH=lib_path,LIBS=libs,CCFLAGS=cxxflags)
Program(target = 'bin/cube',source = 'redbook/cube.c' ,CPPPATH=inc_path,LIBPATH=lib_path,LIBS=libs,CCFLAGS=cxxflags)
Program(target = 'bin/double',source = 'redbook/double.c' ,CPPPATH=inc_path,LIBPATH=lib_path,LIBS=libs,CCFLAGS=cxxflags)
Program(target = 'bin/fog',source = 'redbook/fog.c' ,CPPPATH=inc_path,LIBPATH=lib_path,LIBS=libs,CCFLAGS=cxxflags)
Program(target = 'bin/hello',source = 'redbook/hello.c' ,CPPPATH=inc_path,LIBPATH=lib_path,LIBS=libs,CCFLAGS=cxxflags)
Program(target = 'bin/light',source = 'redbook/light.c' ,CPPPATH=inc_path,LIBPATH=lib_path,LIBS=libs,CCFLAGS=cxxflags)
Program(target = 'bin/lines',source = 'redbook/lines.c' ,CPPPATH=inc_path,LIBPATH=lib_path,LIBS=libs,CCFLAGS=cxxflags)
Program(target = 'bin/material',source = 'redbook/material.c' ,CPPPATH=inc_path,LIBPATH=lib_path,LIBS=libs,CCFLAGS=cxxflags)
Program(target = 'bin/mipmap',source = 'redbook/mipmap.c' ,CPPPATH=inc_path,LIBPATH=lib_path,LIBS=libs,CCFLAGS=cxxflags)
Program(target = 'bin/model',source = 'redbook/model.c' ,CPPPATH=inc_path,LIBPATH=lib_path,LIBS=libs,CCFLAGS=cxxflags)
Program(target = 'bin/movelight',source = 'redbook/movelight.c' ,CPPPATH=inc_path,LIBPATH=lib_path,LIBS=libs,CCFLAGS=cxxflags)
Program(target = 'bin/planet',source = 'redbook/planet.c' ,CPPPATH=inc_path,LIBPATH=lib_path,LIBS=libs,CCFLAGS=cxxflags)
Program(target = 'bin/polyoff',source = 'redbook/polyoff.c' ,CPPPATH=inc_path,LIBPATH=lib_path,LIBS=libs,CCFLAGS=cxxflags)
Program(target = 'bin/robot',source = 'redbook/robot.c' ,CPPPATH=inc_path,LIBPATH=lib_path,LIBS=libs,CCFLAGS=cxxflags)
Program(target = 'bin/scene',source = 'redbook/scene.c' ,CPPPATH=inc_path,LIBPATH=lib_path,LIBS=libs,CCFLAGS=cxxflags)
Program(target = 'bin/smooth',source = 'redbook/smooth.c' ,CPPPATH=inc_path,LIBPATH=lib_path,LIBS=libs,CCFLAGS=cxxflags)
#Program(target = 'bin/stencil',source = 'redbook/stencil.c' ,CPPPATH=inc_path,LIBPATH=lib_path,LIBS=libs,CCFLAGS=cxxflags)
#Program(target = 'bin/stroke',source = 'redbook/stroke.c' ,CPPPATH=inc_path,LIBPATH=lib_path,LIBS=libs,CCFLAGS=cxxflags)
Program(target = 'bin/texbind',source = 'redbook/texbind.c' ,CPPPATH=inc_path,LIBPATH=lib_path,LIBS=libs,CCFLAGS=cxxflags)
Program(target = 'bin/texsub',source = 'redbook/texsub.c' ,CPPPATH=inc_path,LIBPATH=lib_path,LIBS=libs,CCFLAGS=cxxflags)
Program(target = 'bin/torus',source = 'redbook/torus.c' ,CPPPATH=inc_path,LIBPATH=lib_path,LIBS=libs,CCFLAGS=cxxflags)
Program(target = 'bin/varray',source = 'redbook/varray.c' ,CPPPATH=inc_path,LIBPATH=lib_path,LIBS=libs,CCFLAGS=cxxflags)
Program(target = 'bin/wrap',source = 'redbook/wrap.c' ,CPPPATH=inc_path,LIBPATH=lib_path,LIBS=libs,CCFLAGS=cxxflags)
 
