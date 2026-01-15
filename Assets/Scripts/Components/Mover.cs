using UnityEngine;
using System;
using System.Collections.Generic;

namespace Components
{
    public enum Ease
    {
        Linear,
        InSine, OutSine, InOutSine,
        InQuad, OutQuad, InOutQuad,
        InCubic, OutCubic, InOutCubic,
        InQuart, OutQuart, InOutQuart,
        InQuint, OutQuint, InOutQuint,
        InExpo, OutExpo, InOutExpo,
        InCirc, OutCirc, InOutCirc,
        InElastic, OutElastic, InOutElastic,
        InBack, OutBack, InOutBack,
        InBounce, OutBounce, InOutBounce,
        Flash, InFlash, OutFlash, InOutFlash // Added Flash dummy
    }

    public enum PathType
    {
        Linear,
        CatmullRom
    }

    public class Mover : MonoBehaviour
    {
        private static Mover _instance;
        public static Mover Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("Mover");
                    _instance = go.AddComponent<Mover>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private List<Tween> tweens = new List<Tween>();

        private void Update()
        {
            for (int i = tweens.Count - 1; i >= 0; i--)
            {
                var tween = tweens[i];
                
                if (tween.isKilled)
                {
                    tweens.RemoveAt(i);
                    continue;
                }
                
                // Check target validity for Transform-bound tweens
                if (tween.targetGameObject != null && tween.target == null) // target was destroyed
                {
                    tweens.RemoveAt(i);
                    continue;
                }

                if (tween.delay > 0)
                {
                    tween.delay -= Time.deltaTime;
                    continue;
                }

                if (!tween.isPlaying) tween.isPlaying = true;

                // Handle Sequence manually? No, sequence updates itself via Update(t) if added to tweens?
                // But Sequence Update logic below assumes it's driven. 
                // Let's rely on standard Tween update.
                
                tween.elapsed += Time.deltaTime;
                float t = tween.duration > 0 ? Mathf.Clamp01(tween.elapsed / tween.duration) : 1f;

                // If it is a sequence, we might not use ease on the sequence itself but linear time?
                // DOTween sequences are linear usually, children have ease.
                // But allow easing on sequence.
                float easedT = EvaluateEase(tween.ease, t);

                tween.Update(easedT);

                if (tween.elapsed >= tween.duration)
                {
                    tween.onComplete?.Invoke();
                    tweens.RemoveAt(i);
                }
            }
        }

        public void AddTween(Tween tween)
        {
            tweens.Add(tween);
        }

        public void KillTweens(object target)
        {
            foreach (var t in tweens)
            {
                if (t.owner == target || (t.target != null && t.target == target))
                {
                    t.isKilled = true;
                }
            }
        }
        
        public bool IsTweening(object target)
        {
             foreach(var tw in tweens) 
             {
                 if (!tw.isKilled && (tw.owner == target || tw.target == target)) return true;
             }
             return false;
        }

        public float EvaluateEase(Ease ease, float t)
        {
            switch (ease)
            {
                case Ease.Linear: return t;
                case Ease.InQuad: return t * t;
                case Ease.OutQuad: return t * (2 - t);
                case Ease.InOutQuad: return t < .5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
                case Ease.InCubic: return t * t * t;
                case Ease.OutCubic: return (--t) * t * t + 1;
                case Ease.InOutCubic: return t < .5f ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;
                case Ease.InBack: { float s = 1.70158f; return t * t * ((s + 1) * t - s); }
                case Ease.OutBack: { float s = 1.70158f; return --t * t * ((s + 1) * t + s) + 1; }
                case Ease.InOutBack: { float s = 1.70158f * 1.525f; return (t *= 2) < 1 ? 0.5f * (t * t * ((s + 1) * t - s)) : 0.5f * ((t -= 2) * t * ((s + 1) * t + s) + 2); }
                default: return t; 
            }
        }
    }

    public static class DOTween
    {
        public static Sequence Sequence()
        {
            var seq = new Sequence();
            Mover.Instance.AddTween(seq); // Auto-play
            return seq;
        }
        
        public static bool IsTweening(object target)
        {
            return Mover.Instance.IsTweening(target);
        }

        public static void Kill(object target)
        {
            Mover.Instance.KillTweens(target);
        }
    }

    public abstract class Tween
    {
        public Transform target; 
        public GameObject targetGameObject; 
        public object owner; 
        
        public float duration;
        public float elapsed;
        public Ease ease = Ease.Linear;
        public Action onComplete;
        public bool isKilled;
        public float delay;
        public bool isPlaying;

