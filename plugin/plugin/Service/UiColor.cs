using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.STD;

namespace Aetherment;

// TODO: can cause a crash when we clear the UIColors if the game touches it at the same time (i think?)
public class UiColor: IDisposable {
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct Color {
		public byte useTheme;
		public uint index;
		public uint clr;
	}
	
	private List<Color> appliedColors = null!;
	private StdVector<AtkUIColorHolder.UIColor, FFXIVClientStructs.STD.Helper.IStaticMemorySpace.Default> vanillaColors;
	private StdVector<AtkUIColorHolder.UIColor, FFXIVClientStructs.STD.Helper.IStaticMemorySpace.Default> moddedColors;
	
	public unsafe UiColor() {
		appliedColors = [];
		StdVector<AtkUIColorHolder.UIColor, FFXIVClientStructs.STD.Helper.IStaticMemorySpace.Default>.ConstructDefaultInPlace(out vanillaColors);
		StdVector<AtkUIColorHolder.UIColor, FFXIVClientStructs.STD.Helper.IStaticMemorySpace.Default>.ConstructDefaultInPlace(out moddedColors);
		StoreVanilla();
		
		Aetherment.Logger.Debug($"{(nint)AtkStage.Instance()->AtkUIColorHolder:X}");
		
		UiColorHandlerHook = Aetherment.HookProv.HookFromAddress<UiColorHandlerDelegate>(Aetherment.SigScanner.ScanText("4C 8B 91 ?? ?? ?? ?? 4C 8B D9 49 8B 02"), UiColorHandler);
		UiColorHandlerHook.Enable();
		
		UiColorHandler2Hook = Aetherment.HookProv.HookFromAddress<UiColorHandler2Delegate>(Aetherment.SigScanner.ScanText("48 8B 05 ?? ?? ?? ?? 44 0F B6 91"), UiColorHandler2);
		UiColorHandler2Hook.Enable();
		
		ColorThingiesHook = Aetherment.HookProv.HookFromAddress<ColorThingiesDelegate>(Aetherment.SigScanner.ScanText("40 53 44 0F B7 52 ?? 4C 8B D9"), ColorThingies);
		ColorThingiesHook.Enable();
		
		UpdateAtkUIColorHolderHook = Aetherment.HookProv.HookFromAddress<UpdateAtkUIColorHolderDelegate>(Aetherment.SigScanner.ScanText("E8 ?? ?? ?? ?? 84 C0 0F 85 ?? ?? ?? ?? 83 BF ?? ?? ?? ?? ?? 48 89 6C 24"), UpdateAtkUIColorHolder);
		UpdateAtkUIColorHolderHook.Enable();
		
		setUiColors = SetUiColors;
	}
	
	public unsafe void Dispose() {
		UiColorHandlerHook.Dispose();
		UiColorHandler2Hook.Dispose();
		ColorThingiesHook.Dispose();
		UpdateAtkUIColorHolderHook.Dispose();
		
		RestoreVanilla();
	}
	
	private static unsafe uint ResolveColor(AtkUIColorHolder* colorholder, byte use_theme, uint index) {
		for(var i = 0; i < colorholder->UIColors.Count; i++) {
			if(colorholder->UIColors[i].RowId == index) {
				if(use_theme != 0)
					return colorholder->UIColors[i].ThemedColor;
				else
					return colorholder->UIColors[i].Color;
			}
		}
		
		return 0xFFFFFFFF;
	}
	
	private unsafe delegate uint UiColorHandlerDelegate(AtkUIColorHolder* c, byte use_theme, uint index);
	private Hook<UiColorHandlerDelegate> UiColorHandlerHook;
	private unsafe uint UiColorHandler(AtkUIColorHolder* colorholder, byte use_theme, uint index) {
		return ResolveColor(colorholder, use_theme, index);
	}
	
	private unsafe delegate uint UiColorHandler2Delegate(AtkStage* atkstage, uint index);
	private Hook<UiColorHandler2Delegate> UiColorHandler2Hook;
	private unsafe uint UiColorHandler2(AtkStage* atkstage, uint index) {
		return ResolveColor(AtkStage.Instance()->AtkUIColorHolder, 1, index);
	}
	
