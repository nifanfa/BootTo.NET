#ifndef EGL_RASTERIZER_STATE_H
#define EGL_RASTERIZER_STATE_H 1

// ==========================================================================
//
// RasterizerState	Rasterizer State Class for 3D Rendering Library
//
//					The rasterizer state maintains all the state information
//					that is necessary to determine the details of the
//					next primitive to be scan-converted.
//
// --------------------------------------------------------------------------
//
// 10-10-2003		Hans-Martin Will	initial version
//
// --------------------------------------------------------------------------
//
// Copyright (c) 2004, Hans-Martin Will. All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are
// met:
//
//	 *  Redistributions of source code must retain the above copyright
// 		notice, this list of conditions and the following disclaimer.
//   *	Redistributions in binary form must reproduce the above copyright
// 		notice, this list of conditions and the following disclaimer in the
// 		documentation and/or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY,
// OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// ==========================================================================


#include "OGLES.h"
#include "fixed.h"
#include "linalg.h"
#include "Color.h"


namespace EGL {
	class Rasterizer;
	class MultiTexture;

	class RasterizerState {

		friend class Rasterizer;
		friend class CodeGenerator;

	public:
		enum Limits {
			LogMaxTextureSize = 12,
			MaxTextureSize = 1 << LogMaxTextureSize
		};

		enum LogicOp {
			LogicOpInvalid = -1,
			LogicOpClear,
			LogicOpAnd,
			LogicOpAndReverse,
			LogicOpCopy,
			LogicOpAndInverted,
			LogicOpNoop,
			LogicOpXor,
			LogicOpOr,
			LogicOpNor,
			LogicOpEquiv,
			LogicOpInvert,
			LogicOpOrReverse,
			LogicOpCopyInverted,
			LogicOpOrInverted,
			LogicOpNand,
			LogicOpSet
		};

		enum ComparisonFunc {
			CompFuncInvalid = -1,
			CompFuncNever,
			CompFuncLess,
			CompFuncEqual,
			CompFuncLEqual,
			CompFuncGreater,
			CompFuncNotEqual,
			CompFuncGEqual,
			CompFuncAlways
		};

		enum BlendFuncDst {
			BlendFuncDstInvalid = -1,
			BlendFuncDstZero,
			BlendFuncDstOne,
			BlendFuncDstSrcColor,
			BlendFuncDstOneMinusSrcColor,
			BlendFuncDstSrcAlpha,
			BlendFuncDstOneMinusSrcAlpha,
			BlendFuncDstDstAlpha,
			BlendFuncDstOneMinusDstAlpha
		};

		enum BlendFuncSrc {
			BlendFuncSrcInvalid = -1,
			BlendFuncSrcZero,
			BlendFuncSrcOne,
			BlendFuncSrcDstColor,
			BlendFuncSrcOneMinusDstColor,
			BlendFuncSrcSrcAlpha,
			BlendFuncSrcOneMinusSrcAlpha,
			BlendFuncSrcDstAlpha,
			BlendFuncSrcOneMinusDstAlpha,
			BlendFuncSrcSrcAlphaSaturate
		};

		enum StencilOp {
			StencilOpInvalid = -1,
			StencilOpZero,
			StencilOpKeep,
			StencilOpReplace,
			StencilOpIncr,
			StencilOpDecr,
			StencilOpInvert
		};

		enum ShadingModel {
			ShadeModelInvalid = -1,
			ShadeModelFlat,
			ShadeModelSmooth
		};

		enum TextureMode {
			TextureModeInvalid = -1,
			TextureModeDecal,
			TextureModeReplace,
			TextureModeBlend,
			TextureModeAdd,
			TextureModeModulate,
			TextureModeCombine
		};

		enum TextureModeCombineFunc {
			TextureModeCombineReplace,
			TextureModeCombineModulate,
			TextureModeCombineAdd,
			TextureModeCombineAddSigned,
			TextureModeCombineInterpolate,
			TextureModeCombineSubtract,
			TextureModeCombineDot3RGB,
			TextureModeCombineDot3RGBA
		};

		enum TextureCombineSrc {
			TextureCombineSrcTexture,
			TextureCombineSrcConstant,
			TextureCombineSrcPrimaryColor,
			TextureCombineSrcPrevious
		};

		enum TextureCombineOp {
			TextureCombineOpSrcColor,
			TextureCombineOpOneMinusSrcColor,
			TextureCombineOpSrcAlpha,
			TextureCombineOpOneMinusSrcAlpha
		};

		enum WrappingMode {
			WrappingModeInvalid = -1,
			WrappingModeClampToEdge,
			WrappingModeRepeat
		};

		enum FilterMode {
			FilterModeInvalid = -1,
			FilterModeNone,
			FilterModeNearest,
			FilterModeLinear
		};

		enum TextureFormat {
			TextureFormatInvalid = -1,
			TextureFormatAlpha = 0,				// 8
			TextureFormatLuminance = 1,			// 8
			TextureFormatLuminanceAlpha = 2,	// 8-8
			TextureFormatRGB8 = 3,				// 8-8-8
			TextureFormatRGBA8 = 4,				// 8-8-8-8
			TextureFormatRGB565 = 5,			// 5-6-5
			TextureFormatRGBA4444 = 6,			// 4-4-4-4
			TextureFormatRGBA5551 = 7			// 5-5-5-1
		};

