/*
 * 
 * OldTVShaderFrag.shader 
 * Use this file to create a awesome old tv effect.
 * 
 * Version 1.00
 * 
 * Developed by Vortex Game Studios LTDA ME. (http://www.vortexstudios.com)
 * Authors:		Alexandre Ribeiro de Sa (@themonkeytail)
 * 
 */


Shader "Vortex Game Studios/OLD TV Shader" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_TvMask ("TV Border Mask", 2D) = "white" {}
		_TvNoise ("TV Noise Texture", 2D) = "white" {}
		_TvStatic ("TV Static Texture", 2D) = "white" {}
		
		_tD ("TV Tube Distrortion", Range(-1.0, 1.0)) = 0.05
		_cA ("Chromatic Aberration", Range(-1.0, 1.0)) = 0.002
		
		_lM ("Scanline Magnetude", Range(0.0, 1.0)) = 0.5
		_lS ("Scanline Size", float) = 640
		
		_nM ("Noise Magnetude", Range(-1.0, 1.0)) = 0.5
		_sM ("Static Magnetude", Range(-1.0, 1.0)) = 0.5
		
		_Noise ("Just a noise", Range(-1.0, 1.0)) = 0.5
	}
	
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		pass { 
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _TvMask;
			sampler2D _TvNoise;
			sampler2D _TvStatic;
			
			half _Noise;
			half _tD;
			half _cA;
			
			half _lM;
			half _lS;
					
			half _nM;
			half _sM;
			
			half _Tv;

			struct v2f {
			    float4  pos : SV_POSITION;
			    float2  uv : TEXCOORD0;
			};

			float4 _MainTex_ST;
			
			half2 rD(half2 c, half2 p) { 
			    half di = _tD; 
			    half2 cc = p-0.5; 
			    half dt = dot(cc,cc)*di; 
			    
			    half3 tS = tex2D(_TvNoise,half2(c.y,_Noise)*5);
			    c.x += tS.y*_nM*(_Tv-0.8);
				return c*(p+cc*(1.0+dt)*dt)/p; 
			} 

			half4 sL(sampler2D c, half2 tC) { 
			    // aberração cromática 
				half4 iC;
				half2 tO = half2(_cA, 0);
				
				// Possivel pre-calcular isso numa textura? Algo semelhante ao normal map?
				iC = half4(tex2D(c,tC-tO).r*0.8,tex2D(c,tC).g*0.8,tex2D(c,tC+tO).b*0.8,1);
	
				iC = (iC*(1.0-_sM))+(half4(tex2D(_TvStatic,(tC+_Noise)*10.0))*_sM);
							
				// scanline
				iC = iC*(1.2*half4(_lM,_lM,_lM,1)*abs(sin(tC.y*_lS))+0.75); 
				if(iC.r>1.0)iC.r = 1.0;
	
				// Saida
				return iC * _Tv;
			}

			v2f vert(appdata_base v) {
    			v2f o;
    			o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
    			o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
    			return o;
			}

			fixed4 frag(v2f i) : COLOR {
				_Tv = tex2D(_TvMask,i.uv).x+1;
				half4 c = sL(_MainTex,rD(i.uv,i.uv));
				return c;
			}
			ENDCG
		}
	} 
	FallBack "Diffuse"
}
