using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using FFXIVClientStructs.FFXIV.Component.GUI;
using TerraFX.Interop.DirectX;

namespace Aetherment;

public class TexFinder: IDisposable {
	public bool shoulddraw = true;
	private bool hr1 = true;
	private int selected = 0;
	private bool locked = false;
	private bool lastheld = false;
	
	private List<List<Texture>> nodes;
	private Dictionary<uint, ResourceView> texture_cache;
	
	private struct Matrix2x2(float M00, float M01, float M10, float M11)
	{
		public float M00 = M00;
		public float M01 = M01;
		public float M10 = M10;
		public float M11 = M11;
		
		public Vector2 Scale() {
			return new Vector2(
				(float)Math.Sqrt(M00 * M00 + M10 * M10),
				(float)Math.Sqrt(M01 * M01 + M11 * M11)
			);
		}
		
		public Matrix2x2 Inverse() {
			// var s = 1 / (M00 * M11 - M10 * M01);
			// return new(M11 * s, -M01 * s, -M10 * s, M00 * s);
			return new(M00, M10, M01, M11);
		}
		
		public Matrix2x2 Normalize() {
			var sx = (float)Math.Sqrt(M00 * M00 + M10 * M10);
			var sy = (float)Math.Sqrt(M01 * M01 + M11 * M11);
			return new(M00 / sx, M01 / sy, M10 / sx, M11 / sy);
		}
		
		public static Vector2 operator *(Vector2 v, Matrix2x2 m) {
			return new Vector2(
				v.X * m.M00 + v.Y * m.M01,
				v.X * m.M10 + v.Y * m.M11
			);
		}
	}
	
	private struct Texture {
		public string path;
		public uint texture;
		public Matrix2x2 screen_matrix;
		public Vector2 screen_pos;
		public Vector2 size;
		public Vector2 part_pos;
		public Vector2 part_size;
		public Vector2 texture_size;
		public float ninegrid_topoffset;
		public float ninegrid_bottomoffset;
		public float ninegrid_leftoffset;
		public float ninegrid_rightoffset;
	}
	
	private unsafe class ResourceView {
		public ID3D11ShaderResourceView* view;
		public ImTextureID handle;
		public double last_used;
	}
	
	public unsafe TexFinder() {
		nodes = new();
		texture_cache = new();
	}
	
	public unsafe void Dispose() {
		lock(texture_cache) {
			foreach(var (_, resource) in texture_cache) {
				resource.view->Release();
			}
			
			texture_cache.Clear();
		}
	}
	
	public void OpenConf() {
		shoulddraw = !shoulddraw;
	}
	
