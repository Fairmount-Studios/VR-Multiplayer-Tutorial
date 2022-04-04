// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

#ifndef AVATAR_CUSTOM_INCLUDED
#define AVATAR_CUSTOM_INCLUDED

#include "UnityCG.cginc"

// TODO*: Documentation here

#include "AvatarCustomTypes.cginc"

#include "Assets/Oculus/Avatar2/Scripts/Skinning/GpuSkinning/Shaders/OvrDecodeFormats.cginc"

// In an effort to not require third parties to define or multi_compile some specific
// keywords, make some logic use branching instead of preprocessor directives. This
// will be a performance hit, but should hopefully be minimal since
// the branches are based on uniforms. Should only be the cost of the branch and not
// vary across warps/waveforms.
static const int OVR_VERTEX_FETCH_MODE_STRUCT = 0;
static const int OVR_VERTEX_FETCH_MODE_EXTERNAL_BUFFERS  = 1;
static const int OVR_VERTEX_FETCH_MODE_EXTERNAL_TEXTURES  = 2;

bool _OvrHasTangents;
bool _OvrInterpolateAttributes;
float _OvrAttributeInterpolationValue;

int _OvrVertexFetchMode;

//-------------------------------------------------------------------------------------
// Vertex based texture fetching related uniforms and functions.

sampler3D u_AttributeTexture;   // NOTE: This texture can be visualized in the Unity editor, just expand in inspector and manually change "Dimension" to "3D" on top line

int u_AttributeTexelX;
int u_AttributeTexelY;
int u_AttributeTexelW;
int u_AttributeTexelH;

float u_AttributeTexInvSizeW;
float u_AttributeTexInvSizeH;
float u_AttributeTexInvSizeD;

float2 u_AttributeScaleBias;

float3 ovrGetAttributeTexCoord(int attributeRowOffset, uint vertIndex, int numAttributes) {
  // Compute texture coordinate in the attribute texture

  // Compute which row in the texel rect
  // the vertex index is
  int row = vertIndex / u_AttributeTexelW;
  int column = vertIndex % u_AttributeTexelW;

  row = row * numAttributes;

  // Calculate texel centers
  column = u_AttributeTexelX + column;
  row = u_AttributeTexelY + row + attributeRowOffset;

  const float3 coord = float3(float(column), float(row), _OvrAttributeInterpolationValue);
  const float3 invSize = float3(u_AttributeTexInvSizeW, u_AttributeTexInvSizeH, u_AttributeTexInvSizeD);

  // Compute texture coordinate for texel center
  return (2.0 * coord + 1.0) * 0.5 * invSize;
}

float3 ovrGetPositionTexCoord(uint vid, int numAttributes) {
  return ovrGetAttributeTexCoord(0,vid, numAttributes);
}

float3 ovrGetNormalTexCoord(uint vid, int numAttributes) {
  return ovrGetAttributeTexCoord(1, vid, numAttributes);
}

float3 ovrGetTangentTexCoord(uint vid, int numAttributes) {
  return ovrGetAttributeTexCoord(2, vid, numAttributes);
}

float4 OvrGetVertexPositionFromTexture(uint vid, int numAttributes, bool applyOffsetAndBias) {
  float4 pos = tex3Dlod(u_AttributeTexture, float4(ovrGetPositionTexCoord(vid, numAttributes), 0));
  [branch]
  if (applyOffsetAndBias) {
    pos = pos * u_AttributeScaleBias.x + u_AttributeScaleBias.y;
  }
  return pos;
}

float4 OvrGetVertexNormalFromTexture(uint vid, int numAttributes, bool applyOffsetAndBias) {
  float4 norm = tex3Dlod(u_AttributeTexture, float4(ovrGetNormalTexCoord(vid, numAttributes), 0));
  [branch]
  if (applyOffsetAndBias) {
    norm = norm * u_AttributeScaleBias.x + u_AttributeScaleBias.y;
  }
  return norm;
}

