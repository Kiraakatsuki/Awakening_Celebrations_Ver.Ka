// Copyright (c) 2023 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections;
using System;
using System.Collections.Generic;
using AwakeningCelebrations.Interaction;
using Mediapipe.Tasks.Vision.HandLandmarker;
using UnityEngine;
using UnityEngine.Rendering;

namespace Mediapipe.Unity.Sample.HandLandmarkDetection
{
  public class HandLandmarkerRunner : VisionTaskApiRunner<HandLandmarker>
  {
    [SerializeField] private HandLandmarkerResultAnnotationController _handLandmarkerResultAnnotationController;

    private Experimental.TextureFramePool _textureFramePool;
    private bool bootstrapReady;
    private bool pipelineStarted;
    private bool imageSourcePrepared;
    private Canvas previewCanvas;

    public readonly HandLandmarkDetectionConfig config = new HandLandmarkDetectionConfig();

    /// <summary>
    /// Raised for each result produced by this runner. Subscribers must copy any
    /// data they retain because the runner reuses its result buffers.
    /// </summary>
    public event Action<HandLandmarkerResult> ResultReceived;
    public bool IsPipelineStarted { get { return pipelineStarted; } }
    public bool IsImageSourcePrepared { get { return imageSourcePrepared; } }
    public Texture CameraPreviewTexture
    {
      get
      {
        if (!GestureCameraConsentGate.ConsentGranted || !isActiveAndEnabled || !pipelineStarted || !imageSourcePrepared || isPaused)
          return null;
        var source = ImageSourceProvider.ImageSource;
        return source != null && source.isPrepared && source.isPlaying ? source.GetCurrentTexture() : null;
      }
    }
    public long FailedTextureReadbackCount { get; private set; }
    public long SkippedTexturePoolFrameCount { get; private set; }

    public void SetPreviewVisible(bool visible)
    {
      if (previewCanvas == null && screen != null)
      {
        previewCanvas = screen.GetComponentInParent<Canvas>();
      }

      if (previewCanvas != null)
      {
        previewCanvas.enabled = visible;
      }
    }

    protected override IEnumerator Start()
    {
      yield return new WaitUntil(() => GestureCameraConsentGate.ConsentGranted);
      bootstrap = FindBootstrap();
      var bootstrapWaitStarted = Time.realtimeSinceStartupAsDouble;
      while (bootstrap != null && !bootstrap.isFinished && Time.realtimeSinceStartupAsDouble - bootstrapWaitStarted < 60.0)
      {
        yield return null;
      }

      if (bootstrap == null || !bootstrap.isFinished)
      {
        Debug.LogError("MediaPipe Bootstrap did not finish initializing within 60 seconds. Camera and hand tracking were not started.", this);
        yield break;
      }

      bootstrapReady = true;
      Debug.Log("MediaPipe Bootstrap is ready; waiting for camera consent to start hand tracking.", this);
      ApplyCameraConsent(GestureCameraConsentGate.ConsentGranted);
    }

    private void OnEnable()
    {
      if (!GestureCameraConsentGate.Register(this))
      {
        enabled = false;
      }
    }

    private void OnDisable()
    {
      GestureCameraConsentGate.Unregister(this);
    }

    public void ApplyCameraConsent(bool consentGranted)
    {
      if (!consentGranted)
      {
        if (pipelineStarted)
        {
          Stop();
        }
        return;
      }

      if (bootstrapReady && !pipelineStarted && isActiveAndEnabled)
      {
        Play();
      }
    }

    public override void Play()
    {
      if (!bootstrapReady || !GestureCameraConsentGate.ConsentGranted)
        return;

      base.Play();
      pipelineStarted = true;
    }

    public override void Pause()
    {
      if (pipelineStarted && imageSourcePrepared)
      {
        base.Pause();
      }
    }

    public override void Resume()
    {
      if (pipelineStarted && imageSourcePrepared && GestureCameraConsentGate.ConsentGranted)
      {
        base.Resume();
      }
    }

    public override void Stop()
    {
      if (!pipelineStarted)
      {
        return;
      }

      pipelineStarted = false;
      imageSourcePrepared = false;
      base.Stop();
      _textureFramePool?.Dispose();
      _textureFramePool = null;
    }

