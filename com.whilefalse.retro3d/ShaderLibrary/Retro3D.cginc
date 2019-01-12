
float _VertexPrecision;
float4x4 _ViewmodelViewMatrix;
float4x4 _ViewmodelObjMatrix;

#ifdef VIEWMODEL_ON
#define RETRO_View2Clip(pos) mul(_ViewmodelViewMatrix, float4( ##pos, 1.0 ))
#define RETRO_Obj2Clip(pos) mul(_ViewmodelObjMatrix, float4( ##pos, 1.0 ))
#else
#define RETRO_View2Clip(pos) UnityViewToClipPos( ##pos )
#define RETRO_Obj2Clip(pos) UnityObjectToClipPos( ##pos )
#endif
