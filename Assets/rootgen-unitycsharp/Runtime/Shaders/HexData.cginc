sampler2D _hexData;
float4 _hexData_TexelSize;

float4 FilterHexData(float4 data) 
{
	#if defined(HEX_MAP_EDIT_MODE)
		data.xy = 1;
	#endif

	return data;
}

float4 GetHexData(appdata_full v, int index) 
{
	float2 uv;
	uv.x = (v.texcoord2[index] + 0.5) * _hexData_TexelSize.x;
	float row = floor(uv.x);
	uv.x -= row;
	uv.y = (row + 0.5) * _hexData_TexelSize.y;
	float4 data = tex2Dlod(_hexData, float4(uv, 0, 0));
	data.w *= 255;
	return FilterHexData(data);
}

float4 GetHexData(float2 hexDataCoordinates) {
	float2 uv = hexDataCoordinates + 0.5;
	uv.x *= _hexData_TexelSize.x;
	uv.y *= _hexData_TexelSize.y;
	return FilterHexData(tex2Dlod(_hexData, float4(uv, 0, 0)));
}