	public:
		RasterizerState();

		bool CompareCommon(const RasterizerState& other) const;
		bool ComparePoint(const RasterizerState& other) const;
		bool CompareLine(const RasterizerState& other) const;
		bool ComparePolygon(const RasterizerState& other) const;

		typedef bool (RasterizerState::*CompareFunction)(const RasterizerState& other) const;

		// ----------------------------------------------------------------------
		// Primitive rendering state
		// ----------------------------------------------------------------------

		void SetTexEnvColor(size_t unit, const Color& color);
		Color GetTexEnvColor(size_t unit) const;

		void SetTextureMode(size_t unit, TextureMode mode);
		TextureMode GetTextureMode(size_t unit) const;

		void SetTextureCombineFuncRGB(size_t unit, TextureModeCombineFunc func);
		TextureModeCombineFunc GetTextureCombineFuncRGB(size_t unit) const;

		void SetTextureCombineFuncAlpha(size_t unit, TextureModeCombineFunc func);
		TextureModeCombineFunc GetTextureCombineFuncAlpha(size_t unit) const;

		void SetTextureCombineSrcRGB(size_t unit, size_t idx, TextureCombineSrc src);
		TextureCombineSrc GetTextureCombineSrcRGB(size_t unit, size_t idx) const;

		void SetTextureCombineSrcAlpha(size_t unit, size_t idx, TextureCombineSrc src);
		TextureCombineSrc GetTextureCombineSrcAlpha(size_t unit, size_t idx) const;

		void SetTextureCombineOpRGB(size_t unit, size_t idx, TextureCombineOp op);
		TextureCombineOp GetTextureCombineOpRGB(size_t unit, size_t idx) const;

		void SetTextureCombineOpAlpha(size_t unit, size_t idx, TextureCombineOp op);
		TextureCombineOp GetTextureCombineOpAlpha(size_t unit, size_t idx) const;

		void SetTextureScaleRGB(size_t unit, EGL_Fixed scale);
		EGL_Fixed GetTextureScaleRGB(size_t unit) const;

		void SetTextureScaleAlpha(size_t unit, EGL_Fixed scale);
		EGL_Fixed GetTextureScaleAlpha(size_t unit) const;

		void SetMinFilterMode(size_t unit, FilterMode mode);
		void SetMagFilterMode(size_t unit, FilterMode mode);
		void SetMipmapFilterMode(size_t unit, FilterMode mode);
		void SetWrappingModeS(size_t unit, WrappingMode mode);
		void SetWrappingModeT(size_t unit, WrappingMode mode);

		FilterMode GetMinFilterMode(size_t unit) const			{ return m_Texture[unit].MinFilterMode; }
		FilterMode GetMagFilterMode(size_t unit) const			{ return m_Texture[unit].MagFilterMode; }
		FilterMode GetMipmapFilterMode(size_t unit) const		{ return m_Texture[unit].MipmapFilterMode; }
		WrappingMode GetWrappingModeS(size_t unit) const		{ return m_Texture[unit].WrappingModeS; }
		WrappingMode GetWrappingModeT(size_t unit) const		{ return m_Texture[unit].WrappingModeT; }

		void SetInternalFormat(size_t unit, TextureFormat format);

		void SetDepthRange(EGL_Fixed zNear, EGL_Fixed zFar);

		void SetFogColor(const Color& color);
		Color GetFogColor() const;
		void EnableFog(bool enabled);
		bool IsEnabledFog() const;

		void SetLineWidth(EGL_Fixed width);
		void SetLineSmoothEnabled(bool enabled);
		bool IsLineSmoothEnabled() const;

		void SetLogicOp(LogicOp opcode);
		LogicOp GetLogicOp() const;

		void SetShadeModel(ShadingModel mode);
		ShadingModel GetShadeModel() const;

		void EnableTexture(size_t unit, bool enabled);
		bool IsEnabledTexture(size_t unit) const;

		void SetPointSmoothEnabled(bool enabled);
		bool IsPointSmoothEnabled() const;
		void SetPointSpriteEnabled(bool enabled);
		bool IsPointSpriteEnabled() const;
		void SetPointCoordReplaceEnabled(size_t unit, bool enabled);
		bool IsPointCoordReplaceEnabled(size_t unit) const;

		void EnablePolygonOffsetFill(bool enabled);
		bool IsEnabledPolygonOffsetFill() const;
		void SetPolygonOffset(EGL_Fixed factor, EGL_Fixed units);
		void SetSampleCoverage(EGL_Fixed value, bool invert);
		EGL_Fixed GetPolygonOffsetFactor() const;
		EGL_Fixed GetPolygonOffsetUnits() const;
		EGL_Fixed GetSampleCoverage() const;
		bool GetSampleCoverageInvert() const;

		// ----------------------------------------------------------------------
		// Fragment rendering state
		// ----------------------------------------------------------------------