float4 OvrGetVertexTangentFromTexture(uint vid, int numAttributes, bool applyOffsetAndBias) {
  float4 tan = tex3Dlod(u_AttributeTexture, float4(ovrGetTangentTexCoord(vid, numAttributes), 0));
  [branch]
  if (applyOffsetAndBias) {
    tan = tan * u_AttributeScaleBias.x + u_AttributeScaleBias.y;
  }
  return tan;
}

//-------------------------------------------------------------------------------------
// "External Buffers" vertex fetch setup
#if SHADER_TARGET >= 35 && (defined(SHADER_API_D3D11) || defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE) || defined(SHADER_API_VULKAN) || (defined(SHADER_API_METAL) && defined(UNITY_COMPILER_HLSLCC)))
#define OVR_SUPPORT_EXTERNAL_BUFFERS
#endif

#if defined(OVR_SUPPORT_EXTERNAL_BUFFERS)
  #include "Assets/Oculus/Avatar2/Scripts/Skinning/GpuSkinning/Shaders/OvrDecodeUtils.cginc"

  ByteAddressBuffer _OvrPositionBuffer; // Bag of uints
  ByteAddressBuffer _OvrFrenetBuffer; // Bag of uints
  float3 _OvrPositionScale;
  float3 _OvrPositionBias;

  int _OvrPositionEncodingPrecision;

  int _OvrPositionsStartAddress;
  int _OvrFrenetStartAddress;

  //-------------------------------------------------------------------------------------
  // Avatar Vertex fetch setup

  float3 OvrGetPositionEntryFromExternalBuffer(uint entryIndex) {
    static const uint STRIDE_32 = 4u * 4u; // 4 32-bit uints for 4 32-bit floats
    static const uint STRIDE_16 = 4u * 2u; // 2 32-bit uints for 4 16 bit unorms or halfs

    float3 position = float3(0.0, 0.0, 0.0);

    [branch] switch(_OvrPositionEncodingPrecision) {
      case OVR_FORMAT_UNORM_16:
        // 2 32-bit uints for 4 16 bit unorms
        position.xyz = OvrUnpackUnorm3x16(
          _OvrPositionBuffer,
          mad(entryIndex, STRIDE_16, _OvrPositionsStartAddress));

        // Apply scale and offset to "de-normalize"
        position.xyz = mad(position.xyz, _OvrPositionScale, _OvrPositionBias);
      break;

      case OVR_FORMAT_FLOAT_32:
        // 4 32-bit uints for 4 32-bit floats
        position.xyz = OvrUnpackFloat3x32(
          _OvrPositionBuffer,
          mad(entryIndex, STRIDE_32, _OvrPositionsStartAddress));
      break;
      case OVR_FORMAT_HALF_16:
        position.xyz = OvrUnpackHalf3x16(
          _OvrPositionBuffer,
          mad(entryIndex, STRIDE_16, _OvrPositionsStartAddress));
      break;
      default:
        // error?
        break;
    }

    return position;
  }

  float4 OvrGetFrenetEntryFromExternalBuffer(uint entryIndex) {
    // Only supporting 10-10-10-2 snorm at the moment
    static const uint STRIDE = 4u; // 1 32-bit uint for 3 10-bit SNorm and 1 2-bit extra
    return OvrUnpackSnorm4x10_10_10_2(
      _OvrFrenetBuffer,
      mad(entryIndex, STRIDE, _OvrFrenetStartAddress));
  }

  float4 OvrGetPositionFromExternalBuffer(uint vertexId) {
    return float4(OvrGetPositionEntryFromExternalBuffer(vertexId), 1.0);
  }

  float3 OvrGetNormalFromExternalBuffer(uint vertexId, bool hasTangents) {
    const uint entryIndex = hasTangents ? vertexId * 2u : vertexId;
    return OvrGetFrenetEntryFromExternalBuffer(entryIndex).xyz;
  }

  float4 OvrGetTangentFromExternalBuffer(uint vertexId) {
    const uint entryIndex = vertexId * 2u + 1u;
    return OvrGetFrenetEntryFromExternalBuffer(entryIndex);
  }

  float4 OvrGetInterpolatedPositionFromExternalBuffer(uint vertexId, float lerpVal) {
    const uint entryIndex = vertexId * 2u; // * 2 due to double buffering
    const float3 pos0 = OvrGetPositionEntryFromExternalBuffer(entryIndex);
    const float3 pos1 = OvrGetPositionEntryFromExternalBuffer(entryIndex + 1u).xyz;

    return float4(lerp(pos0, pos1, lerpVal), 1.0);
  }

  float3 OvrGetInterpolatedNormalFromExternalBuffer(uint vertexId, float lerpVal, bool hasTangents) {
    const uint entryIndex =  vertexId * (hasTangents ? 4u : 2u); // * 2 for double buffer, another * 2 if tangents
    const float3 norm0 = OvrGetFrenetEntryFromExternalBuffer(entryIndex).xyz;
    const float3 norm1 = OvrGetFrenetEntryFromExternalBuffer(entryIndex + 1u).xyz;

    return lerp(norm0, norm1, lerpVal);
  }

  float4 OvrGetInterpolatedTangentFromExternalBuffer(uint vertexId, float lerpVal) {
    const uint entryIndex =  vertexId * 4u + 2u; // * 4 for double buffer + tangent
    const float4 tan0 = OvrGetFrenetEntryFromExternalBuffer(entryIndex);
    const float3 tan1 = OvrGetFrenetEntryFromExternalBuffer(entryIndex + 1u).xyz;

    return float4(lerp(tan0.xyz, tan1, lerpVal), tan0.w);
  }
