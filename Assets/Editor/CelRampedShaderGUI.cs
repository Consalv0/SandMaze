using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.Remoting.Messaging;
using UnityEngine.EventSystems;
using UnityEditor.UI;
using UnityEngine.UI;

[CanEditMultipleObjects]
public class CelRampedShaderGUI : ShaderGUI {
	public enum BlendMode {
		Opaque,
		Cutout,
		Fade,
		Transparent
	}

	MaterialProperty color;
	MaterialProperty cutoutAlpha;
	MaterialProperty rampSaturation;
	MaterialProperty highlightColor;
	MaterialProperty shadowColor;
	MaterialProperty roughness;
	MaterialProperty specular;

	MaterialProperty textureMap;
	MaterialProperty textureRamp;
	MaterialProperty noLightRamp;

	//MaterialProperty outlineColor;
	//MaterialProperty outlineWeight;

	//MaterialProperty rimColor;
	//MaterialProperty rimMin;
	//MaterialProperty rimMax;

	MaterialProperty modeBlend;
	//MaterialProperty srcBlend;
	//MaterialProperty dstBlend;
	//MaterialProperty zWrite;

	void FindProperties(MaterialProperty[] props) {
		color = FindProperty("_Color", props);
		cutoutAlpha = FindProperty("_Cutoff", props);
		rampSaturation = FindProperty("_RampSaturation", props);
		highlightColor = FindProperty("_HColor", props);
		shadowColor = FindProperty("_SColor", props);
		roughness = FindProperty("_Roughness", props);
		specular = FindProperty("_Specular", props);
		noLightRamp = FindProperty("_LightRamp", props);

		textureMap = FindProperty("_MainTex", props);
		textureRamp = FindProperty("_Ramp", props);

		//outlineColor = FindProperty("_OutlineColor", props);
		//outlineWeight = FindProperty("_OutlineWeight", props);

		//rimColor = FindProperty("_RimColor", props);
		//rimMax = FindProperty("_RimMax", props);
		//rimMin = FindProperty("_RimMin", props);

		modeBlend = FindProperty("_Mode", props);
		//srcBlend = FindProperty("_SrcBlend", props);
		//dstBlend = FindProperty("_DstBlend", props);
		//zWrite = FindProperty("_ZWrite", props);
	}