		void SetAlphaFunc(ComparisonFunc func, EGL_Fixed ref);
		ComparisonFunc GetAlphaFunc() const;
		EGL_Fixed GetAlphaRef() const;
		void SetBlendFunc(BlendFuncSrc sfactor, BlendFuncDst dfactor);
		BlendFuncSrc GetBlendFuncSrc() const;
		BlendFuncDst GetBlendFuncDst() const;
		void SetColorMask(bool red, bool green, bool blue, bool alpha);
		Color GetColorMask() const;

		void SetDepthFunc(ComparisonFunc func);
		ComparisonFunc GetDepthFunc() const;
		void SetDepthMask(bool flag);
		bool GetDepthMask() const;

		void SetScissor(I32 x, I32 y, U32 width, U32 height);

		void EnableAlphaTest(bool enabled);
		void EnableDepthTest(bool enabled);
		void EnableLogicOp(bool enabled);
		void EnableScissorTest(bool enabled);
		void EnableBlending(bool enabled);
		void EnableStencilTest(bool enabled);

		bool IsEnabledAlphaTest() const;
		bool IsEnabledDepthTest() const;
		bool IsEnabledLogicOp() const;
		bool IsEnabledScissorTest() const;
		bool IsEnabledBlending() const;
		bool IsEnabledStencilTest() const;

		void SetStencilFunc(ComparisonFunc func, I32 ref, U32 mask);
		ComparisonFunc GetStencilFunc() const;
		I32 GetStencilRef() const;
		I32 GetStencilComparisonMask() const;
		void SetStencilMask(U32 mask);
		U32 GetStencilMask() const;

		void SetStencilOp(StencilOp fail, StencilOp zfail, StencilOp zpass);
		StencilOp GetStencilOpFail() const;
		StencilOp GetStencilOpFailZFail() const;
		StencilOp GetStencilOpFailZPass() const;

	private:
		// ----------------------------------------------------------------------
		// Primitve specific rendering state
		// ----------------------------------------------------------------------

		struct PointState {
			PointState() {
				SmoothEnabled = false;
				SpriteEnabled = false;
				CoordReplaceEnabled = false;
			}

			PointState(const PointState& other) {
				SmoothEnabled = other.SmoothEnabled;
				SpriteEnabled = other.SpriteEnabled;
				CoordReplaceEnabled = other.CoordReplaceEnabled;
			}

			PointState& operator=(const PointState& other) {
				SmoothEnabled = other.SmoothEnabled;
				SpriteEnabled = other.SpriteEnabled;
				CoordReplaceEnabled = other.CoordReplaceEnabled;
				return *this;
			}

			bool operator==(const PointState& other) const {
				return	SmoothEnabled == other.SmoothEnabled &&
					SpriteEnabled == other.SpriteEnabled &&
					(!SpriteEnabled ||
					 CoordReplaceEnabled == other.CoordReplaceEnabled);
			}

			bool				SmoothEnabled;
			bool				SpriteEnabled;
			bool				CoordReplaceEnabled;
		}
								m_Point;

		struct LineState {
			LineState() {
				SmoothEnabled = false;
				Width = EGL_ONE;
			}

			LineState(const LineState& other) {
				SmoothEnabled = other.SmoothEnabled;
				Width = other.Width;
			}

			LineState& operator=(const LineState& other) {
				SmoothEnabled = other.SmoothEnabled;
				Width = other.Width;
				return *this;
			}

			bool operator==(const LineState& other) const {
				return SmoothEnabled == other.SmoothEnabled &&
					Width == other.Width;
			}

			bool				SmoothEnabled;
			EGL_Fixed			Width;	// current line width
		}
								m_Line;


		struct PolygonState {
			PolygonState() {
				OffsetFillEnabled = false;
				OffsetFactor = 0;
				OffsetUnits = 0;
			}

			PolygonState(const PolygonState& other) {
				OffsetFillEnabled = other.OffsetFillEnabled;
				OffsetFactor = other.OffsetFactor;
				OffsetUnits = other.OffsetUnits;
			}

			PolygonState& operator=(const PolygonState& other) {
				OffsetFillEnabled = other.OffsetFillEnabled;
				OffsetFactor = other.OffsetFactor;
				OffsetUnits = other.OffsetUnits;
				return *this;
			}

			bool operator==(const PolygonState& other) const {
				return OffsetFillEnabled == other.OffsetFillEnabled &&
					(!OffsetFillEnabled ||
					 OffsetFactor == other.OffsetFactor &&
					 OffsetUnits == other.OffsetUnits);
			}

			bool				OffsetFillEnabled;
			EGL_Fixed			OffsetFactor;
			EGL_Fixed			OffsetUnits;
		}
								m_Polygon;

		// ----------------------------------------------------------------------
		// General rendering state
		// ----------------------------------------------------------------------

		struct FogState {
			FogState() {
				Enabled = false;
				Color = EGL::Color(0, 0, 0, 0);

			}

			FogState(const FogState& other) {
				Enabled = other.Enabled;
				Color = other.Color;
			}

			FogState& operator=(const FogState& other) {
				Enabled = other.Enabled;
				Color = other.Color;
				return *this;
			}

			bool operator==(const FogState& other) const {
				return Enabled == other.Enabled &&
					(!Enabled || Color == other.Color);
			}

