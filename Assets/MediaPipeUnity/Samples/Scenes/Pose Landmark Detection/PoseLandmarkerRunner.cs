// Copyright (c) 2023 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections;
using ExerciseGame.Core.Pose;
using Mediapipe.Tasks.Vision.PoseLandmarker;
using UnityEngine;
using UnityEngine.Rendering;

namespace Mediapipe.Unity.Sample.PoseLandmarkDetection
{
    public class PoseLandmarkerRunner : VisionTaskApiRunner<PoseLandmarker>
    {
        [SerializeField]
        private PoseLandmarkerResultAnnotationController _poseLandmarkerResultAnnotationController;

        [SerializeField]
        private PoseDataProvider _poseDataProvider;

        private Experimental.TextureFramePool _textureFramePool;

        public readonly PoseLandmarkDetectionConfig config = new PoseLandmarkDetectionConfig();

        public override void Stop()
        {
            base.Stop();

            _textureFramePool?.Dispose();
            _textureFramePool = null;

            // Clear our application-level pose data when stopping.
            if (_poseDataProvider != null)
            {
                _poseDataProvider.ClearPose(0);
            }
        }

        protected override IEnumerator Run()
        {
            Debug.Log($"Delegate = {config.Delegate}");
            Debug.Log($"Image Read Mode = {config.ImageReadMode}");
            Debug.Log($"Model = {config.ModelName}");
            Debug.Log($"Running Mode = {config.RunningMode}");
            Debug.Log($"NumPoses = {config.NumPoses}");
            Debug.Log($"MinPoseDetectionConfidence = {config.MinPoseDetectionConfidence}");
            Debug.Log($"MinPosePresenceConfidence = {config.MinPosePresenceConfidence}");
            Debug.Log($"MinTrackingConfidence = {config.MinTrackingConfidence}");
            Debug.Log($"OutputSegmentationMasks = {config.OutputSegmentationMasks}");

            // ------------------------------------------------------------
            // Load model
            // ------------------------------------------------------------

            yield return AssetLoader.PrepareAssetAsync(config.ModelPath);

            var options = config.GetPoseLandmarkerOptions(
              config.RunningMode == Tasks.Vision.Core.RunningMode.LIVE_STREAM
                ? OnPoseLandmarkDetectionOutput
                : null
            );

            taskApi = PoseLandmarker.CreateFromOptions(
              options,
              GpuManager.GpuResources
            );

            // ------------------------------------------------------------
            // Start camera
            // ------------------------------------------------------------

            var imageSource = ImageSourceProvider.ImageSource;

            yield return imageSource.Play();

            if (!imageSource.isPrepared)
            {
                Logger.LogError(
                  TAG,
                  "Failed to start ImageSource, exiting..."
                );

                yield break;
            }

            // ------------------------------------------------------------
            // Texture frame pool
            // ------------------------------------------------------------

            _textureFramePool = new Experimental.TextureFramePool(
              imageSource.textureWidth,
              imageSource.textureHeight,
              TextureFormat.RGBA32,
              10
            );

            // ------------------------------------------------------------
            // Initialize screen
            // ------------------------------------------------------------

            screen.Initialize(imageSource);

            // ------------------------------------------------------------
            // Initialize skeleton annotation
            // ------------------------------------------------------------

            SetupAnnotationController(
              _poseLandmarkerResultAnnotationController,
              imageSource
            );

            _poseLandmarkerResultAnnotationController.InitScreen(
              imageSource.textureWidth,
              imageSource.textureHeight
            );

            // ------------------------------------------------------------
            // Image transformation
            // ------------------------------------------------------------

            var transformationOptions =
              imageSource.GetTransformationOptions();

            var flipHorizontally =
              transformationOptions.flipHorizontally;

            var flipVertically =
              transformationOptions.flipVertically;

            // Always use rotation = 0.
            var imageProcessingOptions =
              new Tasks.Vision.Core.ImageProcessingOptions(
                rotationDegrees: 0
              );

            // ------------------------------------------------------------
            // Async GPU readback
            // ------------------------------------------------------------

            AsyncGPUReadbackRequest req = default;

            var waitUntilReqDone =
              new WaitUntil(() => req.done);

            var waitForEndOfFrame =
              new WaitForEndOfFrame();

            // ------------------------------------------------------------
            // Result container
            // ------------------------------------------------------------

            var result = PoseLandmarkerResult.Alloc(
              options.numPoses,
              options.outputSegmentationMasks
            );

            // ------------------------------------------------------------
            // GPU support
            // ------------------------------------------------------------

            var canUseGpuImage =
              SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3 &&
              GpuManager.GpuResources != null;

            using var glContext =
              canUseGpuImage
                ? GpuManager.GetGlContext()
                : null;

            // ------------------------------------------------------------
            // Main processing loop
            // ------------------------------------------------------------

            while (true)
            {
                if (isPaused)
                {
                    yield return new WaitWhile(() => isPaused);
                }

                if (!_textureFramePool.TryGetTextureFrame(out var textureFrame))
                {
                    yield return new WaitForEndOfFrame();
                    continue;
                }

                // ----------------------------------------------------------
                // Build MediaPipe input image
                // ----------------------------------------------------------

                Image image;

                switch (config.ImageReadMode)
                {
                    case ImageReadMode.GPU:

                        if (!canUseGpuImage)
                        {
                            throw new System.Exception(
                              "ImageReadMode.GPU is not supported"
                            );
                        }

                        textureFrame.ReadTextureOnGPU(
                          imageSource.GetCurrentTexture(),
                          flipHorizontally,
                          flipVertically
                        );

                        image = textureFrame.BuildGPUImage(glContext);

                        yield return waitForEndOfFrame;

                        break;

                    case ImageReadMode.CPU:

                        yield return waitForEndOfFrame;

                        textureFrame.ReadTextureOnCPU(
                          imageSource.GetCurrentTexture(),
                          flipHorizontally,
                          flipVertically
                        );

                        image = textureFrame.BuildCPUImage();

                        textureFrame.Release();

                        break;

                    case ImageReadMode.CPUAsync:

                    default:

                        req = textureFrame.ReadTextureAsync(
                          imageSource.GetCurrentTexture(),
                          flipHorizontally,
                          flipVertically
                        );

                        yield return waitUntilReqDone;

                        if (req.hasError)
                        {
                            Debug.LogWarning(
                              "Failed to read texture from the image source"
                            );

                            continue;
                        }

                        image = textureFrame.BuildCPUImage();

                        textureFrame.Release();

                        break;
                }

                // ----------------------------------------------------------
                // Run MediaPipe
                // ----------------------------------------------------------

                switch (taskApi.runningMode)
                {
                    case Tasks.Vision.Core.RunningMode.IMAGE:

                        if (taskApi.TryDetect(
                          image,
                          imageProcessingOptions,
                          ref result))
                        {
                            _poseLandmarkerResultAnnotationController
                              .DrawNow(result);

                            UpdatePoseData(
                              result,
                              GetCurrentTimestampMillisec()
                            );
                        }
                        else
                        {
                            _poseLandmarkerResultAnnotationController
                              .DrawNow(default);

                            ClearPoseData(
                              GetCurrentTimestampMillisec()
                            );
                        }

                        DisposeAllMasks(result);

                        break;


                    case Tasks.Vision.Core.RunningMode.VIDEO:

                        if (taskApi.TryDetectForVideo(
                          image,
                          GetCurrentTimestampMillisec(),
                          imageProcessingOptions,
                          ref result))
                        {
                            _poseLandmarkerResultAnnotationController
                              .DrawNow(result);

                            UpdatePoseData(
                              result,
                              GetCurrentTimestampMillisec()
                            );
                        }
                        else
                        {
                            _poseLandmarkerResultAnnotationController
                              .DrawNow(default);

                            ClearPoseData(
                              GetCurrentTimestampMillisec()
                            );
                        }

                        DisposeAllMasks(result);

                        break;


                    case Tasks.Vision.Core.RunningMode.LIVE_STREAM:

                        taskApi.DetectAsync(
                          image,
                          GetCurrentTimestampMillisec(),
                          imageProcessingOptions
                        );

                        break;
                }
            }
        }