        public Tween SetEase(Ease ease) { this.ease = ease; return this; }
        public Tween OnComplete(Action action) { this.onComplete = action; return this; }
        public Tween SetDelay(float delay) { this.delay = delay; return this; }
        public void Play() { this.isPlaying = true; }
        public abstract void Update(float t);
    }

    public class Sequence : Tween
    {
        private Queue<Tween> queue = new Queue<Tween>();
        private Tween current;

        public Sequence()
        {
            this.duration = float.MaxValue; 
        }

        public void Append(Tween t)
        {
            t.isKilled = true; // Stop it from running on main loop
            t.isKilled = false; // Revive for sequence
            queue.Enqueue(t);
        }
        
        public void Join(Tween t) { Append(t); } 

        public override void Update(float t)
        {
            if (current == null)
            {
                if (queue.Count > 0)
                {
                    current = queue.Dequeue();
                    current.elapsed = 0;
                }
                else
                {
                    // Sequence finished
                    this.isKilled = true; 
                    this.onComplete?.Invoke();
                    return;
                }
            }

            if (current != null)
            {
                if (current.delay > 0)
                {
                    current.delay -= Time.deltaTime;
                }
                else
                {
                    current.elapsed += Time.deltaTime;
                    float progress = current.duration > 0 ? Mathf.Clamp01(current.elapsed / current.duration) : 1f;
                    
                    // Use Mover's ease evaluator
                    float easedProgress = Mover.Instance.EvaluateEase(current.ease, progress);
                    current.Update(easedProgress);
                }

                if (current.elapsed >= current.duration && current.delay <= 0)
                {
                    current.onComplete?.Invoke();
                    current = null;
                }
            }
        }
    }

    public class MoveTween : Tween
    {
        Vector3 startPos;
        Vector3 endPos;
        bool isLocal;
        bool xOnly;
        bool zOnly;

        public MoveTween(Transform target, Vector3 endPos, float duration, bool isLocal = false)
        {
            this.target = target;
            this.targetGameObject = target.gameObject;
            this.owner = target;
            this.duration = duration;
            this.isLocal = isLocal;
            this.startPos = isLocal ? target.localPosition : target.position;
            this.endPos = endPos;
        }
        
        public MoveTween(Transform target, float endCoord, float duration, bool isX) 
        {
            this.target = target;
            this.targetGameObject = target.gameObject;
            this.owner = target;
            this.duration = duration;
            this.startPos = target.position;
            this.endPos = target.position;
            if (isX) { this.endPos.x = endCoord; xOnly = true; }
            else { this.endPos.z = endCoord; zOnly = true; }
        }

        public override void Update(float t)
        {
            if (target == null) return;
            
            if (xOnly)
            {
                Vector3 p = target.position;
                p.x = Mathf.LerpUnclamped(startPos.x, endPos.x, t);
                target.position = p;
            }
            else if (zOnly)
            {
                Vector3 p = target.position;
                p.z = Mathf.LerpUnclamped(startPos.z, endPos.z, t);
                target.position = p;
            }
            else
            {
                Vector3 newPos = Vector3.LerpUnclamped(startPos, endPos, t);
                if (isLocal) target.localPosition = newPos;
                else target.position = newPos;
            }
        }
    }

    public class ScaleTween : Tween
    {
        Vector3 startScale;
        Vector3 endScale;

        public ScaleTween(Transform target, Vector3 endScale, float duration)
        {
            this.target = target;
            this.targetGameObject = target.gameObject;
            this.owner = target;
            this.duration = duration;
            this.startScale = target.localScale;
            this.endScale = endScale;
        }

        public override void Update(float t)
        {
             if (target == null) return;
             target.localScale = Vector3.LerpUnclamped(startScale, endScale, t);
        }
    }

    public class RotateTween : Tween
    {
        Quaternion startRot;
        Quaternion endRot;
        bool isLocal;

        public RotateTween(Transform target, Vector3 endEuler, float duration, bool isLocal = false)
        {
            this.target = target;
            this.targetGameObject = target.gameObject;
            this.owner = target;
            this.duration = duration;
            this.startRot = isLocal ? target.localRotation : target.rotation;
            this.endRot = Quaternion.Euler(endEuler);
            this.isLocal = isLocal;
        }

        public override void Update(float t)
        {
            if (target == null) return;
            Quaternion newRot = Quaternion.SlerpUnclamped(startRot, endRot, t);
            if (isLocal) target.localRotation = newRot;
            else target.rotation = newRot;
        }
    }