			bool				Enabled;
            EGL::Color				Color;
		}
								m_Fog;

		struct TextureState {
			TextureState() {
				Enabled = false;
				EnvColor = Color(0, 0, 0, 0);
				Mode = TextureModeModulate;
				MinFilterMode = FilterModeNearest;
				MagFilterMode = FilterModeNearest;
				MipmapFilterMode = FilterModeNone;
				WrappingModeS = WrappingModeRepeat;
				WrappingModeT = WrappingModeRepeat;
				InternalFormat = TextureFormatLuminance;				
				CombineFuncRGB = TextureModeCombineModulate; 
				CombineFuncAlpha = TextureModeCombineModulate;

				CombineSrcRGB[0] = CombineSrcAlpha[0] = TextureCombineSrcTexture;
				CombineSrcRGB[1] = CombineSrcAlpha[1] = TextureCombineSrcPrevious;
				CombineSrcRGB[2] = CombineSrcAlpha[2] = TextureCombineSrcConstant;

				CombineOpRGB[0] = CombineOpRGB[1] = TextureCombineOpSrcColor;
				CombineOpRGB[2] = CombineOpAlpha[0] =
				CombineOpAlpha[1] = CombineOpAlpha[2] = TextureCombineOpSrcAlpha;

				ScaleRGB = ScaleAlpha = EGL_ONE;
			}

			TextureState(const TextureState& other) {
				Enabled = other.Enabled;
				EnvColor = other.EnvColor;
				Mode = other.Mode;
				MinFilterMode = other.MinFilterMode;
				MagFilterMode = other.MagFilterMode;
				MipmapFilterMode = other.MipmapFilterMode;
				WrappingModeS = other.WrappingModeS;
				WrappingModeT = other.WrappingModeT;
				InternalFormat = other.InternalFormat;
				CombineFuncRGB = other.CombineFuncRGB;
				CombineFuncAlpha = other.CombineFuncAlpha;

				for (int i = 0; i < 3; ++i) {
					CombineSrcRGB[i] = other.CombineSrcRGB[i];
					CombineSrcAlpha[i] = other.CombineSrcAlpha[i];
					CombineOpRGB[i] = other.CombineOpRGB[i];
					CombineOpAlpha[i] = other.CombineOpAlpha[i];
				}

				ScaleRGB = other.ScaleRGB;
				ScaleAlpha = other.ScaleAlpha;
			}

			TextureState& operator=(const TextureState& other) {
				Enabled = other.Enabled;
				EnvColor = other.EnvColor;
				Mode = other.Mode;
				MinFilterMode = other.MinFilterMode;
				MagFilterMode = other.MagFilterMode;
				MipmapFilterMode = other.MipmapFilterMode;
				WrappingModeS = other.WrappingModeS;
				WrappingModeT = other.WrappingModeT;
				InternalFormat = other.InternalFormat;
				CombineFuncRGB = other.CombineFuncRGB;
				CombineFuncAlpha = other.CombineFuncAlpha;

				for (int i = 0; i < 3; ++i) {
					CombineSrcRGB[i] = other.CombineSrcRGB[i];
					CombineSrcAlpha[i] = other.CombineSrcAlpha[i];
					CombineOpRGB[i] = other.CombineOpRGB[i];
					CombineOpAlpha[i] = other.CombineOpAlpha[i];
				}

				ScaleRGB = other.ScaleRGB;
				ScaleAlpha = other.ScaleAlpha;

				return *this;
			}

			bool operator==(const TextureState& other) const {
				if (!(Enabled == other.Enabled))
					return false;
				
				if (!Enabled)
					return true;

				if (!(EnvColor == other.EnvColor) ||
					Mode != other.Mode ||
					MinFilterMode != other.MinFilterMode ||
					MagFilterMode != other.MagFilterMode ||
					MipmapFilterMode != other.MipmapFilterMode ||
					WrappingModeS != other.WrappingModeS ||
					WrappingModeT != other.WrappingModeT ||
					InternalFormat != other.InternalFormat)
					return false;

				if (Mode == TextureModeCombine) {

					if (CombineFuncRGB != other.CombineFuncRGB ||
						CombineFuncAlpha != other.CombineFuncAlpha ||
						ScaleRGB != other.ScaleRGB ||
						ScaleAlpha != other.ScaleAlpha)
						return false;

					for (int i = 0; i < 3; ++i) {
						if (CombineSrcRGB[i] != other.CombineSrcRGB[i] ||
							CombineSrcAlpha[i] != other.CombineSrcAlpha[i] ||
							CombineOpRGB[i] != other.CombineOpRGB[i] ||
							CombineOpAlpha[i] != other.CombineOpAlpha[i])
							return false;
					}
				}

				return true;
			}

			bool					Enabled;
			Color					EnvColor;
			TextureMode				Mode;
			TextureModeCombineFunc	CombineFuncRGB, CombineFuncAlpha;
			TextureCombineSrc		CombineSrcRGB[3], CombineSrcAlpha[3];
			TextureCombineOp		CombineOpRGB[3], CombineOpAlpha[3];
			EGL_Fixed				ScaleRGB, ScaleAlpha;
			FilterMode				MinFilterMode;
			FilterMode				MagFilterMode;
			FilterMode				MipmapFilterMode;
			WrappingMode			WrappingModeS;
			WrappingMode			WrappingModeT;
			TextureFormat			InternalFormat;
		}
								m_Texture[EGL_NUM_TEXTURE_UNITS];