        // =================================================================
        // MediaPipe → Our Pose System
        // =================================================================

        private void OnPoseLandmarkDetectionOutput(
          PoseLandmarkerResult result,
          Image image,
          long timestamp)
        {
            // Keep the original MediaPipe skeleton rendering.
            _poseLandmarkerResultAnnotationController
              .DrawLater(result);

            // Copy the pose into our own system.
            UpdatePoseData(
              result,
              timestamp
            );

            DisposeAllMasks(result);
        }


        private void UpdatePoseData(
          PoseLandmarkerResult result,
          long timestamp)
        {
            if (_poseDataProvider == null)
            {
                return;
            }

            // No pose detected.
            if (result.poseLandmarks == null ||
                result.poseLandmarks.Count == 0)
            {
                ClearPoseData(timestamp);
                return;
            }

            // We currently support one person.
            var pose = result.poseLandmarks[0];

            if (pose.landmarks == null ||
                pose.landmarks.Count == 0)
            {
                ClearPoseData(timestamp);
                return;
            }

            int landmarkCount = pose.landmarks.Count;

            var landmarks =
              new PoseLandmarkData[landmarkCount];

            for (int i = 0; i < landmarkCount; i++)
            {
                var landmark = pose.landmarks[i];

                landmarks[i] = new PoseLandmarkData(
                  new Vector3(
                    landmark.x,
                    landmark.y,
                    landmark.z
                  ),
                   landmark.visibility ?? 0f,
                   landmark.presence ?? 0f
                );
            }

            _poseDataProvider.UpdatePose(
              landmarks,
              timestamp
            );
        }


        private void ClearPoseData(long timestamp)
        {
            if (_poseDataProvider == null)
            {
                return;
            }

            _poseDataProvider.ClearPose(timestamp);
        }


        // =================================================================
        // Segmentation mask cleanup
        // =================================================================

        private void DisposeAllMasks(
          PoseLandmarkerResult result)
        {
            if (result.segmentationMasks != null)
            {
                foreach (var mask in result.segmentationMasks)
                {
                    mask.Dispose();
                }
            }
        }
    }
}