    public class CanvasGroupFadeTween : Tween
    {
        CanvasGroup canvasGroup;
        float startAlpha;
        float endAlpha;

        public CanvasGroupFadeTween(CanvasGroup canvasGroup, float endAlpha, float duration)
        {
            this.canvasGroup = canvasGroup;
            this.targetGameObject = canvasGroup.gameObject;
            this.owner = canvasGroup;
            this.duration = duration;
            this.startAlpha = canvasGroup.alpha;
            this.endAlpha = endAlpha;
        }

        public override void Update(float t)
        {
            Debug.Log("Lerping alpha: " + t);
            if (canvasGroup == null) return;
            canvasGroup.alpha = Mathf.LerpUnclamped(startAlpha, endAlpha, t);
        }
    }

    public class PathTween : Tween
    {
        Vector3[] path;
        PathType pathType;
        Vector3 startPos;
        List<Vector3> crPath;

        public PathTween(Transform target, Vector3[] path, float duration, PathType pathType)
        {
            this.target = target;
            this.targetGameObject = target.gameObject;
            this.owner = target;
            this.duration = duration;
            this.path = path;
            this.pathType = pathType;
            this.startPos = target.position;
            
            if (pathType == PathType.CatmullRom)
            {
                 List<Vector3> fullPath = new List<Vector3>();
                fullPath.Add(startPos);
                fullPath.AddRange(path);
                 
                 crPath = new List<Vector3>(fullPath);
                 crPath.Insert(0, fullPath[0]);
                 crPath.Add(fullPath[fullPath.Count - 1]);
            }
        }

        public override void Update(float t)
        {
            if (target == null) return;

            if (pathType == PathType.CatmullRom && crPath != null && crPath.Count >= 4)
            {
                 int numSections = crPath.Count - 3;
                 float currPt = t * numSections;
                 int currInt = (int)currPt;
                 if (currInt >= numSections) currInt = numSections - 1;
                 float u = currPt - currInt;
                 
                 Vector3 p0 = crPath[currInt];
                 Vector3 p1 = crPath[currInt + 1];
                 Vector3 p2 = crPath[currInt + 2];
                 Vector3 p3 = crPath[currInt + 3];

                 target.position = 0.5f * (
                    (2f * p1) +
                    (-p0 + p2) * u +
                    (2f * p0 - 5f * p1 + 4f * p2 - p3) * u * u +
                    (-p0 + 3f * p1 - 3f * p2 + p3) * u * u * u
                );
            }
            else
            {
                 Vector3 endP = (path != null && path.Length > 0) ? path[path.Length-1] : startPos;
                  target.position = Vector3.Lerp(startPos, endP, t);
            }
        }
    }

    public static class MoverExtensions
    {
        public static Tween DOMove(this Transform t, Vector3 endValue, float duration)
        {
            var tween = new MoveTween(t, endValue, duration);
            Mover.Instance.AddTween(tween);
            return tween;
        }
        
        public static Tween DOLocalMove(this Transform t, Vector3 endValue, float duration)
        {
            var tween = new MoveTween(t, endValue, duration, true);
            Mover.Instance.AddTween(tween);
            return tween;
        }
        
        public static Tween DOMoveX(this Transform t, float endValue, float duration)
        {
            var tween = new MoveTween(t, endValue, duration, true);
            Mover.Instance.AddTween(tween);
            return tween;
        }
        
        public static Tween DOMoveZ(this Transform t, float endValue, float duration)
        {
            var tween = new MoveTween(t, endValue, duration, false);
            Mover.Instance.AddTween(tween);
            return tween;
        }

        public static Tween DOLocalRotate(this Transform t, Vector3 endValue, float duration)
        {
            var tween = new RotateTween(t, endValue, duration, true);
            Mover.Instance.AddTween(tween);
            return tween;
        }

        public static Tween DOScale(this Transform t, Vector3 endValue, float duration)
        {
            var tween = new ScaleTween(t, endValue, duration);
            Mover.Instance.AddTween(tween);
            return tween;
        }

        public static Tween DOPath(this Transform t, Vector3[] path, float duration, PathType pathType = PathType.Linear)
        {
             var tween = new PathTween(t, path, duration, pathType);
             Mover.Instance.AddTween(tween);
             return tween;
        }

        public static void DOKill(this Transform t)
        {
            Mover.Instance.KillTweens(t);
        }

        public static Tween DOFade(this CanvasGroup cg, float endValue, float duration)
        {
            var tween = new CanvasGroupFadeTween(cg, endValue, duration);
            Mover.Instance.AddTween(tween);
            return tween;
        }
    }
}