#endif

// First, define a function which takes explicit data types, then define a macro which expands
// an arbitrary vertex structure definition into the function parameters
OvrVertexData OvrCreateVertexData(
  float4 vPos,
  float3 vNorm,
  float4 vTan,
  uint vertexId)
{
  // Backward compatibility/optimization support if application is ok with additional variants
  // The shader compiler should optimize out branches that are based on static const values
#if defined(OVR_VERTEX_FETCH_VERT_BUFFER)
  static const int fetchMode = OVR_VERTEX_FETCH_MODE_STRUCT;
#elif defined(OVR_VERTEX_FETCH_EXTERNAL_BUFFER) && defined(OVR_SUPPORT_EXTERNAL_BUFFERS)
  static const int fetchMode = OVR_VERTEX_FETCH_MODE_EXTERNAL_BUFFERS;
#elif defined(OVR_VERTEX_FETCH_TEXTURE) || defined(OVR_VERTEX_FETCH_TEXTURE_UNORM)
  static const int fetchMode = OVR_VERTEX_FETCH_MODE_EXTERNAL_TEXTURES;
#else
  const int fetchMode = _OvrVertexFetchMode;
#endif

#if defined(OVR_VERTEX_HAS_TANGENTS)
  static const bool hasTangents = true;
#elif defined(OVR_VERTEX_NO_TANGENTS)
  static const bool hasTangents = false;
#else
  const bool hasTangents = _OvrHasTangents;
#endif

#if defined(OVR_VERTEX_INTERPOLATE_ATTRIBUTES)
  static const bool interpolateAttributes = true;
#elif defined(OVR_VERTEX_DO_NOT_INTERPOLATE_ATTRIBUTES)
  static const bool interpolateAttributes = false;
#else
  const bool interpolateAttributes = _OvrInterpolateAttributes;
#endif

  const float lerpValue = _OvrAttributeInterpolationValue;

  OvrVertexData vertData;
  vertData.vertexId = vertexId;

  // Hope that the compiler branches here. The [branch] attribute here seems to lead to compile
  // probably due to "use of gradient function, such as tex3d"
  if (fetchMode == OVR_VERTEX_FETCH_MODE_EXTERNAL_TEXTURES) {
    int numAttributes = 2;

    // Backwards compatibility with existing keywords.
    // OVR_VERTEX_FETCH_TEXTURE_UNORM means normalized attributes, OVR_VERTEX_FETCH_TEXTURE
    // means not normalized. Neither keyword means that the "fetch mode" was via
    // a property and there is no property for normalized attributes or not. So in that
    // scenario, always apply offset and bias
    #if defined(OVR_VERTEX_FETCH_TEXTURE)
      static const bool applyOffsetAndBias = false;
    #else
      static const bool applyOffsetAndBias = true;
    #endif


    if (hasTangents) {
      numAttributes = 3;
      vertData.tangent = OvrGetVertexTangentFromTexture(vertexId, numAttributes, applyOffsetAndBias);
    } else {
      vertData.tangent = vTan;
    }

    vertData.position = OvrGetVertexPositionFromTexture(vertexId, numAttributes, applyOffsetAndBias);
    vertData.normal = OvrGetVertexNormalFromTexture(vertexId, numAttributes, applyOffsetAndBias);
#ifdef OVR_SUPPORT_EXTERNAL_BUFFERS
  } else if (fetchMode == OVR_VERTEX_FETCH_MODE_EXTERNAL_BUFFERS) {
    [branch]
    if (interpolateAttributes) {
      vertData.position = OvrGetInterpolatedPositionFromExternalBuffer(vertexId, lerpValue);
      vertData.normal = OvrGetInterpolatedNormalFromExternalBuffer(vertexId, lerpValue, hasTangents);

      [branch]
      if (hasTangents) {
        vertData.tangent = OvrGetInterpolatedTangentFromExternalBuffer(vertexId, lerpValue);
      } else {
        vertData.tangent = vTan;
      }
    } else {
      vertData.position = OvrGetPositionFromExternalBuffer(vertexId);
      vertData.normal = OvrGetNormalFromExternalBuffer(vertexId, hasTangents);

      [branch]
      if (hasTangents) {
        vertData.tangent = OvrGetTangentFromExternalBuffer(vertexId);
      } else {
        vertData.tangent = vTan;
      }
    }
#endif
  } else {
    vertData.position = vPos;
    vertData.normal = vNorm;
    vertData.tangent = vTan;
  }

  return vertData;
} // end OvrCreateVertexData

