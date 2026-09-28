Common:
- [x] Make it compile
- [x] Window implementation
- [x] Fix shader compilation
- [x] Downscale textures
- [x] Touch controls
- [x] Keyboard controls
- [x] Menu
- [x] Config
- [x] Merge S60v3 and ^3 branches
- [ ] Simplify geometry
- [ ] Move map to top left corner
- [ ] Too many draw calls
- [ ] Audio

GLES 2.0 specific:
- [x] Optimize to always run 10+ fps
- [x] Fix skinning shader (cutscene heads require it)
- [x] Fix face animations in optimized builds
- [x] Disappearing background in cutscenes
- [x] Huge lag spikes on VC4
- [x] Skinning shader is still broken on VC4
- [ ] Optimize to always run 20+ fps
- [ ] Use etc1 compression
- [ ] HUD still disappearing sometimes on VC3
- [ ] Effects are slowing the render on VC4

GLES 1.1 specific:
- [x] Backport to STLport v4
- [x] GLES 1.1 engine
- [x] Keyboard mappings for both QWERTY/12-keys
- [x] Fix cutscene heads
- [ ] Use pvrtc compression
- [ ] Optimize to always run 10+ fps
- [ ] Fix z-buffer issues (world clips through hud)
- [ ] Fix lighting

Issues:
- [ ] Fix crash in Texture::destroy on exit
- [x] Save menu is too slow
- [x] Crash when picking up a bat