    protected override IEnumerator Run()
    {
      yield return RunWithFailureHandling(RunPipeline());
    }

    private IEnumerator RunWithFailureHandling(IEnumerator pipeline)
    {
      var stack = new Stack<IEnumerator>();
      stack.Push(pipeline);

      while (stack.Count > 0)
      {
        var currentEnumerator = stack.Peek();
        object currentYield = null;
        bool movedNext;
        Exception pipelineFailure = null;
        try
        {
          movedNext = currentEnumerator.MoveNext();
          if (movedNext)
          {
            currentYield = currentEnumerator.Current;
          }
        }
        catch (Exception exception)
        {
          movedNext = false;
          pipelineFailure = exception;
        }

        if (pipelineFailure != null)
        {
          Debug.LogError($"MediaPipe hand tracking startup or processing failed: {pipelineFailure}", this);
          // Allow StartCoroutine to finish assigning the runner coroutine handle before
          // the base runner's Stop() attempts to stop it.
          yield return null;
          while (stack.Count > 0)
          {
            (stack.Pop() as IDisposable)?.Dispose();
          }
          pipelineStarted = false;
          imageSourcePrepared = false;
          base.Stop();
          _textureFramePool?.Dispose();
          _textureFramePool = null;
          Debug.LogWarning("Hand tracking resources were released after failure. Toggle camera consent off and on to retry.", this);
          yield break;
        }

        if (!movedNext)
        {
          (currentEnumerator as IDisposable)?.Dispose();
          stack.Pop();
          continue;
        }

        if (currentYield is IEnumerator nestedEnumerator)
        {
          stack.Push(nestedEnumerator);
        }
        else
        {
          yield return currentYield;
        }
      }
    }