		struct ScissorTestState {
			ScissorTestState() {
				Enabled = false;
				X = 0;
				Y = 0;
				Width = 1024;
				Height = 1024;
			}

			ScissorTestState(const ScissorTestState& other) {
				Enabled = other.Enabled;
				X = other.X;
				Y = other.Y;
				Width = other.Width;
				Height = other.Height;
			}

			ScissorTestState& operator=(const ScissorTestState& other) {
				Enabled = other.Enabled;
				X = other.X;
				Y = other.Y;
				Width = other.Width;
				Height = other.Height;
				return *this;
			}

			bool operator==(const ScissorTestState& other) const {
				return Enabled == other.Enabled &&
					(!Enabled ||
					 X == other.X && Y == other.Y &&
					 Width == other.Width && Height == other.Height);
			}

			bool				Enabled;
			I32					X, Y;
			U32					Width, Height;
		}
								m_ScissorTest;

		struct MaskState {
			MaskState() {
				Red = true;
				Blue = true;
				Green = true;
				Alpha = true;
				Depth = true;
			}

			MaskState(const MaskState& other) {
				Red = other.Red;
				Blue = other.Blue;
				Green = other.Green;
				Alpha = other.Alpha;
				Depth = other.Depth;
			}

			MaskState& operator=(const MaskState& other) {
				Red = other.Red;
				Blue = other.Blue;
				Green = other.Green;
				Alpha = other.Alpha;
				Depth = other.Depth;
				return *this;
			}

			bool operator==(const MaskState& other) const {
				return Red == other.Red &&
					Green == other.Green &&
					Blue == other.Blue &&
					Alpha == other.Alpha &&
					Depth == other.Depth;
			}

			bool				Red;	// if true, write red channel
			bool				Green;	// if true, write green channel
			bool				Blue;	// if true, write blue channel
			bool				Alpha;	// if true, write alpha channel
			bool				Depth;	// if true, write depth buffer
		}
								m_Mask;

		struct AlphaState {
			AlphaState() {
				Enabled = false;
				Func = CompFuncAlways;
				Reference = 0; 
			}

			AlphaState(const AlphaState& other) {
				Enabled = other.Enabled;
				Func = other.Func;
				Reference = other.Reference; 
			}

			AlphaState& operator=(const AlphaState& other) {
				Enabled = other.Enabled;
				Func = other.Func;
				Reference = other.Reference; 
				return *this;
			}

			bool operator==(const AlphaState& other) const {
				return Enabled == other.Enabled &&
					(!Enabled ||
					 Func == other.Func &&
					 Reference == other.Reference);
			}

			bool				Enabled;
			ComparisonFunc		Func;
			EGL_Fixed			Reference;
		}
								m_Alpha;

		struct BlendState {
			BlendState() {
				Enabled = false;
				FuncSrc = BlendFuncSrcOne;
				FuncDst = BlendFuncDstZero;
			}

			BlendState(const BlendState& other) {
				Enabled = other.Enabled;
				FuncSrc = other.FuncSrc;
				FuncDst = other.FuncDst;
			}

			BlendState& operator=(const BlendState& other) {
				Enabled = other.Enabled;
				FuncSrc = other.FuncSrc;
				FuncDst = other.FuncDst;
				return *this;
			}

			bool operator==(const BlendState& other) const {
				return Enabled == other.Enabled &&
					(!Enabled ||
					 FuncSrc == other.FuncSrc && FuncDst == other.FuncDst);
			}

			bool				Enabled;
			BlendFuncSrc		FuncSrc;
			BlendFuncDst		FuncDst;
		}
								m_Blend;

		struct DepthTestState {
			DepthTestState() {
				Enabled = false;
				Func = CompFuncLess;
			}

			DepthTestState(const DepthTestState& other) {
				Enabled = other.Enabled;
				Func = other.Func;
			}

			DepthTestState& operator=(const DepthTestState& other) {
				Enabled = other.Enabled;
				Func = other.Func;
				return *this;
			}

			bool operator==(const DepthTestState& other) const {
				return Enabled == other.Enabled &&
					(!Enabled || Func == other.Func);
			}

			bool				Enabled;
			ComparisonFunc		Func;
		}
								m_DepthTest;

		struct LogicOpState {
			LogicOpState() {
				Enabled = false;
				Opcode = LogicOpCopy;
			}

			LogicOpState(const LogicOpState& other) {
				Enabled = other.Enabled;
				Opcode = other.Opcode;
			}

			LogicOpState& operator=(const LogicOpState& other) {
				Enabled = other.Enabled;
				Opcode = other.Opcode;
				return *this;
			}

			bool operator==(const LogicOpState& other) const {
				return Enabled == other.Enabled &&
					(!Enabled || Opcode == other.Opcode);
			}

			bool				Enabled;
			LogicOp				Opcode;
		}
								m_LogicOp;

