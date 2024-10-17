// ref: https://github.com/yukpiz/unity-webgl-movie/blob/master/Assets/Plugins/WebGLMovieTexture.jslib
// ref: https://www.patrykgalach.com/2020/04/27/unity-js-plugin/
let plugin = {
    MindARFace_GetVideoWidth: function () {
      return Module.MindARFace.GetVideoWidth();
    },
  
    MindARFace_GetVideoHeight: function () {
      return Module.MindARFace.GetVideoHeight();
    },
  
    MindARFace_TextureUpdate: function (tex) {
      Module.MindARFace.TextureUpdate(tex);
    },

    MindARFace_SetCallbacks: function (onStartARPtr, onCameraConfigChangePtr, onUpdatePtr) {
      Module.MindARFace.onStartARPtr = onStartARPtr;
      Module.MindARFace.onCameraConfigChangePtr = onCameraConfigChangePtr;
      Module.MindARFace.onUpdatePtr = onUpdatePtr;
    },
  
    MindARFace_StartAR: function () {
      Module.MindARFace.StartAR();
    },

    MindARFace_StopAR: function () {    
      Module.MindARFace.StopAR();
    },
    
    MindARFace_IsRunning: function () {    
      return Module.MindARFace.IsRunning();
    },  
  
    MindARFace_SetIsFacingUser: function(value) {
      Module.MindARFace.SetIsFacingUser(value);
    },
  
    MindARFace_SetFilterMinCF: function(value) {
      Module.MindARFace.SetFilterMinCF(value);
    },
  
    MindARFace_SetFilterBeta: function(value) {
      Module.MindARFace.SetFilterBeta(value);
    },
  
    MindARFace_GetCameraParamsPtr: function() {
      return Module.MindARFace.GetCameraParamsPtr();
    },

    MindARFace_GetFaceMeshMatrixPtr: function() {
      return Module.MindARFace.GetFaceMeshMatrixPtr();
    },

    MindARFace_GetFaceMeshVerticesPtr: function() {
      return Module.MindARFace.GetFaceMeshVerticesPtr();
    }
  }
  
  // Add plugin functions
  mergeInto(LibraryManager.library, plugin);
  