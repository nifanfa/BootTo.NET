#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <assert.h>

#include "../rwbase.h"
#include "../rwerror.h"
#include "../rwplg.h"
#include "../rwpipeline.h"
#include "../rwobjects.h"
#include "../rwengine.h"

#include "rwgles1.h"
#include "rwgles1impl.h"

#define PLUGIN_ID ID_DRIVER

namespace rw {
namespace gles1 {

int32 nativeRasterOffset;

static uint32
getLevelSize(Raster *raster, int32 level)
{
	int i;
	Gl1Raster *natras = GETGL1RASTEREXT(raster);

	int w = raster->originalWidth;
	int h = raster->originalHeight;
	int s = raster->originalStride;
	int minDim = 1;

//#ifdef RW_GLES1
//	switch(natras->internalFormat){
//	case GL_COMPRESSED_RGBA_S3TC_DXT1_EXT:
//	case GL_COMPRESSED_RGB_S3TC_DXT1_EXT:
//	case GL_COMPRESSED_RGBA_S3TC_DXT3_EXT:
//	case GL_COMPRESSED_RGBA_S3TC_DXT5_EXT:
//		minDim = 4;
//		break;
//	}
//#endif

	for(i = 0; i < level; i++){
		if(w > minDim){
			w /= 2;
			s /= 2;
		}
		if(h > minDim)
			h /= 2;
	}

	return s*h;
}

#ifdef RW_GLES1

static Raster*
rasterCreateTexture(Raster *raster)
{
	if(raster->format & (Raster::PAL4 | Raster::PAL8)){
		RWERROR((ERR_NOTEXTURE));
		return nil;
	}

	Gl1Raster *natras = GETGL1RASTEREXT(raster);
	switch(raster->format & 0xF00){
	case Raster::C8888:
		natras->internalFormat = GL_RGBA;
		natras->format = GL_RGBA;
		natras->type = GL_UNSIGNED_BYTE;
		natras->hasAlpha = 1;
		natras->bpp = 4;
		raster->depth = 32;
		break;
	case Raster::C888:
		natras->internalFormat = GL_RGB;
		natras->format = GL_RGB;
		natras->type = GL_UNSIGNED_BYTE;
		natras->hasAlpha = 0;
		natras->bpp = 3;
		raster->depth = 24;
		break;
	case Raster::C565:
		natras->internalFormat = GL_RGB;
		natras->format = GL_RGB;
		natras->type = GL_UNSIGNED_SHORT_5_6_5;
		natras->hasAlpha = 0;
		natras->bpp = 2;
		raster->depth = 16;
		break;
	case Raster::C4444:
		natras->internalFormat = GL_RGBA;
		natras->format = GL_RGBA;
		natras->type = GL_UNSIGNED_SHORT_4_4_4_4;
		natras->hasAlpha = 1;
		natras->bpp = 2;
		raster->depth = 16;
		break;
	case Raster::C1555:
//		natras->internalFormat = GL_RGB5_A1;
		natras->internalFormat = GL_RGBA;
		natras->format = GL_RGBA;
		natras->type = GL_UNSIGNED_SHORT_5_5_5_1;
		natras->hasAlpha = 1;
		natras->bpp = 2;
		raster->depth = 16;
		break;
	default:
		RWERROR((ERR_INVRASTER));
		return nil;
	}

	natras->internalFormat = natras->format;

	raster->stride = raster->width*natras->bpp;

	if(raster->format & Raster::MIPMAP){
		int w = raster->width;
		int h = raster->height;
		natras->numLevels = 1;
		while(w != 1 || h != 1){
			natras->numLevels++;
			if(w > 1) w /= 2;
			if(h > 1) h /= 2;
		}
	}
	natras->autogenMipmap = (raster->format & (Raster::MIPMAP|Raster::AUTOMIPMAP)) == (Raster::MIPMAP|Raster::AUTOMIPMAP);
	if(natras->autogenMipmap)
		natras->numLevels = 1;

	glGenTextures(1, &natras->texid);
	uint32 prev = bindTexture(natras->texid);
//	glTexImage2D(GL_TEXTURE_2D, 0, natras->internalFormat,
////	             raster->width, raster->height,
//		1,1,
//	             0, natras->format, natras->type, nil);
	// TODO: allocate other levels...probably
	natras->filterMode = 0;
	natras->addressU = 0;
	natras->addressV = 0;
	natras->maxAnisotropy = 1;

	bindTexture(prev);
	return raster;
}

static Raster*
rasterCreateCameraTexture(Raster *raster)
{
	if(raster->format & (Raster::PAL4 | Raster::PAL8)){
		RWERROR((ERR_NOTEXTURE));
		return nil;
	}

	// TODO: figure out what the backbuffer is and use that as a default
	Gl1Raster *natras = GETGL1RASTEREXT(raster);
	switch(raster->format & 0xF00){
	case Raster::C8888:
		natras->internalFormat = GL_RGBA;
		natras->format = GL_RGBA;
		natras->type = GL_UNSIGNED_BYTE;
		natras->hasAlpha = 1;
		natras->bpp = 4;
		break;
	case Raster::C4444:
		natras->internalFormat = GL_RGBA;
		natras->format = GL_RGBA;
		natras->type = GL_UNSIGNED_SHORT_4_4_4_4;
		natras->hasAlpha = 1;
		natras->bpp = 2;
		break;
	case Raster::C888:
	default:
		natras->internalFormat = GL_RGB;
		natras->format = GL_RGB;
		natras->type = GL_UNSIGNED_BYTE;
		natras->hasAlpha = 0;
		natras->bpp = 3;
		break;
	case Raster::C565:
		natras->internalFormat = GL_RGB;
		natras->format = GL_RGB;
		natras->type = GL_UNSIGNED_SHORT_5_6_5;
		natras->hasAlpha = 0;
		natras->bpp = 2;
		break;
	case Raster::C1555:
//		natras->internalFormat = GL_RGB5_A1;
		natras->internalFormat = GL_RGBA;
		natras->format = GL_RGBA;
		natras->type = GL_UNSIGNED_SHORT_5_5_5_1;
		natras->hasAlpha = 1;
		natras->bpp = 2;
		break;
	}

	natras->internalFormat = natras->format;

	raster->stride = raster->width*natras->bpp;

	natras->autogenMipmap = (raster->format & (Raster::MIPMAP|Raster::AUTOMIPMAP)) == (Raster::MIPMAP|Raster::AUTOMIPMAP);

	glGenTextures(1, &natras->texid);
	uint32 prev = bindTexture(natras->texid);
	glTexImage2D(GL_TEXTURE_2D, 0, natras->internalFormat,
//	             raster->width, raster->height,
		1,1,
	             0, natras->format, natras->type, nil);
	natras->filterMode = 0;
	natras->addressU = 0;
	natras->addressV = 0;
	natras->maxAnisotropy = 1;

	bindTexture(prev);


//	glGenFramebuffers(1, &natras->fbo);
//	bindFramebuffer(natras->fbo);
//	glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, natras->texid, 0);
//	bindFramebuffer(0);
	natras->fboMate = nil;

	return raster;
}

static Raster*
rasterCreateCamera(Raster *raster)
{
	Gl1Raster *natras = GETGL1RASTEREXT(raster);

	// TODO: set/check width, height, depth, format?

	// used for locking right now
//	raster->format = Raster::C888;
//	natras->internalFormat = GL_RGB;
//	natras->format = GL_RGB;
//	natras->type = GL_UNSIGNED_BYTE;
//	natras->hasAlpha = 0;
//	natras->bpp = 3;
//
//	natras->autogenMipmap = 0;
	
	raster->originalWidth = raster->width;
	raster->originalHeight = raster->height;
	raster->stride = 0;
	raster->pixels = nil;

	natras->texid = 0;
	natras->fbo = 0;
	natras->fboMate = nil;

	return raster;
}

static Raster*
rasterCreateZbuffer(Raster *raster)
{
	Gl1Raster *natras = GETGL1RASTEREXT(raster);
	
	raster->originalWidth = raster->width;
	raster->originalHeight = raster->height;
	raster->stride = 0;
	raster->pixels = nil;
	return raster;
}
#endif

/*
{ 0, 0, 0 },
{ 16, 4, GL_RGBA },	// 1555
{ 16, 3, GL_RGB },	// 565
{ 16, 4, GL_RGBA },	// 4444
{ 0, 0, 0 },	// LUM8
{ 32, 4, GL_RGBA },	// 8888
{ 24, 3, GL_RGB },	// 888
{ 16, 3, GL_RGB },	// D16
{ 24, 3, GL_RGB },	// D24
{ 32, 4, GL_RGBA },	// D32
{ 16, 3, GL_RGB },	// 555

0,
GL_RGB5_A1,
GL_RGB5,
GL_RGBA4,
0,
GL_RGBA8,
GL_RGB8,
GL_RGB5,
GL_RGB8,
GL_RGBA8,
GL_RGB5
*/

Raster*
rasterCreate(Raster *raster)
{
	Gl1Raster *natras = GETGL1RASTEREXT(raster);

	natras->isCompressed = 0;
	natras->hasAlpha = 0;
	natras->numLevels = 1;

	Raster *ret = raster;

	if(raster->width == 0 || raster->height == 0){
		raster->flags |= Raster::DONTALLOCATE;
		raster->stride = 0;
		goto ret;
	}
	if(raster->flags & Raster::DONTALLOCATE)
		goto ret;

	switch(raster->type){
#ifdef RW_GLES1
	case Raster::NORMAL:
	case Raster::TEXTURE:
		ret = rasterCreateTexture(raster);
		break;
	case Raster::CAMERATEXTURE:
		ret = rasterCreateCameraTexture(raster);
		break;
	case Raster::ZBUFFER:
		ret = rasterCreateZbuffer(raster);
		break;
	case Raster::CAMERA:
		ret = rasterCreateCamera(raster);
		break;
#endif

	default:
		RWERROR((ERR_INVRASTER));
		return nil;
	}

ret:
	raster->originalWidth = raster->width;
	raster->originalHeight = raster->height;
	raster->originalStride = raster->stride;
	raster->originalPixels = raster->pixels;
	return ret;
}

uint8*
rasterLock(Raster *raster, int32 level, int32 lockMode)
{
#ifdef RW_GLES1
	Gl1Raster *natras = GETGL1RASTEREXT(raster);
	uint8 *px;
	uint32 allocSz;
	int i;

	assert(raster->privateFlags == 0);

	switch(raster->type){
	case Raster::NORMAL:
	case Raster::TEXTURE:
	case Raster::CAMERATEXTURE:
		for(i = 0; i < level; i++){
			if(raster->width > 1){
				raster->width /= 2;
				raster->stride /= 2;
			}
			if(raster->height > 1)
				raster->height /= 2;
		}

		allocSz = getLevelSize(raster, level);
		px = (uint8*)rwMalloc(allocSz, MEMDUR_EVENT | ID_DRIVER);
		assert(raster->pixels == nil);
		raster->pixels = px;

		if(lockMode & Raster::LOCKREAD || !(lockMode & Raster::LOCKNOFETCH)){
			if(natras->backingStore){
				assert(level < natras->backingStore->numlevels);
				assert(allocSz >= natras->backingStore->levels[level].size);
				memcpy(px, natras->backingStore->levels[level].data, allocSz);
			} else {
				memset(px, 0, allocSz);
			}
		}

		raster->privateFlags = lockMode;
		break;

	case Raster::CAMERA:
		if(lockMode & Raster::PRIVATELOCK_WRITE)
			assert(0 && "can't lock framebuffer for writing");
		raster->width = glGlobals.presentWidth;
		raster->height = glGlobals.presentHeight;
		raster->stride = raster->width*natras->bpp;
		assert(natras->bpp == 3);
		allocSz = raster->height*raster->stride;
		px = (uint8*)rwMalloc(allocSz, MEMDUR_EVENT | ID_DRIVER);
		assert(raster->pixels == nil);
		raster->pixels = px;

		memset(px, 0, allocSz);
		glReadPixels(0, 0, raster->width, raster->height, GL_RGB, GL_UNSIGNED_BYTE, px);

		raster->privateFlags = lockMode;
		break;

	default:
		assert(0 && "cannot lock this type of raster yet");
		return nil;
	}

	return px;
#else
	return nil;
#endif
}

void rasterUnlock(Raster *raster, int32 level){
#ifdef RW_GLES1
    Gl1Raster *natras = GETGL1RASTEREXT(raster);

    assert(raster->pixels);

    switch(raster->type){
    case Raster::NORMAL:
    case Raster::TEXTURE:
    case Raster::CAMERATEXTURE:
        if(raster->privateFlags & Raster::LOCKWRITE){
            if(level != 0) break;

            // === GUARDAR EN BACKING STORE ANTES DE SUBIR A GPU ===
            if(natras->backingStore == nil){
                int numLevels = natras->numLevels;
                // RasterLevels tiene: int numlevels; struct { uint32 size; uint8 *data; } levels[];
                natras->backingStore = (RasterLevels*)malloc(
                    sizeof(RasterLevels) + sizeof(natras->backingStore->levels[0]) * numLevels);
                natras->backingStore->numlevels = numLevels;
                for(int i = 0; i < numLevels; i++){
                    uint32 sz = getLevelSize(raster, i);
                    natras->backingStore->levels[i].size = sz;
                    natras->backingStore->levels[i].data = (uint8*)malloc(sz);
                    memset(natras->backingStore->levels[i].data, 0, sz);
                }
            }
            // copiar píxeles actuales al backing store
            {
                uint32 sz = getLevelSize(raster, level);
                assert(level < natras->backingStore->numlevels);
                memcpy(natras->backingStore->levels[level].data, raster->pixels, sz);
            }
            // === FIN BACKING STORE ===

            uint32 prev = bindTexture(natras->texid);

            // flip vertical (igual que antes)
            {
                int stride = raster->width * natras->bpp;
                uint8_t *tmp = (uint8_t*)rwMalloc(stride, MEMDUR_FUNCTION | ID_DRIVER);
                for(int y = 0; y < raster->height / 2; y++){
                    uint8_t *a = (uint8_t*)raster->pixels + y * stride;
                    uint8_t *b = (uint8_t*)raster->pixels + (raster->height - 1 - y) * stride;
                    memcpy(tmp, a, stride);
                    memcpy(a, b, stride);
                    memcpy(b, tmp, stride);
                }
                rwFree(tmp);
            }

            if(!natras->isCompressed){
                if(natras->format == GL_RGBA && natras->type == GL_UNSIGNED_BYTE){
                    // convertir RGBA8888 → RGBA4444
                    uint16_t *pixels16 = (uint16_t*)rwMalloc(raster->width * raster->height * 2, MEMDUR_FUNCTION | ID_DRIVER);
                    uint8_t *pixels8 = (uint8_t*)raster->pixels;
                    for(int i = 0; i < raster->width * raster->height; i++){
                        uint8_t r = pixels8[i*4+0];
                        uint8_t g = pixels8[i*4+1];
                        uint8_t b = pixels8[i*4+2];
                        uint8_t a = pixels8[i*4+3];
                        pixels16[i] = ((r>>4)<<12)|((g>>4)<<8)|((b>>4)<<4)|(a>>4);
                    }
                    glTexImage2D(GL_TEXTURE_2D, level, GL_RGBA,
                        raster->width, raster->height, 0,
                        GL_RGBA, GL_UNSIGNED_SHORT_4_4_4_4, pixels16);
                    rwFree(pixels16);
                } else {
                    glTexImage2D(GL_TEXTURE_2D, level, natras->internalFormat,
                        raster->width, raster->height, 0,
                        natras->format, natras->type, raster->pixels);
                }
            }

            bindTexture(prev);
        }
        break;

    case Raster::CAMERA:
        break;
    }

    rwFree(raster->pixels);
    raster->pixels = nil;
#endif
	raster->width = raster->originalWidth;
	raster->height = raster->originalHeight;
	raster->stride = raster->originalStride;
	raster->pixels = raster->originalPixels;
	raster->privateFlags = 0;
}

int32
rasterNumLevels(Raster *raster)
{
	return GETGL1RASTEREXT(raster)->numLevels;
}

// Almost the same as d3d9 and ps2 function
bool32
imageFindRasterFormat(Image *img, int32 type,
	int32 *pWidth, int32 *pHeight, int32 *pDepth, int32 *pFormat)
{
	int32 width, height, depth, format;

	assert((type&0xF) == Raster::TEXTURE);

//	for(width = 1; width < img->width; width <<= 1);
//	for(height = 1; height < img->height; height <<= 1);
	// Perhaps non-power-of-2 textures are acceptable?
	width = img->width;
	height = img->height;

	depth = img->depth;

	if(depth <= 8)
		depth = 32;

	switch(depth){
	case 32:
		if(img->hasAlpha())
			format = Raster::C8888;
		else{
			format = Raster::C888;
			depth = 24;
		}
		break;
	case 24:
		format = Raster::C888;
		break;
	case 16:
		format = Raster::C1555;
		break;

	case 8:
	case 4:
	default:
		RWERROR((ERR_INVRASTER));
		return 0;
	}

	format |= type;

	*pWidth = width;
	*pHeight = height;
	*pDepth = depth;
	*pFormat = format;

	return 1;
}

bool32
rasterFromImage(Raster *raster, Image *image)
{
	if((raster->type&0xF) != Raster::TEXTURE)
		return 0;

	void (*conv)(uint8 *out, uint8 *in) = nil;

	// Unpalettize image if necessary but don't change original
	Image *truecolimg = nil;
	if(image->depth <= 8){
		truecolimg = Image::create(image->width, image->height, image->depth);
		truecolimg->pixels = image->pixels;
		truecolimg->stride = image->stride;
		truecolimg->palette = image->palette;
		truecolimg->unpalettize();
		image = truecolimg;
	}

	Gl1Raster *natras = GETGL1RASTEREXT(raster);
	int32 format = raster->format&0xF00;
	assert(!natras->isCompressed);
	switch(image->depth){
	case 32:
		if(format == Raster::C8888)
			conv = conv_RGBA8888_from_RGBA8888;
		else if(format == Raster::C888)
			conv = conv_RGB888_from_RGB888;
		else
			goto err;
		break;
	case 24:
		if(format == Raster::C8888)
			conv = conv_RGBA8888_from_RGB888;
		else if(format == Raster::C888)
			conv = conv_RGB888_from_RGB888;
		else
			goto err;
		break;
	case 16:
		if(format == Raster::C8888)
			conv = conv_RGBA8888_from_ARGB1555;
		else if(format == Raster::C1555)
			conv = conv_RGBA5551_from_ARGB1555;
		else
			goto err;
		break;

	case 8:
	case 4:
	default:
	err:
		RWERROR((ERR_INVRASTER));
		return 0;
	}

	natras->hasAlpha = image->hasAlpha();

	bool unlock = false;
	if(raster->pixels == nil){
		raster->lock(0, Raster::LOCKWRITE|Raster::LOCKNOFETCH);
		unlock = true;
	}

	uint8 *pixels = raster->pixels;
	assert(pixels);
	uint8 *imgpixels = image->pixels + (image->height-1)*image->stride;

	int x, y;
	assert(image->width == raster->width);
	assert(image->height == raster->height);
	for(y = 0; y < image->height; y++){
		uint8 *imgrow = imgpixels;
		uint8 *rasrow = pixels;
		for(x = 0; x < image->width; x++){
			conv(rasrow, imgrow);
			imgrow += image->bpp;
			rasrow += natras->bpp;
		}
		imgpixels -= image->stride;
		pixels += raster->stride;
	}
	if(unlock)
		raster->unlock(0);

	if(truecolimg)
		truecolimg->destroy();

	return 1;
}

Image*
rasterToImage(Raster *raster)
{
	int32 depth;
	Image *image;

	bool unlock = false;
	if(raster->pixels == nil){
		raster->lock(0, Raster::LOCKREAD);
		unlock = true;
	}

	Gl1Raster *natras = GETGL1RASTEREXT(raster);
	if(natras->isCompressed){
		// TODO
		RWERROR((ERR_INVRASTER));
		return nil;
	}

	void (*conv)(uint8 *out, uint8 *in) = nil;
	switch(raster->format & 0xF00){
	case Raster::C1555:
		depth = 16;
		conv = conv_ARGB1555_from_RGBA5551;
		break;
	case Raster::C8888:
		depth = 32;
		conv = conv_RGBA8888_from_RGBA8888;
		break;
	case Raster::C888:
		depth = 24;
		conv = conv_RGB888_from_RGB888;
		break;

	default:
	case Raster::C555:
	case Raster::C565:
	case Raster::C4444:
	case Raster::LUM8:
		RWERROR((ERR_INVRASTER));
		return nil;
	}

	if(raster->format & Raster::PAL4 ||
	   raster->format & Raster::PAL8){
		RWERROR((ERR_INVRASTER));
		return nil;
	}
		
	uint8 *in, *out;
	image = Image::create(raster->width, raster->height, depth);
	image->allocate();

	uint8 *imgpixels = image->pixels + (image->height-1)*image->stride;
	uint8 *pixels = raster->pixels;

	int x, y;
	assert(image->width == raster->width);
	assert(image->height == raster->height);
	for(y = 0; y < image->height; y++){
		uint8 *imgrow = imgpixels;
		uint8 *rasrow = pixels;
		for(x = 0; x < image->width; x++){
			conv(imgrow, rasrow);
			imgrow += image->bpp;
			rasrow += natras->bpp;
		}
		imgpixels -= image->stride;
		pixels += raster->stride;
	}

	if(unlock)
		raster->unlock(0);

	return image;
}

static void*
createNativeRaster(void *object, int32 offset, int32)
{
	Gl1Raster *ras = PLUGINOFFSET(Gl1Raster, object, offset);
	ras->texid = 0;
	ras->fbo = 0;
	ras->fboMate = nil;
	ras->backingStore = nil;
	return object;
}

void evictRaster(Raster *raster);

static void*
destroyNativeRaster(void *object, int32 offset, int32)
{
	Raster *raster = (Raster*)object;
	Gl1Raster *natras = PLUGINOFFSET(Gl1Raster, object, offset);
#ifdef RW_GLES1
	evictRaster(raster);
	switch(raster->type){
	case Raster::NORMAL:
	case Raster::TEXTURE:
		glDeleteTextures(1, &natras->texid);
		break;

	case Raster::CAMERATEXTURE:
		break;

	case Raster::ZBUFFER:
		break;

	case Raster::CAMERA:
		if(natras->fboMate){
			// Break apart from currently associated zbuffer
			Gl1Raster *zras = GETGL1RASTEREXT(natras->fboMate);
			zras->fboMate = nil;
			natras->fboMate = nil;
		}
		break;
	}
	natras->texid = 0;
	natras->fbo = 0;

	if(natras->backingStore){
        for(int i = 0; i < natras->backingStore->numlevels; i++)
            free(natras->backingStore->levels[i].data);
        free(natras->backingStore);
        natras->backingStore = nil;
    }
#endif
	return object;
}

static void*
copyNativeRaster(void *dst, void *, int32 offset, int32)
{
	Gl1Raster *d = PLUGINOFFSET(Gl1Raster, dst, offset);
	d->texid = 0;
	d->fbo = 0;
	d->fboMate = nil;
	d->backingStore = nil;
	return dst;
}

Texture*
readNativeTexture(Stream *stream)
{
	uint32 platform;
	if(!findChunk(stream, ID_STRUCT, nil, nil)){
		RWERROR((ERR_CHUNK, "STRUCT"));
		return nil;
	}
	platform = stream->readU32();
	if(platform != PLATFORM_GLES1){
		RWERROR((ERR_PLATFORM, platform));
		return nil;
	}
	Texture *tex = Texture::create(nil);
	if(tex == nil)
		return nil;

	// Texture
	tex->filterAddressing = stream->readU32();
	stream->read8(tex->name, 32);
	stream->read8(tex->mask, 32);

	// Raster
	uint32 format = stream->readU32();
	int32 width = stream->readI32();
	int32 height = stream->readI32();
	int32 depth = stream->readI32();
	int32 numLevels = stream->readI32();

	// Native raster
	int32 subplatform = stream->readI32();
	int32 flags = stream->readI32();
	int32 compression = stream->readI32();

	Raster *raster;
	Gl1Raster *natras;
	if(flags & 2){
		tex->destroy();
		RWERROR((ERR_FORMAT_UNSUPPORTED));
		return nil;
	}else{
		raster = Raster::create(width, height, depth, format | Raster::TEXTURE, PLATFORM_GLES1);
	}
	assert(raster);
	natras = GETGL1RASTEREXT(raster);
	tex->raster = raster;

	uint32 size;
	uint8 *data;
	for(int32 i = 0; i < numLevels; i++){
		size = stream->readU32();
		data = raster->lock(i, Raster::LOCKWRITE|Raster::LOCKNOFETCH);
		stream->read8(data, size);
		raster->unlock(i);
	}
	return tex;
}

void
writeNativeTexture(Texture *tex, Stream *stream)
{
	Raster *raster = tex->raster;
	Gl1Raster *natras = GETGL1RASTEREXT(raster);

	int32 chunksize = getSizeNativeTexture(tex);
	writeChunkHeader(stream, ID_STRUCT, chunksize-12);
	stream->writeU32(PLATFORM_GLES1);

	// Texture
	stream->writeU32(tex->filterAddressing);
	stream->write8(tex->name, 32);
	stream->write8(tex->mask, 32);

	// Raster
	int32 numLevels = natras->numLevels;
	stream->writeI32(raster->format);
	stream->writeI32(raster->width);
	stream->writeI32(raster->height);
	stream->writeI32(raster->depth);
	stream->writeI32(numLevels);

	// Native raster
	int32 flags = 0;
	int32 compression = 0;
	if(natras->hasAlpha)
		flags |= 1;
	if(natras->isCompressed){
		flags |= 2;
		switch(natras->internalFormat){
		default:
			assert(0 && "unknown compression");
		}
	}
	stream->writeI32(1);
	stream->writeI32(flags);
	stream->writeI32(compression);
	// TODO: auto mipmaps?

	uint32 size;
	uint8 *data;
	for(int32 i = 0; i < numLevels; i++){
		size = getLevelSize(raster, i);
		stream->writeU32(size);
		data = raster->lock(i, Raster::LOCKREAD);
		stream->write8(data, size);
		raster->unlock(i);
	}
}

uint32
getSizeNativeTexture(Texture *tex)
{
	uint32 size = 12 + 72 + 32;
	int32 levels = tex->raster->getNumLevels();
	for(int32 i = 0; i < levels; i++)
		size += 4 + getLevelSize(tex->raster, i);
	return size;
}



void registerNativeRaster(void)
{
	nativeRasterOffset = Raster::registerPlugin(sizeof(Gl1Raster),
	                                            ID_RASTERGLES1,
	                                            createNativeRaster,
	                                            destroyNativeRaster,
	                                            copyNativeRaster);
}

}
}
