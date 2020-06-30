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

// Retrive the texure data for one of three possible Hexes
// touching the vertex described by v, corresonding to one
// of three possible indices specified by uv3Index [0/1/2].
float4 GetHexData(appdata_full v, int uv3Index) {
	float2 uv;
	
	// Becaue a texture is being sampled, the UV coordinates
	// need to be aligned with the centers of pixels.
	// Therefore all row major indices are offset by
	// 0.5f.

	// Multiplying by _hexData_TexelSize.x is equivalent
	// to dividing by the width of the map texture.
	// The result is a number in the form of Z.U,
	// 	Example: 3.4, 3rd row, U coordiante = 4
	uv.x = 
		(v.texcoord2[uv3Index] + 0.5) *
		_hexData_TexelSize.x;

	// Extract the row by flooring the number in the form
	// Z.U to obtain Z.
	float row = floor(uv.x);

	// Extract the U coordinate by subtracting the row
	// component from Z.U to obtain U.
	uv.x -= row;

	// Obtain the V coordinate by dividing the
	// row by the texture height (multiplying by
	// _hexData_TexelSize.y)
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