		struct StencilState {
			StencilState() {
				Enabled = false;
				Func = CompFuncAlways;
				Reference = 0;
				Mask = 0xFFFFFFFFu;
				ComparisonMask = 0xFFFFFFFFu;
				Fail = StencilOpKeep;
				ZFail = StencilOpKeep;
				ZPass = StencilOpKeep;
			}

			StencilState(const StencilState& other) {
				Enabled = other.Enabled;
				Func = other.Func;
				Reference = other.Reference;
				ComparisonMask = other.ComparisonMask;
				Mask = other.Mask;
				Fail = other.Fail;
				ZFail = other.ZFail;
				ZPass = other.ZPass;
			}

			StencilState& operator=(const StencilState& other) {
				Enabled = other.Enabled;
				Func = other.Func;
				Reference = other.Reference;
				Mask = other.Mask;
				ComparisonMask = other.ComparisonMask;
				Fail = other.Fail;
				ZFail = other.ZFail;
				ZPass = other.ZPass;
				return *this;
			}

			bool operator==(const StencilState& other) const {
				return Enabled == other.Enabled &&
					(!Enabled ||
					 Func == other.Func &&
					 Reference == other.Reference &&
					 Mask == other.Mask &&
					 ComparisonMask == other.ComparisonMask &&
					 Fail == other.Fail &&
					 ZFail == other.ZFail &&
					 ZPass == other.ZPass);
			}

			bool				Enabled;
			ComparisonFunc		Func;
			I32					Reference;
			U32					ComparisonMask;
			U32					Mask;

			StencilOp			Fail, ZFail, ZPass;
		}
								m_Stencil;

		ShadingModel			m_ShadingModel;

		EGL_Fixed				m_SampleCoverage;
		bool					m_InvertSampleCoverage;

	};


	// ----------------------------------------------------------------------
	// Inline member definitions
	// ----------------------------------------------------------------------


	inline void RasterizerState :: SetTexEnvColor(size_t unit, const Color& color) {
		m_Texture[unit].EnvColor = color;
	}

	inline Color RasterizerState :: GetTexEnvColor(size_t unit) const {
		return m_Texture[unit].EnvColor;
	}

	inline void RasterizerState :: SetTextureMode(size_t unit, TextureMode mode) {
		m_Texture[unit].Mode = mode;
	}

	inline RasterizerState::TextureMode RasterizerState :: GetTextureMode(size_t unit) const {
		return m_Texture[unit].Mode;
	}

	inline void RasterizerState :: SetMinFilterMode(size_t unit, FilterMode mode) {
		m_Texture[unit].MinFilterMode = mode;
	}

	inline void RasterizerState :: SetMagFilterMode(size_t unit, FilterMode mode) {
		m_Texture[unit].MagFilterMode = mode;
	}

	inline void RasterizerState :: SetMipmapFilterMode(size_t unit, FilterMode mode) {
		m_Texture[unit].MipmapFilterMode = mode;
	}

	inline void RasterizerState :: SetWrappingModeS(size_t unit, WrappingMode mode) {
		m_Texture[unit].WrappingModeS = mode;
	}

	inline void RasterizerState :: SetWrappingModeT(size_t unit, WrappingMode mode) {
		m_Texture[unit].WrappingModeT = mode;
	}

	inline void RasterizerState :: SetInternalFormat(size_t unit, TextureFormat format) {
		m_Texture[unit].InternalFormat = format;
	}

	inline void RasterizerState :: SetFogColor(const Color& color) {
		m_Fog.Color = color;
	}

	inline void RasterizerState :: EnableFog(bool enabled) {
		m_Fog.Enabled = enabled;
	}

	inline void RasterizerState :: SetLineWidth(EGL_Fixed width) {
		m_Line.Width = width;
	}

	inline void RasterizerState :: SetLogicOp(LogicOp opcode) {
		m_LogicOp.Opcode = opcode;
	}

	inline void RasterizerState :: SetShadeModel(ShadingModel mode) {
		m_ShadingModel = mode;
	}

	inline RasterizerState::ShadingModel RasterizerState :: GetShadeModel() const {
		return m_ShadingModel;
	}

	inline void RasterizerState :: EnableTexture(size_t unit, bool enabled) {
		m_Texture[unit].Enabled = enabled;
	}

	inline void RasterizerState :: EnablePolygonOffsetFill(bool enabled) {
		m_Polygon.OffsetFillEnabled = enabled;
	}

	inline bool RasterizerState :: IsEnabledPolygonOffsetFill() const {
		return m_Polygon.OffsetFillEnabled;
	}

	inline void RasterizerState :: SetPolygonOffset(EGL_Fixed factor, EGL_Fixed units) {
		m_Polygon.OffsetFactor = factor;
		m_Polygon.OffsetUnits = units;
	}

	inline void RasterizerState :: SetSampleCoverage(EGL_Fixed value, bool invert) {
		m_SampleCoverage = value;
		m_InvertSampleCoverage = invert;
	}

	// ----------------------------------------------------------------------
	// Fragment rendering state
	// ----------------------------------------------------------------------

	inline void RasterizerState :: SetAlphaFunc(ComparisonFunc func, EGL_Fixed ref) {
		m_Alpha.Func = func;
		m_Alpha.Reference = ref;
	}

