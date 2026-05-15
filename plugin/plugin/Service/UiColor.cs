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
	
	private List<AtkUIColorHolder.UIColor> originalColors;
	// private Dictionary<(bool, uint), uint> colors;
	
	public unsafe UiColor() {
		originalColors = new();
		var c = AtkStage.Instance()->AtkUIColorHolder;
		for(var i = 0; i < c->UIColors.Count; i++) {
			originalColors.Add(c->UIColors[i]);
		}
		
		// dont use a delegate since a lot of elements dont actually use it???
		// colors = new();
		
		// UiColorHandlerHook = Aetherment.HookProv.HookFromAddress<UiColorHandlerDelegate>(Aetherment.SigScanner.ScanText("4C 8B 91 ?? ?? ?? ?? 4C 8B D9 49 8B 02"), UiColorHandler);
		// UiColorHandlerHook.Enable();
		
		setUiColors = SetUiColors;
	}
	
	public unsafe void Dispose() {
		ResetColors();
		
		// UiColorHandlerHook.Dispose();
	}
	
	// private unsafe delegate uint UiColorHandlerDelegate(nint self, byte use_theme, uint index);
	// private Hook<UiColorHandlerDelegate> UiColorHandlerHook;
	// private uint UiColorHandler(nint self, byte use_theme, uint index) {
	// 	lock(colors) {
	// 		if(colors.TryGetValue((use_theme != 0, index), out var color)) {
	// 			return color;
	// 		}
	// 	}
	// 	
	// 	return UiColorHandlerHook.Original(self, use_theme, index);
	// }
	
	private unsafe void ResetColors() {
		var c = AtkStage.Instance()->AtkUIColorHolder;
		c->UIColors.Clear();
		for(var i = 0; i < originalColors.Count; i++) {
			c->UIColors.AddCopy(originalColors[i]);
		}
	}
	
	public SetUiColorsDelegate setUiColors;
	public unsafe delegate void SetUiColorsDelegate(Color* array, nint length);
	public unsafe void SetUiColors(Color* array, nint length) {
		// lock(colors) {
		// 	colors.Clear();
		// 	for(int i = 0; i < length; i++) {
		// 		var color = array[i];
		// 		colors.Add((color.useTheme != 0, color.index), color.clr);
		// 	}
		// }
		
		ResetColors();
		var c = AtkStage.Instance()->AtkUIColorHolder;
		for(int i = 0; i < length; i++) {
			var color = array[i];
			for(var j = 0; j < c->UIColors.Count; j++) {
				if(c->UIColors[j].RowId == color.index) {
					c->UIColors[j].Color = color.clr;
					
					// if(color.useTheme != 0) {
					// 	c->UIColors[j].ThemedColor = color.clr;
					// } else {
					// 	c->UIColors[j].Color = color.clr;
					// }
				}
			}
		}
	}
}