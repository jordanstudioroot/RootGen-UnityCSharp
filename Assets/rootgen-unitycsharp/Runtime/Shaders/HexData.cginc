// 2D texture globally decalred in HexMapShaderData.cs 
sampler2D _hexData;

// Vector4 declared globally in HexMapShaderData.cs where each coordiante
// represents one of the following:
// 	x: 1f / number of columns in the hex map (the texture width)
//  y: 1f / number of rows in the hex map (the texture height)
//  z: number of columns in the hex map
//  w: number of rows in the hex map
float4 _hexData_TexelSize;

// Utility function for filtering hex data for visibility if edit mode is
// enabled, setting the sampled hex data to be completely visible if edit
// mode is enabled.
float4 FilterHexData(float4 data) {
	#if defined(HEX_MAP_EDIT_MODE)
		data.xy = 1;
	#endif

	return data;
}

// Retrieve the texture data for a hex at the specified vertex represented
// by appdata_full corresponding to the index stored in the vertices' uv3
// coordinate.
float4 GetHexData(appdata_full v, int index) {
	float2 uv;
	
	// Three row-major array indices in the form of (column * Columns) + row
	// for all 3 possible hexes touching v are contained in v.texcoord2.
	//
	// The U coordinate for the hex with the specified vector3 index [index]
	// is obtained by multiplying the corresponding row major index (with an
	// offset of .5) with the width of the texture and flooring the result.
	// Multiplying the row (also with an offset of .5) by the texture height
	// produces the v coordiante. 
	uv.x = (v.texcoord2[index] + 0.5) * _hexData_TexelSize.x;
	float row = floor(uv.x);
	uv.x -= row;
	uv.y = (row + 0.5) * _hexData_TexelSize.y;

	// After obtaining the texture uv coordinates, the global 2d texture
	// for the hex map is sampled at the cooresponding coordinate, and
	// the result stored in a float4.
	float4 data = tex2Dlod(_hexData, float4(uv, 0, 0));

	// Because the w coordinate of _hexData contains the terrain type stored
	// as a byte value originally, it must be converted back to a byte vale.
	data.w *= 255;

	// Filter the hex data for visibility in edit mode and return it.
	return FilterHexData(data);
}

// Simplified function for retrieving hex data when the texture coordinates
// are already known.
float4 GetHexData(float2 hexDataCoordinates) {
	float2 uv = hexDataCoordinates + 0.5;
	uv.x *= _hexData_TexelSize.x;
	uv.y *= _hexData_TexelSize.y;
	return FilterHexData(tex2Dlod(_hexData, float4(uv, 0, 0)));
}