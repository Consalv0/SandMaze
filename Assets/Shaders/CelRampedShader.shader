// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/CelRamped Shader"
{
	Properties
	{
		[Header(Colors)]
    [HideInInspector]
		_Color ("Color", Color) = (0.5,0.5,0.5,1.0)
    [HideInInspector]
    _Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.5
    [HideInInspector]
		_HColor ("Highlight Color", Color) = (0.9,0.9,0.9,1.0)
    [HideInInspector]
		_SColor ("Shadow Color", Color) = (0.1,0.1,0.1,1.0)
    [HideInInspector]
    _Roughness ("Roughness", Range(0, 1)) = 0.5
    [HideInInspector]
    _Specular ("Specular", Range(0, 1)) = 0.49
    [HideInInspector]
    _RampSaturation("Ramp Gray Scale", Float) = 0

  	[Header(Diffuse and Ramp)]
    [HideInInspector]
		_MainTex ("Texture (RGB) Alpha (A)", 2D) = "white" {}
    [HideInInspector]
		_Ramp ("Color Ramp (RGB) Alpha (A)", 2D) = "gray" {}
		[HideInInspector]
		[Toggle(LightRampa)] _LightRamp("Use Light Ramp", Float) = 0

    [Header(Outline)]
    _OutlineColor ("Outline Color", Color) = (0.5,0.5,0.5,1.0)
    _OutlineWidth ("Outline Width", Range (0, 0.01)) = 0.001

    [Header(Rim)]
		_RimColor ("Rim Color", Color) = (0.8,0.8,0.8,0.6)
		_RimStrength ("Rim Strenght", Range(-1,1)) = 0.5

  	// BLENDING STATE
    [HideInInspector]
    _Mode("Render Mode", Float) = 0
    [HideInInspector]
    _SrcBlend ("__src", Float) = 1.0
    [HideInInspector]
    _DstBlend ("__dst", Float) = 0.0
    [HideInInspector]
    _ZWrite ("__zw", Float) = 1.0
	}

CGINCLUDE
#include "UnityCG.cginc"

struct appdata {
	float4 vertex : POSITION;
	float3 normal : NORMAL;
};

struct v2f {
	float4 pos : POSITION;
	float4 color : COLOR;
};

uniform float _OutlineWidth;
uniform float4 _OutlineColor;
fixed4 _Color;
fixed _Mode;
fixed _Cutoff;

v2f vert(appdata v) {
	// just make a copy of incoming vertex data but scaled according to normal direction
	v2f o;
	o.pos = UnityObjectToClipPos(v.vertex);
	if (_OutlineWidth > 0) {
		float3 norm   = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal);
		float2 offset = TransformViewToProjection(norm.xy);

		o.pos.xy += offset * o.pos.z * _OutlineWidth;
	}
	o.color.rgb = _OutlineColor.rgb;
	fixed alpha = _OutlineColor.a * _Color.a;
	if (_Mode == 1)
		if (alpha <= _Cutoff)
			alpha = 0;
		else
			alpha = 1;
	o.color.a = alpha;
	return o;
}
ENDCG

	SubShader {
		Tags { "RenderType" = "Transparent" }

		Pass {
			Name "BASE"
			Cull Back
			Blend [_SrcBlend] [_DstBlend]

			SetTexture [_OutlineColor] {
				ConstantColor (0,0,0,0)
				Combine constant
			}
		}

		Pass {
			Name "OUTLINE"
			Tags { "LightMode" = "Always" }
			Cull Front

			Blend [_SrcBlend] [_DstBlend]

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			half4 frag(v2f i) :COLOR {
				return i.color;
			}
			ENDCG
		}

		Blend [_SrcBlend] [_DstBlend]
    ZWrite [_ZWrite]

		CGPROGRAM
		#pragma multi_compile LIGHTRAMP_OFF LIGHTRAMP_ON
    #pragma surface surf Custom keepalpha fullforwardshadows approxview halfasview
		#pragma target 2.0
		#pragma glsl


		//================================================================
		// VARIABLES

		sampler2D _MainTex;

		fixed4 _RimColor;
		fixed _RimStrength;

		struct Input {
			fixed2 uv_MainTex;
			float3 viewDir;
      float3 worldRefl;
		};

		//================================================================
		// CUSTOM LIGHTING

		//Lighting-related variables
		fixed3 _HColor;
		fixed3 _SColor;
		sampler2D _Ramp;
    float _RampSaturation;
    fixed _Roughness;
    fixed _Specular;

		//Custom SurfaceOutput
		struct SurfaceOutputCustom
		{
			fixed3 Albedo;
			fixed3 Normal;
			fixed3 Emission;
			fixed Specular;
			fixed Alpha;
		};

		inline fixed4 LightingCustom (SurfaceOutputCustom s, fixed3 lightDir, fixed3 viewDir, fixed atten) {

		#ifdef LIGHTRAMP_ON
			fixed ndl = pow(max(0, dot(s.Normal, lightDir) * (1 - _Roughness * 0.5)), .015 + _Specular * 5);
      fixed3 ramp = tex2D(_Ramp, fixed2(ndl, 0.5));
      if (_RampSaturation > 0) {
        ramp = lerp(ramp, dot(ramp, float3(.222, .707, .071)), _RampSaturation);
      }
		#endif

		#ifdef LIGHTRAMP_OFF
			fixed ndl = pow(max(0, dot(s.Normal, lightDir) * (1 - _Roughness)), .015 + _Specular * 2);
			fixed3 ramp;
			if (_Specular < 0.2) {
    		if (ndl > 0.98) {
        	ramp = fixed3(1.0, 1.0, 1.0);
    		} else if (ndl > 0.8) {
        	ramp = _Color.rgb;
    		} else if (ndl > 0.3) {
      		ramp = _Color.rgb * 0.7;
    		} else {
        	ramp = _Color.rgb * 0.2;
    		}
			}	else if (_Specular < 0.5) {
    		if (ndl > 0.78) {
        	ramp = _Color.rgb;
    		} else if (ndl > 0.5) {
        	ramp = _Color.rgb * 0.7;
    		} else {
      		ramp = _Color.rgb * 0.2;
    		}
			}	else if (_Specular < 0.9) {
    		if (ndl > 0.3) {
        	ramp = _Color.rgb * 0.5;
    		} else {
        	ramp = _Color.rgb * 0.2;
    		}
			} else {
        ramp = _Color.rgb * 0.5;
			}
		#endif

		#if !(POINT) && !(SPOT)
			ramp *= atten * s.Alpha;
		#endif

			ramp = lerp(_SColor.r, _HColor.r, ramp);
			fixed4 c;
			if (s.Alpha > 0.1) {
				c.rgb = s.Albedo * _LightColor0.rgb * ramp * (atten * 2) + ramp - 0.35;
			} else {
				c.rgb = s.Albedo * _LightColor0.rgb;
			}
		#ifdef LIGHTRAMP_OFF
			fixed intensity = dot(c.rgb, lerp(0.4, 0.6, _Color));
			c.rgb = lerp(intensity, c.rgb, 1.5);
		#endif

		#if (POINT || SPOT)
			c.rgb *= atten * s.Alpha;
		#endif

			c.a = s.Alpha;
			return c;
		}

		//================================================================
		// SURFACE FUNCTION

		void surf (Input IN, inout SurfaceOutputCustom o) {
			fixed4 mainTex = tex2D(_MainTex, IN.uv_MainTex) * _Color;
      fixed rim = dot(IN.viewDir, o.Normal);
			if (_RimStrength > 0)
      	rim = smoothstep(_RimStrength + .001, _RimStrength, rim);
			else
	      rim = smoothstep(_RimStrength + 1, _RimStrength + 1.001, rim);

			o.Emission = rim * _RimColor.a * _RimColor.rgb;
      o.Albedo = mainTex.rgb;

      fixed alpha = mainTex.a * _Color.a;
      if (_Mode == 1)
        if (alpha <= _Cutoff)
          alpha = 0;
        else
          alpha = 1;
      o.Alpha = alpha;
		}

		ENDCG
	}

	Fallback "Diffuse"
  CustomEditor "CelRampedShaderGUI"
}
