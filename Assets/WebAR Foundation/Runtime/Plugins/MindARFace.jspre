Module["MindARFace"] = {
    this: this, 
  
    video: null,
  
    onStartARPtr: null,

    onCameraConfigChangePtr: null,
  
    onUpdatePtr: null,

    isRunning: false,

    filterMinCF: 0.001, 

    filterBeta: 10,

    controller: null,

    isFacingUser: true,
    
    memory: {},
  
    texPtr: null,
  
    invisibleMatrix: [0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],

    SetFilterMinCF: function(filterMinCF) {
      this.filterMinCF = filterMinCF;
    },
  
    SetFilterBeta: function(filterBeta) {
      this.filterBeta = filterBeta;
    },

    SetIsFacingUser: function(isFacingUser) {
      this.isFacingUser = isFacingUser;
    },
  
    GetExternalJS: function () {
      return new Promise((resolve) => {
        const SCRIPT_URL = "https://cdn.jsdelivr.net/npm/mind-ar@1.2.3/dist/mindar-face.prod.js";
        // console.log("get external js...", SCRIPT_URL);
  
        let script = document.querySelector(`script[src="${SCRIPT_URL}"]`);
        if (!script) {
          script = document.createElement("script");
          script.src = SCRIPT_URL;
          script.type = "module";
          script.async = true;
          document.body.appendChild(script);
  
          script.addEventListener("load", () => {
            // console.log("window mindar: ", window.MINDAR);
            resolve();
          });
        } else {
          resolve();
        }
      });
    },
  
    TextureUpdate: function (tex) {
      this.texPtr = tex;
    },
  
    StartAR: async function () {
      await this.GetExternalJS();
      await this.StartVideo();
      await this.StartMindAR();
      this.isRunning = true;
  
      if (this.onStartARPtr) {
        Module.dynCall_v(this.onStartARPtr);
      }    
    },

    StopAR: function() {
      if (!this.isRunning) return;
  
      this.controller.stopProcessVideo();
  
      const tracks = this.video.srcObject.getTracks();
      tracks.forEach(function (track) {
        track.stop();
      });
      this.video.remove();
    },

    IsRunning: function() {
      return this.isRunning;
    },
  
    StartVideo: function () {
      //navigator.mediaDevices.getUserMedia({audio: false, video: {facingMode: FacingModes[this.facingMode]}})
      return new Promise((resolve) => {
        navigator.mediaDevices.getUserMedia({audio: false, video: {
          facingMode: (this.isFacingUser ? 'user' : 'environment')
        }})
          .then(stream => {
              this.video = document.createElement('video');
              this.video.playsInline = true;
              this.video.srcObject = stream;
              this.video.addEventListener('loadedmetadata', () => {
                  this.video.play();
                  this.video.width = this.video.videoWidth;
                  this.video.height = this.video.videoHeight;
                  resolve();
              });
          });
      });
    },
  
    StartMindAR: async function() {
      const input = this.video;
  
      const controller = new window.MINDAR.FACE.Controller({
        filterMinCF: this.filterMinCF, // OneEuroFilter, min cutoff frequency
        filterBeta: this.filterBeta, // OneEuroFilter, beta
      })

      const NUM_LANDMARKS = 468;
        
      controller.onUpdate = ({hasFace, estimateResult}) => {
        if (hasFace) {
            const {metricLandmarks, faceMatrix, faceScale} = estimateResult;

            const faceMeshVertices = [];
            for (let i = 0; i < NUM_LANDMARKS; i++) {
              faceMeshVertices.push(metricLandmarks[i][0]);
              faceMeshVertices.push(metricLandmarks[i][1]);
              faceMeshVertices.push(metricLandmarks[i][2]);
            }

            // transpose
            const faceM = [
              faceMatrix[0], faceMatrix[4], faceMatrix[8], faceMatrix[12],
              faceMatrix[1], faceMatrix[5], faceMatrix[9], faceMatrix[13],
              faceMatrix[2], faceMatrix[6], faceMatrix[10], faceMatrix[14],
              faceMatrix[3], faceMatrix[7], faceMatrix[11], faceMatrix[15],
            ]

            HEAPF32.set(faceM, this.memory.faceMeshMatrixPtr >> 2);
            HEAPF32.set(faceMeshVertices, this.memory.faceMeshLVerticesPtr >> 2);
        } else {
            HEAPF32.set(this.invisibleMatrix, this.memory.faceMeshMatrixPtr >> 2);
        }
        if (this.onUpdatePtr) {
            Module.dynCall_vi(this.onUpdatePtr, hasFace?1:0);
        }
      };

      this.controller = controller;

      const flipFace = this.isFacingUser;
      await controller.setup(flipFace);

      this.memory.cameraParamsPtr = _malloc(4 * 4);
      this.memory.faceMeshMatrixPtr = _malloc(16 * 4);
      this.memory.faceMeshLVerticesPtr = _malloc(NUM_LANDMARKS * 3 * 4);
  
      const drawWebcamTexture = () => {
        if (this.video === null) return;      
  
        GLctx.bindTexture(GLctx.TEXTURE_2D, GL.textures[this.texPtr]);
        GLctx.pixelStorei(GLctx.UNPACK_FLIP_Y_WEBGL, true);
        // GLctx.texImage2D(GLctx.TEXTURE_2D, 0, GLctx.RGBA, GLctx.RGBA, GLctx.UNSIGNED_BYTE, this.video);
        GLctx.texSubImage2D(GLctx.TEXTURE_2D, 0, 0, 0, GLctx.RGBA, GLctx.UNSIGNED_BYTE, this.video);
        GLctx.pixelStorei(GLctx.UNPACK_FLIP_Y_WEBGL, false);
  
        window.requestAnimationFrame(drawWebcamTexture);
      }
      window.requestAnimationFrame(drawWebcamTexture);


      const onResize = () => {
        this.video.width = this.video.videoWidth;
        this.video.height = this.video.videoHeight;

        controller.onInputResized(this.video);
        const {fov, aspect, near, far} = controller.getCameraParams();

        //console.log("camera params: ", fov, aspect, near, far);
        // console.log("this.memory.cameraParamsPtr: " + this.memory.cameraParamsPtr);
        HEAPF32.set([fov, aspect, near, far], this.memory.cameraParamsPtr >> 2);

        if (this.onCameraConfigChangePtr) {
          Module.dynCall_v(this.onCameraConfigChangePtr);
        }
      }
      window.addEventListener('resize', onResize);

      onResize();

      controller.processVideo(input);
    },
  
    GetCameraParamsPtr() {
      return this.memory.cameraParamsPtr;
    },

    GetFaceMeshMatrixPtr() {
      return this.memory.faceMeshMatrixPtr;
    },

    GetFaceMeshVerticesPtr() {
      return this.memory.faceMeshLVerticesPtr;
    },
  
    GetVideoWidth: function () {
      return this.video.width;
    },
  
    GetVideoHeight: function () {
      return this.video.height;
    }
  }
  