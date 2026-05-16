using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace Aetherment;

public class UiColor: IDisposable {
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct Color {
		public byte useTheme;
		public uint index;
		public uint clr;
	}
	
	private byte originalTheme = 0;
	private List<AtkUIColorHolder.UIColor> originalColors = null!;
	private List<Color> appliedColors = null!;
	
	public unsafe UiColor() {
		originalTheme = 0;
		originalColors = [];
		appliedColors = [];
		StoreOriginals();
		
		// UiColorHandlerHook = Aetherment.HookProv.HookFromAddress<UiColorHandlerDelegate>(Aetherment.SigScanner.ScanText("4C 8B 91 ?? ?? ?? ?? 4C 8B D9 49 8B 02"), UiColorHandler);
		// UiColorHandlerHook.Enable();
		
		ReadASHDAndLoadTextureHook = Aetherment.HookProv.HookFromAddress<ReadASHDAndLoadTextureDelegate>(Aetherment.SigScanner.ScanText("E8 ?? ?? ?? ?? 48 8B 84 24 ?? ?? ?? ?? BA ?? ?? ?? ?? 66 44 89 6C 24"), ReadASHDAndLoadTexture);
		ReadASHDAndLoadTextureHook.Enable();
		
		UpdateAtkUIColorHolderHook = Aetherment.HookProv.HookFromAddress<UpdateAtkUIColorHolderDelegate>(Aetherment.SigScanner.ScanText("E8 ?? ?? ?? ?? 84 C0 0F 85 ?? ?? ?? ?? 83 BF ?? ?? ?? ?? ?? 48 89 6C 24"), UpdateAtkUIColorHolder);
		UpdateAtkUIColorHolderHook.Enable();
		
		setUiColors = SetUiColors;
	}
	
	public unsafe void Dispose() {
		// UiColorHandlerHook.Dispose();
		ReadASHDAndLoadTextureHook.Dispose();
		UpdateAtkUIColorHolderHook.Dispose();
		
		RestoreOriginals();
	}
	
	// private unsafe delegate uint UiColorHandlerDelegate(AtkUIColorHolder* c, byte use_theme, uint index);
	// private Hook<UiColorHandlerDelegate> UiColorHandlerHook;
	// private unsafe uint UiColorHandler(AtkUIColorHolder* c, byte use_theme, uint index) {
	// 	for(var j = 0; j < c->UIColors.Count; j++) {
	// 		if(c->UIColors[j].RowId == index) {
	// 			if(use_theme != 0)
	// 				return c->UIColors[j].ThemedColor;
	// 			else
	// 				return c->UIColors[j].Color;
	// 		}
	// 	}
	// 	
	// 	return 0xFFFFFFFF;
	// }
	
	private unsafe delegate void ReadASHDAndLoadTextureDelegate(long a, long b, long c, long d, uint e, byte f);
	private Hook<ReadASHDAndLoadTextureDelegate> ReadASHDAndLoadTextureHook;
	private unsafe void ReadASHDAndLoadTexture(long a, long b, long c, long d, uint e, byte f) {
		var col = AtkStage.Instance()->AtkUIColorHolder;
		var theme = col->ActiveColorThemeType;
		col->ActiveColorThemeType = originalTheme;
		ReadASHDAndLoadTextureHook.Original(a, b, c, d, e, f);
		col->ActiveColorThemeType = theme;
	}
	
	private unsafe delegate byte UpdateAtkUIColorHolderDelegate(long a);
	private Hook<UpdateAtkUIColorHolderDelegate> UpdateAtkUIColorHolderHook;
	private unsafe byte UpdateAtkUIColorHolder(long a) {
		RestoreOriginals();
		var r = UpdateAtkUIColorHolderHook.Original(a);
		StoreOriginals();
		ApplyChanges();
		return r;
	}
	
	private unsafe void StoreOriginals() {
		var c = AtkStage.Instance()->AtkUIColorHolder;
		originalTheme = c->ActiveColorThemeType;
		originalColors = [];
		for(var i = 0; i < c->UIColors.Count; i++) {
			originalColors.Add(c->UIColors[i]);
		}
	}
	
	private unsafe void RestoreOriginals() {
		var c = AtkStage.Instance()->AtkUIColorHolder;
		c->ActiveColorThemeType = originalTheme;
		c->UIColors.Clear();
		foreach(var clr in originalColors) {
			c->UIColors.AddCopy(clr);
		}
	}
	
	private unsafe void ApplyChanges() {
		lock(appliedColors) {
			var c = AtkStage.Instance()->AtkUIColorHolder;
			
			if(c->ActiveColorThemeType == 0)
				c->ActiveColorThemeType = 1;
			
			for(var i = 0; i < c->UIColors.Count; i++) {
				for(var j = 0; j < appliedColors.Count; j++) {
					if(appliedColors[j].index == c->UIColors[i].RowId) {
						if(appliedColors[j].useTheme != 0)
							c->UIColors[i].ThemedColor = appliedColors[j].clr;
						else
							c->UIColors[i].Color = appliedColors[j].clr;
					}
				}
			}
		}
	}
	
	public SetUiColorsDelegate setUiColors;
	public unsafe delegate void SetUiColorsDelegate(Color* array, nint length);
	public unsafe void SetUiColors(Color* array, nint length) {
		RestoreOriginals();
		
		lock(appliedColors) {
			appliedColors.Clear();
			for(int i = 0; i < length; i++) {
				appliedColors.Add(array[i]);
			}
		}
		
		ApplyChanges();
	}
}