	public void Draw() {
		// texture cache maintance
		lock(texture_cache) {
			var time = (double)Stopwatch.GetTimestamp() / Stopwatch.Frequency;
			var remove = new List<uint>();
			foreach(var (key, resource) in texture_cache) {
				if(time - resource.last_used > 5.0) {
					remove.Add(key);
				}
			}
			
			foreach(var key in remove) {
				texture_cache.Remove(key);
			}
		}
		
		if(!shoulddraw)
			return;
		
		// update last used
		foreach(var nodes2 in nodes) {
			foreach(var node in nodes2) {
				if(texture_cache.TryGetValue(node.texture, out var view)) {
					view.last_used = (double)Stopwatch.GetTimestamp() / Stopwatch.Frequency;
				}
			}
		}
		
		// ui
		if(ImGui.GetIO().KeyCtrl && ImGui.GetIO().KeyShift) {
			if(!lastheld) {
				locked = !locked;
				lastheld = true;
			}
		} else
			lastheld = false;
		
		// window
		ImGui.SetNextWindowSize(new Vector2(500, 400), ImGuiCond.FirstUseEver);
		ImGui.Begin("Texture Finder", ref shoulddraw);
		
		var draw = ImGui.GetWindowDrawList();
		var draw2 = ImGui.GetForegroundDrawList();
		var padding = ImGui.GetStyle().FramePadding;
		
		ImGui.Text($"[Debug] Texture cache size: {texture_cache.Count}");
		
		ImGui.Checkbox("High res", ref hr1);
		ImGui.Text($"{(locked ? "Locked" : "Unlocked")} (Shift + Ctrl to toggle)");
		
		// texture selection
		if(nodes.Count > 0) {
			if(ImGui.Button("<"))
				selected -= 1;
			ImGui.SameLine();
			ImGui.SetNextItemWidth(ImGui.CalcTextSize($"{nodes.Count - 1}").X + padding.X * 2);
			ImGui.InputInt("##selected", ref selected, 0, 0);
			ImGui.SameLine();
			ImGui.Text($"/");
			ImGui.SameLine();
			ImGui.Text($"{nodes.Count - 1}");
			ImGui.SameLine();
			if(ImGui.Button(">"))
				selected += 1;
			selected = Math.Clamp(selected, 0, nodes.Count - 1);
		}
		
		// screen part highlight
		for(var i = 0; i < nodes.Count; i++) {
			var textures = nodes[i];
			for(var j = 0; j < textures.Count; j++) {
				var texture = textures[j];
				var p1 = texture.screen_pos + Vector2.Zero * texture.screen_matrix;
				var p2 = texture.screen_pos + new Vector2(texture.size.X, 0) * texture.screen_matrix;
				var p3 = texture.screen_pos + texture.size * texture.screen_matrix;
				var p4 = texture.screen_pos + new Vector2(0, texture.size.Y) * texture.screen_matrix;
				draw2.AddLine(p1, p2, 0x900000FF);
				draw2.AddLine(p2, p3, 0x900000FF);
				draw2.AddLine(p3, p4, 0x900000FF);
				draw2.AddLine(p4, p1, 0x900000FF);
			}
		}
		
		// textures
		if(nodes.Count > 0) {
			var textures = nodes[selected];
			var size = textures.Count == 9 ? ImGui.GetContentRegionAvail() / 3 - padding * 2 : ImGui.GetContentRegionAvail();
			for(var i = 0; i < textures.Count; i++) {
				var texture = textures[i];
				
				if(i % 3 != 0)
					ImGui.SameLine();
				
				ImGui.BeginChild($"9grid{i}", size);
				if(ImGui.Button(texture.path, new Vector2(0, ImGui.GetFontSize() + padding.Y * 2)))
					ImGui.SetClipboardText(texture.path);
				if(ImGui.IsItemHovered())
					ImGui.SetTooltip("Copy to clipboard");
				
				// texture preview
				var s = ImGui.GetContentRegionAvail();
				var ratio = Math.Min(s.X / texture.texture_size.X, s.Y / texture.texture_size.Y);
				var pos = ImGui.GetCursorScreenPos();
				var imgsize = new Vector2(texture.texture_size.X, texture.texture_size.Y) * ratio;
				if(texture_cache.TryGetValue(texture.texture, out var tex))
					draw.AddImage(tex.handle, pos, pos + imgsize);
				
				// part highlight
				var scale = hr1 ? 2 : 1;
				var (u, v) = (texture.part_pos.X * scale, texture.part_pos.Y * scale);
				var (w, h) = (texture.part_size.X * scale * ratio, texture.part_size.Y * scale * ratio);
				
				pos += new Vector2(u, v) * ratio;
				draw.AddRect(pos, pos + new Vector2(w, h), 0xFF00FF00);
				
				if(textures.Count == 1) {
					var rs = scale * ratio;
					draw.AddLine(pos + new Vector2(0, texture.ninegrid_topoffset * rs), pos + new Vector2(w - 1, texture.ninegrid_topoffset * rs), 0xFF00FF00);
					draw.AddLine(pos + new Vector2(0, h - texture.ninegrid_bottomoffset * rs), pos + new Vector2(w - 1, h - texture.ninegrid_bottomoffset * rs), 0xFF00FF00);
					draw.AddLine(pos + new Vector2(texture.ninegrid_leftoffset * rs, 0), pos + new Vector2(texture.ninegrid_leftoffset * rs, h - 1), 0xFF00FF00);
					draw.AddLine(pos + new Vector2(w - texture.ninegrid_rightoffset * rs, 0), pos + new Vector2(w - texture.ninegrid_rightoffset * rs, h - 1), 0xFF00FF00);
				}
				
				ImGui.EndChild();
				
				// screen part highlight part 2, we do it again since multiple elements could overrlay hiding the green selected one
				var p1 = texture.screen_pos + Vector2.Zero * texture.screen_matrix;
				var p2 = texture.screen_pos + new Vector2(texture.size.X, 0) * texture.screen_matrix;
				var p3 = texture.screen_pos + texture.size * texture.screen_matrix;
				var p4 = texture.screen_pos + new Vector2(0, texture.size.Y) * texture.screen_matrix;
				draw2.AddLine(p1, p2, 0xFF00FF00);
				draw2.AddLine(p2, p3, 0xFF00FF00);
				draw2.AddLine(p3, p4, 0xFF00FF00);
				draw2.AddLine(p4, p1, 0xFF00FF00);
			}
		}
		
		if(!locked)
			UpdateNodeCache();
		
		ImGui.End();
	}
	
	private unsafe void UpdateNodeCache() {
		nodes.Clear();
		var cursor = ImGui.GetMousePos();
		var man = AtkStage.Instance()->RaptureAtkUnitManager;
		for(var i = man->DepthLayers.Length - 1; i >= 0; i--) {
			var layer = man->DepthLayers[i];
			for(var j = 0; j < layer.Count; j++) {
				var addon = layer.Entries[j].Value;
				if(!addon->IsVisible)
					continue;
				
				var root = addon->RootNode;
				CheckNode(root, cursor);
			}
		}
	}
	