    private IEnumerator RunPipeline()
    {
      Debug.Log($"Delegate = {config.Delegate}");
      Debug.Log($"Image Read Mode = {config.ImageReadMode}");
      Debug.Log($"Running Mode = {config.RunningMode}");
      Debug.Log($"NumHands = {config.NumHands}");
      Debug.Log($"MinHandDetectionConfidence = {config.MinHandDetectionConfidence}");
      Debug.Log($"MinHandPresenceConfidence = {config.MinHandPresenceConfidence}");
      Debug.Log($"MinTrackingConfidence = {config.MinTrackingConfidence}");

      yield return AssetLoader.PrepareAssetAsync(config.ModelPath);
      Debug.Log($"Hand Landmarker model asset prepared: {config.ModelPath}", this);

      var options = config.GetHandLandmarkerOptions(config.RunningMode == Tasks.Vision.Core.RunningMode.LIVE_STREAM ? OnHandLandmarkDetectionOutput : null);
      taskApi = HandLandmarker.CreateFromOptions(options, GpuManager.GpuResources);
      Debug.Log("Hand Landmarker task initialized.", this);
      var imageSource = ImageSourceProvider.ImageSource;

      yield return imageSource.Play();

      if (!imageSource.isPrepared)
      {
        throw new InvalidOperationException("ImageSource.Play completed without preparing a camera texture.");
      }
      imageSourcePrepared = true;
      Debug.Log($"Camera source ready: {imageSource.sourceName ?? "unknown"}, {imageSource.textureWidth}x{imageSource.textureHeight}, requested {imageSource.frameRate:0.##} FPS, rotation {imageSource.rotation}, mirrored={imageSource.isFrontFacing || imageSource.isHorizontallyFlipped}.", this);

      // Use RGBA32 as the input format.
      // TODO: When using GpuBuffer, MediaPipe assumes that the input format is BGRA, so maybe the following code needs to be fixed.
      _textureFramePool = new Experimental.TextureFramePool(imageSource.textureWidth, imageSource.textureHeight, TextureFormat.RGBA32, 10);

      // NOTE: The screen will be resized later, keeping the aspect ratio.
      // The application scene runs this pipeline headlessly. The sample scene's
      // preview and annotation UI are optional and must not gate camera tracking.
      if (screen != null)
      {
        screen.Initialize(imageSource);
      }
      if (_handLandmarkerResultAnnotationController != null)
      {
        SetupAnnotationController(_handLandmarkerResultAnnotationController, imageSource);
      }

      var transformationOptions = imageSource.GetTransformationOptions();
      var flipHorizontally = transformationOptions.flipHorizontally;
      var flipVertically = transformationOptions.flipVertically;
      var imageProcessingOptions = new Tasks.Vision.Core.ImageProcessingOptions(rotationDegrees: (int)transformationOptions.rotationAngle);

      AsyncGPUReadbackRequest req = default;
      var waitUntilReqDone = new WaitUntil(() => req.done);
      var waitForEndOfFrame = new WaitForEndOfFrame();
      var result = HandLandmarkerResult.Alloc(options.numHands);

      // NOTE: we can share the GL context of the render thread with MediaPipe (for now, only on Android)
      var canUseGpuImage = SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3 && GpuManager.GpuResources != null;
      using var glContext = canUseGpuImage ? GpuManager.GetGlContext() : null;

      while (true)
      {
        if (isPaused)
        {
          yield return new WaitWhile(() => isPaused);
        }

        if (!_textureFramePool.TryGetTextureFrame(out var textureFrame))
        {
          SkippedTexturePoolFrameCount++;
          yield return new WaitForEndOfFrame();
          continue;
        }

        // Build the input Image
        Image image;
        switch (config.ImageReadMode)
        {
          case ImageReadMode.GPU:
            if (!canUseGpuImage)
            {
              throw new System.Exception("ImageReadMode.GPU is not supported");
            }
            textureFrame.ReadTextureOnGPU(imageSource.GetCurrentTexture(), flipHorizontally, flipVertically);
            image = textureFrame.BuildGPUImage(glContext);
            // TODO: Currently we wait here for one frame to make sure the texture is fully copied to the TextureFrame before sending it to MediaPipe.
            // This usually works but is not guaranteed. Find a proper way to do this. See: https://github.com/homuler/MediaPipeUnityPlugin/pull/1311
            yield return waitForEndOfFrame;
            break;
          case ImageReadMode.CPU:
            yield return waitForEndOfFrame;
            textureFrame.ReadTextureOnCPU(imageSource.GetCurrentTexture(), flipHorizontally, flipVertically);
            image = textureFrame.BuildCPUImage();
            textureFrame.Release();
            break;
          case ImageReadMode.CPUAsync:
          default:
            req = textureFrame.ReadTextureAsync(imageSource.GetCurrentTexture(), flipHorizontally, flipVertically);
            yield return waitUntilReqDone;

            if (req.hasError)
            {
              FailedTextureReadbackCount++;
              Debug.LogWarning($"Failed to read texture from the image source");
              // The frame was acquired from the pool before the readback. Return it
              // even on failure so repeated transient readback errors cannot exhaust
              // the finite pool and stop subsequent camera frames from being read.
              textureFrame.Release();
              continue;
            }
            image = textureFrame.BuildCPUImage();
            textureFrame.Release();
            break;
        }

        switch (taskApi.runningMode)
        {
          case Tasks.Vision.Core.RunningMode.IMAGE:
            if (taskApi.TryDetect(image, imageProcessingOptions, ref result))
            {
              _handLandmarkerResultAnnotationController?.DrawNow(result);
              ResultReceived?.Invoke(result);
            }
            else
            {
              _handLandmarkerResultAnnotationController?.DrawNow(default);
              ResultReceived?.Invoke(default);
            }
            break;
          case Tasks.Vision.Core.RunningMode.VIDEO:
            if (taskApi.TryDetectForVideo(image, GetCurrentTimestampMillisec(), imageProcessingOptions, ref result))
            {
              _handLandmarkerResultAnnotationController?.DrawNow(result);
              ResultReceived?.Invoke(result);
            }
            else
            {
              _handLandmarkerResultAnnotationController?.DrawNow(default);
              ResultReceived?.Invoke(default);
            }
            break;
          case Tasks.Vision.Core.RunningMode.LIVE_STREAM:
            taskApi.DetectAsync(image, GetCurrentTimestampMillisec(), imageProcessingOptions);
            break;
        }
      }
    }

    private void OnHandLandmarkDetectionOutput(HandLandmarkerResult result, Image image, long timestamp)
    {
      ResultReceived?.Invoke(result);
      _handLandmarkerResultAnnotationController?.DrawLater(result);
    }
  }
}