	inline void RasterizerState :: SetBlendFunc(BlendFuncSrc sfactor, BlendFuncDst dfactor) {
		m_Blend.FuncSrc = sfactor;
		m_Blend.FuncDst = dfactor;
	}

	inline void RasterizerState :: SetColorMask(bool red, bool green, bool blue, bool alpha) {
		m_Mask.Alpha = alpha;
		m_Mask.Red = red;
		m_Mask.Green = green;
		m_Mask.Blue = blue;
	}

	inline Color RasterizerState :: GetColorMask() const {
		return Color(m_Mask.Red		? 0xff : 0,
					 m_Mask.Green	? 0xff : 0,
					 m_Mask.Blue	? 0xff : 0,
					 m_Mask.Alpha	? 0xff : 0);
	}

	inline bool RasterizerState :: GetDepthMask() const {
		return m_Mask.Depth;
	}

	inline U32 RasterizerState :: GetStencilMask() const {
		return m_Stencil.Mask;
	}

	inline void RasterizerState :: SetDepthFunc(ComparisonFunc func) {
		m_DepthTest.Func = func;
	}

	inline void RasterizerState :: SetDepthMask(bool flag) {
		m_Mask.Depth = flag;
	}

	inline void RasterizerState :: SetScissor(I32 x, I32 y, U32 width, U32 height) {
		m_ScissorTest.X = x;
		m_ScissorTest.Y = y;
		m_ScissorTest.Width = width;
		m_ScissorTest.Height = height;
	}

	inline void RasterizerState :: EnableAlphaTest(bool enabled) {
		m_Alpha.Enabled = enabled;
	}

	inline void RasterizerState :: EnableDepthTest(bool enabled) {
		m_DepthTest.Enabled = enabled;
	}

	inline void RasterizerState :: EnableLogicOp(bool enabled) {
		m_LogicOp.Enabled = enabled;
	}

	inline void RasterizerState :: EnableScissorTest(bool enabled) {
		m_ScissorTest.Enabled = enabled;
	}

	inline void RasterizerState :: EnableBlending(bool enabled) {
		m_Blend.Enabled = enabled;
	}

	inline void RasterizerState :: SetStencilFunc(ComparisonFunc func, I32 ref, U32 mask) {
		m_Stencil.Func = func;
		m_Stencil.Reference = ref;
		m_Stencil.ComparisonMask = mask;
	}

	inline void RasterizerState :: SetStencilMask(U32 mask) {
		m_Stencil.Mask = mask;
	}

	inline void RasterizerState :: SetStencilOp(StencilOp fail, StencilOp zfail, StencilOp zpass) {
		m_Stencil.Fail = fail;
		m_Stencil.ZFail = zfail;
		m_Stencil.ZPass = zpass;
	}

	inline void RasterizerState :: EnableStencilTest(bool enabled) {
		m_Stencil.Enabled = enabled;
	}

	inline void RasterizerState :: SetLineSmoothEnabled(bool enabled) {
		m_Line.SmoothEnabled = enabled;
	}

	inline void RasterizerState :: SetPointSmoothEnabled(bool enabled) {
		m_Point.SmoothEnabled = enabled;
	}

	inline void RasterizerState :: SetPointSpriteEnabled(bool enabled) {
		m_Point.SpriteEnabled = enabled;
	}

	inline void RasterizerState :: SetPointCoordReplaceEnabled(size_t unit, bool enabled) {
		m_Point.CoordReplaceEnabled = enabled;
	}

	inline bool RasterizerState :: IsPointCoordReplaceEnabled(size_t unit) const {
		return m_Point.CoordReplaceEnabled;
	}

	inline bool RasterizerState :: IsEnabledFog() const {
		return m_Fog.Enabled;
	}

	inline Color RasterizerState :: GetFogColor() const {
		return m_Fog.Color;
	}

	inline EGL_Fixed RasterizerState :: GetPolygonOffsetFactor() const {
		return m_Polygon.OffsetFactor;
	}

	inline EGL_Fixed RasterizerState :: GetPolygonOffsetUnits() const {
		return m_Polygon.OffsetUnits;
	}

	inline EGL_Fixed RasterizerState :: GetSampleCoverage() const {
		return m_SampleCoverage;
	}

	inline bool RasterizerState :: GetSampleCoverageInvert() const {
		return m_InvertSampleCoverage;
	}

	inline bool RasterizerState :: IsPointSmoothEnabled() const {
		return m_Point.SmoothEnabled;
	}

	inline bool RasterizerState :: IsLineSmoothEnabled() const {
		return m_Line.SmoothEnabled;
	}

	inline bool RasterizerState :: IsEnabledAlphaTest() const {
		return m_Alpha.Enabled;
	}

	inline bool RasterizerState :: IsEnabledDepthTest() const {
		return m_DepthTest.Enabled;
	}

	inline bool RasterizerState :: IsEnabledLogicOp() const {
		return m_LogicOp.Enabled;
	}

	inline bool RasterizerState :: IsEnabledScissorTest() const {
		return m_ScissorTest.Enabled;
	}

	inline bool RasterizerState :: IsEnabledBlending() const {
		return m_Blend.Enabled;
	}