	private unsafe void CheckNode(AtkResNode* node, Vector2 cursor, int depth = 1) {
		if(node == null)
			return;
		
		// ImGui.Text($"{new string('\t', depth)}{(nint)node:X} {node->Type} {nodes.Count}");
		
		if(!node->IsVisible())
			return;
		
		var child = node->ChildNode;
		if((ushort)node->Type >= 1000)
			child = ((AtkComponentNode*)node)->Component->UldManager.RootNode;
		
		var children = new List<nint>();
		while(child != null) {
			// CheckNode(child, cursor, depth + 1);
			children.Add((nint)child);
			child = child->PrevSiblingNode;
		}
		
		for(var i = children.Count - 1; i >= 0; i--) {
			CheckNode((AtkResNode*)children[i], cursor, depth + 1);
		}
		
		var screen_matrix = new Matrix2x2(node->Transform.M11, node->Transform.M12, node->Transform.M21, node->Transform.M22).Inverse();
		var screen_pos = new Vector2(node->ScreenX, node->ScreenY);
		cursor = (cursor - screen_pos) * screen_matrix.Normalize().Inverse() / screen_matrix.Scale();
		
		if(cursor.X < 0 || cursor.Y < 0 || cursor.X > node->Width || cursor.Y > node->Height)
			return;
		
		switch(node->Type) {
			case NodeType.Image: {
				var n = (AtkImageNode*)node;
				var part = n->PartsList->Parts[n->PartId];
				var tex = part.UldAsset->AtkTexture;
				var texture = tex.TextureType == TextureType.Resource ?
					tex.Resource->KernelTextureObject :
					tex.KernelTexture;
				
				if(texture != null) {
					var tex_key = CacheTexture(tex);
					
					nodes.Add([new() {
						path = tex.Resource->TexFileResourceHandle->ResourceHandle.FileName.ToString(),
						texture = tex_key,
						screen_matrix = screen_matrix,
						screen_pos = screen_pos,
						size = new(node->Width, node->Height),
						part_pos = new(part.U, part.V),
						part_size = new(part.Width, part.Height),
						texture_size = new(texture->ActualWidth, texture->ActualHeight),
					}]);
				}
				
				break;
			}
			
			case NodeType.NineGrid: {
				var n = (AtkNineGridNode*)node;
				var partcount = (n->PartsTypeRenderType & 1) == 1 ? 9 : 1;
				
				var textures = new List<Texture>();
				for(var i = 0; i < partcount; i++) {
					var part = n->PartsList->Parts[n->PartId + i];
					var tex = part.UldAsset->AtkTexture;
					var texture = tex.TextureType == TextureType.Resource ?
						tex.Resource->KernelTextureObject :
						tex.KernelTexture;
					
					if(texture != null) {
						var tex_key = CacheTexture(tex);
						
						textures.Add(new() {
							path = tex.Resource->TexFileResourceHandle->ResourceHandle.FileName.ToString(),
							texture = tex_key,
							screen_matrix = screen_matrix,
							screen_pos = screen_pos,
							size = new(node->Width, node->Height),
							part_pos = new(part.U, part.V),
							part_size = new(part.Width, part.Height),
							texture_size = new(texture->ActualWidth, texture->ActualHeight),
							ninegrid_topoffset = n->TopOffset,
							ninegrid_bottomoffset = n->BottomOffset,
							ninegrid_leftoffset = n->LeftOffset,
							ninegrid_rightoffset = n->RightOffset,
						});
					}
				}
				
				nodes.Add(textures);
				
				break;
			}
			
			default:
				break;
		}
	}
	
	private unsafe uint CacheTexture(AtkTexture tex) {
		var key = tex.TextureType == TextureType.Resource ?
			(uint)tex.Resource->TexFileResourceHandle->ResourceHandle.FileName.ToString().GetHashCode() :
			1;
		
		if(texture_cache.ContainsKey(key))
			return key;
		
		// clone the resource so that we dont crash if the game decides to clean it up while we are locked
		var device = (ID3D11Device*)Aetherment.Interface.UiBuilder.DeviceHandle;
		ID3D11DeviceContext* context;
		device->GetImmediateContext(&context);
		
		var texture = tex.TextureType == TextureType.Resource ?
			tex.Resource->KernelTextureObject :
			tex.KernelTexture;
		
		var og_view = (ID3D11ShaderResourceView*)texture->D3D11ShaderResourceView;
		ID3D11Resource* og_resource;
		og_view->GetResource(&og_resource);
		var guid = typeof(ID3D11Texture2D).GUID;
		ID3D11Texture2D* og_texture;
		og_resource->QueryInterface(&guid, (void**)&og_texture);
		D3D11_TEXTURE2D_DESC desc;
		og_texture->GetDesc(&desc);
		
		ID3D11Texture2D* new_texture;
		device->CreateTexture2D(&desc, null, &new_texture);
		context->CopyResource((ID3D11Resource*)new_texture, og_resource);
		
		ID3D11ShaderResourceView* new_view;
		device->CreateShaderResourceView((ID3D11Resource*)new_texture, null, &new_view);
		
		lock(texture_cache) {
			texture_cache.Add(key, new() {
				view = new_view,
				handle = new ImTextureID(new_view),
				last_used = (double)Stopwatch.GetTimestamp() / Stopwatch.Frequency,
			});
		}
		
		return key;
	}
}