	public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties) {
		FindProperties(properties);
		EditorGUI.BeginChangeCheck();

		Material material = materialEditor.target as Material;
		BlendMode render = (BlendMode)Enum.ToObject(typeof(BlendMode), Convert.ToInt32(modeBlend.floatValue));
		render = (BlendMode)EditorGUILayout.EnumPopup("Render Mode", (BlendMode)Enum.ToObject(typeof(BlendMode), Convert.ToInt32(modeBlend.floatValue)));
		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Maps And Color", EditorStyles.boldLabel);
		EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("", GUILayout.MinWidth(105));
			materialEditor.TexturePropertyMiniThumbnail(GUILayoutUtility.GetLastRect(), textureMap, "Albedo", "Albedo (RGB) and Transparency (A)");
			GUILayout.FlexibleSpace();
			EditorGUIUtility.labelWidth = 420;
			EditorGUILayout.LabelField("", GUILayout.MinWidth(60));
			color.colorValue = materialEditor.ColorProperty(GUILayoutUtility.GetLastRect(), color, "");
			EditorGUIUtility.labelWidth = 0;
		EditorGUILayout.EndHorizontal();
		EditorGUI.indentLevel += 1;
		if (render == BlendMode.Cutout)
			cutoutAlpha.floatValue = EditorGUILayout.Slider("Cutout Alpha", cutoutAlpha.floatValue, 0, 1);
		if (textureMap.textureValue != null) {
			GUILayout.Space(20);
			EditorGUILayout.LabelField("");
			materialEditor.TextureScaleOffsetProperty(GUILayoutUtility.GetLastRect(), textureMap);
		}
		EditorGUILayout.Space();
		EditorGUI.indentLevel += -1;

		EditorGUILayout.BeginHorizontal();
		try {
			if (!textureRamp.textureValue) throw new UnityException();
			(textureRamp.textureValue as Texture2D).GetPixels();
			EditorGUILayout.LabelField("", GUILayout.MinWidth(105));
			materialEditor.TexturePropertyMiniThumbnail(GUILayoutUtility.GetLastRect(), textureRamp, "Light Ramp", "Albedo (RGB) and Transparency (A)");
			GUILayout.FlexibleSpace();
			EditorGUIUtility.labelWidth = 420;
			EditorGUILayout.LabelField("", GUILayout.MinWidth(60));
			Rect last = GUILayoutUtility.GetLastRect();
			Rect offset = new Rect(last.x - 1.4f, last.y - 1.6f, last.width + 2.4f, last.height + 3);
			var boldtext = new GUIStyle(GUI.skin.textField);
			if (Event.current.type == EventType.Repaint) {
				boldtext.Draw(offset, textureRamp.textureValue, false, false, true, false);
			}
			EditorGUI.DrawPreviewTexture(last, textureRamp.textureValue ?
																	ToGrayscale(textureRamp.textureValue as Texture2D, rampSaturation.floatValue) :
																	new Texture2D(1, 1, TextureFormat.Alpha8, false));
		} catch (UnityException) {
			EditorGUIUtility.labelWidth = 100;
			EditorGUILayout.LabelField("", GUILayout.MaxWidth(0));
			materialEditor.TexturePropertyMiniThumbnail(GUILayoutUtility.GetLastRect(), textureRamp, "Light Ramp", "Albedo (RGB) and Transparency (A)");
			GUILayout.FlexibleSpace();
			EditorGUILayout.HelpBox("No Preview, texture not readable.", MessageType.None);
		}
		EditorGUIUtility.labelWidth = 0;
		EditorGUILayout.EndHorizontal();
		EditorGUI.indentLevel += 1;
		if (textureRamp.textureValue) {
			noLightRamp.floatValue = 0;
			material.EnableKeyword("LIGHTRAMP_ON");
			material.DisableKeyword("LIGHTRAMP_OFF");
		} else {
			noLightRamp.floatValue = 1;
			material.EnableKeyword("LIGHTRAMP_OFF");
			material.DisableKeyword("LIGHTRAMP_ON");
		}
		// noLightRamp.floatValue = EditorGUILayout.Toggle(noLightRamp.floatValue.Equals(0), GUILayout.MinWidth(60)) ? 1 : 0;
		EditorGUILayout.Space();
		if (textureRamp.textureValue != null) {
			rampSaturation.floatValue = EditorGUILayout.Slider("Gray Scale", rampSaturation.floatValue, 0, 1);
			EditorGUILayout.Space();
		}
		EditorGUILayout.BeginHorizontal();
			EditorGUIUtility.labelWidth = 75;
			shadowColor.colorValue = ColorLuminance(EditorGUILayout.ColorField("Shadow", shadowColor.colorValue));
			highlightColor.colorValue = ColorLuminance(EditorGUILayout.ColorField("Highlight", highlightColor.colorValue));
			EditorGUIUtility.labelWidth = 0;
		EditorGUILayout.EndHorizontal();
		roughness.floatValue = EditorGUILayout.Slider("Roughness", roughness.floatValue, 0, 1);
		specular.floatValue = EditorGUILayout.Slider("Specular", specular.floatValue, 0, 1);
		// if (textureRamp.textureValue != null) {
		// 	GUILayout.Space(20);
		// 	EditorGUILayout.LabelField("");
		// 	materialEditor.TextureScaleOffsetProperty(GUILayoutUtility.GetLastRect(), textureRamp);
		// }
		EditorGUILayout.Space();
		EditorGUI.indentLevel += -1;

		modeBlend.floatValue = render.GetHashCode();
		SetMaterialBlendMode(material, render);

		base.OnGUI(materialEditor, properties);
	}

	public static Texture ToGrayscale(Texture2D tex, float amount) {
		if (!tex) return tex;
		try {
			Texture2D texture = new Texture2D(tex.width, tex.height);
			var texColor = tex.GetPixels();
			for (int i = 0; i < texColor.Length; i++) {
				float grayValue = Vector3.Dot(new Vector3(texColor[i].r, texColor[i].g, texColor[i].b),
																			new Vector3(.222f, .707f, .071f));
				texColor[i].r = Mathf.Lerp(texColor[i].r, grayValue, amount);
				texColor[i].g = Mathf.Lerp(texColor[i].g, grayValue, amount);
				texColor[i].b = Mathf.Lerp(texColor[i].b, grayValue, amount);
			}
			texture.SetPixels(texColor);
			texture.filterMode = tex.filterMode;
			return texture;

		} catch (UnityException) {
			return tex;
		}
	}

	public static float Luminance(Color color) {
		Vector3 nColor = new Vector3(color.r, color.g, color.b);
		float lColor = Vector3.Dot(nColor, new Vector3(.222f, .707f, .071f));
		return lColor;
	}

	public static Color ColorLuminance(Color color) {
		Vector3 nColor = new Vector3(color.r, color.g, color.b);
		nColor = Vector3.Dot(nColor, new Vector3(.222f, .707f, .071f)) * Vector3.one;
		color = new Color(nColor.x, nColor.y, nColor.z);
		return color;
	}

	public static void SetMaterialBlendMode(Material material, BlendMode blendMode) {
		switch (blendMode) {
			case BlendMode.Opaque:
				material.SetOverrideTag("RenderType", "Opaque");
				material.SetInt("_SrcBlend", 1);
				material.SetInt("_DstBlend", 0);
				material.SetInt("_ZWrite", 1);
				material.renderQueue = -1;
				break;
			case BlendMode.Cutout:
				material.SetOverrideTag("RenderType", "TransparentCutout");
				material.SetInt("_SrcBlend", 5);
				material.SetInt("_DstBlend", 10);
				material.SetInt("_ZWrite", 1);
				material.renderQueue = 2450;

				break;
			case BlendMode.Fade:
				material.SetOverrideTag("RenderType", "Transparent");
				material.SetInt("_SrcBlend", 5);
				material.SetInt("_DstBlend", 10);
				material.SetInt("_ZWrite", 0);
				material.renderQueue = 3000;
				break;
			case BlendMode.Transparent:
				material.SetOverrideTag("RenderType", "Transparent");
				material.SetInt("_SrcBlend", 1);
				material.SetInt("_DstBlend", 10);
				material.SetInt("_ZWrite", 0);
				material.renderQueue = 3000;
				break;
		}
	}
}
