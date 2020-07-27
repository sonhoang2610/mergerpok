using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ParadoxNotion.Design;

using DG.Tweening;

namespace FlowCanvas.Nodes.DOTween{

	[Category("Functions/Extensions/DOTween")]
	public class TweenPosition : LatentActionNode<Transform, Vector3, float, Ease, bool>{
		private Tween tween;
		public override IEnumerator Invoke(Transform transform, Vector3 position, float duration, Ease easeType, bool relative){
			var active = true;
			tween = transform.DOMove(position, duration);
			tween.SetEase(easeType);
            if (relative) tween.SetRelative();
			tween.OnComplete( ()=> { active = false; } );
			while (active) yield return null;
		}
		public override void OnBreak(){ tween.Kill(); }
	}

	[Category("Functions/Extensions/DOTween")]
	public class TweenRotation : LatentActionNode<Transform, Vector3, float, Ease, bool>{
		private Tween tween;
		public override IEnumerator Invoke(Transform transform, Vector3 rotation, float duration, Ease easeType, bool relative){
			var active = true;
			tween = transform.DORotate(rotation, duration);
			tween.SetEase(easeType);
			if (relative) tween.SetRelative();
			tween.OnComplete( ()=> { active = false; } );
			while (active) yield return null;
		}
		public override void OnBreak(){ tween.Kill(); }
	}

	[Category("Functions/Extensions/DOTween")]
	public class TweenScale : LatentActionNode<Transform, Vector3, float, Ease, bool>{
		private Tween tween;
		public override IEnumerator Invoke(Transform transform, Vector3 scale, float duration, Ease easeType, bool relative){
			var active = true;
			tween = transform.DOScale(scale, duration);
			tween.SetEase(easeType);
			if (relative) tween.SetRelative();
			tween.OnComplete( ()=> { active = false; } );
			while (active) yield return null;
		}
		public override void OnBreak(){ tween.Kill(); }
	}

	[Category("Functions/Extensions/DOTween")]
	public class TweenLookAt : LatentActionNode<Transform, Vector3, float, Ease, AxisConstraint>{
		private Tween tween;
		public override IEnumerator Invoke(Transform transform, Vector3 target, float duration, Ease easeType, AxisConstraint axisConstraint){
			var active = true;
			tween = transform.DOLookAt(target, duration, axisConstraint);
			tween.SetEase(easeType);
			tween.OnComplete( ()=> { active = false; } );
			while (active) yield return null;			
		}
		public override void OnBreak(){ tween.Kill(); }
	}

	[Category("Functions/Extensions/DOTween")]
	public class PunchPosition : LatentActionNode<Transform, Vector3, float, Ease, int>{
		private Tween tween;
		public override IEnumerator Invoke(Transform transform, Vector3 punch, float duration, Ease easeType, int vibrato){
			var active = true;
			tween = transform.DOPunchPosition(punch, duration, vibrato);
			tween.SetEase(easeType);
			tween.OnComplete( ()=> { active = false; } );
			while (active) yield return null;						
		}
		public override void OnBreak(){ tween.Kill(); }
	}

	[Category("Functions/Extensions/DOTween")]
	public class PunchRotation : LatentActionNode<Transform, Vector3, float, Ease, int>{
		private Tween tween;
		public override IEnumerator Invoke(Transform transform, Vector3 punch, float duration, Ease easeType, int vibrato){
			var active = true;
			tween = transform.DOPunchRotation(punch, duration, vibrato);
			tween.SetEase(easeType);
			tween.OnComplete( ()=> { active = false; } );
			while (active) yield return null;						
		}
		public override void OnBreak(){ tween.Kill(); }
	}