	inline bool RasterizerState :: IsEnabledStencilTest() const {
		return m_Stencil.Enabled;
	}

	inline bool RasterizerState :: IsEnabledTexture(size_t unit) const {
		return m_Texture[unit].Enabled;
	}

	inline bool RasterizerState :: IsPointSpriteEnabled() const {
		return m_Point.SpriteEnabled;
	}

	inline RasterizerState::LogicOp RasterizerState :: GetLogicOp() const {
		return m_LogicOp.Opcode;
	}

	inline RasterizerState::ComparisonFunc RasterizerState :: GetAlphaFunc() const {
		return m_Alpha.Func;
	}

	inline EGL_Fixed RasterizerState :: GetAlphaRef() const {
		return m_Alpha.Reference;
	}

	inline RasterizerState::BlendFuncSrc RasterizerState :: GetBlendFuncSrc() const {
		return m_Blend.FuncSrc;
	}

	inline RasterizerState::BlendFuncDst RasterizerState :: GetBlendFuncDst() const {
		return m_Blend.FuncDst;
	}

	inline RasterizerState::ComparisonFunc RasterizerState :: GetDepthFunc() const {
		return m_DepthTest.Func;
	}

	inline RasterizerState::ComparisonFunc RasterizerState :: GetStencilFunc() const {
		return m_Stencil.Func;
	}

	inline I32 RasterizerState :: GetStencilRef() const {
		return m_Stencil.Reference;
	}

	inline I32 RasterizerState :: GetStencilComparisonMask() const {
		return m_Stencil.ComparisonMask;
	}

	inline RasterizerState::StencilOp RasterizerState :: GetStencilOpFail() const {
		return m_Stencil.Fail;
	}

	inline RasterizerState::StencilOp RasterizerState :: GetStencilOpFailZFail() const {
		return m_Stencil.ZFail;
	}

	inline RasterizerState::StencilOp RasterizerState :: GetStencilOpFailZPass() const {
		return m_Stencil.ZPass;
	}

	inline void RasterizerState :: SetTextureCombineFuncRGB(size_t unit, TextureModeCombineFunc func) {
		m_Texture[unit].CombineFuncRGB = func;
	}

	inline RasterizerState::TextureModeCombineFunc RasterizerState :: GetTextureCombineFuncRGB(size_t unit) const {
		return m_Texture[unit].CombineFuncRGB;
	}

	inline void RasterizerState :: SetTextureCombineFuncAlpha(size_t unit, TextureModeCombineFunc func) {
		m_Texture[unit].CombineFuncAlpha = func;
	}

	inline RasterizerState::TextureModeCombineFunc RasterizerState :: GetTextureCombineFuncAlpha(size_t unit) const {
		return m_Texture[unit].CombineFuncAlpha;
	}

	inline void RasterizerState :: SetTextureCombineSrcRGB(size_t unit, size_t idx, TextureCombineSrc src) {
		m_Texture[unit].CombineSrcRGB[idx] = src;
	}

	inline RasterizerState::TextureCombineSrc RasterizerState :: GetTextureCombineSrcRGB(size_t unit, size_t idx) const {
		return m_Texture[unit].CombineSrcRGB[idx];
	}

	inline void RasterizerState :: SetTextureCombineSrcAlpha(size_t unit, size_t idx, TextureCombineSrc src) {
		m_Texture[unit].CombineSrcAlpha[idx] = src;
	}

	inline RasterizerState::TextureCombineSrc RasterizerState :: GetTextureCombineSrcAlpha(size_t unit, size_t idx) const {
		return m_Texture[unit].CombineSrcAlpha[idx];
	}

	inline void RasterizerState :: SetTextureCombineOpRGB(size_t unit, size_t idx, TextureCombineOp op) {
		m_Texture[unit].CombineOpRGB[idx] = op;
	}

	inline RasterizerState::TextureCombineOp RasterizerState :: GetTextureCombineOpRGB(size_t unit, size_t idx) const {
		return m_Texture[unit].CombineOpRGB[idx];
	}

	inline void RasterizerState :: SetTextureCombineOpAlpha(size_t unit, size_t idx, TextureCombineOp op) {
		m_Texture[unit].CombineOpAlpha[idx] = op;
	}

	inline RasterizerState::TextureCombineOp RasterizerState :: GetTextureCombineOpAlpha(size_t unit, size_t idx) const {
		return m_Texture[unit].CombineOpAlpha[idx];
	}

	inline void RasterizerState :: SetTextureScaleRGB(size_t unit, EGL_Fixed scale) {
		m_Texture[unit].ScaleRGB = scale;
	}

	inline EGL_Fixed RasterizerState :: GetTextureScaleRGB(size_t unit) const {
		return m_Texture[unit].ScaleRGB;
	}

	inline void RasterizerState :: SetTextureScaleAlpha(size_t unit, EGL_Fixed scale) {
		m_Texture[unit].ScaleAlpha = scale;
	}

	inline EGL_Fixed RasterizerState :: GetTextureScaleAlpha(size_t unit) const {
		return m_Texture[unit].ScaleAlpha;
	}


}

#endif //ndef EGL_RASTERIZER_STATE_H

