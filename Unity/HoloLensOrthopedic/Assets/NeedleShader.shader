// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/VertColorWithShadow"
{
	Properties
	{
		_MainColor("Main Color", COLOR) = (0,0,0,1)
		_ShadowColor("Shadow Color", COLOR) = (1,0,0,1)
	}

		SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 100

		Pass
	{
		Tags{ "LightMode" = "ForwardBase" }

		CGPROGRAM
#pragma multi_compile_fwdbase
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"
#include "AutoLight.cginc"

		float4 _MainColor;
	float4 _ShadowColor;

	struct appdata
	{
		float4 vertex : POSITION;
	};

	struct v2f
	{
		float4 pos : SV_POSITION;
		float4 mainColor : COLOR;
		LIGHTING_COORDS(1,2)
	};

	v2f vert(appdata v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.mainColor = _MainColor;

		TRANSFER_VERTEX_TO_FRAGMENT(o);

		return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{
		fixed atten = 1.0 - LIGHT_ATTENUATION(i);
	fixed3 shadowColor = atten * _ShadowColor.rgb;
	fixed4 finalColor = i.mainColor + fixed4(shadowColor, 1.0);

	return finalColor;
	}
		ENDCG
	}

		// Pass to render object as a shadow caster
		Pass
	{
		Name "ShadowCaster"
		Tags{ "LightMode" = "ShadowCaster" }

		ZWrite On ZTest LEqual Cull Off

		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_shadowcaster
#include "UnityCG.cginc"

		struct v2f {
		V2F_SHADOW_CASTER;
	};

	v2f vert(appdata_base v)
	{
		v2f o;
		TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
			return o;
	}

	float4 frag(v2f i) : SV_Target
	{
		return float4(1.0, 1.0, 1.0, 1.0);
	}
		ENDCG
	}
	}
}