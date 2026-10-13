using UnityEngine;
using Mediapipe.Unity.Sample.HandLandmarkDetection;

namespace AwakeningCelebrations.Interaction
{
    /// <summary>
    /// Session-only camera permission shared by the application and the single
    /// MediaPipe Hand Landmarker runner. Camera capture is denied by default.
    /// </summary>
    public static class GestureCameraConsentGate
    {
        private static bool consentGranted;
        private static HandLandmarkerRunner registeredRunner;

        public static bool ConsentGranted { get { return consentGranted; } }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSession()
        {
            consentGranted = false;
            registeredRunner = null;
        }

        public static void SetConsent(bool granted)
        {
            consentGranted = granted;
            if (registeredRunner != null)
                registeredRunner.ApplyCameraConsent(granted);
        }

        public static bool Register(HandLandmarkerRunner runner)
        {
            if (registeredRunner != null && registeredRunner != runner)
            {
                Debug.LogError("Only one HandLandmarkerRunner may own camera capture at a time.", runner);
                return false;
            }

            registeredRunner = runner;
            runner.ApplyCameraConsent(consentGranted);
            return true;
        }

        public static void Unregister(HandLandmarkerRunner runner)
        {
            if (registeredRunner != runner)
                return;

            registeredRunner.ApplyCameraConsent(false);
            registeredRunner = null;
        }
    }
}