#define OVR_CREATE_VERTEX_DATA(v) \
  OvrCreateVertexData( \
    OVR_GET_VERTEX_POSITION_FIELD(v), \
    OVR_GET_VERTEX_NORMAL_FIELD(v), \
    OVR_GET_VERTEX_TANGENT_FIELD(v), \
    OVR_GET_VERTEX_VERT_ID_FIELD(v))

// Initialization for "required fields" in the vertex input struct for the vertex shader.
// Written as a macro to be expandable in future
#define OVR_INITIALIZE_VERTEX_FIELDS(v)

// Initializes the fields for a defined default vertex structure
void OvrInitializeDefaultAppdata(inout OvrDefaultAppdata v) {
  OVR_INITIALIZE_VERTEX_FIELDS(v);
  UNITY_SETUP_INSTANCE_ID(v);
}

// Initializes the fields for a defined default vertex structure
// and creates the OvrVertexData for the vertex as well as overrides
// applicable fields in OvrDefaultAppdata with fields from OvrVertexData.
// Mainly useful in surface shader vertex functions.
OvrVertexData OvrInitializeDefaultAppdataAndPopulateWithVertexData(inout OvrDefaultAppdata v) {
  OvrInitializeDefaultAppdata(v);
  OvrVertexData vertexData = OVR_CREATE_VERTEX_DATA(v);

  OVR_SET_VERTEX_POSITION_FIELD(v, vertexData.position);
  OVR_SET_VERTEX_NORMAL_FIELD(v, vertexData.normal);
  OVR_SET_VERTEX_TANGENT_FIELD(v, vertexData.tangent);

  return vertexData;
}

#endif // AVATAR_CUSTOM_INCLUDED
