Shader "Stencils/StencilMask"
{
	Properties
	{
		//8=always,3 is equals
		_StencilComp ("Stencil Comparison", Float) = 8
		
		_Stencil ("Stencil ID", Float) = 0
		
		//2=replace, 0=keep, 1=zero
		_StencilOp ("Stencil Operation", Float) = 2
		
		//not using yet
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255
	}
	SubShader
	{
		Tags
		{
			"RenderType" = "Opaque"
			"Queue" = "Geometry"
		}
		ColorMask 0
		ZWrite off
		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp]
		}

		Pass
		{
			CGPROGRAM
				
			#pragma vertex vert
			#pragma fragment frag

			//we only target the hololens (and the unity editor) so take advantage of shader model 5
			#pragma target 5.0
			#pragma only_renderers d3d11
			
			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				return o;
			}

			half4 frag(v2f i) : COLOR
			{
				return half4(1,1,0,1);
			}
			ENDCG
		}
	}
}