	private unsafe delegate void ColorThingiesDelegate(long a, long b, long c, long d, byte e);
	private Hook<ColorThingiesDelegate> ColorThingiesHook;
	private unsafe void ColorThingies(long a, long b, long c, long d, byte e) {
		var colorholder = AtkStage.Instance()->AtkUIColorHolder;
		var theme = colorholder->ActiveColorThemeType;
		colorholder->ActiveColorThemeType = 1;
		ColorThingiesHook.Original(a, b, c, d, e);
		colorholder->ActiveColorThemeType = theme;
	}
	
	private unsafe delegate byte UpdateAtkUIColorHolderDelegate(long a);
	private Hook<UpdateAtkUIColorHolderDelegate> UpdateAtkUIColorHolderHook;
	private unsafe byte UpdateAtkUIColorHolder(long a) {
		// RestoreVanilla();
		var r = UpdateAtkUIColorHolderHook.Original(a);
		if(r == 1) {
			StoreVanilla();
			StoreModded();
			ApplyModded();
		}
		return r;
	}
	
	private unsafe void StoreVanilla() {
		var colorholder = AtkStage.Instance()->AtkUIColorHolder;
		var len = colorholder->UIColors.LongCount;
		var size = sizeof(AtkUIColorHolder.UIColor) * len;
		vanillaColors.EnsureCapacity(len);
		Buffer.MemoryCopy(colorholder->UIColors.First, vanillaColors.First, size, size);
		vanillaColors.Last = vanillaColors.First + len;
	}
	
	private unsafe void RestoreVanilla() {
		var colorholder = AtkStage.Instance()->AtkUIColorHolder;
		var len = vanillaColors.LongCount;
		var size = sizeof(AtkUIColorHolder.UIColor) * len;
		colorholder->UIColors.EnsureCapacity(len);
		Buffer.MemoryCopy(vanillaColors.First, colorholder->UIColors.First, size, size);
		colorholder->UIColors.Last = colorholder->UIColors.First + len;
	}
	
	private unsafe void StoreModded() {
		var len = vanillaColors.LongCount;
		var size = sizeof(AtkUIColorHolder.UIColor) * len;
		moddedColors.EnsureCapacity(len);
		Buffer.MemoryCopy(vanillaColors.First, moddedColors.First, size, size);
		moddedColors.Last = moddedColors.First + len;
		
		if(AtkStage.Instance()->AtkUIColorHolder->ActiveColorThemeType == 0) {
			for(var i = 0; i < moddedColors.Count; i++) {
				moddedColors[i].ThemedColor = moddedColors[i].Color;
			}
		}
		
		lock(appliedColors) {
			for(var i = 0; i < moddedColors.Count; i++) {
				for(var j = 0; j < appliedColors.Count; j++) {
					if(appliedColors[j].index == moddedColors[i].RowId) {
						if(appliedColors[j].useTheme != 0)
							moddedColors[i].ThemedColor = appliedColors[j].clr;
						else
							moddedColors[i].Color = appliedColors[j].clr;
					}
				}
			}
		}
	}
	
	private unsafe void ApplyModded() {
		var colorholder = AtkStage.Instance()->AtkUIColorHolder;
		var len = moddedColors.LongCount;
		var size = sizeof(AtkUIColorHolder.UIColor) * len;
		colorholder->UIColors.EnsureCapacity(len);
		Buffer.MemoryCopy(moddedColors.First, colorholder->UIColors.First, size, size);
		colorholder->UIColors.Last = colorholder->UIColors.First + len;
	}
	
	public SetUiColorsDelegate setUiColors;
	public unsafe delegate void SetUiColorsDelegate(Color* array, nint length);
	public unsafe void SetUiColors(Color* array, nint length) {
		lock(appliedColors) {
			appliedColors.Clear();
			for(int i = 0; i < length; i++) {
				appliedColors.Add(array[i]);
			}
		}
		
		StoreModded();
		ApplyModded();
	}
}