	[Category("Functions/Extensions/DOTween")]
	public class PunchScale : LatentActionNode<Transform, Vector3, float, Ease, int>{
		private Tween tween;
		public override IEnumerator Invoke(Transform transform, Vector3 punch, float duration, Ease easeType, int vibrato){
			var active = true;
			tween = transform.DOPunchScale(punch, duration, vibrato);
			tween.SetEase(easeType);
			tween.OnComplete( ()=> { active = false; } );
			while (active) yield return null;						
		}
		public override void OnBreak(){ tween.Kill(); }
	}

	[Category("Functions/Extensions/DOTween")]
	public class ShakePosition : LatentActionNode<Transform, Vector3, float, Ease, int>{
		private Tween tween;
		public override IEnumerator Invoke(Transform transform, Vector3 strength, float duration, Ease easeType, int vibrato){
			var active = true;
			tween = transform.DOShakePosition(duration, strength, vibrato);
			tween.SetEase(easeType);
			tween.OnComplete( ()=> { active = false; } );
			while (active) yield return null;						
		}
		public override void OnBreak(){ tween.Kill(); }
	}

	[Category("Functions/Extensions/DOTween")]
	public class ShakeRotation : LatentActionNode<Transform, Vector3, float, Ease, int>{
		private Tween tween;
		public override IEnumerator Invoke(Transform transform, Vector3 strength, float duration, Ease easeType, int vibrato){
			var active = true;
			tween = transform.DOShakeRotation(duration, strength, vibrato);
			tween.SetEase(easeType);
			tween.OnComplete( ()=> { active = false; } );
			while (active) yield return null;						
		}
		public override void OnBreak(){ tween.Kill(); }
	}

	[Category("Functions/Extensions/DOTween")]
	public class ShakeScale : LatentActionNode<Transform, Vector3, float, Ease, int>{
		private Tween tween;
		public override IEnumerator Invoke(Transform transform, Vector3 strength, float duration, Ease easeType, int vibrato){
			var active = true;
			tween = transform.DOShakeScale(duration, strength, vibrato);
			tween.SetEase(easeType);
			tween.OnComplete( ()=> { active = false; } );
			while (active) yield return null;						
		}
		public override void OnBreak(){ tween.Kill(); }
	}

	[Category("Functions/Extensions/DOTween/Paths")]
	public class TweenPath : LatentActionNode<Transform, IList<Vector3>, float, Ease, PathType, PathMode>{
		private Tween tween;
		public override IEnumerator Invoke(Transform transform, IList<Vector3> waypoints, float duration, Ease easeType, PathType pathType, PathMode pathMode){
			var active = true;
			tween = transform.DOPath(waypoints.ToArray(), duration, pathType, pathMode);
			tween.SetEase(easeType);
			tween.OnComplete( ()=> { active = false; } );
			while (active) yield return null;									
		}
		public override void OnBreak(){ tween.Kill(); }
	}

	[Category("Functions/Extensions/DOTween/Direct Values")]
	public class TweenFloat : LatentActionNode<float, float, float, Ease>{
		private Tween tween;
		public float value{get; private set;}
		public override IEnumerator Invoke(float from, float to, float duration, Ease easeType){
			var active = true;
			value = from;
			tween = DG.Tweening.DOTween.To( ()=> value, x=> value = x, to, duration );
			tween.SetEase(easeType);
			tween.OnComplete( ()=>{active = false;} );
			while (active) yield return null;
		}
		public override void OnBreak(){ tween.Kill(); }
	}

	[Category("Functions/Extensions/DOTween/Direct Values")]
	public class TweenColor : LatentActionNode<Color, Color, float, Ease>{
		private Tween tween;
		public Color value{get; private set;}
		public override IEnumerator Invoke(Color from, Color to, float duration, Ease easeType){
			var active = true;
			value = from;
			tween = DG.Tweening.DOTween.To( ()=> value, x=> value = x, to, duration );
			tween.SetEase(easeType);
			tween.OnComplete( ()=>{active = false;} );
			while (active) yield return null;
		}
		public override void OnBreak(){ tween.Kill(); }
	}
}